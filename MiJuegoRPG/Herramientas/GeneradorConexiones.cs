using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiJuegoRPG.Herramientas
{
    /// <summary>
    /// Genera conexiones cardinales (N/E/S/O) entre sectores en base a la grilla de `mapa.txt`.
    /// - Lee la grilla (55x55 esperado) donde cada celda contiene el Id de sector (p.ej. "8_23").
    /// - Si dos celdas son adyacentes (arriba/abajo/izquierda/derecha), crea conexión entre sus IDs.
    /// - Asegura bidireccionalidad y elimina duplicados.
    /// - Mantiene conexiones existentes, solo agrega las faltantes por adyacencia.
    /// - No crea conexiones para celdas vacías o con códigos no válidos.
    /// </summary>
    public static class GeneradorConexiones
    {
        private static readonly Regex IdRegex = new Regex("^\\d+_\\d+$", RegexOptions.Compiled);

        public static void Generar(string rutaMapaTxt, string rutaSectoresBase)
        {
            if (!File.Exists(rutaMapaTxt))
            {
                Console.WriteLine($"[GeneradorConexiones] No existe mapa.txt en: {rutaMapaTxt}");
                return;
            }

            // Leer grilla: separar por líneas y espacios/tabs
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
                grid.Add(celdas);
            }

            int filas = grid.Count;
            int cols = grid.Count > 0 ? grid[0].Count : 0;
            if (filas == 0 || cols == 0)
            {
                Console.WriteLine("[GeneradorConexiones] Grilla vacía o inválida en mapa.txt");
                return;
            }

            // Mapa de id -> lista de conexiones por adyacencia
            var adyacencias = new Dictionary<string, HashSet<string>>();

            Func<int, int, string?> idEn = (r, c) =>
            {
                if (r < 0 || c < 0 || r >= filas || c >= cols) return null;
                var val = grid[r][c];
                if (string.IsNullOrWhiteSpace(val)) return null;
                // Admite guiones o marcadores no válidos; solo aceptar \d_\d
                if (!IdRegex.IsMatch(val)) return null;
                return val;
            };

            // Construir lista de adyacencias cardinales
            for (int r = 0; r < filas; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var id = idEn(r, c);
                    if (id == null) continue;
                    if (!adyacencias.ContainsKey(id)) adyacencias[id] = new HashSet<string>();

                    var vecinos = new List<string?>
                    {
                        idEn(r - 1, c), // Norte
                        idEn(r + 1, c), // Sur
                        idEn(r, c - 1), // Oeste
                        idEn(r, c + 1)  // Este
                    };
                    foreach (var v in vecinos)
                    {
                        if (v == null || v == id) continue;
                        adyacencias[id].Add(v);
                        if (!adyacencias.ContainsKey(v)) adyacencias[v] = new HashSet<string>();
                        adyacencias[v].Add(id);
                    }
                }
            }

            // Aplicar a archivos JSON de sectores
            var archivos = Directory.GetFiles(rutaSectoresBase, "*.json", SearchOption.AllDirectories);
            int modificados = 0;
            foreach (var file in archivos)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("Id", out var idProp))
                    {
                        // Intentar inferir por nombre de archivo
                        var nombre = Path.GetFileNameWithoutExtension(file);
                        if (!IdRegex.IsMatch(nombre)) continue;
                        ActualizarConexionesArchivo(file, nombre, adyacencias, json);
                        modificados++;
                        continue;
                    }
                    var id = idProp.GetString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(id) || !IdRegex.IsMatch(id)) continue;
                    ActualizarConexionesArchivo(file, id, adyacencias, json);
                    modificados++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GeneradorConexiones] Error en {file}: {ex.Message}");
                }
            }

            Console.WriteLine($"[GeneradorConexiones] Conexiones cardinales aplicadas. Archivos procesados: {archivos.Length}, modificados: {modificados}");
        }

        /// <summary>
        /// Asegura que todas las conexiones sean bidireccionales: si A->B existe, agrega B->A si falta.
        /// No elimina conexiones; solo agrega las ausentes para mantener simetría.
        /// </summary>
        public static void NormalizarBidireccionalidad(string rutaSectoresBase)
        {
            var archivos = Directory.GetFiles(rutaSectoresBase, "*.json", SearchOption.AllDirectories);
            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true };

            // Cargar a memoria id -> conexiones
            var porId = new Dictionary<string, (string file, HashSet<string> conns, Dictionary<string, object> raw)>();
            foreach (var file in archivos)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var raw = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options) ?? new Dictionary<string, object>();
                    string id = Path.GetFileNameWithoutExtension(file);
                    if (raw.TryGetValue("Id", out var idVal) && idVal is JsonElement idEl && idEl.ValueKind == JsonValueKind.String)
                    {
                        var idStr = idEl.GetString();
                        if (!string.IsNullOrWhiteSpace(idStr)) id = idStr!;
                    }
                    var hs = new HashSet<string>();
                    if (raw.TryGetValue("Conexiones", out var connVal) && connVal is JsonElement el && el.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in el.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                var s = item.GetString();
                                if (!string.IsNullOrWhiteSpace(s)) hs.Add(s!);
                            }
                        }
                    }
                    porId[id] = (file, hs, raw);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Normalizador] Error leyendo {file}: {ex.Message}");
                }
            }

            // Asegurar simetría
            int fixes = 0;
            foreach (var (id, tuple) in porId)
            {
                var (file, conns, raw) = tuple;
                foreach (var dest in conns.ToList())
                {
                    if (porId.TryGetValue(dest, out var t2))
                    {
                        if (!t2.conns.Contains(id))
                        {
                            t2.conns.Add(id);
                            fixes++;
                            porId[dest] = (t2.file, t2.conns, t2.raw);
                        }
                    }
                }
            }

            // Escribir cambios
            int escritos = 0;
            foreach (var kv in porId)
            {
                var id = kv.Key;
                var (file, conns, raw) = kv.Value;
                raw["Conexiones"] = conns.OrderBy(x => x).ToArray();
                var nuevo = JsonSerializer.Serialize(raw, options);
                File.WriteAllText(file, nuevo);
                escritos++;
            }
            Console.WriteLine($"[Normalizador] Bidireccionalidad asegurada. Fixes: {fixes}, Archivos actualizados: {escritos}");
        }

        private static void ActualizarConexionesArchivo(string file, string id, Dictionary<string, HashSet<string>> adyacencias, string jsonOriginal)
        {
            // Deserializar a un diccionario para preservar claves desconocidas
            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true };
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonOriginal, options) ?? new Dictionary<string, object>();

            // Obtener conexiones existentes como lista de strings si existe
            HashSet<string> existentes = new HashSet<string>();
            if (data.TryGetValue("Conexiones", out var connVal) && connVal is JsonElement el && el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) existentes.Add(s!);
                    }
                }
            }

            // Unir con adyacencias
            if (adyacencias.TryGetValue(id, out var adys))
            {
                foreach (var a in adys) existentes.Add(a);
            }

            data["Conexiones"] = existentes.OrderBy(x => x).ToArray();

            // Reescribir archivo
            var nuevo = JsonSerializer.Serialize(data, options);
            File.WriteAllText(file, nuevo);
        }
    }
}
