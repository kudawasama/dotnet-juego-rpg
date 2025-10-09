using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Core.Combate
{
    // Fases principales (extensible). Mantener orden estable para comparaciones.
    public enum ActionPhase : byte { Windup=0, Cast=1, Channel=2, Impact=3, Recovery=4, Cooldown=5, Finished=6, Cancelled=7 }

    public enum RngStreamId : byte { Core=0, Crit=1, Proc=2, Ai=3, Loot=4, Dot=5 }

    public interface IRng { int NextInt(int maxExclusive); double NextDouble(); }
    public interface IRngFactory { IRng GetStream(RngStreamId id); }

    internal sealed class SplitRngFactory : IRngFactory
    {
        private readonly int _baseSeed;
        private readonly Dictionary<RngStreamId, IRng> _streams = new();
        public SplitRngFactory(int baseSeed) => _baseSeed = baseSeed;
        public IRng GetStream(RngStreamId id)
        {
            if (_streams.TryGetValue(id, out var r)) return r;
            var seeded = new XorShift32(_baseSeed + (int)id * 10007);
            _streams[id] = seeded; return seeded;
        }
        private sealed class XorShift32 : IRng
        {
            private uint _state; public XorShift32(int seed){ _state = (uint)(seed==0?0xA341316C:seed); }
            private uint Next(){ uint x=_state; x^=x<<13; x^=x>>17; x^=x<<5; _state=x; return x; }
            public int NextInt(int maxExclusive)=> (int)(Next()% (uint)maxExclusive);
            public double NextDouble()=> Next() / (double)uint.MaxValue;
        }
    }

    // Clave de orden determinista.
    public readonly struct ActionOrderKey : IComparable<ActionOrderKey>
    {
        public readonly int ScheduledTick;
        public readonly byte PhaseOrdinal;
        public readonly byte PriorityTier;
        public readonly int SpeedScore;
        public readonly int Sequence;
        public readonly int ActorId;
        public ActionOrderKey(int scheduledTick, byte phaseOrdinal, byte priorityTier, int speedScore, int sequence, int actorId)
        { ScheduledTick=scheduledTick; PhaseOrdinal=phaseOrdinal; PriorityTier=priorityTier; SpeedScore=speedScore; Sequence=sequence; ActorId=actorId; }
        public int CompareTo(ActionOrderKey o)
        {
            int c; if((c=ScheduledTick.CompareTo(o.ScheduledTick))!=0) return c;
            if((c=PhaseOrdinal.CompareTo(o.PhaseOrdinal))!=0) return c;
            if((c=PriorityTier.CompareTo(o.PriorityTier))!=0) return c;
            if((c=o.SpeedScore.CompareTo(SpeedScore))!=0) return c; // mayor velocidad primero
            if((c=Sequence.CompareTo(o.Sequence))!=0) return c;
            return ActorId.CompareTo(o.ActorId);
        }
        public override string ToString()=>$"T={ScheduledTick} P={PhaseOrdinal} Pr={PriorityTier} S={SpeedScore} Seq={Sequence} A={ActorId}";
    }

    // Evento de combate determinista.
    public enum CombatEventType : byte { ActionImpact, DamageApplied, ActionStart, ActionCancelled, Death, DotTick }
    public readonly struct CombatEvent
    {
        public readonly int Tick;
        public readonly CombatEventType Type;
        public readonly int ActorId;
        public readonly int TargetId;
        public readonly int RefActionSeq;
        public readonly int V1; // daño, etc.
        public readonly int V2; // crítico flag (0/1) u otro
        public CombatEvent(int tick, CombatEventType type, int actorId, int targetId, int refActionSeq, int v1=0, int v2=0)
        { Tick=tick; Type=type; ActorId=actorId; TargetId=targetId; RefActionSeq=refActionSeq; V1=v1; V2=v2; }
        public override string ToString()=> $"[{Tick}] {Type} A:{ActorId} -> T:{TargetId} act:{RefActionSeq} v1:{V1} v2:{V2}";
    }

    public sealed class CombatEventLog
    {
        private readonly List<CombatEvent> _events = new(256);
        public IReadOnlyList<CombatEvent> Events => _events;
        public void Add(CombatEvent ev) => _events.Add(ev);
        public int ComputeDeterministicHash()
        {
            unchecked
            {
                int h=17; foreach(var e in _events){ h = h*31 + e.Tick; h = h*31 + (int)e.Type; h = h*31 + e.ActorId; h = h*31 + e.TargetId; h = h*31 + e.RefActionSeq; h = h*31 + e.V1; h = h*31 + e.V2; }
                return h;
            }
        }
    }

    public sealed class CombatContext
    {
        public IRngFactory Rng { get; }
        public CombatEventLog Log { get; }
        public CombatContext(IRngFactory rng, CombatEventLog log){ Rng=rng; Log=log; }
    }

    public abstract class CombatAction
    {
        public int SequenceId { get; internal set; }
        public int ActorId { get; }
        public int TargetId { get; }
        public ActionPhase Phase { get; private set; }
        public ActionOrderKey OrderKey { get; private set; }
        private int _phaseStartTick;
        // Proveedor del tick actual (inyectado por la timeline). Centralizado aquí para evitar cast a tipos concretos.
        internal Func<int>? CurrentTickProvider { get; set; }
        protected abstract int PhaseDurationTicks(ActionPhase phase);
        protected abstract byte PriorityTier { get; }
        protected abstract int SpeedScore { get; }
        protected abstract void OnImpact(CombatContext ctx);

        protected CombatAction(int actorId, int targetId){ ActorId=actorId; TargetId=targetId; }

    internal void Init(int currentTick, int sequence, CombatEventLog log){ SequenceId=sequence; Phase=ActionPhase.Cast; _phaseStartTick=currentTick; RebuildKey(currentTick); log.Add(new CombatEvent(currentTick, CombatEventType.ActionStart, ActorId, TargetId, SequenceId)); }

        private void RebuildKey(int currentTick)
        { OrderKey = new ActionOrderKey(CalculateScheduledTick(currentTick), (byte)Phase, PriorityTier, SpeedScore, SequenceId, ActorId); }
        private int CalculateScheduledTick(int currentTick)
        {
            if (Phase==ActionPhase.Cast) return _phaseStartTick + PhaseDurationTicks(ActionPhase.Cast);
            if (Phase==ActionPhase.Impact) return currentTick;
            return currentTick; // demás fases no generan impacto nuevo
        }

        internal void Advance(CombatContext ctx, int currentTick)
        {
            var dur = PhaseDurationTicks(Phase);
            if (dur>0 && currentTick - _phaseStartTick < dur) return; // aún en la fase
            switch(Phase)
            {
                case ActionPhase.Cast:
                    Phase = ActionPhase.Impact;
                    RebuildKey(currentTick);
                    // Impacto inmediato en esta transición
                    OnImpact(ctx);
                    ctx.Log.Add(new CombatEvent(currentTick, CombatEventType.ActionImpact, ActorId, TargetId, SequenceId));
                    Phase = ActionPhase.Recovery;
                    _phaseStartTick = currentTick; RebuildKey(currentTick);
                    break;
                case ActionPhase.Recovery:
                    Phase = ActionPhase.Finished; RebuildKey(currentTick); break;
            }
        }
        public bool IsFinished => Phase==ActionPhase.Finished || Phase==ActionPhase.Cancelled;
    }

    public sealed class SimpleAttackAction : CombatAction
    {
        private readonly int _cast;
        private readonly int _recovery;
        private readonly int _baseDamage;
        public SimpleAttackAction(int actorId, int targetId, int castTicks, int recoveryTicks, int baseDamage) : base(actorId, targetId)
        { _cast=castTicks; _recovery=recoveryTicks; _baseDamage=baseDamage; }
        protected override int PhaseDurationTicks(ActionPhase phase) => phase switch { ActionPhase.Cast=>_cast, ActionPhase.Recovery=>_recovery, _=>0};
        protected override byte PriorityTier => 5;
        protected override int SpeedScore => 100; // placeholder
        protected override void OnImpact(CombatContext ctx)
        {
            var crit = ctx.Rng.GetStream(RngStreamId.Crit).NextDouble() < 0.1;
            int dmg = crit ? (int)(_baseDamage * 1.5) : _baseDamage;
            ctx.Log.Add(new CombatEvent(CurrentTickProvider?.Invoke() ?? 0, CombatEventType.DamageApplied, ActorId, TargetId, SequenceId, dmg, crit?1:0));
        }
    }

    public sealed class CombatTimeline
    {
        private readonly List<CombatAction> _actions = new();
        private readonly CombatContext _ctx;
        private int _seq;
        public int CurrentTick { get; private set; }
        public CombatEventLog Log => _ctx.Log;
        public CombatTimeline(int seed)
        { var log = new CombatEventLog(); _ctx = new CombatContext(new SplitRngFactory(seed), log); }
    public void Enqueue(CombatAction action){ action.CurrentTickProvider = () => CurrentTick; action.Init(CurrentTick, ++_seq, _ctx.Log); _actions.Add(action); }
        public void Tick()
        {
            CurrentTick++;
            _actions.Sort((a,b)=>a.OrderKey.CompareTo(b.OrderKey));
            foreach (var a in _actions) if(!a.IsFinished) a.Advance(_ctx, CurrentTick);
            for (int i=_actions.Count-1;i>=0;i--) if (_actions[i].IsFinished) _actions.RemoveAt(i);
        }
    }
}
