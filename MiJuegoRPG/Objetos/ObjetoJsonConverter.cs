using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                // Clonar opciones para insertar converter leniente de strings (si no está)
                var localOpts = new JsonSerializerOptions(options);
                bool tieneLenient = false;
                foreach (var c in localOpts.Converters)
                {
                    if (c is MiJuegoRPG.Motor.Servicios.LenientStringConverter)
                    {
                        tieneLenient = true;
                        break;
                    }
                }

                if (!tieneLenient)
                    localOpts.Converters.Insert(0, new MiJuegoRPG.Motor.Servicios.LenientStringConverter());

                Objeto? obj = tipo switch
                {
                    "Pocion" => JsonSerializer.Deserialize<Pocion>(root.GetRawText(), localOpts),
                    "Arma" => JsonSerializer.Deserialize<Arma>(root.GetRawText(), localOpts),
                    "Material" => JsonSerializer.Deserialize<Material>(root.GetRawText(), localOpts),
                    "Casco" => JsonSerializer.Deserialize<Casco>(root.GetRawText(), localOpts),
                    "Armadura" => JsonSerializer.Deserialize<Armadura>(root.GetRawText(), localOpts),
                    "Botas" => JsonSerializer.Deserialize<Botas>(root.GetRawText(), localOpts),
                    "Cinturon" => JsonSerializer.Deserialize<Cinturon>(root.GetRawText(), localOpts),
                    "Collar" => JsonSerializer.Deserialize<Collar>(root.GetRawText(), localOpts),
                    "Pantalon" => JsonSerializer.Deserialize<Pantalon>(root.GetRawText(), localOpts),
                    "Accesorio" => JsonSerializer.Deserialize<Accesorio>(root.GetRawText(), localOpts),
                    _ => throw new JsonException($"Tipo de objeto desconocido: {tipo}")
                };
                if (obj != null)
                {
                    // Normalizar rareza tolerando variantes legacy (case-insensitive) y valores nulos
                    // Intentar leer el valor bruto 'Rareza' para detectar si viene como número (enum legacy)
                    if (root.TryGetProperty("Rareza", out var rarezaProp) && rarezaProp.ValueKind != JsonValueKind.String)
                    {
                        try
                        {
                            // Interpretar entero como índice del enum Rareza legacy
                            if (rarezaProp.ValueKind == JsonValueKind.Number && rarezaProp.TryGetInt32(out int idx))
                            {
                                // Mapeo básico según orden legacy (Rota=0, Pobre=1, Normal=2, Superior=3, Rara=4, Legendaria=5, Ornamentada=6)
                                obj.Rareza = idx switch
                                {
                                    0 => "Rota",
                                    1 => "Pobre",
                                    2 => "Comun",      // Normal -> Comun
                                    3 => "Superior",
                                    4 => "Rara",
                                    5 => "Legendaria",
                                    6 => "Ornamentada",
                                    _ => "Comun"
                                };
                            }
                        }
                        catch { obj.Rareza = "Comun"; }
                    }
                    if (string.IsNullOrWhiteSpace(obj.Rareza))
                        obj.Rareza = "Comun"; // fallback estándar
                    var r = obj.Rareza.Trim();
                    // Mapear variantes conocidas
                    if (r.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                        r = "Comun";
                    if (r.Equals("PocoComun", StringComparison.OrdinalIgnoreCase) || r.Equals("Poco Comun", StringComparison.OrdinalIgnoreCase))
                        r = "Superior";
                    if (r.Equals("Raro", StringComparison.OrdinalIgnoreCase))
                        r = "Rara";
                    if (r.Equals("Epico", StringComparison.OrdinalIgnoreCase))
                        r = "Epica";
                    if (r.Equals("Legendario", StringComparison.OrdinalIgnoreCase))
                        r = "Legendaria";
                    // Mapear strings numéricas (producidas por converter leniente)
                    if (r is "0" or "1" or "2" or "3" or "4" or "5" or "6")
                    {
                        r = r switch
                        {
                            "0" => "Rota",
                            "1" => "Pobre",
                            "2" => "Comun",
                            "3" => "Superior",
                            "4" => "Rara",
                            "5" => "Legendaria",
                            "6" => "Ornamentada",
                            _ => "Comun"
                        };
                    }
                    obj.Rareza = r;
                }
                return obj;
            }
        }

        public override void Write(Utf8JsonWriter writer, Objeto value, JsonSerializerOptions options)
        {
            var tipo = value.GetType().Name;
            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            using (var doc = JsonDocument.Parse(json))
            {
                writer.WriteStartObject();
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    prop.WriteTo(writer);
                }
                writer.WriteString("TipoObjeto", tipo);
                writer.WriteEndObject();
            }
        }
    }
}
