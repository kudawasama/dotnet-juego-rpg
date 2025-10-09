namespace MiJuegoRPG.Core.Combate.Enums
{
    public enum ActionPhase : byte
    {
        Windup = 0,
        Cast = 1,
        Channel = 2,
        Impact = 3,
        Recovery = 4,
        Cooldown = 5,
        Finished = 6,
        Cancelled = 7
    }
}
