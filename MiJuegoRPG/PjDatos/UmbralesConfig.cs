namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Configuración de umbrales de supervivencia (advertencia/crítico).
    /// Separado de SupervivenciaConfig para cumplir SA1402 (un tipo por archivo).
    /// </summary>
    public class UmbralesConfig
    {
        public UmbralValores Advertencia { get; set; } = new UmbralValores();
        public UmbralValores Critico { get; set; } = new UmbralValores();
    }

    /// <summary>
    /// Valores específicos para un umbral de supervivencia.
    /// Incluido en el mismo archivo que UmbralesConfig por dependencia directa.
    /// </summary>
    public class UmbralValores
    {
        public double Hambre
        {
            get; set;
        }
        public double Sed
        {
            get; set;
        }
        public double Fatiga
        {
            get; set;
        }
        public double Frio
        {
            get; set;
        }
        public double Calor
        {
            get; set;
        }
    }
}
