namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento de supervivencia - Umbral cruzado
    /// </summary>
    public class EventoSupervivenciaUmbralCruzado : IEventoJuego
    {
        /// <summary>
        /// Tipo de supervivencia: "Hambre" | "Sed" | "Fatiga"
        /// </summary>
        public string Tipo
        {
            get;
        }
        /// <summary>
        /// Estado anterior: OK | ADVERTENCIA | CRÍTICO
        /// </summary>
        public string EstadoAnterior
        {
            get;
        }
        /// <summary>
        /// Estado nuevo: OK | ADVERTENCIA | CRÍTICO
        /// </summary>
        public string EstadoNuevo
        {
            get;
        }
        /// <summary>
        /// Valor 0..1 en el momento del cambio
        /// </summary>
        public double Valor
        {
            get;
        }
        public EventoSupervivenciaUmbralCruzado(string tipo, string estadoAnterior, string estadoNuevo, double valor)
        {
            Tipo = tipo;
            EstadoAnterior = estadoAnterior;
            EstadoNuevo = estadoNuevo;
            Valor = valor;
        }
    }
}