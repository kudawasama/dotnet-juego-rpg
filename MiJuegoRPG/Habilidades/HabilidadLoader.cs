using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Habilidades
{
    public static class HabilidadLoader
    {
        public static List<HabilidadData> CargarTodas(string carpeta)
        {
            var habilidades = new List<HabilidadData>();
            var archivos = Directory.GetFiles(carpeta, "*.json", SearchOption.AllDirectories);
            foreach (var archivo in archivos)
            {
                var json = File.ReadAllText(archivo);
                // Permitir que un archivo contenga lista o un único objeto (se normaliza a lista)
                List<HabilidadData>? lista = null;
                try
                {
                    lista = JsonSerializer.Deserialize<List<HabilidadData>>(json, JsonOptions());
                }
                catch
                {
                    try
                    {
                        var single = JsonSerializer.Deserialize<HabilidadData>(json, JsonOptions());
                        if (single != null) lista = new List<HabilidadData> { single };
                    }
                    catch { /* tolerante: archivo inválido, se ignora */ }
                }
                if (lista != null)
                {
                    // Normalizar IDs duplicadas y whitespace
                    foreach (var h in lista)
                    {
                        if (h == null) continue;
                        h.Id = h.Id?.Trim() ?? string.Empty;
                        h.Nombre = string.IsNullOrWhiteSpace(h.Nombre) ? h.Id : h.Nombre.Trim();
                        h.Tipo = h.Tipo?.Trim() ?? string.Empty;
                        h.Categoria = h.Categoria?.Trim() ?? string.Empty;
                        h.Descripcion = h.Descripcion?.Trim() ?? string.Empty;
                        h.AccionId = h.AccionId?.Trim();
                        // Limpieza de evoluciones nulas
                        if (h.Evoluciones == null) h.Evoluciones = new List<EvolucionData>();
                        foreach (var evo in h.Evoluciones)
                        {
                            if (evo == null) continue;
                            evo.Id = evo.Id?.Trim() ?? string.Empty;
                            evo.Nombre = string.IsNullOrWhiteSpace(evo.Nombre) ? evo.Id : evo.Nombre.Trim();
                            if (evo.Condiciones == null) evo.Condiciones = new List<CondicionData>();
                        }
                    }
                    habilidades.AddRange(lista.Where(x => x != null));
                }
            }
            return habilidades;
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }
    }

    public class HabilidadData
    {
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Activa | Pasiva
    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Dictionary<string, int>? AtributosNecesarios { get; set; } = new Dictionary<string, int>();
    public List<CondicionData> Condiciones { get; set; } = new List<CondicionData>();
    public string Beneficio { get; set; } = string.Empty;
    public List<string> Mejoras { get; set; } = new List<string>();
    public bool Oculta { get; set; } = false;
    public int? Exp { get; set; } = 0;
    public int? CostoMana { get; set; }
    public string? AccionId { get; set; }
    public List<EvolucionData> Evoluciones { get; set; } = new List<EvolucionData>();
    }

    public class CondicionData
    {
    public string Tipo { get; set; } = string.Empty; // Nivel | Mision | NvHabilidad | NvJugador | Ataque | etc.
    public string Accion { get; set; } = string.Empty;
    public int? Cantidad { get; set; } = null;
    public string Restriccion { get; set; } = string.Empty;
    }

    public class EvolucionData
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<CondicionData> Condiciones { get; set; } = new List<CondicionData>();
        public string Beneficio { get; set; } = string.Empty;
        public int? CostoMana { get; set; }
        public List<string>? Mejoras { get; set; }
    }

    /// <summary>
    /// Servicio simple para obtener habilidades habilitables para un personaje, en base a atributos/condiciones mínimas.
    /// No aplica efectos, sólo determina elegibilidad y provee el objeto de progreso listo para ser aprendido.
    /// </summary>
    public static class HabilidadCatalogService
    {
        private static List<HabilidadData>? _cache;
    public static IReadOnlyList<HabilidadData> Todas => _cache ??= HabilidadLoader.CargarTodas(MiJuegoRPG.Motor.Servicios.PathProvider.CombineData(Path.Combine("habilidades")));

        public static IEnumerable<HabilidadData> ElegiblesPara(MiJuegoRPG.Personaje.Personaje pj)
        {
            foreach (var h in Todas)
            {
                if (h == null || string.IsNullOrWhiteSpace(h.Id)) continue;
                if (pj.Habilidades.ContainsKey(h.Id)) continue; // ya aprendida
                if (h.AtributosNecesarios != null && !pj.CumpleRequisitosHabilidad(h.AtributosNecesarios)) continue;
                if (!CumpleCondicionesBasicas(pj, h.Condiciones)) continue;
                yield return h;
            }
        }

        public static MiJuegoRPG.Personaje.HabilidadProgreso AProgreso(HabilidadData h)
        {
            var progreso = new MiJuegoRPG.Personaje.HabilidadProgreso
            {
                Id = h.Id,
                Nombre = string.IsNullOrWhiteSpace(h.Nombre) ? h.Id : h.Nombre,
                Exp = h.Exp ?? 0,
                AtributosNecesarios = h.AtributosNecesarios != null ? new Dictionary<string, int>(h.AtributosNecesarios, System.StringComparer.OrdinalIgnoreCase) : null,
                Evoluciones = new List<MiJuegoRPG.Personaje.EvolucionHabilidad>()
            };
            foreach (var evo in h.Evoluciones ?? new List<EvolucionData>())
            {
                progreso.Evoluciones.Add(new MiJuegoRPG.Personaje.EvolucionHabilidad
                {
                    Id = evo.Id,
                    Nombre = string.IsNullOrWhiteSpace(evo.Nombre) ? evo.Id : evo.Nombre,
                    Beneficio = evo.Beneficio ?? string.Empty,
                    Condiciones = (evo.Condiciones ?? new List<CondicionData>())
                        .ConvertAll(c => new MiJuegoRPG.Personaje.CondicionEvolucion { Tipo = c.Tipo, Cantidad = (int)(c.Cantidad ?? 0) })
                });
            }
            return progreso;
        }

        private static bool CumpleCondicionesBasicas(MiJuegoRPG.Personaje.Personaje pj, List<CondicionData> condiciones)
        {
            if (condiciones == null || condiciones.Count == 0) return true;
            foreach (var c in condiciones)
            {
                if (c == null) continue;
                switch ((c.Tipo ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "nivel":
                        if (c.Cantidad.HasValue && pj.Nivel < c.Cantidad.Value) return false; break;
                    case "mision":
                        if (!string.IsNullOrWhiteSpace(c.Accion))
                        {
                            if (pj.MisionesCompletadas == null || !pj.MisionesCompletadas.Exists(m => string.Equals(m.Id, c.Accion, StringComparison.OrdinalIgnoreCase))) return false;
                        }
                        break;
                    // Otras condiciones más complejas (entrenamiento, uso, etc.) se evaluarán in-game por eventos; aquí no bloqueamos.
                    default:
                        break;
                }
            }
            return true;
        }
    }
}
