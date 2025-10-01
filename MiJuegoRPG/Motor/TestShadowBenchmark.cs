using System;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Benchmark sintético para comparar output esperado legacy vs DamagePipeline bajo parámetros controlados.
    /// Legacy actual: daño base -> defensa (con penetración) -> mitigación. NO aplica crítico (solo marca flag).
    /// Pipeline: incluye crítico multiplicando y posible reducción de penetración en crítico.
    /// </summary>
    public static class TestShadowBenchmark
    {
        private class DummyTarget : ICombatiente
        {
            public string Nombre { get; set; } = "Dummy";
            public int Vida { get; set; }
            public int VidaMaxima { get; set; }
            public int Defensa { get; set; }
            public int DefensaMagica { get; set; }
            public bool EstaVivo => Vida > 0;
            public int AtacarFisico(ICombatiente o) => 0;
            public int AtacarMagico(ICombatiente o) => 0;
            public void RecibirDanioFisico(int d) { Vida -= Math.Max(1, d); if (Vida < 0) Vida = 0; }
            public void RecibirDanioMagico(int d) { Vida -= Math.Max(1, d); if (Vida < 0) Vida = 0; }
        }

        /// <summary>
        /// Ejecuta N simulaciones y muestra estadísticos. No altera gameplay real.
        /// </summary>
        // Parámetros con sentinelas: critMult <=0, critScalingF<0 usan config; pen<0 usa default interno (0.20); critChance<0 usa 0.40
        public static void Run(int n, double baseDamage = 100, int defensa = 50, double mitig = 0.10,
            double pen = -1, double critChance = -1, double critMult = -1, bool legacyConCritico = true, double critScalingF = -1)
        {
            if (n <= 0) n = 100;
            var rng = RandomService.Instancia;
            var cfg = CombatConfig.LoadOrDefault();
            if (critMult <= 0) critMult = cfg.CritMultiplier;
            if (critScalingF < 0) critScalingF = cfg.CritScalingFactor;
            if (pen < 0) pen = 0.20; // caso sintético estable
            if (critChance < 0) critChance = 0.40;
            int legacyTotal = 0;
            int pipelineTotal = 0;
            int crits = 0;
            int minPipe = int.MaxValue, maxPipe = 0;
            int minLegacy = int.MaxValue, maxLegacy = 0;
            for (int i = 0; i < n; i++)
            {
                // LEGACY (simplificado según flujo actual sin multiplicador crítico):
                double defEff = defensa * (1.0 - pen);
                if (defEff < 0) defEff = 0;
                double afterDef = baseDamage - defEff;
                if (afterDef < 1) afterDef = 1;
                double legacy = afterDef * (1.0 - mitig);
                if (legacyConCritico)
                {
                    // Aplicar crítico comparable (mismo scaling F) para apples-to-apples
                    double mult = critMult;
                    double f = Math.Clamp(critScalingF, 0.0, 1.0);
                    if (rng.NextDouble() < critChance)
                    {
                        legacy *= (1.0 + (mult - 1.0) * f);
                    }
                }
                if (legacy < 1) legacy = 1;
                int legacyInt = (int)Math.Round(legacy, MidpointRounding.AwayFromZero);
                legacyTotal += legacyInt;
                if (legacyInt < minLegacy) minLegacy = legacyInt;
                if (legacyInt > maxLegacy) maxLegacy = legacyInt;

                // PIPELINE
                double effectiveCritChance = critChance;
                if (cfg.UseCritDiminishingReturns)
                {
                    effectiveCritChance = CombatBalanceConfig.CritChanceWithDR(critChance, cfg.CritChanceHardCap, cfg.CritDiminishingK);
                }
                var req = new DamagePipeline.Request
                {
                    Atacante = null!,
                    Objetivo = new DummyTarget { Defensa = defensa, DefensaMagica = defensa, Vida = 999999, VidaMaxima = 999999 },
                    BaseDamage = (int)baseDamage,
                    EsMagico = false,
                    PrecisionBase = 1.0,
                    PrecisionExtra = 0,
                    EvasionObjetivo = 0,
                    Penetracion = pen,
                    MitigacionPorcentual = mitig,
                    CritChance = effectiveCritChance,
                    CritMultiplier = critMult,
                    VulnerabilidadMult = 1.0,
                    MinHitClamp = 1.0,
                    ForzarCritico = false,
                    ForzarImpacto = true,
                    ReducePenetracionEnCritico = cfg.ReducePenetracionEnCritico,
                    FactorPenetracionCritico = cfg.FactorPenetracionCritico,
                    CritScalingFactor = critScalingF
                };
                var res = DamagePipeline.Calcular(in req, rng);
                pipelineTotal += res.FinalDamage;
                if (res.FueCritico) crits++;
                if (res.FinalDamage < minPipe) minPipe = res.FinalDamage;
                if (res.FinalDamage > maxPipe) maxPipe = res.FinalDamage;
            }

            double avgLegacy = legacyTotal / (double)n;
            double avgPipeline = pipelineTotal / (double)n;
            double diff = avgPipeline - avgLegacy;
            double diffPct = avgLegacy > 0 ? diff / avgLegacy * 100.0 : 0;
            double critRate = crits / (double)n * 100.0;
            Console.WriteLine("--- Shadow Benchmark ---");
            Console.WriteLine($"Config: CritMult={critMult:0.00} F={critScalingF:0.00} PenCritFactor={cfg.FactorPenetracionCritico:0.00} DR={(cfg.UseCritDiminishingReturns ? "ON" : "OFF")}");
            Console.WriteLine($"Muestras: {n}");
            Console.WriteLine($"Legacy Avg: {avgLegacy:0.00}");
            Console.WriteLine($"Pipeline Avg: {avgPipeline:0.00}");
            Console.WriteLine($"Diff: {diff:0.00} ({diffPct:0.0}%)");
            Console.WriteLine($"CritRate Real: {critRate:0.0}%");
            Console.WriteLine($"Legacy   Min={minLegacy} Max={maxLegacy}");
            Console.WriteLine($"Pipeline Min={minPipe} Max={maxPipe}");

            // Threshold PASS/FAIL (por defecto 5%). Permite override via env: SHADOW_BENCH_THRESHOLD=7.5
            double thresholdPct = 5.0;
            var env = Environment.GetEnvironmentVariable("SHADOW_BENCH_THRESHOLD");
            if (!string.IsNullOrWhiteSpace(env) && double.TryParse(env.Replace('%',' ').Trim(), out var t) && t > 0 && t < 100)
                thresholdPct = t;
            bool pass = Math.Abs(diffPct) <= thresholdPct;
            Console.WriteLine($"Threshold: ±{thresholdPct:0.0}% → {(pass ? "PASS" : "FAIL")}");
            if (!pass)
            {
                Console.WriteLine("Sugerencia: ajustar CritMultiplier, penetración efectiva o aplicar normalización en paso crítico para acercar medias.");
            }
        }

        public struct SweepResult { public double F; public double PenCrit; public double AvgLegacy; public double AvgPipeline; public double DiffPct; public double CritRate; }

        public static void Sweep(int n, double baseDamage, int defensa, double mitig, double pen, double critChance, double critMult,
            double[] factors, double[] penCritFactors)
        {
            Console.WriteLine("--- Shadow Benchmark Sweep ---");
            Console.WriteLine($"Muestras por combinación: {n}");
            Console.WriteLine("F\tPenCrit\tLegacyAvg\tPipeAvg\tDiff%\tCritRate%");
            foreach (var f in factors)
            {
                foreach (var penCrit in penCritFactors)
                {
                    // Asegurar config runtime ajustando FactorPenetracionCritico temporalmente
                    var cfg = CombatConfig.LoadOrDefault();
                    cfg.FactorPenetracionCritico = penCrit; // no persistimos
                    // Ejecutar una mini corrida interna reutilizando lógica Run (pero necesitamos forzar config penCrit)
                    // Clon simple del bucle (evitamos complicar Run con overrides locales de config).
                    var rng = RandomService.Instancia;
                    int legacyTotal = 0, pipelineTotal = 0, crits = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double defEff = defensa * (1.0 - pen);
                        if (defEff < 0) defEff = 0;
                        double afterDef = baseDamage - defEff;
                        if (afterDef < 1) afterDef = 1;
                        double legacy = afterDef * (1.0 - mitig);
                        // Legacy con crítico normalizado (apples-to-apples)
                        if (rng.NextDouble() < critChance)
                        {
                            legacy *= (1.0 + (critMult - 1.0) * f);
                        }
                        int legacyInt = (int)Math.Round(legacy, MidpointRounding.AwayFromZero);
                        legacyTotal += legacyInt;

                        double effectiveCritChance = critChance;
                        if (cfg.UseCritDiminishingReturns)
                        {
                            effectiveCritChance = CombatBalanceConfig.CritChanceWithDR(critChance, cfg.CritChanceHardCap, cfg.CritDiminishingK);
                        }
                        var req = new DamagePipeline.Request
                        {
                            Atacante = null!,
                            Objetivo = new DummyTarget { Defensa = defensa, DefensaMagica = defensa, Vida = 999999, VidaMaxima = 999999 },
                            BaseDamage = (int)baseDamage,
                            EsMagico = false,
                            PrecisionBase = 1.0,
                            PrecisionExtra = 0,
                            EvasionObjetivo = 0,
                            Penetracion = pen,
                            MitigacionPorcentual = mitig,
                            CritChance = effectiveCritChance,
                            CritMultiplier = critMult,
                            CritScalingFactor = f,
                            VulnerabilidadMult = 1.0,
                            MinHitClamp = 0.05,
                            ForzarCritico = false,
                            ForzarImpacto = true,
                            ReducePenetracionEnCritico = cfg.ReducePenetracionEnCritico,
                            FactorPenetracionCritico = penCrit
                        };
                        var res = DamagePipeline.Calcular(in req, rng);
                        pipelineTotal += res.FinalDamage;
                        if (res.FueCritico) crits++;
                    }
                    double avgLegacy = legacyTotal / (double)n;
                    double avgPipeline = pipelineTotal / (double)n;
                    double diffPct = avgLegacy > 0 ? (avgPipeline - avgLegacy) / avgLegacy * 100.0 : 0;
                    double critRate = crits / (double)n * 100.0;
                    Console.WriteLine($"{f:0.00}\t{penCrit:0.00}\t{avgLegacy:0.00}\t{avgPipeline:0.00}\t{diffPct:0.0}\t{critRate:0.0}");
                }
            }
        }
    }
}
