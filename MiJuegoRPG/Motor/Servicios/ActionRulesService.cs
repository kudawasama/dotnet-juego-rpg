using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Servicio que gestiona reglas de acciones en combate (por ahora, cooldowns).
    /// Futuro: centralizar verificación de recursos/costos para mantener consistencia.
    /// </summary>
    public class ActionRulesService
    {
        // Cooldowns por combatiente y por nombre de acción
        private readonly Dictionary<ICombatiente, Dictionary<string, int>> cooldowns = new();
        // Acumuladores de regeneración de maná por combatiente
        private readonly Dictionary<ICombatiente, double> manaRegenAcumulada = new();

        // Parámetros de regeneración (configurables vía progression.json)
        private double manaRegenBase = 0.2;               // por turno
        private double manaRegenFactor = 0.02;            // multiplicador sobre Estadisticas.RegeneracionMana
        private double manaRegenMaxPorTurno = 1.0;        // tope duro por turno
                                                           // Parámetros de regeneración fuera de combate (descanso/pasivo)
        private double manaRegenFueraBase = 0.5;          // por tick de descanso
        private double manaRegenFueraFactor = 0.05;       // multiplicador sobre Estadisticas.RegeneracionMana
        private double manaRegenFueraMaxPorTick = 3.0;    // tope por tick fuera de combate

        public ActionRulesService()
        {
            // Intentar cargar parámetros desde progression.json (opcional)
            try
            {
                var ruta = PathProvider.CombineData("progression.json");
                if (System.IO.File.Exists(ruta))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(ruta));
                    var root = doc.RootElement;
                    if (root.TryGetProperty("ManaRegenCombateBase", out var p1) && p1.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenBase = p1.GetDouble();
                    if (root.TryGetProperty("ManaRegenCombateFactor", out var p2) && p2.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenFactor = p2.GetDouble();
                    if (root.TryGetProperty("ManaRegenCombateMaxPorTurno", out var p3) && p3.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenMaxPorTurno = p3.GetDouble();
                    // Fuera de combate (opcional)
                    if (root.TryGetProperty("ManaRegenFueraBase", out var f1) && f1.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenFueraBase = f1.GetDouble();
                    if (root.TryGetProperty("ManaRegenFueraFactor", out var f2) && f2.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenFueraFactor = f2.GetDouble();
                    if (root.TryGetProperty("ManaRegenFueraMaxPorTick", out var f3) && f3.ValueKind == System.Text.Json.JsonValueKind.Number)
                        manaRegenFueraMaxPorTick = f3.GetDouble();
                }
            }
            catch { /* fallback a defaults */ }
        }

        /// <summary>
        /// Verifica si el actor tiene recursos suficientes para ejecutar la acción.
        /// Por ahora sólo valida maná en personajes. No consume recursos.
        /// </summary>
        /// <returns></returns>
        public bool TieneRecursos(ICombatiente actor, IAccionCombate accion, out string mensaje)
        {
            mensaje = string.Empty;
            if (accion == null)
                return true;
            // Sólo Personaje tiene maná (por ahora)
            if (accion.CostoMana > 0 && actor is MiJuegoRPG.Personaje.Personaje pj)
            {
                if (pj.ManaActual < accion.CostoMana)
                {
                    mensaje = "No tienes maná suficiente.";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Consume recursos requeridos por la acción (p. ej., maná) en el actor.
        /// Devuelve false si no pudo consumir (p. ej., maná insuficiente).
        /// </summary>
        /// <returns></returns>
        public bool ConsumirRecursos(ICombatiente actor, IAccionCombate accion)
        {
            if (accion == null)
                return true;
            if (accion.CostoMana > 0 && actor is MiJuegoRPG.Personaje.Personaje pj)
            {
                if (pj.ManaActual < accion.CostoMana)
                    return false;
                return pj.GastarMana(accion.CostoMana);
            }
            return true;
        }

        public bool EstaEnCooldown(ICombatiente actor, IAccionCombate accion, out int turnosRestantes)
        {
            turnosRestantes = 0;
            if (accion.CooldownTurnos <= 0)
                return false;
            if (!cooldowns.TryGetValue(actor, out var map))
                return false;
            if (!map.TryGetValue(accion.Nombre, out var cd))
                return false;
            turnosRestantes = Math.Max(0, cd);
            return cd > 0;
        }

        public void AplicarCooldown(ICombatiente actor, IAccionCombate accion)
        {
            if (accion.CooldownTurnos <= 0)
                return;
            if (!cooldowns.TryGetValue(actor, out var map))
            {
                map = new Dictionary<string, int>();
                cooldowns[actor] = map;
            }
            map[accion.Nombre] = accion.CooldownTurnos;
        }

        public void AvanzarCooldownsDe(ICombatiente actor)
        {
            if (!cooldowns.TryGetValue(actor, out var map))
                return;
            var keys = map.Keys.ToList();
            foreach (var k in keys)
            {
                map[k] = Math.Max(0, map[k] - 1);
            }
        }

        /// <summary>
        /// Regenera maná para el actor (si es Personaje) según parámetros configurados.
        /// Usa acumulación fraccional por turno y retorna la cantidad efectivamente recuperada (entera).
        /// </summary>
        /// <returns></returns>
        public int RegenerarManaTurno(ICombatiente actor)
        {
            if (actor is not MiJuegoRPG.Personaje.Personaje pj)
                return 0;
            // Si ya está a tope, no acumular
            if (pj.ManaActual >= pj.ManaMaxima)
                return 0;
            double regen = manaRegenBase + (pj.Estadisticas.RegeneracionMana * manaRegenFactor);
            // Penalización por supervivencia (si existe servicio y config). Se aplica solo a jugador.
            try
            {
                var sup = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual()?.SupervivenciaService;
                if (sup != null)
                {
                    var (etH, etS, etF) = sup.EtiquetasHSF(pj.Hambre, pj.Sed, pj.Fatiga);
                    double f = sup.FactorRegen(etH, etS, etF);
                    regen *= f;
                }
            }
            catch { }
            if (regen < 0)
                regen = 0;
            if (regen > manaRegenMaxPorTurno)
                regen = manaRegenMaxPorTurno;
            manaRegenAcumulada.TryGetValue(actor, out var acum);
            acum += regen;
            int ganar = (int)Math.Floor(acum);
            if (ganar > 0)
            {
                pj.RecuperarMana(ganar);
                acum -= ganar;
            }
            manaRegenAcumulada[actor] = acum;
            return ganar;
        }

        /// <summary>
        /// Regeneración de maná fuera de combate (p. ej., al descansar). No usa acumulador; aplica clamp por tick.
        /// Devuelve la cantidad efectivamente recuperada.
        /// </summary>
        /// <returns></returns>
        public int RegenerarManaFueraCombate(ICombatiente actor)
        {
            if (actor is not MiJuegoRPG.Personaje.Personaje pj)
                return 0;
            if (pj.ManaActual >= pj.ManaMaxima)
                return 0;
            double regen = manaRegenFueraBase + (pj.Estadisticas.RegeneracionMana * manaRegenFueraFactor);
            try
            {
                var sup = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual()?.SupervivenciaService;
                if (sup != null)
                {
                    var (etH, etS, etF) = sup.EtiquetasHSF(pj.Hambre, pj.Sed, pj.Fatiga);
                    double f = sup.FactorRegen(etH, etS, etF);
                    regen *= f;
                }
            }
            catch { }
            if (regen < 0)
                regen = 0;
            if (regen > manaRegenFueraMaxPorTick)
                regen = manaRegenFueraMaxPorTick;
            int ganar = (int)Math.Round(regen);
            if (ganar <= 0)
                ganar = 1; // como descanso, al menos 1 si no está a tope
            int espacio = pj.ManaMaxima - pj.ManaActual;
            int efectivo = Math.Min(ganar, espacio);
            if (efectivo > 0)
                pj.RecuperarMana(efectivo);
            return efectivo;
        }
    }
}
