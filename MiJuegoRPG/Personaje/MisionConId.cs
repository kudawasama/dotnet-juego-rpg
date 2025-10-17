using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Clase auxiliar para misiones con Id y condiciones.
    /// </summary>
    public class MisionConId
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UbicacionNPC { get; set; } = string.Empty;
        public List<string> Requisitos { get; set; } = new List<string>();
        public List<string> Recompensas { get; set; } = new List<string>();
        public int ExpNivel { get; set; } = 0;
        public Dictionary<string, int> ExpAtributos { get; set; } = new Dictionary<string, int>();
        public string Estado { get; set; } = string.Empty;
        public string SiguienteMisionId { get; set; } = string.Empty;
        public List<string> Condiciones { get; set; } = new List<string>();
    }
}