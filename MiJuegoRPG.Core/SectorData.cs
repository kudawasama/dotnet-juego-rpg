using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.PjDatos
{
    public class SectorData
    {
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("nombre")] public string Nombre { get; set; } = string.Empty;
        [JsonPropertyName("bioma")] public string Region { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Ruta";
        public string Descripcion { get; set; } = string.Empty;
        public int NivelMinimo { get; set; }
        public int NivelMaximo { get; set; }
        public List<string> Enemigos { get; set; } = new();
        public List<string> Conexiones { get; set; } = new();
        public bool CiudadInicial { get; set; }
        public List<string> Eventos { get; set; } = new();
        public Dictionary<string, object> Requisitos { get; set; } = new();
    public List<NodoRecoleccionData>? NodosRecoleccion { get; set; }
        [JsonPropertyName("ciudadPrincipal")] public bool CiudadPrincipal { get; set; }
        [JsonPropertyName("esCentroCiudad")] public bool EsCentroCiudad { get; set; }
        [JsonPropertyName("parteCiudad")] public string? ParteCiudad { get; set; }
        public string Rutas { get; set; } = string.Empty;
    }
}
