using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class SectorData
    {
        public string Id { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Ciudad";
        public string Descripcion { get; set; } = string.Empty;
        public int NivelMinimo { get; set; }
        public int NivelMaximo { get; set; }
        public List<string> Enemigos { get; set; } = new List<string>();
        public List<string> Conexiones { get; set; } = new List<string>();
        public bool CiudadInicial { get; set; } = false;
        public List<string> Eventos { get; set; } = new List<string>();
        public Dictionary<string, object> Requisitos { get; set; } = new Dictionary<string, object>();
        public List<MiJuegoRPG.Motor.NodoRecoleccion>? NodosRecoleccion { get; set; }

        // Nuevos metadatos de ciudad
        public bool CiudadPrincipal { get; set; } = false;
        public bool EsCentroCiudad { get; set; } = false;
        public string? ParteCiudad { get; set; } = null;
        public string Rutas { get; set; } = string.Empty;
    }
}
