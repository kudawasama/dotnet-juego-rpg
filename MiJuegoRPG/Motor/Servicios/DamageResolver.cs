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
        // @AgenteCombate: mantener orden estable (Base -> Hit/Evasion -> Pen -> Defensa -> Mitigacion -> Critico -> Vulnerabilidad -> Redondeo)
        // @AgenteCombate: si se modifica lógica de penetración crítica revisar tests CritPenetracionInteractionTests y DamagePipelineOrderTests
        private static CombatConfig combatConfig = CombatConfig.LoadOrDefault();
        private static bool configLoaded = false;
        private static ShadowAgg shadowAgg = new ShadowAgg();

        private class ShadowAgg
        {
            public int Muestras;
            public double SumaDiffAbsoluta;
            public double SumaDiffPct;
            public double MinPct = double.MaxValue;
            public double MaxPct = double.MinValue;
            public void Registrar(int legacy, int nuevo)
            {
                if (legacy <= 0 && nuevo <= 0)
                {
                    return;
                }

                Muestras++;
                int diff = nuevo - legacy;
                double pct = legacy > 0 ? (double)diff / legacy : 0;
                SumaDiffAbsoluta += diff;
                SumaDiffPct += pct;
                if (pct < MinPct) MinPct = pct;
                if (pct > MaxPct) MaxPct = pct;
                if (Muestras % 25 == 0)
                {
                    Logger.Debug($"[ShadowAgg] muestras={Muestras} avgDiffAbs={SumaDiffAbsoluta/Muestras:0.00} avgDiffPct={(SumaDiffPct/Muestras*100):0.0}% minPct={(MinPct*100):0.0}% maxPct={(MaxPct*100):0.0}%");
                }
            }
            public string ResumenFinal()
            {
                if (Muestras == 0)
                {
                    return "[ShadowAgg] Sin muestras registradas";
                }

                double avgAbs = SumaDiffAbsoluta / Muestras;
                double avgPct = (SumaDiffPct / Muestras) * 100.0;
                return $"[ShadowAgg][Final] muestras={Muestras} avgDiffAbs={avgAbs:0.00} avgDiffPct={avgPct:0.0}% minPct={(MinPct*100):0.0}% maxPct={(MaxPct*100):0.0}%";
            }
            public void Reset()
            {
                Muestras = 0; SumaDiffAbsoluta = 0; SumaDiffPct = 0; MinPct = double.MaxValue; MaxPct = double.MinValue;
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
            catch
            {
                // Intencional: tolerante a fallo de carga, se mantiene config por defecto
            }
            configLoaded = true;
        }

        /// <summary>
        /// Devuelve un resumen agregado de las diferencias shadow (si estaban activas) y opcionalmente resetea el acumulador.
        /// </summary>
        /// <param name="reset">Indica si se limpia el acumulador tras recuperar el resumen.</param>
        /// <returns>Cadena resumen o mensaje sin muestras.</returns>
        public static string ObtenerResumenShadow(bool reset = false)
        {
            var txt = shadowAgg.ResumenFinal();
            if (reset)
            {
                shadowAgg.Reset();
            }
            return txt;
        }

        /// <summary>
        /// Construye un mensaje explicativo para ataques físicos, describiendo pasos:
        /// Base → Defensa(±Penetración) → Mitigación → Crítico (nota) → Final.
        /// No altera el cálculo, solo explica con aproximación basada en propiedades públicas.
        /// </summary>
        private static string FormatearDetalleFisico(ICombatiente ejecutor, ICombatiente objetivo, int danioBase, int danioFinal, bool fueCritico)
        {
            if (danioBase <= 0 || danioFinal < 0)
            {
                return string.Empty;
            }
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
                    {
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - mitig), System.MidpointRounding.AwayFromZero));
                    }
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
                    partes.Add($"Mitigación {mitig * 100:0}%");
                }

                if (fueCritico)
                {
                    partes.Add("Crítico"); // Solo nota: el crítico no altera el cálculo actual
                }
                partes.Add($"Daño final: {danioFinal}");
                return string.Join("; ", partes);
            }
            catch
            {
                // Intencional: si falla el formato, se omite detalle
                return string.Empty;
            }
        }

        /// <summary>
        /// Construye un mensaje explicativo para ataques mágicos: Base → Defensa Mágica(±Pen) → Mitigación → Resistencia "magia" → Vulnerabilidad → Final.
        /// </summary>
        private static string FormatearDetalleMagico(ICombatiente ejecutor, ICombatiente objetivo, int danioBase, int danioFinal, bool fueCritico)
        {
            if (danioBase <= 0 || danioFinal < 0)
            {
                return string.Empty;
            }
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
                    {
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - mitig), System.MidpointRounding.AwayFromZero));
                    }
                    if (ene.ResistenciasElementales.TryGetValue("magia", out var r)) resMag = System.Math.Clamp(r, 0.0, 0.9);
                    if (resMag > 0)
                    {
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * (1.0 - resMag), System.MidpointRounding.AwayFromZero));
                    }
                    if (ene.VulnerabilidadesElementales.TryGetValue("magia", out var v)) vulnMag = System.Math.Clamp(v, 1.0, 1.5);
                    if (vulnMag > 1.0)
                    {
                        afterDefInt = (int)System.Math.Max(1, System.Math.Round(afterDefInt * vulnMag, System.MidpointRounding.AwayFromZero));
                    }
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
                if (defEfectiva > 0)
                {
                    partes.Add($"Defensa mágica efectiva {defEfectiva:0} {(pen > 0 ? $"(Pen {pen*100:0}% )" : "")}");
                }
                if (mitig > 0)
                {
                    partes.Add($"Mitigación {mitig*100:0}%");
                }
                if (resMag > 0)
                {
                    partes.Add($"Resistencia magia {resMag*100:0}%");
                }
                if (vulnMag > 1.0)
                {
                    partes.Add($"Vulnerabilidad +{(vulnMag - 1.0) * 100:0}%");
                }
                if (fueCritico)
                {
                    partes.Add("Crítico");
                }
                partes.Add($"Daño final: {danioFinal}");
                return string.Join("; ", partes);
            }
            catch
            {
                // Intencional: si falla el formato, se omite detalle
                return string.Empty;
            }
        }

        /// <summary>
        /// Resuelve un ataque físico aprovechando la lógica existente del ejecutor.
        /// No modifica el daño retornado actualmente; solo anota si fue crítico.
        /// </summary>
        public ResultadoAccion ResolverAtaqueFisico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            EnsureConfig();
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

            int danio;
            int danioAplicado;
            bool usandoNuevoLive = false;
            if (combatConfig.UseNewDamagePipelineLive && ejecutor is MiJuegoRPG.Personaje.Personaje pjLive)
            {
                // Camino live: reproducir cálculo aproximado usando pipeline (forzar impacto para mantener consistencia con legado actual)
                usandoNuevoLive = true;
                double critChanceLive = pjLive.Estadisticas.CritChance;
                if (combatConfig.UseCritDiminishingReturns)
                {
                    critChanceLive = CombatBalanceConfig.CritChanceWithDR(critChanceLive, combatConfig.CritChanceHardCap, combatConfig.CritDiminishingK);
                }
                var reqLive = new DamagePipeline.Request
                {
                    Atacante = ejecutor,
                    Objetivo = objetivo,
                    BaseDamage = ejecutor.AtacarFisico(objetivo), // reutiliza método para base (weapon+stats)
                    EsMagico = false,
                    PrecisionBase = pjLive.Estadisticas.Precision,
                    PrecisionExtra = 0,
                    EvasionObjetivo = 0,
                    Penetracion = pjLive.Estadisticas.Penetracion,
                    MitigacionPorcentual = 0, // legado incluía mitigación interna — TODO: integrar cuando se separe
                    CritChance = critChanceLive,
                    CritMultiplier = pjLive.Estadisticas.CritMult <= 0 ? combatConfig.CritMultiplier : pjLive.Estadisticas.CritMult,
                    CritScalingFactor = combatConfig.CritScalingFactor,
                    VulnerabilidadMult = 1.0,
                    MinHitClamp = combatConfig.MinHit,
                    ForzarCritico = false,
                    ForzarImpacto = true,
                    ReducePenetracionEnCritico = combatConfig.ReducePenetracionEnCritico,
                    FactorPenetracionCritico = combatConfig.FactorPenetracionCritico
                };
                var liveRes = DamagePipeline.Calcular(in reqLive, RandomService.Instancia);
                danio = liveRes.FinalDamage;

                // Aplicar daño real al objetivo ahora (pipeline no muta)
                objetivo.RecibirDanioFisico(danio);
                danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);
            }
            else
            {
                // Legacy
                if (GameplayToggles.PenetracionEnabled && (ejecutor is MiJuegoRPG.Personaje.Personaje pjPen))
                {
                    double pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                    danio = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarFisico(objetivo));
                }
                else
                {
                    danio = ejecutor.AtacarFisico(objetivo);
                }
                danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);
            }

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

            
            // Shadow run nuevo pipeline (no altera gameplay) si está activado en config.
            if (!usandoNuevoLive && combatConfig.UseNewDamagePipelineShadow && danioAplicado > 0 && ejecutor is MiJuegoRPG.Personaje.Personaje pjShadow)
            {
                try
                {
                    var rngShadow = RandomService.Instancia;
                    
                    // Preparar CritChance con DR si procede
                    double critChance = pjShadow.Estadisticas.CritChance;
                    if (combatConfig.UseCritDiminishingReturns)
                    {
                        critChance = CombatBalanceConfig.CritChanceWithDR(
                            critChance,
                            combatConfig.CritChanceHardCap,
                            combatConfig.CritDiminishingK);
                    }
                    var req = new DamagePipeline.Request
                    {
                        Atacante = ejecutor,
                        Objetivo = objetivo,
                        BaseDamage = danio, // legacy considera defensa internamente; aquí usamos el daño base retornado
                        EsMagico = false,
                        PrecisionBase = pjShadow.Estadisticas.Precision,
                        PrecisionExtra = 0,
                        EvasionObjetivo = 0, // sin estadística aún
                        Penetracion = pjShadow.Estadisticas.Penetracion,
                        MitigacionPorcentual = 0, // legacy la aplica dentro del método AtacarFisico
                        CritChance = critChance,
                        CritMultiplier = pjShadow.Estadisticas.CritMult <= 0 ? combatConfig.CritMultiplier : pjShadow.Estadisticas.CritMult,
                        CritScalingFactor = combatConfig.CritScalingFactor,
                        VulnerabilidadMult = 1.0,
                        MinHitClamp = combatConfig.MinHit,
                        ForzarCritico = false,
                        ForzarImpacto = true, // evitar variación por hit chance en shadow hasta tener estadística completa
                        ReducePenetracionEnCritico = combatConfig.ReducePenetracionEnCritico,
                        FactorPenetracionCritico = combatConfig.FactorPenetracionCritico
                    };
                    var nuevo = DamagePipeline.Calcular(in req, rngShadow);

                    // Registrar comparación para ajuste futuro
                    Logger.Debug($"[ShadowDamagePipeline] legacy={danioAplicado} pipeline={nuevo.FinalDamage} base={danio} crit={(nuevo.FueCritico ? 1 : 0)}");
                    shadowAgg.Registrar(danioAplicado, nuevo.FinalDamage);
                }
                catch (System.Exception ex)
                {
                    Logger.Warn($"[ShadowDamagePipeline] Error: {ex.Message}");
                }
            }

            
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
                if (!string.IsNullOrWhiteSpace(detalle))
                {
                    res.Mensajes.Add(detalle);
                }
            }

            return res;
        }

        /// <summary>
        /// Resuelve un ataque mágico delegando el cálculo principal al ejecutor.
        /// Mantiene el daño actual (no altera fórmulas), anota metadatos y mensajes.
        /// </summary>
        public ResultadoAccion ResolverAtaqueMagico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            EnsureConfig();
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

            if (combatConfig.UseNewDamagePipelineShadow && danioAplicado > 0 && ejecutor is MiJuegoRPG.Personaje.Personaje pjShadow)
            {
                try
                {
                    var rngShadowM = RandomService.Instancia;
                    double critChance = pjShadow.Estadisticas.CritChance;
                    if (combatConfig.UseCritDiminishingReturns)
                    {
                        critChance = CombatBalanceConfig.CritChanceWithDR(
                            critChance,
                            combatConfig.CritChanceHardCap,
                            combatConfig.CritDiminishingK);
                    }
                    var req = new DamagePipeline.Request
                    {
                        Atacante = ejecutor,
                        Objetivo = objetivo,
                        BaseDamage = danio,
                        EsMagico = true,
                        PrecisionBase = 1.0,
                        PrecisionExtra = 0,
                        EvasionObjetivo = 0,
                        Penetracion = pjShadow.Estadisticas.Penetracion,
                        MitigacionPorcentual = 0,
                        CritChance = critChance,
                        CritMultiplier = pjShadow.Estadisticas.CritMult <= 0 ? 1.5 : pjShadow.Estadisticas.CritMult,
                        VulnerabilidadMult = 1.0,
                        MinHitClamp = combatConfig.MinHit,
                        ForzarCritico = false,
                        ForzarImpacto = true,
                        ReducePenetracionEnCritico = combatConfig.ReducePenetracionEnCritico,
                        FactorPenetracionCritico = combatConfig.FactorPenetracionCritico
                    };
                    var nuevo = DamagePipeline.Calcular(in req, rngShadowM);
                    Logger.Debug($"[ShadowDamagePipeline][Magico] legacy={danioAplicado} pipeline={nuevo.FinalDamage} base={danio} crit={(nuevo.FueCritico ? 1 : 0)}");
                    shadowAgg.Registrar(danioAplicado, nuevo.FinalDamage);
                }
                catch (System.Exception ex)
                {
                    Logger.Warn($"[ShadowDamagePipeline][Magico] Error: {ex.Message}");
                }
            }

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
                if (!string.IsNullOrWhiteSpace(detalle))
                {
                    res.Mensajes.Add(detalle);
                }
            }

            return res;
        }

        // ObtenerResumenShadow movido arriba para cumplir orden de miembros estáticos
    }
}
