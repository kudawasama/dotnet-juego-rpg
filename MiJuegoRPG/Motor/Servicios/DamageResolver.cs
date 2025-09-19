using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Resolver de daño mínimo: delega el cálculo principal al método existente del ejecutor
    /// (AtacarFisico/AtacarMagico) para no romper fórmulas actuales, y añade metadatos (crítico)
    /// y mensajes complementarios de forma no intrusiva.
    /// </summary>
    public class DamageResolver
    {
        /// <summary>
        /// Construye un mensaje explicativo para ataques físicos, describiendo pasos:
        /// Base → Defensa(±Penetración) → Mitigación → Crítico (nota) → Final.
        /// No altera el cálculo, solo explica con aproximación basada en propiedades públicas.
        /// </summary>
        private static string FormatearDetalleFisico(ICombatiente ejecutor, ICombatiente objetivo, int danioBase, int danioFinal, bool fueCritico)
        {
            if (danioBase <= 0 || danioFinal < 0) return "";
            try
            {
                double pen = 0.0;
                if (GameplayToggles.PenetracionEnabled && ejecutor is MiJuegoRPG.Personaje.Personaje pjPen)
                {
                    pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                }

                int afterDefInt;
                double defEfectiva;
                double mitig = 0.0;
                if (objetivo is MiJuegoRPG.Enemigos.Enemigo ene)
                {
                    defEfectiva = System.Math.Round(ene.Defensa * (1.0 - pen), System.MidpointRounding.AwayFromZero);
                    afterDefInt = System.Math.Max(1, danioBase - (int)defEfectiva);
                    mitig = System.Math.Clamp(ene.MitigacionFisicaPorcentaje, 0.0, 0.9);
                    if (mitig > 0)
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - mitig), System.MidpointRounding.AwayFromZero));
                }
                else if (objetivo is MiJuegoRPG.Personaje.Personaje pj)
                {
                    // Defensa total del jugador (atributo + bonificadores); sin mitigación porcentual específica
                    double defensaTotal = pj.Defensa + pj.ObtenerBonificadorAtributo("Defensa") + pj.ObtenerBonificadorEstadistica("Defensa Física");
                    defEfectiva = System.Math.Max(0.0, defensaTotal * (1.0 - pen));
                    afterDefInt = (int)System.Math.Max(1, System.Math.Round(danioBase - defEfectiva, System.MidpointRounding.AwayFromZero));
                }
                else
                {
                    // Fallback genérico
                    defEfectiva = 0.0;
                    afterDefInt = danioBase;
                }

                // Armar cadena explicativa
                var partes = new System.Collections.Generic.List<string>();
                partes.Add($"Base {danioBase}");
                if (defEfectiva > 0)
                {
                    double redPorDef = System.Math.Clamp((danioBase - System.Math.Max(1, danioBase - defEfectiva)) / System.Math.Max(1.0, danioBase), 0.0, 1.0);
                    partes.Add($"Defensa efectiva {defEfectiva:0} {(pen > 0 ? $"(Pen {pen*100:0}% )" : "")}");
                }
                if (mitig > 0)
                {
                    partes.Add($"Mitigación {mitig*100:0}%");
                }
                if (fueCritico)
                {
                    partes.Add("Crítico"); // Solo nota: el crítico no altera el cálculo actual
                }
                partes.Add($"Daño final: {danioFinal}");
                return string.Join("; ", partes);
            }
            catch { return ""; }
        }

        /// <summary>
        /// Construye un mensaje explicativo para ataques mágicos: Base → Defensa Mágica(±Pen) → Mitigación → Resistencia "magia" → Vulnerabilidad → Final.
        /// </summary>
        private static string FormatearDetalleMagico(ICombatiente ejecutor, ICombatiente objetivo, int danioBase, int danioFinal, bool fueCritico)
        {
            if (danioBase <= 0 || danioFinal < 0) return "";
            try
            {
                double pen = 0.0;
                if (GameplayToggles.PenetracionEnabled && ejecutor is MiJuegoRPG.Personaje.Personaje pjPen)
                {
                    pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                }

                int afterDefInt;
                double defEfectiva;
                double mitig = 0.0;
                double resMag = 0.0;
                double vulnMag = 1.0;
                if (objetivo is MiJuegoRPG.Enemigos.Enemigo ene)
                {
                    defEfectiva = System.Math.Round(ene.DefensaMagica * (1.0 - pen), System.MidpointRounding.AwayFromZero);
                    afterDefInt = System.Math.Max(1, danioBase - (int)defEfectiva);
                    mitig = System.Math.Clamp(ene.MitigacionMagicaPorcentaje, 0.0, 0.9);
                    if (mitig > 0)
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - mitig), System.MidpointRounding.AwayFromZero));
                    if (ene.ResistenciasElementales.TryGetValue("magia", out var r)) resMag = System.Math.Clamp(r, 0.0, 0.9);
                    if (resMag > 0)
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - resMag), System.MidpointRounding.AwayFromZero));
                    if (ene.VulnerabilidadesElementales.TryGetValue("magia", out var v)) vulnMag = System.Math.Clamp(v, 1.0, 1.5);
                    if (vulnMag > 1.0)
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * vulnMag, System.MidpointRounding.AwayFromZero));
                }
                else if (objetivo is MiJuegoRPG.Personaje.Personaje pj)
                {
                    double defTot = pj.DefensaMagica + pj.ObtenerBonificadorAtributo("Resistencia") + pj.ObtenerBonificadorEstadistica("Defensa Mágica");
                    defEfectiva = System.Math.Max(0.0, defTot * (1.0 - pen));
                    afterDefInt = (int)System.Math.Max(1, System.Math.Round(danioBase - defEfectiva, System.MidpointRounding.AwayFromZero));
                }
                else
                {
                    defEfectiva = 0.0;
                    afterDefInt = danioBase;
                }

                var partes = new System.Collections.Generic.List<string>();
                partes.Add($"Base {danioBase}");
                if (defEfectiva > 0) partes.Add($"Defensa mágica efectiva {defEfectiva:0} {(pen > 0 ? $"(Pen {pen*100:0}% )" : "")}");
                if (mitig > 0) partes.Add($"Mitigación {mitig*100:0}%");
                if (resMag > 0) partes.Add($"Resistencia magia {resMag*100:0}%");
                if (vulnMag > 1.0) partes.Add($"Vulnerabilidad +{(vulnMag-1.0)*100:0}%");
                if (fueCritico) partes.Add("Crítico");
                partes.Add($"Daño final: {danioFinal}");
                return string.Join("; ", partes);
            }
            catch { return ""; }
        }

        /// <summary>
        /// Resuelve un ataque físico aprovechando la lógica existente del ejecutor.
        /// No modifica el daño retornado actualmente; solo anota si fue crítico.
        /// </summary>
        public ResultadoAccion ResolverAtaqueFisico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            int vidaAntes = objetivo.Vida;
            // Paso 0 (opcional): Chequeo de precisión previa al ataque físico.
            // Si está activo el toggle global y el ejecutor es un Personaje, usamos su estadística de Precisión.
            // En caso de fallar, no delegamos al método de ataque y devolvemos un resultado con 0 daño.
            if (GameplayToggles.PrecisionCheckEnabled && ejecutor is MiJuegoRPG.Personaje.Personaje pjPrec)
            {
                // Base: precisión del ejecutor, con caps desde configuración
                double pHit = CombatBalanceConfig.ClampPrecision(pjPrec.Estadisticas.Precision);
                // Penalización por estados de Supervivencia (hambre/sed/fatiga)
                try
                {
                    var juego = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual();
                    var sup = juego?.supervivenciaService;
                    if (sup != null)
                    {
                        var (etH, etS, etF) = sup.EtiquetasHSF(pjPrec.Hambre, pjPrec.Sed, pjPrec.Fatiga);
                        double f = sup.FactorPrecision(etH, etS, etF); // típicamente <= 1.0 en advertencia/crítico
                        pHit = CombatBalanceConfig.ClampPrecision(pHit * f);
                    }
                }
                catch { /* tolerante: si no hay juego/servicio, no penaliza */ }
                var rng0 = RandomService.Instancia;
                bool acierta = rng0.NextDouble() < pHit;
                if (!acierta)
                {
                    var miss = new ResultadoAccion
                    {
                        NombreAccion = "Ataque Físico",
                        Ejecutor = ejecutor,
                        Objetivo = objetivo,
                        DanioBase = 0,
                        DanioReal = 0,
                        EsMagico = false,
                        ObjetivoDerrotado = !objetivo.EstaVivo,
                        FueEvadido = true,
                    };
                    miss.Mensajes.Add($"{ejecutor.Nombre} falla el Ataque Físico sobre {objetivo.Nombre} (precisión insuficiente).");
                    return miss;
                }
            }

            // Ejecuta el ataque físico según la lógica vigente. Si el toggle de penetración está activo
            // y el ejecutor es Personaje, propagamos su penetración al receptor mediante contexto ambiental.
            int danio;
            if (GameplayToggles.PenetracionEnabled && (ejecutor is MiJuegoRPG.Personaje.Personaje pjPen))
            {
                double pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danio = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarFisico(objetivo));
            }
            else
            {
                danio = ejecutor.AtacarFisico(objetivo);
            }
            int danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Físico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danioAplicado,
                EsMagico = false,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };

            // Si no hubo daño aplicado, interpretamos como evasión/fallo (o mitigación total);
            // más preciso que depender del valor retornado por AtacarFisico (que puede ser pre-defensas).
            res.FueEvadido = danioAplicado == 0;

            // Crítico (no intrusivo): si el ejecutor es Personaje, usar su estadística CritChance/Critico como probabilidad (0..1 aprox.)
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is MiJuegoRPG.Personaje.Personaje pj)
            {
                // Preferir CritChance si está disponible (>0); de lo contrario usar 'Critico' legacy.
                double raw = pj.Estadisticas.CritChance > 0 ? pj.Estadisticas.CritChance : pj.Estadisticas.Critico;
                // Clamp conservador: 0..0.95; si CritChance>=1.0, consideramos crítico forzado (útil para pruebas deterministas)
                pCrit = System.Math.Clamp(raw, 0.0, 0.95);
                if (raw >= 1.0)
                {
                    pCrit = 1.0; // fuerza crítico
                    forceCrit = true;
                }
            }
            var rng = RandomService.Instancia;
            bool fueCrit = danioAplicado > 0 && (forceCrit || rng.NextDouble() < pCrit);
            res.FueCritico = fueCrit;

            // Mensajes base (mantener compatibilidad con pruebas que revisan el primer mensaje)
            res.Mensajes.Add($"{ejecutor.Nombre} usa Ataque Físico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño.");
            if (res.FueEvadido)
            {
                res.Mensajes.Add("¡El objetivo evadió el ataque!");
            }
            else if (fueCrit)
            {
                res.Mensajes.Add("¡Golpe crítico!");
            }

            // Mensaje explicativo adicional (didáctico) solo si está activo el modo verbose
            if (!res.FueEvadido && GameplayToggles.CombatVerbose)
            {
                var detalle = FormatearDetalleFisico(ejecutor, objetivo, res.DanioBase, res.DanioReal, res.FueCritico);
                if (!string.IsNullOrWhiteSpace(detalle)) res.Mensajes.Add(detalle);
            }

            return res;
        }

        /// <summary>
        /// Resuelve un ataque mágico delegando el cálculo principal al ejecutor.
        /// Mantiene el daño actual (no altera fórmulas), anota metadatos y mensajes.
        /// </summary>
        public ResultadoAccion ResolverAtaqueMagico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            int vidaAntes = objetivo.Vida;

            // Ejecuta el ataque mágico según la lógica vigente; aplica el mismo mecanismo de penetración
            // para defensa mágica si el toggle está activo.
            int danio;
            if (GameplayToggles.PenetracionEnabled && (ejecutor is MiJuegoRPG.Personaje.Personaje pjPen))
            {
                double pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danio = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarMagico(objetivo));
            }
            else
            {
                danio = ejecutor.AtacarMagico(objetivo);
            }
            int danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Mágico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danioAplicado,
                EsMagico = true,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };

            // Si no hubo daño aplicado, interpretamos como evasión/fallo (o mitigación total)
            res.FueEvadido = danioAplicado == 0;

            // Crítico (no intrusivo) igual que en físico
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is MiJuegoRPG.Personaje.Personaje pj)
            {
                double raw = pj.Estadisticas.CritChance > 0 ? pj.Estadisticas.CritChance : pj.Estadisticas.Critico;
                pCrit = System.Math.Clamp(raw, 0.0, 0.95);
                if (raw >= 1.0)
                {
                    pCrit = 1.0; // fuerza crítico en pruebas
                    forceCrit = true;
                }
            }
            var rng = RandomService.Instancia;
            bool fueCrit = danioAplicado > 0 && (forceCrit || rng.NextDouble() < pCrit);
            res.FueCritico = fueCrit;

            // Mensajes
            res.Mensajes.Add($"{ejecutor.Nombre} lanza Ataque Mágico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño mágico.");
            if (res.FueEvadido)
            {
                res.Mensajes.Add("¡El objetivo evadió el ataque!");
            }
            else if (fueCrit)
            {
                res.Mensajes.Add("¡Golpe crítico!");
            }

            // Mensaje explicativo adicional para mágico solo si verbose está activo
            if (!res.FueEvadido && GameplayToggles.CombatVerbose)
            {
                var detalle = FormatearDetalleMagico(ejecutor, objetivo, res.DanioBase, res.DanioReal, res.FueCritico);
                if (!string.IsNullOrWhiteSpace(detalle)) res.Mensajes.Add(detalle);
            }

            return res;
        }
    }
}
