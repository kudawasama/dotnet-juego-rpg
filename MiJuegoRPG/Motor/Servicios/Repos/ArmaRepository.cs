using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de armas.
    /// Base: DatosJuego/Equipo/armas/** (archivos individuales u objetos/listas)
    /// Overlay: PjDatos/armas.json (lista) o PjDatos/armas_overlay.json
    /// Reglas: primer archivo base fija arma por Nombre (case-insensitive); overlay reemplaza.
    /// Campos tolerantes; rareza normalizada. Errores por archivo = warn y continuar.
    /// </summary>
    public class ArmaRepository
    {
        private readonly Dictionary<string, ArmaData> cache = new(StringComparer.OrdinalIgnoreCase);
        private bool loaded;

        public ArmaRepository()
        {
        }

        private void EnsureLoaded()
        {
            if (loaded)
                return;
            loaded = true;
            try
            {
                CargarBase();
                AplicarOverlay();
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ArmaRepository] Error carga inicial: {ex.Message}");
            }
        }

        private void CargarBase()
        {
            var dir = PathProvider.ArmasDir();
            if (!Directory.Exists(dir))
                return;
            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json))
                        continue;
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elem in doc.RootElement.EnumerateArray())
                        {
                            if (elem.ValueKind != JsonValueKind.Object)
                                continue;
                            var data = Parse(elem, file);
                            if (data != null)
                                AgregarBaseSiNoExiste(data);
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        var data = Parse(doc.RootElement, file);
                        if (data != null)
                            AgregarBaseSiNoExiste(data);
                    }
                }
                catch (Exception exFile)
                {
                    Logger.Warn($"[ArmaRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos =
            {
                PathProvider.PjDatosPath("armas_overlay.json"),
                PathProvider.PjDatosPath("armas.json")
            };
            foreach (var ruta in candidatos)
            {
                if (!File.Exists(ruta))
                    continue;
                try
                {
                    var json = File.ReadAllText(ruta);
                    if (string.IsNullOrWhiteSpace(json))
                        continue;
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var lista = JsonSerializer.Deserialize<List<ArmaData>>(json, opts);
                    if (lista == null)
                        continue;
                    foreach (var a in lista)
                    {
                        if (string.IsNullOrWhiteSpace(a.Nombre))
                            continue;
                        a.Rareza = RarezaNormalizer.Normalizar(a.Rareza);
                        cache[a.Nombre] = a; // overlay reemplaza
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[ArmaRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static ArmaData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                var ad = new ArmaData
                {
                    Nombre = nombre.Trim(),
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza")),
                    Tipo = LeerString(obj, "tipo") ?? LeerString(obj, "Tipo") ?? "Arma",
                };
                // Campos opcionales numéricos
                if (TryGetInt(obj, out int dmg, "daño", "dano", "Daño", "Dano"))
                    ad.Daño = dmg;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel", "nivelRequerido", "NivelRequerido"))
                    ad.NivelRequerido = nivel;
                if (TryGetInt(obj, out int perf, "perfeccion", "Perfeccion"))
                    ad.Perfeccion = perf;
                if (TryGetInt(obj, out int dmin, "dañomin", "DanoMin", "DañoMin"))
                    ad.DañoMin = dmin;
                if (TryGetInt(obj, out int dmax, "dañomax", "DanoMax", "DañoMax"))
                    ad.DañoMax = dmax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin"))
                    ad.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax"))
                    ad.PerfeccionMax = pmax;
                return ad;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ArmaRepository] Objeto inválido en '{file}': {ex.Message}");
                return null;
            }
        }

        private static string? LeerString(JsonElement obj, string prop)
        {
            if (obj.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
            return null;
        }

        private static bool TryGetInt(JsonElement obj, out int value, params string[] props)
        {
            foreach (var p in props)
            {
                if (obj.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i))
                {
                    value = i;
                    return true;
                }
            }
            value = 0;
            return false;
        }

        private void AgregarBaseSiNoExiste(ArmaData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre))
                return;
            if (cache.ContainsKey(data.Nombre))
                return; // primer archivo base gana
            cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<ArmaData> Todas()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<ArmaData> ?? new List<ArmaData>(cache.Values);
        }

        public bool TryGet(string nombre, out ArmaData? data)
        {
            EnsureLoaded();
            return cache.TryGetValue(nombre, out data);
        }

        public void Invalidate()
        {
            cache.Clear();
            loaded = false;
        }
    }
}
