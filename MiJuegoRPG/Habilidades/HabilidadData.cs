using System.Collections.Generic;

namespace MiJuegoRPG.Habilidades
{
    public class HabilidadData
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Activa | Pasiva
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Dictionary<string, int>? AtributosNecesarios { get; set; } = new Dictionary<string, int>();
        public List<CondicionData> Condiciones { get; set; } = new List<CondicionData>();
        public string Beneficio { get; set; } = string.Empty;
        public List<string> Mejoras { get; set; } = new List<string>();
        public bool Oculta { get; set; } = false;
        public int? Exp { get; set; } = 0;
        public int? CostoMana
        {
            get; set;
        }
        public string? AccionId
        {
            get; set;
        }
        public List<EvolucionData> Evoluciones { get; set; } = new List<EvolucionData>();
    }
}