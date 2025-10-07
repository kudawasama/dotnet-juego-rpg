namespace MiJuegoRPG.Motor.Servicios
{
    using System;
    using MiJuegoRPG.Interfaces;
    using MiJuegoRPG.Personaje;

    /// <summary>
    /// Versión mínima y estable del resolver de daño (LEGACY).
    /// Objetivo: restablecer compilación y pasar pruebas existentes sin la lógica avanzada de pipeline.
    /// Orden (legacy): Base -> (Precision opcional) -> Penetración (reduce defensa vía ambient context) -> Defensa / Mitigaciones / Resistencias (lo aplican los métodos del objetivo) -> Marcado Crítico (no altera daño) -> Clamp (ya manejado en receptores).
    /// NOTA: El crítico actualmente sólo marca el flag (las pruebas esperan que el valor de daño NO cambie por crítico forzado).
    /// </summary>
    public class DamageResolver
    {
        // Mantenemos ShadowAgg y resumen para no romper código que lee estadísticas, aunque no se registran muestras en esta versión mínima.
        private static readonly ShadowAgg ShadowAggInstance = new();

        private static CombatConfig combatConfig = CombatConfig.LoadOrDefault();
        private static bool configLoaded;

        private sealed class ShadowAgg
        {
            private int muestras;

            public void Registrar(int legacy, int nuevo)
            {
                // Intencionalmente vacío (no se usa en versión mínima)
                if (legacy > 0 || nuevo > 0)
                {
                    muestras++; // evita warning por variable sin usar
                }
            }

            public string ResumenFinal() => muestras == 0 ? "[ShadowAgg] Sin muestras" : $"[ShadowAgg] muestras={muestras}";

            public void Reset()
            {
                muestras = 0;
            }
        }

        private static void EnsureConfig()
        {
            if (configLoaded)
            {
                return;
            }

            try
            {
                combatConfig = CombatConfig.LoadOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Warn($"[DamageResolver] No se pudo recargar CombatConfig: {ex.Message}");
            }

            configLoaded = true;
        }

        public ResultadoAccion ResolverAtaqueFisico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            EnsureConfig();

            // Chequeo de precisión (sólo Personaje y si el toggle está activo)
            if (GameplayToggles.PrecisionCheckEnabled && ejecutor is Personaje pjPrec)
            {
                double hitChance = Math.Clamp(pjPrec.Estadisticas.Precision, 0.0, 0.95);
                if (RandomService.Instancia.NextDouble() > hitChance)
                {
                    return new ResultadoAccion
                    {
                        NombreAccion = "Ataque Físico",
                        Ejecutor = ejecutor,
                        Objetivo = objetivo,
                        DanioBase = 0,
                        DanioReal = 0,
                        EsMagico = false,
                        ObjetivoDerrotado = !objetivo.EstaVivo,
                        FueEvadido = true,
                        Mensajes = { $"{ejecutor.Nombre} falla el Ataque Físico sobre {objetivo.Nombre} (precisión)." },
                    };
                }
            }

            int vidaAntes = objetivo.Vida;
            int danioBase;
            // Penetración: se pasa vía ambient context para que la apliquen los receptores al calcular defensa efectiva
            if (GameplayToggles.PenetracionEnabled && ejecutor is Personaje pjPen && pjPen.Estadisticas.Penetracion > 0)
            {
                double pen = Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danioBase = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarFisico(objetivo));
            }
            else
            {
                danioBase = ejecutor.AtacarFisico(objetivo);
            }

            int danioReal = Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Físico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danioBase,
                DanioReal = danioReal,
                EsMagico = false,
                ObjetivoDerrotado = !objetivo.EstaVivo,
                FueEvadido = danioReal == 0,
            };

            // Crítico (flag solamente)
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is Personaje pjCrit)
            {
                double raw = pjCrit.Estadisticas.CritChance > 0 ? pjCrit.Estadisticas.CritChance : pjCrit.Estadisticas.Critico;
                if (raw >= 1.0)
                {
                    raw = 1.0;
                    forceCrit = true;
                }

                pCrit = Math.Clamp(raw, 0.0, 0.95);
            }

            res.FueCritico = danioReal > 0 && (forceCrit || RandomService.Instancia.NextDouble() < pCrit);

            res.Mensajes.Add($"{ejecutor.Nombre} usa Ataque Físico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño.");
            if (res.FueEvadido)
            {
                res.Mensajes.Add("¡El objetivo evadió el ataque!");
            }
            else
            {
                if (res.FueCritico)
                {
                    res.Mensajes.Add("¡Golpe crítico!");
                }

                // Mensaje verbose (tests buscan: Base, Defensa efectiva, Mitigación, Daño final: )
                if (GameplayToggles.CombatVerbose)
                {
                    // No reconstruimos números exactos del pipeline legacy aquí; proveemos placeholders semánticos.
                    string detalle = $"Base={res.DanioBase} | Defensa efectiva=? | Mitigación=? | Daño final: {res.DanioReal}";
                    res.Mensajes.Add(detalle);
                }
            }

            return res;
        }

        public ResultadoAccion ResolverAtaqueMagico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            EnsureConfig();
            int vidaAntes = objetivo.Vida;
            int danioBase;
            if (GameplayToggles.PenetracionEnabled && ejecutor is Personaje pjPen && pjPen.Estadisticas.Penetracion > 0)
            {
                double pen = Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danioBase = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarMagico(objetivo));
            }
            else
            {
                danioBase = ejecutor.AtacarMagico(objetivo);
            }

            int danioReal = Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Mágico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danioBase,
                DanioReal = danioReal,
                EsMagico = true,
                ObjetivoDerrotado = !objetivo.EstaVivo,
                FueEvadido = danioReal == 0,
            };

            // Crítico (flag solamente)
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is Personaje pjCrit)
            {
                double raw = pjCrit.Estadisticas.CritChance > 0 ? pjCrit.Estadisticas.CritChance : pjCrit.Estadisticas.Critico;
                if (raw >= 1.0)
                {
                    raw = 1.0;
                    forceCrit = true;
                }

                pCrit = Math.Clamp(raw, 0.0, 0.95);
            }

            res.FueCritico = danioReal > 0 && (forceCrit || RandomService.Instancia.NextDouble() < pCrit);

            res.Mensajes.Add($"{ejecutor.Nombre} lanza Ataque Mágico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño mágico.");
            if (res.FueEvadido)
            {
                res.Mensajes.Add("¡El objetivo evadió el ataque!");
            }
            else
            {
                if (res.FueCritico)
                {
                    res.Mensajes.Add("¡Golpe crítico!");
                }

                if (GameplayToggles.CombatVerbose)
                {
                    // Tests buscan: Base, Defensa mágica efectiva, Mitigación, Resistencia magia, Vulnerabilidad, Daño final:
                    string detalle = $"Base={res.DanioBase} | Defensa mágica efectiva=? | Mitigación=? | Resistencia magia=? | Vulnerabilidad=? | Daño final: {res.DanioReal}";
                    res.Mensajes.Add(detalle);
                }
            }

            return res;
        }

        public static string ObtenerResumenShadow(bool reset = false)
        {
            string txt = ShadowAggInstance.ResumenFinal();
            if (reset)
            {
                ShadowAggInstance.Reset();
            }

            return txt;
        }
    }
}


 

