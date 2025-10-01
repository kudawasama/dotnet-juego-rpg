using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de pantalones.
    /// </summary>
    public class PantalonesRepository
    {
        private readonly Dictionary<string, PantalonData> _cache = new(StringComparer.OrdinalIgnoreCase);
        private bool _loaded;

        private void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try { CargarBase(); AplicarOverlay(); }
            catch (Exception ex) { Logger.Warn($"[PantalonesRepository] Error carga inicial: {ex.Message}"); }
        }

        private void CargarBase()
        {
            var dir = PathProvider.PantalonesDir();
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            if (el.ValueKind != JsonValueKind.Object) continue;
                            var data = Parse(el, file);
                            if (data != null) AgregarBaseSiNoExiste(data);
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        var data = Parse(doc.RootElement, file);
                        if (data != null) AgregarBaseSiNoExiste(data);
                    }
                }
                catch (Exception exFile)
                {
                    Logger.Warn($"[PantalonesRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos = { PathProvider.PjDatosPath("pantalones_overlay.json"), PathProvider.PjDatosPath("pantalones.json") };
            foreach (var ruta in candidatos)
            {
                if (!File.Exists(ruta)) continue;
                try
                {
                    var json = File.ReadAllText(ruta);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    var lista = JsonSerializer.Deserialize<List<PantalonData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (lista == null) continue;
                    foreach (var p in lista)
                    {
                        if (string.IsNullOrWhiteSpace(p.Nombre)) continue;
                        p.Rareza = RarezaNormalizer.Normalizar(p.Rareza);
                        _cache[p.Nombre] = p;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[PantalonesRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static PantalonData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre)) return null;
                var data = new PantalonData
                {
                    Nombre = nombre.Trim(),
                    TipoObjeto = LeerString(obj, "tipoObjeto") ?? LeerString(obj, "TipoObjeto") ?? "Pantalon",
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza") ?? "Comun"),
                    SetId = LeerString(obj, "setId") ?? LeerString(obj, "SetId")
                };
                if (TryGetInt(obj, out int def, "defensa", "Defensa")) data.Defensa = def;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel")) data.Nivel = nivel;
                if (TryGetInt(obj, out int pfix, "perfeccion", "Perfeccion")) data.Perfeccion = pfix;
                if (TryGetInt(obj, out int nmin, "nivelmin", "NivelMin")) data.NivelMin = nmin;
                if (TryGetInt(obj, out int nmax, "nivelmax", "NivelMax")) data.NivelMax = nmax;
                if (TryGetInt(obj, out int dmin, "defensamin", "DefensaMin")) data.DefensaMin = dmin;
                if (TryGetInt(obj, out int dmax, "defensamax", "DefensaMax")) data.DefensaMax = dmax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin")) data.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax")) data.PerfeccionMax = pmax;
                var csv = LeerString(obj, "rarezasPermitidasCsv") ?? LeerString(obj, "RarezasPermitidasCsv");
                if (!string.IsNullOrWhiteSpace(csv)) data.RarezasPermitidasCsv = csv;
                return data;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[PantalonesRepository] Objeto inválido en '{file}': {ex.Message}");
                return null;
            }
        }

        private static string? LeerString(JsonElement obj, string prop)
        {
            if (obj.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String) return v.GetString();
            return null;
        }

        private static bool TryGetInt(JsonElement obj, out int value, params string[] props)
        {
            foreach (var p in props)
            {
                if (obj.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) { value = i; return true; }
            }
            value = 0; return false;
        }

        private void AgregarBaseSiNoExiste(PantalonData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre)) return;
            if (_cache.ContainsKey(data.Nombre)) return;
            _cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<PantalonData> Todas()
        {
            EnsureLoaded();
            return _cache.Values as IReadOnlyCollection<PantalonData> ?? new List<PantalonData>(_cache.Values);
        }

        public bool TryGet(string nombre, out PantalonData? data)
        {
            EnsureLoaded();
            return _cache.TryGetValue(nombre, out data);
        }

        public void Invalidate()
        {
            _cache.Clear();
            _loaded = false;
        }
    }
}
