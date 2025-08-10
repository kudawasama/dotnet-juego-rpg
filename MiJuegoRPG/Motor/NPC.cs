using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class NPC
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public required string Ubicacion { get; set; }
        public required List<string> Misiones { get; set; }
    }
}
