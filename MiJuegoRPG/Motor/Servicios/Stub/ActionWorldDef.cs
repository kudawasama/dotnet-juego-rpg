// ActionWorldDef
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ActionWorldDef
    {
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("energia")]
        public int CosteEnergia { get; set; } = 1;

        [JsonPropertyName("tiempo")]
        public int CosteTiempoMin { get; set; } = 1;

        [JsonPropertyName("cooldown")]
        public int CooldownMin { get; set; } = 0;

        [JsonPropertyName("requisitos")]
        public RequisitosAccion? Requisitos { get; set; }

        [JsonPropertyName("consecuencias")]
        public ConsecuenciasAccion? Consecuencias { get; set; }

        // Aliases para compatibilidad
        public int Energia
        {
            get => CosteEnergia;
            set => CosteEnergia = value;
        }
        public int Tiempo
        {
            get => CosteTiempoMin;
            set => CosteTiempoMin = value;
        }
        public int Cooldown
        {
            get => CooldownMin;
            set => CooldownMin = value;
        }
    }

    public class RequisitosAccion
    {
        [JsonPropertyName("clase")]
        public List<string>? Clase { get; set; }

        [JsonPropertyName("atributos")]
        public Dictionary<string, int>? Atributos { get; set; }
    }

    public class ConsecuenciasAccion
    {
        public float ChanceDeteccion { get; set; } = 0.0f;
        public string? TipoDelito { get; set; }
        public string? DelitoId { get; set; }
    }

    public class ResultadoDelito
    {
        public bool AlertaCiudad { get; set; }
        public int ReputacionCambiada { get; set; }
        public int MultaAplicada { get; set; }
        public string DelitoId { get; set; } = string.Empty;
    }
}
