using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Convierte cualquier token JSON primitivo (number, bool, null) a string en deserialización.
    /// Evita fallos cuando datos legacy almacenan números donde hoy se espera string.
    /// </summary>
    public class LenientStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                JsonTokenType.True => "true",
                JsonTokenType.False => "false",
                JsonTokenType.Null => string.Empty,
                _ => JsonDocument.ParseValue(ref reader).RootElement.ToString()
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
