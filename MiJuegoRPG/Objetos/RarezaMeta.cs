namespace MiJuegoRPG.Objetos
{
    public class RarezaMeta
    {
        public string Nombre { get; set; } = string.Empty;

        public double Peso { get; set; }

        public int PerfMin { get; set; }

        public int PerfMax { get; set; }

        public double PerfAvg { get; set; }

        public double BaseStatMult { get; set; }

        public double Prob { get; set; }

        public double ScarcityRaw { get; set; }

        public double ScarcityNorm { get; set; }

        public double PriceMult { get; set; }
    }
}
