// ZonaPoliticaConfig DTO
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ZonaPoliticaConfig
    {
        [JsonPropertyName("zonas")]
        public Dictionary<string, Dictionary<string, PoliticaZonaDto>> Zonas { get; set; } = new();
    }
}
