namespace MiJuegoRPG.PjDatos
{
    public class ReglasBioma
    {
        public double TempDia
        {
            get; set;
        }
        public double TempNoche
        {
            get; set;
        }
        public double SedMultiplier { get; set; } = 1.0;
        public double HambreMultiplier { get; set; } = 1.0;
        public double FatigaMultiplier { get; set; } = 1.0;
    }
}
