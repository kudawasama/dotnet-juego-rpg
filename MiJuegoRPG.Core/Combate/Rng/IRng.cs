namespace MiJuegoRPG.Core.Combate.Rng
{
    public interface IRng // Contrato mínimo para generadores de números aleatorios deterministas.
    {
        int NextInt(int maxExclusive);

        double NextDouble();
    }
}
