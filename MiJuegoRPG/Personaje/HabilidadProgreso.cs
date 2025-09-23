using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Progreso y metadatos de una habilidad aprendida por el personaje.
    /// </summary>
    public class HabilidadProgreso
    {
        /// <summary>Identificador único de la habilidad (coincide con los JSON de DatosJuego/habilidades y set/equipo).</summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>Nombre legible de la habilidad.</summary>
        public string Nombre { get; set; } = string.Empty;
        /// <summary>Experiencia acumulada en la habilidad.</summary>
        public int Exp { get; set; } = 0;
        /// <summary>Requisitos de atributos mínimos para usar la habilidad (opcional).</summary>
        public Dictionary<string, int>? AtributosNecesarios { get; set; }
        /// <summary>Posibles evoluciones y sus condiciones.</summary>
        public List<EvolucionHabilidad> Evoluciones { get; set; } = new List<EvolucionHabilidad>();
        /// <summary>IDs de evoluciones ya desbloqueadas.</summary>
        public HashSet<string> EvolucionesDesbloqueadas { get; set; } = new HashSet<string>();
        /// <summary>
        /// Nivel calculado. Política simple por defecto: cada 10 exp => +1 nivel (nivel base 1).
        /// </summary>
        public int Nivel => (Exp / 10) + 1;
    }

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

    /// <summary>
    /// Condición para evolución de habilidad.
    /// Tipo ejemplos: "NvHabilidad", "NvJugador", "Ataque" (u otros contadores).
    /// </summary>
    public class CondicionEvolucion
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
