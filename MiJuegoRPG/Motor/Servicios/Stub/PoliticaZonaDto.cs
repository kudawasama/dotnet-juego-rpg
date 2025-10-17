// PoliticaZonaDto
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    public class PoliticaZonaDto
    {
        [JsonPropertyName("permitido")]
        public bool Permitido { get; set; } = true;

        [JsonPropertyName("delitoId")]
        public string? DelitoId { get; set; }

        [JsonPropertyName("risky")]
        public bool Risky { get; set; } = false;
    }
}
