using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de cascos.
    /// Base: DatosJuego/Equipo/cascos/** (archivos objeto o lista).
    /// Overlay: PjDatos/cascos_overlay.json o PjDatos/cascos.json reemplaza por Nombre (case-insensitive).
    /// Rareza normalizada; errores tolerados (warn) sin abortar.
    /// </summary>
    public class CascosRepository
    {
        private readonly Dictionary<string, CascoData> cache = new(StringComparer.OrdinalIgnoreCase);
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
            catch (Exception ex)
            {
                Logger.Warn($"[CascosRepository] Error carga inicial: {ex.Message}");
            }
        }

        private void CargarBase()
        {
            var dir = PathProvider.CascosDir();
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
                    Logger.Warn($"[CascosRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos =
            {
                PathProvider.PjDatosPath("cascos_overlay.json"),
                PathProvider.PjDatosPath("cascos.json")
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
                    var lista = JsonSerializer.Deserialize<List<CascoData>>(json, opts);
                    if (lista == null)
                        continue;
                    foreach (var c in lista)
                    {
                        if (string.IsNullOrWhiteSpace(c.Nombre))
                            continue;
                        c.Rareza = RarezaNormalizer.Normalizar(c.Rareza);
                        cache[c.Nombre] = c; // overlay reemplaza
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CascosRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static CascoData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                var cd = new CascoData
                {
                    Nombre = nombre.Trim(),
                    TipoObjeto = LeerString(obj, "tipoObjeto") ?? LeerString(obj, "TipoObjeto") ?? "Casco",
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza") ?? "Comun"),
                    SetId = LeerString(obj, "setId") ?? LeerString(obj, "SetId")
                };
                if (TryGetInt(obj, out int def, "defensa", "Defensa"))
                    cd.Defensa = def;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel"))
                    cd.Nivel = nivel;
                if (TryGetInt(obj, out int pfix, "perfeccion", "Perfeccion"))
                    cd.Perfeccion = pfix;
                if (TryGetInt(obj, out int nmin, "nivelmin", "NivelMin"))
                    cd.NivelMin = nmin;
                if (TryGetInt(obj, out int nmax, "nivelmax", "NivelMax"))
                    cd.NivelMax = nmax;
                if (TryGetInt(obj, out int dmin, "defensamin", "DefensaMin"))
                    cd.DefensaMin = dmin;
                if (TryGetInt(obj, out int dmax, "defensamax", "DefensaMax"))
                    cd.DefensaMax = dmax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin"))
                    cd.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax"))
                    cd.PerfeccionMax = pmax;
                var csv = LeerString(obj, "rarezasPermitidasCsv") ?? LeerString(obj, "RarezasPermitidasCsv");
                if (!string.IsNullOrWhiteSpace(csv))
                    cd.RarezasPermitidasCsv = csv;
                return cd;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[CascosRepository] Objeto inválido en '{file}': {ex.Message}");
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

        private void AgregarBaseSiNoExiste(CascoData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre))
                return;
            if (cache.ContainsKey(data.Nombre))
                return;
            cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<CascoData> Todas()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<CascoData> ?? new List<CascoData>(cache.Values);
        }

        public bool TryGet(string nombre, out CascoData? data)
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
