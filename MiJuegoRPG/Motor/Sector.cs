using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Sector
    {
        public required string Nombre
        {
            get; set;
        }
        public required string Descripcion
        {
            get; set;
        }
        public bool Descubierto { get; set; } = false;
        public List<string> Eventos { get; set; } = new List<string>();
        public List<string> EnemigosPosibles { get; set; } = new List<string>();
        public List<string> ObjetosPosibles { get; set; } = new List<string>();
        public List<string> Conexiones { get; set; } = new List<string>();
    }
}