using System.Text.Json;
using System.IO;

namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Representa los requisitos y dependencias para desbloquear una clase superior.
    /// </summary>
    public class ClaseProgresion
    {
        /// <summary>Gets or sets nombre de la clase superior (ej: Nigromante).</summary>
        public required string Nombre
        {
            get; set;
        }

        /// <summary>Gets or sets clases previas requeridas para evolucionar (ej: Mago, Hechicero).</summary>
        public List<string> ClasesPrevias { get; set; } = new List<string>();

        /// <summary>Gets or sets nivel mínimo requerido para evolucionar.</summary>
        public int NivelMinimo
        {
            get; set;
        }

        /// <summary>Gets or sets atributos requeridos y sus valores mínimos.</summary>
        public Dictionary<string, double> AtributosRequeridos { get; set; } = new Dictionary<string, double>();

        /// <summary>Gets or sets reputación mínima requerida (puedes omitir si no usas reputación).</summary>
        public int ReputacionMinima
        {
            get; set;
        }

        /// <summary>Gets or sets nombre de la misión única requerida para evolucionar (opcional).</summary>
        public string? MisionUnica
        {
            get; set;
        }

        /// <summary>Gets or sets nombre del objeto único requerido para evolucionar (opcional).</summary>
        public string? ObjetoUnico
        {
            get; set;
        }

        /// <summary>Gets or sets a value indicating whether indica si la clase es oculta y no se muestra al jugador hasta cumplir requisitos.</summary>
        public bool Oculta { get; set; } = false;
        // Puedes agregar más condiciones según tu sistema
    }
}
