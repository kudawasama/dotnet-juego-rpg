using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Herramientas
{
    /// <summary>
    /// Recorre los sectores y, si no tienen nodos propios, escribe una lista inicial basada en el bioma.
    /// - Preserva sectores que ya tienen "nodosRecoleccion" no vacío
    /// - Limita el número de nodos escritos por sector (max)
    /// - Copia nombre/tipo/requiere/materiales (como objetos {Nombre,Cantidad})/cooldown base desde plantillas del bioma
    /// - No escribe campos runtime (UltimoUso)
    /// </summary>
    public static class HidratadorNodos
    {
        public static void HidratarDesdeBiomas(string rutaSectoresBase, int maxPorSector = 5)
        {
            var archivos = Directory.GetFiles(rutaSectoresBase, "*.json", SearchOption.AllDirectories);
            int modificados = 0;
            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true, PropertyNameCaseInsensitive = true };

            foreach (var file in archivos)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    // Trabajamos sobre un diccionario para preservar campos desconocidos
                    var raw = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                    if (raw == null) continue;

                    // Si es ciudad, no debe tener nodos de recolección: limpiar si existieran y continuar
                    string tipo = ExtraerString(raw, "tipo");
                    // Ignoramos esCentroCiudad para la lógica de hidratación
                    if (!string.IsNullOrEmpty(tipo) && tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                    {
                        if (raw.ContainsKey("nodosRecoleccion"))
                        {
                            // Establecer a lista vacía para deshabilitar recolección en ciudades
                            raw["nodosRecoleccion"] = new object[] { };
                            var nuevoCiudad = JsonSerializer.Serialize(raw, options);
                            File.WriteAllText(file, nuevoCiudad);
                            modificados++;
                        }
                        continue; // no generar en ciudades
                    }

                    // Si ya hay nodos no tocamos (solo para no-ciudad)
                    if (raw.TryGetValue("nodosRecoleccion", out var nodosVal) && nodosVal is JsonElement el && el.ValueKind == JsonValueKind.Array && el.GetArrayLength() > 0)
                        continue;

                    // Determinar bioma
                    string bioma = ExtraerString(raw, "bioma");
                    if (string.IsNullOrWhiteSpace(bioma)) continue; // sin bioma, no generamos
                    if (!TablaBiomas.Biomas.TryGetValue(bioma, out var b)) continue; // bioma no definido

                    // Generar muestra: comunes + posibilidad de 1 raro
                    var lista = new List<NodoRecoleccion>();
                    if (b.NodosComunes != null) lista.AddRange(b.NodosComunes);
                    if (b.NodosRaros != null && b.NodosRaros.Count > 0)
                    {
                        // Añadir al menos un raro de forma opcional (20%) para darle sabor
                        var rnd = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                        if (rnd.NextDouble() < 0.2)
                            lista.Add(b.NodosRaros[rnd.Next(0, b.NodosRaros.Count)]);
                    }
                    if (lista.Count == 0) continue;

                    // Limitar y proyectar a una forma serializable simple
                    var proyectados = lista
                        .Take(Math.Max(1, Math.Min(maxPorSector, lista.Count)))
                        .Select(n => new
                        {
                            Nombre = n.Nombre,
                            Tipo = n.Tipo,
                            Requiere = n.Requiere,
                            Materiales = n.Materiales,
                            Cooldown = n.Cooldown > 0 ? n.Cooldown : (n.CooldownBase ?? 0),
                            Rareza = n.Rareza,
                            ProduccionMin = n.ProduccionMin,
                            ProduccionMax = n.ProduccionMax
                        })
                        .ToList();

                    raw["nodosRecoleccion"] = proyectados;
                    var nuevo = JsonSerializer.Serialize(raw, options);
                    File.WriteAllText(file, nuevo);
                    modificados++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HidratadorNodos] Error en {file}: {ex.Message}");
                }
            }

            Console.WriteLine($"[HidratadorNodos] Hidratación completada. Archivos modificados: {modificados}");
        }

        private static string ExtraerString(Dictionary<string, object> raw, string key)
        {
            if (raw.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.String)
                return el.GetString() ?? string.Empty;
            return string.Empty;
        }
    }
}
