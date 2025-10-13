namespace MiJuegoRPG.Motor.Servicios.Compat
{
    // Clase alias en español para transición progresiva sin romper referencias existentes.
    public class ServicioRecoleccion : RecoleccionService
    {
        public ServicioRecoleccion(Juego juego)
            : base(juego) { }
    }
}