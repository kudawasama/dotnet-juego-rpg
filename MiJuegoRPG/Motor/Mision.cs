using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Mision
    {
        public Mision() {
            Nombre = "";
            Descripcion = "";
            UbicacionNPC = "";
            Destino = "";
            RutaDesbloqueada = "";
        }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; } = "No iniciada"; // No iniciada, En progreso, Completada
        public List<string> Requisitos { get; set; } = new List<string>();
        public List<string> Recompensas { get; set; } = new List<string>();
        public string UbicacionNPC { get; set; }
        public string Destino { get; set; }
        public bool DesbloqueaRuta { get; set; } = false;
        public string RutaDesbloqueada { get; set; }
    }

    // Eliminada clase NPC duplicada. Usar la versión de NPC.cs
}
