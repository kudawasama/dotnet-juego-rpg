using System.Collections.Generic;

namespace MiJuegoRPG.Habilidades
{
    public class EvolucionData
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<CondicionData> Condiciones { get; set; } = new List<CondicionData>();
        public string Beneficio { get; set; } = string.Empty;
        public int? CostoMana
        {
            get; set;
        }
        public List<string>? Mejoras
        {
            get; set;
        }
    }
}