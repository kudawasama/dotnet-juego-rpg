namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento de subida de nivel de personaje
    /// </summary>
    public class EventoNivelSubido : IEventoJuego
    {
        public int Nivel
        {
            get;
        }
        public EventoNivelSubido(int nivel)
        {
            Nivel = nivel;
        }
    }
}