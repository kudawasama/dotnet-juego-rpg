using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Objetos
{
    public class ObjetoJsonConverter : JsonConverter<Objeto>
    {
        public override Objeto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("TipoObjeto", out var tipoProp))
                    throw new JsonException("No se encontró el campo TipoObjeto en el objeto.");
                var tipo = tipoProp.GetString();
                switch (tipo)
                {
                    case "Pocion":
                        return JsonSerializer.Deserialize<Pocion>(root.GetRawText(), options);
                    case "Arma":
                        return JsonSerializer.Deserialize<Arma>(root.GetRawText(), options);
                    default:
                        throw new JsonException($"Tipo de objeto desconocido: {tipo}");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Objeto value, JsonSerializerOptions options)
        {
            var tipo = value.GetType().Name;
            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            using (var doc = JsonDocument.Parse(json))
            {
                writer.WriteStartObject();
                writer.WriteString("TipoObjeto", tipo);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    prop.WriteTo(writer);
                }
                writer.WriteEndObject();
            }
        }
    }
}
