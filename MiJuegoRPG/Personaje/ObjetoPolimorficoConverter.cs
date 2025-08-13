using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Personaje
{
    public class ObjetoPolimorficoConverter : JsonConverter<Objeto>
    {
        public override Objeto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;
                if (!root.TryGetProperty("TipoObjeto", out var tipoProp))
                    throw new JsonException("Falta el campo TipoObjeto para deserializar Objeto.");
                string tipo = tipoProp.GetString() ?? "";
                switch (tipo.ToLower())
                {
                    case "pocion":
                        return JsonSerializer.Deserialize<Pocion>(root.GetRawText(), options);
                    case "arma":
                        return JsonSerializer.Deserialize<Arma>(root.GetRawText(), options);
                    case "material":
                        return JsonSerializer.Deserialize<Material>(root.GetRawText(), options);
                    // Agrega aquí más tipos según tus clases concretas
                    default:
                        throw new JsonException($"TipoObjeto desconocido: {tipo}");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Objeto value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
