namespace MiJuegoRPG.Core.Combate.Enums
{
    public enum RngStreamId : byte
    {
        /// <summary>
        /// Core random number stream.
        /// </summary>
        Core = 0,

        /// <summary>
        /// Critical hit random number stream.
        /// </summary>
        Crit = 1,

        /// <summary>
        /// Proc (procedure) random number stream.
        /// </summary>
        Proc = 2,

        /// <summary>
        /// AI random number stream.
        /// </summary>
        Ai = 3,

        /// <summary>
        /// Loot random number stream.
        /// </summary>
        Loot = 4,

        /// <summary>
        /// Damage over time (DoT) random number stream.
        /// </summary>
        Dot = 5,
    }
}
