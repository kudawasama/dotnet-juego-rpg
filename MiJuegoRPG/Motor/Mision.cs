using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Mision
    {
        public Mision()
        {
            Id = string.Empty;
            Condiciones = new List<string>();
            Nombre = "";
            Descripcion = "";
            Estado = "No iniciada";
            Requisitos = new List<string>();
            Recompensas = new List<string>();
            UbicacionNPC = "";
            Destino = "";
            DesbloqueaRuta = false;
            RutaDesbloqueada = "";
        }

        public string Id { get; set; }
        public List<string> Condiciones { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public List<string> Requisitos { get; set; }
        public List<string> Recompensas { get; set; }
        public string UbicacionNPC { get; set; }
        public string Destino { get; set; }
        public bool DesbloqueaRuta { get; set; }
        public string RutaDesbloqueada { get; set; }
    }

    // Eliminada clase NPC duplicada. Usar la versión de NPC.cs
}
