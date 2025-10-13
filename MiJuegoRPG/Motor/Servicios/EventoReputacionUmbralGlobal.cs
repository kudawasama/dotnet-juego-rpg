namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento de reputación - Umbral global
    /// </summary>
    public class EventoReputacionUmbralGlobal : IEventoJuego
    {
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
        public EventoReputacionUmbralGlobal(string bandaId, int valorAnterior, int valorNuevo, bool subida, string mensaje)
        {
            BandaId = bandaId;
            ValorAnterior = valorAnterior;
            ValorNuevo = valorNuevo;
            Subida = subida;
            Mensaje = mensaje;
        }
    }
}