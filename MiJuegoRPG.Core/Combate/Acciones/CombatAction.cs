namespace MiJuegoRPG.Core.Combate.Acciones
{
    using System;
    using MiJuegoRPG.Core.Combate.Context;
    using MiJuegoRPG.Core.Combate.Enums;
    using MiJuegoRPG.Core.Combate.Eventos;
    using MiJuegoRPG.Core.Combate.Orden;

    public abstract class CombatAction
    {
        private int phaseStartTick;

        protected CombatAction(int actorId, int targetId)
        {
            ActorId = actorId;
            TargetId = targetId;
        }

        // Public properties
        public int SequenceId
        {
            get; internal set;
        }

        public int ActorId
        {
            get;
        }

        public int TargetId
        {
            get;
        }

        public ActionPhase Phase
        {
            get; private set;
        }

        public ActionOrderKey OrderKey
        {
            get; private set;
        }

        public bool IsFinished => Phase == ActionPhase.Finished || Phase == ActionPhase.Cancelled;

        // Internal properties
        internal Func<int>? CurrentTickProvider
        {
            get; set;
        }

        // Protected abstract properties
        protected abstract byte PriorityTier
        {
            get;
        }

        protected abstract int SpeedScore
        {
            get;
        }

        // Internal methods (En español es "métodos internos", su funcion es para uso dentro del ensamblado)
        internal void Init(int currentTick, int sequence, CombatEventLog log)
        {
            SequenceId = sequence;
            Phase = ActionPhase.Cast;
            phaseStartTick = currentTick;
            RebuildKey(currentTick);
            log.Add(new CombatEvent(currentTick, CombatEventType.ActionStart, ActorId, TargetId, SequenceId));
        }

        internal void Advance(CombatContext ctx, int currentTick)
        {
            var dur = PhaseDurationTicks(Phase);
            if (dur > 0 && currentTick - phaseStartTick < dur)
            {
                return;
            }

            switch (Phase)
            {
                case ActionPhase.Cast:
                    Phase = ActionPhase.Impact;
                    RebuildKey(currentTick);
                    OnImpact(ctx);
                    ctx.Log.Add(new CombatEvent(currentTick, CombatEventType.ActionImpact, ActorId, TargetId, SequenceId));
                    Phase = ActionPhase.Recovery;
                    phaseStartTick = currentTick;
                    RebuildKey(currentTick);
                    break;

                case ActionPhase.Recovery:
                    Phase = ActionPhase.Finished;
                    RebuildKey(currentTick);
                    break;
            }
        }

        // Protected abstract methods
        protected abstract int PhaseDurationTicks(ActionPhase phase);

        protected abstract void OnImpact(CombatContext ctx);

        // Private methods
        private void RebuildKey(int currentTick)
        {
            OrderKey = new ActionOrderKey(CalculateScheduledTick(currentTick), (byte)Phase, PriorityTier, SpeedScore, SequenceId, ActorId);
        }

        private int CalculateScheduledTick(int currentTick)
        {
            if (Phase == ActionPhase.Cast)
            {
                return phaseStartTick + PhaseDurationTicks(ActionPhase.Cast);
            }

            if (Phase == ActionPhase.Impact)
            {
                return currentTick;
            }

            return currentTick;
        }
    }
}
