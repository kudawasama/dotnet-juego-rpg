namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Multiplicadores de supervivencia por contexto (bioma, actividad, etc.).
    /// Separado de SupervivenciaConfig para cumplir SA1402 (un tipo por archivo).
    /// </summary>
    public class MultiplicadoresContexto
    {
        public double Hambre { get; set; } = 1.0;
        public double Sed { get; set; } = 1.0;
        public double Fatiga { get; set; } = 1.0;
    }
}
