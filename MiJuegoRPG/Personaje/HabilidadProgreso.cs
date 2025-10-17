using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Progreso y metadatos de una habilidad aprendida por el personaje.
    /// </summary>
    public class HabilidadProgreso
    {
        /// <summary>Gets or sets identificador único de la habilidad (coincide con los JSON de DatosJuego/habilidades y set/equipo).</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets nombre legible de la habilidad.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Gets or sets experiencia acumulada en la habilidad.</summary>
        public int Exp { get; set; } = 0;

        /// <summary>Gets or sets requisitos de atributos mínimos para usar la habilidad (opcional).</summary>
        public Dictionary<string, int>? AtributosNecesarios
        {
            get; set;
        }

        /// <summary>Gets or sets posibles evoluciones y sus condiciones.</summary>
        public List<EvolucionHabilidad> Evoluciones { get; set; } = new List<EvolucionHabilidad>();

        /// <summary>Gets or sets iDs de evoluciones ya desbloqueadas.</summary>
        public HashSet<string> EvolucionesDesbloqueadas { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets nivel calculado. Política simple por defecto: cada 10 exp => +1 nivel (nivel base 1).
        /// </summary>
        public int Nivel => (Exp / 10) + 1;
    }

    // SA1402: EvolucionHabilidad y CondicionEvolucion se movieron a archivos separados:
    // - EvolucionHabilidad.cs
    // - CondicionEvolucion.cs
}
