namespace MiJuegoRPG.Motor.Servicios.Compat
{
    /// Clases alias en español para transición progresiva sin romper referencias existentes.
    public class ServicioProgresion : ProgressionService
    {
        public ServicioProgresion(string? rutaConfig = null) : base(rutaConfig) { }
    }
    public class ServicioRecoleccion : RecoleccionService
    {
        public ServicioRecoleccion(Juego juego) : base(juego) { }
    }
    public class ServicioGuardado : GuardadoService { }
    public class ServicioEnergia : EnergiaService { }
}