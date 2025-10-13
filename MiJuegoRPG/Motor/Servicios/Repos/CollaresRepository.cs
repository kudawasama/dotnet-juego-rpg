using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de collares (bonificación defensa/energía).
    /// </summary>
    public class CollaresRepository
    {
        private readonly Dictionary<string, CollarData> cache = new(StringComparer.OrdinalIgnoreCase);
        private bool loaded;

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
            catch (Exception ex) { Logger.Warn($"[CollaresRepository] Error carga inicial: {ex.Message}"); }
        }

        private void CargarBase()
        {
            var dir = PathProvider.CollaresDir();
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
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            if (el.ValueKind != JsonValueKind.Object)
                                continue;
                            var data = Parse(el, file);
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
                    Logger.Warn($"[CollaresRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos = { PathProvider.PjDatosPath("collares_overlay.json"), PathProvider.PjDatosPath("collares.json") };
            foreach (var ruta in candidatos)
            {
                if (!File.Exists(ruta))
                    continue;
                try
                {
                    var json = File.ReadAllText(ruta);
                    if (string.IsNullOrWhiteSpace(json))
                        continue;
                    var lista = JsonSerializer.Deserialize<List<CollarData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (lista == null)
                        continue;
                    foreach (var c in lista)
                    {
                        if (string.IsNullOrWhiteSpace(c.Nombre))
                            continue;
                        c.Rareza = RarezaNormalizer.Normalizar(c.Rareza);
                        cache[c.Nombre] = c;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CollaresRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static CollarData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                var data = new CollarData
                {
                    Nombre = nombre.Trim(),
                    TipoObjeto = LeerString(obj, "tipoObjeto") ?? LeerString(obj, "TipoObjeto") ?? "Collar",
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza") ?? "Comun"),
                    SetId = LeerString(obj, "setId") ?? LeerString(obj, "SetId")
                };
                if (TryGetInt(obj, out int def, "bonificaciondefensa", "BonificacionDefensa"))
                    data.BonificacionDefensa = def;
                if (TryGetInt(obj, out int ener, "bonificacionenergia", "BonificacionEnergia"))
                    data.BonificacionEnergia = ener;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel"))
                    data.Nivel = nivel;
                if (TryGetInt(obj, out int pfix, "perfeccion", "Perfeccion"))
                    data.Perfeccion = pfix;
                if (TryGetInt(obj, out int nmin, "nivelmin", "NivelMin"))
                    data.NivelMin = nmin;
                if (TryGetInt(obj, out int nmax, "nivelmax", "NivelMax"))
                    data.NivelMax = nmax;
                if (TryGetInt(obj, out int dmin, "bonificaciondefensamin", "BonificacionDefensaMin"))
                    data.BonificacionDefensaMin = dmin;
                if (TryGetInt(obj, out int dmax, "bonificaciondefensamax", "BonificacionDefensaMax"))
                    data.BonificacionDefensaMax = dmax;
                if (TryGetInt(obj, out int emin, "bonificacionenergiamin", "BonificacionEnergiaMin"))
                    data.BonificacionEnergiaMin = emin;
                if (TryGetInt(obj, out int emax, "bonificacionenergiamax", "BonificacionEnergiaMax"))
                    data.BonificacionEnergiaMax = emax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin"))
                    data.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax"))
                    data.PerfeccionMax = pmax;
                var csv = LeerString(obj, "rarezasPermitidasCsv") ?? LeerString(obj, "RarezasPermitidasCsv");
                if (!string.IsNullOrWhiteSpace(csv))
                    data.RarezasPermitidasCsv = csv;
                return data;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[CollaresRepository] Objeto inválido en '{file}': {ex.Message}");
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

        private void AgregarBaseSiNoExiste(CollarData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre))
                return;
            if (cache.ContainsKey(data.Nombre))
                return;
            cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<CollarData> Todas()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<CollarData> ?? new List<CollarData>(cache.Values);
        }

        public bool TryGet(string nombre, out CollarData? data)
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
