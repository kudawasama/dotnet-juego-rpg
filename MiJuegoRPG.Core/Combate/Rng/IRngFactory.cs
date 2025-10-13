namespace MiJuegoRPG.Core.Combate.Rng
{
    using MiJuegoRPG.Core.Combate.Enums;

    public interface IRngFactory
    {
        IRng GetStream(RngStreamId id);
    }
}
