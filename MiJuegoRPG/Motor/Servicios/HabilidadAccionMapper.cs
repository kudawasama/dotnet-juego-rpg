using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Mapea habilidades aprendidas (por Id/Nombre) a acciones de combate concretas.
    /// - Permite definir sinónimos y defaults tolerantes.
    /// - Intenta aplicar CostoMana/Cooldown desde el catálogo si existen; si no, usa los de la acción por defecto.
    /// - Si no encuentra mapping, retorna null (no se mostrará en menú).
    /// </summary>
    public static class HabilidadAccionMapper
    {
        private static readonly Dictionary<string, Func<IAccionCombate>> BaseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Ataque físico
            ["ataque_fisico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion(),
            ["ataquefisico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion(),
            ["ataque fisico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion(),
            ["golpe"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion(),
            ["slash"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion(),
            // Ataque mágico
            ["ataque_magico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion(),
            ["ataquemagico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion(),
            ["ataque magico"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion(),
            ["hechizo"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion(),
            ["bola de fuego"] = () => new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion(),
            // Veneno
            ["veneno"] = () => new MiJuegoRPG.Motor.Acciones.AplicarVenenoAccion(),
            ["aplicar veneno"] = () => new MiJuegoRPG.Motor.Acciones.AplicarVenenoAccion(),
            ["poison"] = () => new MiJuegoRPG.Motor.Acciones.AplicarVenenoAccion(),
        };

        /// <summary>
        /// Crea una acción de combate para la habilidad indicada.
        /// Usa Id y Nombre (normalizados) para resolver el mapping.
        /// </summary>
        /// <returns></returns>
        public static IAccionCombate? CrearAccionPara(string habilidadId, MiJuegoRPG.Personaje.HabilidadProgreso progreso)
        {
            // 1) Preferir AccionId explícito en el catálogo, si existe
            try
            {
                var data = MiJuegoRPG.Habilidades.HabilidadCatalogService.Todas
                    .FirstOrDefault(h => string.Equals(h.Id, progreso.Id, StringComparison.OrdinalIgnoreCase));
                if (data != null && !string.IsNullOrWhiteSpace(data.AccionId))
                {
                    var accId = data.AccionId.Trim();
                    if (BaseMap.TryGetValue(accId, out var factoryExp))
                    {
                        var accionBase = factoryExp();
                        return AplicarCostosDesdeCatalogo(accionBase, data, progreso);
                    }
                }
            }
            catch { /* tolerante */ }

            // 2) Fallback por Id/Nombre y sinónimos
            var claves = NombresPosibles(habilidadId, progreso?.Nombre);
            foreach (var k in claves)
            {
                if (BaseMap.TryGetValue(k, out var factory))
                {
                    var accionBase = factory();
                    return AplicarCostosDesdeCatalogo(accionBase, null, progreso!);
                }
            }
            return null;
        }

        /// <summary>
        /// Devuelve el AccionId explícito definido en el catálogo para una habilidad (si existe),
        /// normalizado. Si no existe, retorna cadena vacía.
        /// </summary>
        /// <returns></returns>
        public static string ResolverAccionIdPara(string habilidadId)
        {
            try
            {
                var data = MiJuegoRPG.Habilidades.HabilidadCatalogService.Todas
                    .FirstOrDefault(h => string.Equals(h.Id, habilidadId, StringComparison.OrdinalIgnoreCase));
                if (data != null && !string.IsNullOrWhiteSpace(data.AccionId))
                    return data.AccionId.Trim();
            }
            catch { }
            return string.Empty;
        }

        private static IAccionCombate AplicarCostosDesdeCatalogo(IAccionCombate accionBase, MiJuegoRPG.Habilidades.HabilidadData? data, MiJuegoRPG.Personaje.HabilidadProgreso progreso)
        {
            try
            {
                data ??= MiJuegoRPG.Habilidades.HabilidadCatalogService.Todas
                    .FirstOrDefault(h => string.Equals(h.Id, progreso.Id, StringComparison.OrdinalIgnoreCase));
                int? costo = data?.CostoMana;
                int? cd = null;
                if (data != null && progreso != null && progreso.EvolucionesDesbloqueadas != null && data.Evoluciones != null)
                {
                    foreach (var evId in progreso.EvolucionesDesbloqueadas)
                    {
                        var ev = data.Evoluciones.FirstOrDefault(e => string.Equals(e.Id, evId, StringComparison.OrdinalIgnoreCase));
                        if (ev != null && ev.CostoMana.HasValue)
                            costo = costo.HasValue ? Math.Min(costo.Value, ev.CostoMana.Value) : ev.CostoMana.Value;
                    }
                }
                if (costo.HasValue || cd.HasValue)
                {
                    return new MiJuegoRPG.Motor.Acciones.AccionCompuestaSimple(
                        nombre: accionBase.Nombre,
                        costoMana: costo ?? accionBase.CostoMana,
                        cooldown: cd ?? accionBase.CooldownTurnos,
                        ejecutar: (ejecutor, objetivo) => accionBase.Ejecutar(ejecutor, objetivo));
                }
                return accionBase;
            }
            catch { return accionBase; }
        }

        /// <summary>
        /// Genera nombres candidatos normalizados a partir de Id/Nombre.
        /// </summary>
        private static IEnumerable<string> NombresPosibles(string id, string? nombre)
        {
            string Norm(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Add(string s)
            {
                s = Norm(s);
                if (!string.IsNullOrEmpty(s))
                    set.Add(s);
                set.Add(s.Replace("_", "").Replace("-", " "));
                set.Add(s.Replace("_", "").Replace("-", ""));
            }
            Add(id);
            if (!string.IsNullOrWhiteSpace(nombre))
                Add(nombre);
            return set;
        }
    }
}
