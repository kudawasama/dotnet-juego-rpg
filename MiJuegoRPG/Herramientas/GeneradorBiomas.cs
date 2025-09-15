using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiJuegoRPG.Herramientas
{
    /// <summary>
    /// Asigna biomas por bandas en función de la distancia a los bordes del mapa (definido por mapa.txt).
    /// Regla por defecto:
    /// - Borde exterior (distancia <= anchoOceanoLejano-1): "Oceano Lejano"
    /// - Siguiente banda (distancia <= anchoOceanoLejano + anchoOceano - 1): "Oceano"
    /// - Interior: conserva bioma existente; si no hay, usa "Campo".
    /// Preserva sectores de tipo "Ciudad" para no sobreescribirlos.
    /// </summary>
    public static class GeneradorBiomas
    {
        private static readonly Regex IdRegex = new Regex("^\\d+_\\d+$", RegexOptions.Compiled);

        public static void AsignarPorBandas(string rutaMapaTxt, string rutaSectoresBase, int anchoOceanoLejano = 1, int anchoOceano = 1)
        {
            if (!File.Exists(rutaMapaTxt))
            {
                Console.WriteLine($"[GeneradorBiomas] No existe mapa.txt en: {rutaMapaTxt}");
                return;
            }

            var lineas = File.ReadAllLines(rutaMapaTxt)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .ToList();
            var grid = new List<List<string>>();
            foreach (var linea in lineas)
            {
                var celdas = linea.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(c => c.Trim())
                                  .ToList();
                if (celdas.Count > 0) grid.Add(celdas);
            }

            int filas = grid.Count;
            int cols = grid.Count > 0 ? grid[0].Count : 0;
            if (filas == 0 || cols == 0)
            {
                Console.WriteLine("[GeneradorBiomas] Grilla vacía o inválida en mapa.txt");
                return;
            }

            // Calcular distancia a borde para cada id
            var distBorde = new Dictionary<string, int>();
            for (int r = 0; r < filas; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var id = grid[r][c];
                    if (!IdRegex.IsMatch(id)) continue;
                    int d = Math.Min(Math.Min(r, c), Math.Min(filas - 1 - r, cols - 1 - c));
                    distBorde[id] = d;
                }
            }

            var archivos = Directory.GetFiles(rutaSectoresBase, "*.json", SearchOption.AllDirectories);
            int modificados = 0;
            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true };

            foreach (var file in archivos)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var raw = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options) ?? new Dictionary<string, object>();
                    string id = Path.GetFileNameWithoutExtension(file);
                    if (raw.TryGetValue("Id", out var idVal) && idVal is JsonElement idEl && idEl.ValueKind == JsonValueKind.String)
                    {
                        id = idEl.GetString() ?? id;
                    }
                    if (!distBorde.ContainsKey(id)) continue; // fuera de grilla o inválido

                    // Si es ciudad, no tocar bioma
                    string tipo = ExtraerString(raw, "tipo");
                    if (!string.IsNullOrEmpty(tipo) && tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string biomaActual = ExtraerString(raw, "bioma");
                    int d = distBorde[id];
                    string? nuevoBioma = null;
                    if (d <= Math.Max(0, anchoOceanoLejano - 1))
                        nuevoBioma = "Oceano Lejano";
                    else if (d <= Math.Max(0, anchoOceanoLejano + anchoOceano - 1))
                        nuevoBioma = "Oceano";
                    else if (string.IsNullOrWhiteSpace(biomaActual))
                        nuevoBioma = "Campo"; // interior sin bioma definido

                    // Asegurar tipo "Ruta" en no-ciudades si falta o viene como "Ciudad" por defecto
                    bool cambioTipo = false;
                    if (string.IsNullOrEmpty(tipo) || tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                    {
                        raw["tipo"] = "Ruta";
                        cambioTipo = true;
                    }

                    if (!string.IsNullOrWhiteSpace(nuevoBioma) && !string.Equals(biomaActual, nuevoBioma, StringComparison.OrdinalIgnoreCase))
                    {
                        raw["bioma"] = nuevoBioma;
                        var nuevo = JsonSerializer.Serialize(raw, options);
                        File.WriteAllText(file, nuevo);
                        modificados++;
                    }
                    else if (cambioTipo)
                    {
                        var nuevo = JsonSerializer.Serialize(raw, options);
                        File.WriteAllText(file, nuevo);
                        modificados++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GeneradorBiomas] Error en {file}: {ex.Message}");
                }
            }

            Console.WriteLine($"[GeneradorBiomas] Asignación por bandas completada. Archivos modificados: {modificados}");
        }

        private static string ExtraerString(Dictionary<string, object> raw, string key)
        {
            if (raw.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.String)
                return el.GetString() ?? string.Empty;
            return string.Empty;
        }
    }
}
