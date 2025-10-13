namespace MiJuegoRPG.Core.Combate.Eventos
{
    /// <summary>
    /// Types of combat events that can occur during a battle.
    /// </summary>
    public enum CombatEventType : byte
    {
        /// <summary>
        /// An action has impacted a target.
        /// </summary>
        ActionImpact,

        /// <summary>
        /// Damage has been applied to a target.
        /// </summary>
        DamageApplied,

        /// <summary>
        /// An action has started.
        /// </summary>
        ActionStart,

        /// <summary>
        /// An action has been cancelled.
        /// </summary>
        ActionCancelled,

        /// <summary>
        /// An actor has died.
        /// </summary>
        Death,

        /// <summary>
        /// A damage-over-time effect has ticked.
        /// </summary>
        DotTick,
    }

    public readonly struct CombatEvent
    {
        public CombatEvent(int tick, CombatEventType type, int actorId, int targetId, int refActionSeq, int v1 = 0, int v2 = 0)
        {
            Tick = tick;
            Type = type;
            ActorId = actorId;
            TargetId = targetId;
            RefActionSeq = refActionSeq;
            V1 = v1;
            V2 = v2;
        }

        public int Tick
        {
            get;
        }

        public CombatEventType Type
        {
            get;
        }

        public int ActorId
        {
            get;
        }

        public int TargetId
        {
            get;
        }

        public int RefActionSeq
        {
            get;
        }

        public int V1
        {
            get;
        }

        public int V2
        {
            get;
        }

        public override string ToString() =>
            $"[{Tick}] {Type} A:{ActorId} -> T:{TargetId} act:{RefActionSeq} v1:{V1} v2:{V2}";
    }
}
