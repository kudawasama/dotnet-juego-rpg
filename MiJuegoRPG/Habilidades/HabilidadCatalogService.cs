using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiJuegoRPG.Habilidades
{
    /// <summary>
    /// Servicio simple para obtener habilidades habilitables para un personaje, en base a atributos/condiciones mínimas.
    /// No aplica efectos, sólo determina elegibilidad y provee el objeto de progreso listo para ser aprendido.
    /// </summary>
    public static class HabilidadCatalogService
    {
        private static List<HabilidadData>? cache;
        public static IReadOnlyList<HabilidadData> Todas => cache ??= HabilidadLoader.CargarTodas(MiJuegoRPG.Motor.Servicios.PathProvider.CombineData(Path.Combine("habilidades")));

        public static IEnumerable<HabilidadData> ElegiblesPara(MiJuegoRPG.Personaje.Personaje pj)
        {
            foreach (var h in Todas)
            {
                if (h == null || string.IsNullOrWhiteSpace(h.Id))
                    continue;
                if (pj.Habilidades.ContainsKey(h.Id))
                    continue; // ya aprendida
                if (h.AtributosNecesarios != null && !pj.CumpleRequisitosHabilidad(h.AtributosNecesarios))
                    continue;
                if (!CumpleCondicionesBasicas(pj, h.Condiciones))
                    continue;
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
            if (condiciones == null || condiciones.Count == 0)
                return true;
            foreach (var c in condiciones)
            {
                if (c == null)
                    continue;
                switch ((c.Tipo ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "nivel":
                        if (c.Cantidad.HasValue && pj.Nivel < c.Cantidad.Value)
                            return false;
                        break;
                    case "mision":
                        if (!string.IsNullOrWhiteSpace(c.Accion))
                        {
                            if (pj.MisionesCompletadas == null || !pj.MisionesCompletadas.Exists(m => string.Equals(m.Id, c.Accion, StringComparison.OrdinalIgnoreCase)))
                                return false;
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