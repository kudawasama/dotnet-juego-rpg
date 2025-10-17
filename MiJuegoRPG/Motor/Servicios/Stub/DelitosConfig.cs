// DelitosConfig y DelitoConfig
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    public class DelitosConfig
    {
        [JsonPropertyName("delitos")]
        public Dictionary<string, DelitoConfig> Delitos { get; set; } = new();
    }

    public class DelitoConfig
    {
        [JsonPropertyName("reputacionPenalty")]
        public int ReputacionPenalty { get; set; }

        [JsonPropertyName("multaMin")]
        public int MultaMin { get; set; }

        [JsonPropertyName("multaMax")]
        public int MultaMax { get; set; }

        [JsonPropertyName("faccionAfectada")]
        public string FaccionAfectada { get; set; } = string.Empty;

        [JsonPropertyName("activaAlerta")]
        public bool ActivaAlerta { get; set; } = false;
    }
}
