namespace MiJuegoRPG.PjDatos
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class SectorData
    {
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        // "bioma" en los JSON → se mapea a Region para mantener compatibilidad con código existente
        [JsonPropertyName("bioma")]
        public string Region { get; set; } = string.Empty;

        // Por defecto los sectores son rutas/campo; evitar que falte 'tipo' en JSON y se trate como ciudad
        public string Tipo { get; set; } = "Ruta";

        public string Descripcion { get; set; } = string.Empty;

        public int NivelMinimo
        {
            get; set;
        }

        public int NivelMaximo
        {
            get; set;
        }

        public List<string> Enemigos { get; set; } = new List<string>();

        public List<string> Conexiones { get; set; } = new List<string>();

        public bool CiudadInicial { get; set; } = false;

        public List<string> Eventos { get; set; } = new List<string>();

        public Dictionary<string, object> Requisitos { get; set; } = new Dictionary<string, object>();

        public List<MiJuegoRPG.Motor.NodoRecoleccion>? NodosRecoleccion
        {
            get; set;
        }

        // Nuevos metadatos de ciudad
        [JsonPropertyName("ciudadPrincipal")]
        public bool CiudadPrincipal { get; set; } = false;

        [JsonPropertyName("esCentroCiudad")]
        public bool EsCentroCiudad { get; set; } = false;

        [JsonPropertyName("parteCiudad")]
        public string? ParteCiudad { get; set; } = null;

        public string Rutas { get; set; } = string.Empty;
    }
}
