using System.Collections.Generic;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Interfaces
{
    public class ResultadoAccion
    {
        public string NombreAccion { get; set; } = string.Empty;
        public ICombatiente Ejecutor { get; set; } = default!;
        public ICombatiente Objetivo { get; set; } = default!;
        // Daño calculado por la acción antes de defensas (si aplica)
        public int DanioBase
        {
            get; set;
        }
        // Daño real aplicado tras defensas y clamps
        public int DanioReal
        {
            get; set;
        }
        public bool EsMagico
        {
            get; set;
        }
        // Metadatos de resolución
        public bool FueCritico { get; set; } = false;
        public bool FueEvadido { get; set; } = false;
        public bool ObjetivoDerrotado
        {
            get; set;
        }
        public List<string> Mensajes { get; set; } = new List<string>();
        // Efectos de estado aplicados por esta acción (p.ej. Veneno)
        public List<IEfecto> EfectosAplicados { get; set; } = new List<IEfecto>();
    }
}
