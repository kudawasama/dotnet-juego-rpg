namespace MiJuegoRPG.Motor.Servicios.Compat
{
    // Clases alias en español para transición progresiva sin romper referencias existentes.
    public class ServicioProgresion : ProgressionService
    {
        public ServicioProgresion(string? rutaConfig = null)
            : base(rutaConfig) { }
    }
        // SA1402: ServicioRecoleccion se movió a ServicioRecoleccion.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ServicioGuardado se movió a ServicioGuardado.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ServicioEnergia se movió a ServicioEnergia.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ServicioGuardado se movió a ServicioGuardado.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ServicioEnergia se movió a ServicioEnergia.cs para cumplir con SA1402 (un tipo por archivo)
}
