namespace MiJuegoRPG.Core.Combate.Enums
{
    public enum ActionPhase : byte
    {
        /// <summary>
        /// The initial phase before the action is executed.
        /// </summary>
        Windup = 0,

        /// <summary>
        /// The phase where the action is being cast.
        /// </summary>
        Cast = 1,

        /// <summary>
        /// The phase where the action is being channeled.
        /// </summary>
        Channel = 2,

        /// <summary>
        /// The phase where the action impacts the target.
        /// </summary>
        Impact = 3,

        /// <summary>
        /// The phase after impact, where recovery occurs.
        /// </summary>
        Recovery = 4,

        /// <summary>
        /// The cooldown phase after the action.
        /// </summary>
        Cooldown = 5,

        /// <summary>
        /// The action has finished.
        /// </summary>
        Finished = 6,

        /// <summary>
        /// The action was cancelled.
        /// </summary>
        Cancelled = 7,
    }
}
