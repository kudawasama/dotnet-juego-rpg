using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de armaduras.
    /// Base: DatosJuego/Equipo/armaduras/** (archivos individuales u objetos/listas).
    /// Overlay: PjDatos/armaduras_overlay.json o PjDatos/armaduras.json (lista) sobreescribe por Nombre.
    /// Reglas: primer archivo base fija (case-insensitive); overlay reemplaza.
    /// Campos tolerantes; rareza normalizada; errores por archivo = warn y continuar.
    /// </summary>
    public class ArmaduraRepository
    {
        private readonly Dictionary<string, ArmaduraData> cache = new(StringComparer.OrdinalIgnoreCase);
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
                Logger.Warn($"[ArmaduraRepository] Error carga inicial: {ex.Message}");
            }
        }

        private void CargarBase()
        {
            var dir = PathProvider.ArmadurasDir();
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
                    Logger.Warn($"[ArmaduraRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos =
            {
                PathProvider.PjDatosPath("armaduras_overlay.json"),
                PathProvider.PjDatosPath("armaduras.json")
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
                    var lista = JsonSerializer.Deserialize<List<ArmaduraData>>(json, opts);
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
                    Logger.Warn($"[ArmaduraRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static ArmaduraData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                var ad = new ArmaduraData
                {
                    Nombre = nombre.Trim(),
                    TipoObjeto = LeerString(obj, "tipoObjeto") ?? LeerString(obj, "TipoObjeto") ?? "Armadura",
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza") ?? "Comun"),
                    SetId = LeerString(obj, "setId") ?? LeerString(obj, "SetId")
                };
                if (TryGetInt(obj, out int def, "defensa", "Defensa"))
                    ad.Defensa = def;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel"))
                    ad.Nivel = nivel;
                if (TryGetInt(obj, out int pfix, "perfeccion", "Perfeccion"))
                    ad.Perfeccion = pfix;
                if (TryGetInt(obj, out int nmin, "nivelmin", "NivelMin"))
                    ad.NivelMin = nmin;
                if (TryGetInt(obj, out int nmax, "nivelmax", "NivelMax"))
                    ad.NivelMax = nmax;
                if (TryGetInt(obj, out int dmin, "defensamin", "DefensaMin"))
                    ad.DefensaMin = dmin;
                if (TryGetInt(obj, out int dmax, "defensamax", "DefensaMax"))
                    ad.DefensaMax = dmax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin"))
                    ad.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax"))
                    ad.PerfeccionMax = pmax;
                // Rarezas CSV
                var csv = LeerString(obj, "rarazasPermitidasCsv") ?? LeerString(obj, "RarezasPermitidasCsv");
                if (!string.IsNullOrWhiteSpace(csv))
                    ad.RarezasPermitidasCsv = csv;
                return ad;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ArmaduraRepository] Objeto inválido en '{file}': {ex.Message}");
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

        private void AgregarBaseSiNoExiste(ArmaduraData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre))
                return;
            if (cache.ContainsKey(data.Nombre))
                return; // primer archivo base gana
            cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<ArmaduraData> Todas()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<ArmaduraData> ?? new List<ArmaduraData>(cache.Values);
        }

        public bool TryGet(string nombre, out ArmaduraData? data)
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
