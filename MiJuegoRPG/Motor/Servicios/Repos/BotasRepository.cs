using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio jerárquico de botas.
    /// Base: DatosJuego/Equipo/botas/** (archivos objeto o lista).
    /// Overlay: PjDatos/botas_overlay.json o PjDatos/botas.json reemplaza por Nombre (case-insensitive).
    /// Rareza normalizada; errores tolerados (warn) sin abortar.
    /// </summary>
    public class BotasRepository
    {
        // @AgenteDatos: validar que primer archivo base gana y overlay reemplaza. Si se factoriza a genérico, mover esta regla a la clase base.
        private readonly Dictionary<string, BotasData> cache = new(StringComparer.OrdinalIgnoreCase);
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
                Logger.Warn($"[BotasRepository] Error carga inicial: {ex.Message}");
            }
        }

        private void CargarBase()
        {
            var dir = PathProvider.BotasDir();
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
                    Logger.Warn($"[BotasRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            string[] candidatos =
            {
                PathProvider.PjDatosPath("botas_overlay.json"),
                PathProvider.PjDatosPath("botas.json")
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
                    var lista = JsonSerializer.Deserialize<List<BotasData>>(json, opts);
                    if (lista == null)
                        continue;
                    foreach (var b in lista)
                    {
                        if (string.IsNullOrWhiteSpace(b.Nombre))
                            continue;
                        b.Rareza = RarezaNormalizer.Normalizar(b.Rareza);
                        cache[b.Nombre] = b; // overlay reemplaza
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[BotasRepository] Overlay '{ruta}' ignorado: {ex.Message}");
                }
            }
        }

        private static BotasData? Parse(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                var bd = new BotasData
                {
                    Nombre = nombre.Trim(),
                    TipoObjeto = LeerString(obj, "tipoObjeto") ?? LeerString(obj, "TipoObjeto") ?? "Botas",
                    Rareza = RarezaNormalizer.Normalizar(LeerString(obj, "rareza") ?? LeerString(obj, "Rareza") ?? "Comun"),
                    SetId = LeerString(obj, "setId") ?? LeerString(obj, "SetId")
                };
                if (TryGetInt(obj, out int def, "defensa", "Defensa"))
                    bd.Defensa = def;
                if (TryGetInt(obj, out int nivel, "nivel", "Nivel"))
                    bd.Nivel = nivel;
                if (TryGetInt(obj, out int pfix, "perfeccion", "Perfeccion"))
                    bd.Perfeccion = pfix;
                if (TryGetInt(obj, out int nmin, "nivelmin", "NivelMin"))
                    bd.NivelMin = nmin;
                if (TryGetInt(obj, out int nmax, "nivelmax", "NivelMax"))
                    bd.NivelMax = nmax;
                if (TryGetInt(obj, out int dmin, "defensamin", "DefensaMin"))
                    bd.DefensaMin = dmin;
                if (TryGetInt(obj, out int dmax, "defensamax", "DefensaMax"))
                    bd.DefensaMax = dmax;
                if (TryGetInt(obj, out int pmin, "perfeccionmin", "PerfeccionMin"))
                    bd.PerfeccionMin = pmin;
                if (TryGetInt(obj, out int pmax, "perfeccionmax", "PerfeccionMax"))
                    bd.PerfeccionMax = pmax;
                var csv = LeerString(obj, "rarezasPermitidasCsv") ?? LeerString(obj, "RarezasPermitidasCsv");
                if (!string.IsNullOrWhiteSpace(csv))
                    bd.RarezasPermitidasCsv = csv;
                return bd;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[BotasRepository] Objeto inválido en '{file}': {ex.Message}");
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

        private void AgregarBaseSiNoExiste(BotasData data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre))
                return;
            if (cache.ContainsKey(data.Nombre))
                return;
            cache[data.Nombre] = data;
        }

        public IReadOnlyCollection<BotasData> Todas()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<BotasData> ?? new List<BotasData>(cache.Values);
        }

        public bool TryGet(string nombre, out BotasData? data)
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
