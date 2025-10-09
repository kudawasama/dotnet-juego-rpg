using MiJuegoRPG.Core.Combate.Context;
using MiJuegoRPG.Core.Combate.Enums;
using MiJuegoRPG.Core.Combate.Rng;

namespace MiJuegoRPG.Core.Combate.Acciones
{
    public sealed class SimpleAttackAction : CombatAction
    {
        private readonly int castTicks;
        private readonly int recoveryTicks;
        private readonly int baseDamage;

        public SimpleAttackAction(int actorId, int targetId, int castTicks, int recoveryTicks, int baseDamage)
            : base(actorId, targetId)
        {
            this.castTicks = castTicks;
            this.recoveryTicks = recoveryTicks;
            this.baseDamage = baseDamage;
        }

    // Mantener valores placeholder similares al diseño original (prioridad media y velocidad base)
    protected override byte PriorityTier => 5;
    protected override int SpeedScore => 100; // TODO: derivar de atributos del actor

        protected override int PhaseDurationTicks(ActionPhase phase)
        {
            return phase switch
            {
                ActionPhase.Cast => castTicks,
                ActionPhase.Recovery => recoveryTicks,
                _ => 0,
            };
        }

        protected override void OnImpact(CombatContext ctx)
        {
            var critRng = ctx.Rng.GetStream(RngStreamId.Crit);
            var isCrit = critRng.NextDouble() < 0.10; // 10% crit fijo por ahora
            var damage = isCrit ? (int)(baseDamage * 1.5) : baseDamage;
            ctx.Log.Add(new Eventos.CombatEvent(ctx.CurrentTick, Eventos.CombatEventType.DamageApplied, ActorId, TargetId, SequenceId, damage, isCrit ? 1 : 0));
        }
    }
}
