namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Configuración de consumo de recursos de supervivencia.
    /// Separado de SupervivenciaConfig para cumplir SA1402 (un tipo por archivo).
    /// </summary>
    public class ConsumoConfig
    {
        public ComidaAguaReduce Comida { get; set; } = new ComidaAguaReduce();
        public ComidaAguaReduce Agua { get; set; } = new ComidaAguaReduce();
        public FatigaReduce Descanso { get; set; } = new FatigaReduce();
        public MedicoReduce Medico { get; set; } = new MedicoReduce();
    }
}
