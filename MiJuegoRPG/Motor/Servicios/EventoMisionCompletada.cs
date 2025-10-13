namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento de misión completada (Id + Nombre)
    /// </summary>
    public class EventoMisionCompletada : IEventoJuego
    {
        public string Id
        {
            get;
        }
        public string Nombre
        {
            get;
        }
        public EventoMisionCompletada(string id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }
}