namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento de reputación - Umbral por facción
    /// </summary>
    public class EventoReputacionUmbralFaccion : IEventoJuego
    {
        public string Faccion
        {
            get;
        }
        public string BandaId
        {
            get;
        }
        public int ValorAnterior
        {
            get;
        }
        public int ValorNuevo
        {
            get;
        }
        public bool Subida
        {
            get;
        }
        public string Mensaje
        {
            get;
        }
        public EventoReputacionUmbralFaccion(string faccion, string bandaId, int valorAnterior, int valorNuevo, bool subida, string mensaje)
        {
            Faccion = faccion;
            BandaId = bandaId;
            ValorAnterior = valorAnterior;
            ValorNuevo = valorNuevo;
            Subida = subida;
            Mensaje = mensaje;
        }
    }
}