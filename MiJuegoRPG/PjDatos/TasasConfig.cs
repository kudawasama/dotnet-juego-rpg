namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Configuración de tasas base de supervivencia por hora.
    /// Separado de SupervivenciaConfig para cumplir SA1402 (un tipo por archivo).
    /// </summary>
    public class TasasConfig
    {
        public double HambrePorHora
        {
            get; set;
        }
        public double SedPorHora
        {
            get; set;
        }
        public double FatigaPorHora
        {
            get; set;
        }
        public double TempRecuperacionPorHora
        {
            get; set;
        }
    }
}
