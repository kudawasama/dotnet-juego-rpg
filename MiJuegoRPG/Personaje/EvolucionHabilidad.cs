using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Definición de una evolución de la habilidad y sus condiciones de desbloqueo.
    /// </summary>
    public class EvolucionHabilidad
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Beneficio { get; set; } = string.Empty;
        public List<CondicionEvolucion> Condiciones { get; set; } = new List<CondicionEvolucion>();
    }
}