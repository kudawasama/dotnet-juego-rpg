namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Evento disparado al subir un atributo base
    /// </summary>
    public class EventoAtributoSubido : IEventoJuego
    {
        public string Atributo
        {
            get;
        }
        public double NuevoValor
        {
            get;
        }
        public EventoAtributoSubido(string atributo, double nuevoValor)
        {
            Atributo = atributo;
            NuevoValor = nuevoValor;
        }
    }
}