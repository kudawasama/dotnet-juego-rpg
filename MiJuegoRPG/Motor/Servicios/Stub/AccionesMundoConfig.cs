// AccionesMundoConfig
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    public class AccionesMundoConfig
    {
        [JsonPropertyName("acciones")]
        public Dictionary<string, ActionWorldDef> Acciones { get; set; } = new();
    }
}
