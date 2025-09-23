using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Habilidades;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Servicio centralizado para registrar acciones de juego y avanzar condiciones de habilidades
    /// definidas en datos (Condiciones[] con Tipo:"accion").
    /// </summary>
    public class AccionRegistry
    {
        public static AccionRegistry Instancia { get; } = new AccionRegistry();

        private AccionRegistry() {}

        /// <summary>
        /// Registra una acción realizada por el personaje y actualiza el progreso hacia
        /// el desbloqueo de habilidades cuyas condiciones referencian dicha acción.
        /// </summary>
        /// <param name="accionId">Id de la acción, debe existir en el catálogo de acciones (no obligatorio para sumar).</param>
        /// <param name="pj">Personaje que realizó la acción.</param>
        /// <param name="contexto">Contexto opcional (sector, npcId, etc.).</param>
        /// <returns>True si alguna habilidad fue desbloqueada durante esta llamada.</returns>
        public bool RegistrarAccion(string accionId, Personaje.Personaje pj, object? contexto = null)
        {
            if (pj == null || string.IsNullOrWhiteSpace(accionId)) return false;
            accionId = accionId.Trim();

            // Garantizar estructura de progreso
            pj.ProgresoAccionesPorHabilidad ??= new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            bool desbloqueo = false;
            // Buscar habilidades en catálogo que tengan condiciones de tipo accion con ese id
            var habilidadesConAccion = HabilidadCatalogService.Todas
                .Where(h => h != null && h.Condiciones != null && h.Condiciones.Any(c => !string.IsNullOrWhiteSpace(c.Accion) && string.Equals(c.Accion, accionId, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var h in habilidadesConAccion)
            {
                if (h == null || string.IsNullOrWhiteSpace(h.Id)) continue;
                // Si ya está aprendida, no hacemos nada (el progreso de acciones es para desbloqueo inicial)
                if (pj.Habilidades != null && pj.Habilidades.ContainsKey(h.Id)) continue;

                // Sumar 1 al contador de esta acción para esta habilidad
                if (!pj.ProgresoAccionesPorHabilidad.TryGetValue(h.Id, out var mapaAccion))
                {
                    mapaAccion = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    pj.ProgresoAccionesPorHabilidad[h.Id] = mapaAccion;
                }
                mapaAccion.TryGetValue(accionId, out var v);
                mapaAccion[accionId] = v + 1;

                // Verificar si cumple todas las condiciones AND básicas y de accion
                if (CumpleTodasCondicionesParaAprender(pj, h))
                {
                    // Aprender habilidad
                    var prog = HabilidadCatalogService.AProgreso(h);
                    pj.AprenderHabilidad(prog);
                    desbloqueo = true;
                }
            }

            return desbloqueo;
        }

        /// <summary>
        /// Devuelve el progreso acumulado del personaje para una acción específica requerida por una habilidad.
        /// </summary>
        public int GetProgreso(Personaje.Personaje pj, string habilidadId, string accionId)
        {
            if (pj?.ProgresoAccionesPorHabilidad == null) return 0;
            if (!pj.ProgresoAccionesPorHabilidad.TryGetValue(habilidadId, out var mapa)) return 0;
            return mapa.TryGetValue(accionId, out var v) ? v : 0;
        }

        private bool CumpleTodasCondicionesParaAprender(Personaje.Personaje pj, HabilidadData h)
        {
            // 1) Atributos mínimos si los hay
            if (h.AtributosNecesarios != null && !pj.CumpleRequisitosHabilidad(h.AtributosNecesarios)) return false;

            // 2) Condiciones básicas ya existentes (nivel, mision, etc.)
            // Reutilizamos el criterio de HabilidadCatalogService.ElegiblesPara pero expandimos para 'accion'
            foreach (var c in h.Condiciones ?? new List<CondicionData>())
            {
                if (c == null) continue;
                var tipo = (c.Tipo ?? string.Empty).Trim().ToLowerInvariant();
                switch (tipo)
                {
                    case "nivel":
                        if (c.Cantidad.HasValue && pj.Nivel < c.Cantidad.Value) return false; break;
                    case "mision":
                        if (!string.IsNullOrWhiteSpace(c.Accion))
                        {
                            if (pj.MisionesCompletadas == null || !pj.MisionesCompletadas.Exists(m => string.Equals(m.Id, c.Accion, StringComparison.OrdinalIgnoreCase))) return false;
                        }
                        break;
                    default:
                        // Cualquier condición que indique una acción concreta (tenga Accion) se trata como contador accionable.
                        if (!string.IsNullOrWhiteSpace(c.Accion))
                        {
                            var req = Math.Max(1, (int)(c.Cantidad ?? 1));
                            int progreso = GetProgreso(pj, h.Id, c.Accion);
                            if (progreso < req) return false;
                        }
                        // Otras condiciones se evaluarán con otros servicios cuando apliquen; no bloquear si no corresponde.
                        break;
                }
            }
            return true;
        }
    }
}
