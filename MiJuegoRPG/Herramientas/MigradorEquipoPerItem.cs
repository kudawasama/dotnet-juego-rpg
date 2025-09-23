using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Herramientas
{
    /// <summary>
    /// Divide los JSON agregados en DatosJuego/Equipo/*.json en archivos por ítem dentro de
    /// subcarpetas por tipo (armas, armaduras, accesorios, botas, cinturones, collares, pantalones, cascos).
    /// No elimina los agregados; crea nuevos archivos por ítem. Soporta "write" (aplica) o dry-run.
    /// </summary>
    public static class MigradorEquipoPerItem
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static void Migrar(bool write)
        {
            string baseEquipo = PathProvider.CombineData("Equipo");
            Directory.CreateDirectory(baseEquipo);

            int creados = 0, existentes = 0, errores = 0;

            // Procesar por cada tipo soportado
            creados += MigrarListaArmas(baseEquipo, new[] { "armas.json", "Armas.json" }, "armas");
            creados += MigrarLista<ArmaduraData>(baseEquipo, new[] { "Armaduras.json", "armaduras.json" }, "armaduras", i => i.Nombre);
            creados += MigrarLista<AccesorioData>(baseEquipo, new[] { "Accesorios.json", "accesorios.json" }, "accesorios", i => i.Nombre);
            creados += MigrarLista<BotasData>(baseEquipo, new[] { "Botas.json", "botas.json" }, "botas", i => i.Nombre);
            creados += MigrarLista<CascoData>(baseEquipo, new[] { "Cascos.json", "cascos.json" }, "cascos", i => i.Nombre);
            creados += MigrarLista<CinturonData>(baseEquipo, new[] { "Cinturones.json", "cinturones.json" }, "cinturones", i => i.Nombre);
            creados += MigrarLista<CollarData>(baseEquipo, new[] { "Collares.json", "collares.json" }, "collares", i => i.Nombre);
            creados += MigrarLista<PantalonData>(baseEquipo, new[] { "Pantalones.json", "pantalones.json" }, "pantalones", i => i.Nombre);

            Console.WriteLine($"[MigradorEquipo] Archivos por ítem a crear (dry-run={(!write)}) ≈ {creados}");

            if (!write)
            {
                Console.WriteLine("[MigradorEquipo] Ejecuta con --migrar-equipo=write para aplicar.");
                return;
            }

            // Ejecutar escritura real
            try
            {
                // Re-ejecutamos con write=true para crear realmente
                CrearDesdeAgregadosArmas(baseEquipo, new[] { "armas.json", "Armas.json" }, "armas", ref existentes, ref errores);
                CrearDesdeAgregados<ArmaduraData>(baseEquipo, new[] { "Armaduras.json", "armaduras.json" }, "armaduras", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<AccesorioData>(baseEquipo, new[] { "Accesorios.json", "accesorios.json" }, "accesorios", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<BotasData>(baseEquipo, new[] { "Botas.json", "botas.json" }, "botas", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<CascoData>(baseEquipo, new[] { "Cascos.json", "cascos.json" }, "cascos", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<CinturonData>(baseEquipo, new[] { "Cinturones.json", "cinturones.json" }, "cinturones", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<CollarData>(baseEquipo, new[] { "Collares.json", "collares.json" }, "collares", i => i.Nombre, ref existentes, ref errores);
                CrearDesdeAgregados<PantalonData>(baseEquipo, new[] { "Pantalones.json", "pantalones.json" }, "pantalones", i => i.Nombre, ref existentes, ref errores);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MigradorEquipo][ERROR] {ex.Message}");
            }

            Console.WriteLine($"[MigradorEquipo] Hecho. Existentes: {existentes}, Errores: {errores}");
        }

        // Dry-run: contar cuántos se crearían
        private static int MigrarLista<T>(string baseEquipo, string[] nombres, string carpeta, Func<T, string> nombreSel)
        {
            var ruta = nombres.Select(n => Path.Combine(baseEquipo, n)).FirstOrDefault(File.Exists);
            if (ruta == null) return 0;
            try
            {
                var json = File.ReadAllText(ruta);
                var lista = JsonSerializer.Deserialize<List<T>>(json);
                if (lista == null) return 0;
                return lista.Count;
            }
            catch { return 0; }
        }

        // Contar armas aun si el esquema no coincide con ArmaData
        private static int MigrarListaArmas(string baseEquipo, string[] nombres, string carpeta)
        {
            var ruta = nombres.Select(n => Path.Combine(baseEquipo, n)).FirstOrDefault(File.Exists);
            if (ruta == null) return 0;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    return doc.RootElement.GetArrayLength();
                return 0;
            }
            catch { return 0; }
        }

        private static void CrearDesdeAgregados<T>(string baseEquipo, string[] nombres, string carpeta, Func<T, string> nombreSel, ref int existentes, ref int errores)
        {
            var ruta = nombres.Select(n => Path.Combine(baseEquipo, n)).FirstOrDefault(File.Exists);
            if (ruta == null) return;
            List<T>? lista = null;
            try
            {
                var json = File.ReadAllText(ruta);
                lista = JsonSerializer.Deserialize<List<T>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MigradorEquipo][WARN] No se pudo leer '{ruta}': {ex.Message}");
                errores++;
                return;
            }
            if (lista == null || lista.Count == 0) return;

            var outDir = Path.Combine(baseEquipo, carpeta);
            Directory.CreateDirectory(outDir);

            foreach (var item in lista)
            {
                try
                {
                    string nombre = nombreSel(item);
                    if (string.IsNullOrWhiteSpace(nombre)) nombre = "sin_nombre";
                    var fileName = Slug(nombre) + ".json";
                    string path = Path.Combine(outDir, fileName);
                    path = AsegurarUnico(path);
                    var contenido = JsonSerializer.Serialize(item, JsonOpts);
                    File.WriteAllText(path, contenido, Encoding.UTF8);
                    Console.WriteLine($"[MigradorEquipo] + {path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MigradorEquipo][ERR] {ex.Message}");
                    errores++;
                }
            }
        }

        private static void CrearDesdeAgregadosArmas(string baseEquipo, string[] nombres, string carpeta, ref int existentes, ref int errores)
        {
            var ruta = nombres.Select(n => Path.Combine(baseEquipo, n)).FirstOrDefault(File.Exists);
            if (ruta == null) return;

            List<ArmaData>? lista = null;
            string raw = File.ReadAllText(ruta);
            try
            {
                // Intento directo
                lista = JsonSerializer.Deserialize<List<ArmaData>>(raw);
            }
            catch
            {
                lista = null;
            }
            if (lista == null)
            {
                // Parseo tolerante desde esquema histórico
                try { lista = ParseArmasDesdeJson(raw); }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MigradorEquipo][WARN] armas: parse tolerante falló: {ex.Message}");
                    errores++;
                    return;
                }
            }
            if (lista == null || lista.Count == 0) return;

            var outDir = Path.Combine(baseEquipo, carpeta);
            Directory.CreateDirectory(outDir);
            foreach (var item in lista)
            {
                try
                {
                    string nombre = item.Nombre ?? "sin_nombre";
                    var fileName = Slug(nombre) + ".json";
                    string path = Path.Combine(outDir, fileName);
                    path = AsegurarUnico(path);
                    var contenido = JsonSerializer.Serialize(item, JsonOpts);
                    File.WriteAllText(path, contenido, Encoding.UTF8);
                    Console.WriteLine($"[MigradorEquipo] + {path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MigradorEquipo][ERR] {ex.Message}");
                    errores++;
                }
            }
        }

        private static List<ArmaData> ParseArmasDesdeJson(string raw)
        {
            var res = new List<ArmaData>();
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return res;
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                try
                {
                    string nombre = el.GetPropertyOrDefault("Nombre", "sin_nombre");
                    int nivel = el.GetIntPropertyOrDefault("NivelRequerido", 1);
                    int danioFis = el.GetIntPropertyOrDefault("DañoFisico", 0);
                    int danioMag = el.GetIntPropertyOrDefault("DañoMagico", 0);
                    int danio = danioFis > 0 ? danioFis : (danioMag > 0 ? danioMag : 1);
                    int perfeccion = el.GetIntPropertyOrDefault("Perfeccion", 50);
                    string rarezaTxt = el.GetPropertyOrDefault("Rareza", "Normal");
                    rarezaTxt = rarezaTxt.Equals("Comun", StringComparison.OrdinalIgnoreCase) ? "Normal" : rarezaTxt;
                    rarezaTxt = rarezaTxt.Equals("PocoComun", StringComparison.OrdinalIgnoreCase) ? "Superior" : rarezaTxt;
                    string categoria = el.GetPropertyOrDefault("Categoria", el.GetPropertyOrDefault("TipoObjeto", "Arma"));
                    string tipo = InferirTipoArma(nombre, categoria);

                    var arma = new ArmaData
                    {
                        Nombre = nombre,
                        Daño = danio,
                        NivelRequerido = nivel,
                        Valor = 0,
                        Tipo = tipo,
                        Rareza = rarezaTxt,
                        Perfeccion = perfeccion
                    };
                    res.Add(arma);
                }
                catch
                {
                    // omitir item inválido
                }
            }
            return res;
        }

        private static string InferirTipoArma(string nombre, string categoria)
        {
            string n = (nombre ?? "").ToLowerInvariant();
            string c = (categoria ?? "Arma").ToLowerInvariant();
            if (c.Contains("escudo")) return "Escudo";
            if (n.Contains("arco")) return "Arco";
            if (n.Contains("báculo") || n.Contains("baculo") || n.Contains("vara")) return "Baston";
            if (n.Contains("daga")) return "Daga";
            if (n.Contains("espadón") || n.Contains("espadon") || n.Contains("martillo") || n.Contains("maza")) return "DosManos";
            return "UnaMano";
        }

        // Helpers para lectura tolerante de JsonElement
        private static string GetPropertyOrDefault(this JsonElement el, string name, string def)
        {
            if (el.ValueKind != JsonValueKind.Object) return def;
            if (!el.TryGetProperty(name, out var prop)) return def;
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString() ?? def,
                JsonValueKind.Number => prop.TryGetInt32(out var i) ? i.ToString() : def,
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => def
            };
        }

        private static int GetIntPropertyOrDefault(this JsonElement el, string name, int def)
        {
            if (el.ValueKind != JsonValueKind.Object) return def;
            if (!el.TryGetProperty(name, out var prop)) return def;
            if (prop.ValueKind == JsonValueKind.Number)
            {
                if (prop.TryGetInt32(out var i)) return i;
                try { return (int)Math.Round(prop.GetDouble()); } catch { return def; }
            }
            if (prop.ValueKind == JsonValueKind.String)
            {
                var s = prop.GetString();
                if (int.TryParse(s, out var i)) return i;
                if (double.TryParse(s, out var d)) return (int)Math.Round(d);
            }
            return def;
        }

        private static string AsegurarUnico(string path)
        {
            if (!File.Exists(path)) return path;
            string dir = Path.GetDirectoryName(path)!;
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int i = 2;
            while (true)
            {
                string p = Path.Combine(dir, $"{name}_{i}{ext}");
                if (!File.Exists(p)) return p;
                i++;
            }
        }

        private static string Slug(string input)
        {
            string s = input.ToLowerInvariant();
            s = s.Replace('ñ', 'n');
            var normalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            s = sb.ToString().Normalize(NormalizationForm.FormC);
            s = Regex.Replace(s, "[^a-z0-9]+", "_");
            s = Regex.Replace(s, "_+", "_").Trim('_');
            if (string.IsNullOrEmpty(s)) s = "item";
            return s;
        }
    }
}
