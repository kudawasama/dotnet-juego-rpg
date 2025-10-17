using System;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Pipeline de daño determinista (Fase 2 - aislado, no integrado aún al combate legacy).
    /// Orden: Base -> Hit/Evasión -> Penetración -> Defensa -> Mitigación% -> Crítico -> Vulnerabilidad -> Redondeo/Mínimo.
    /// No muta objetivos (solo calcula); aplicar daño real es responsabilidad externa.
    /// </summary>
    public static class DamagePipeline
    {
        public struct Request
        {
            public ICombatiente Atacante;
            public ICombatiente Objetivo;
            public int BaseDamage;              // DB ya calculado (Weapon + stats)
            public bool EsMagico;               // Selecciona defensa/mags
            public double PrecisionBase;        // Ej: 0.90
            public double PrecisionExtra;       // Stats precision (0..1)
            public double EvasionObjetivo;      // 0..1
            public double Penetracion;          // 0..1
            public double MitigacionPorcentual; // 0..1 (después de defensa)
            public double CritChance;           // 0..1
            public double CritMultiplier;       // >=1 (multiplicador bruto)
            public double CritScalingFactor;    // 0..1 factor de normalización (F) aplicado solo a la porción extra
            public double VulnerabilidadMult;   // >=1 (1 si no aplica)
            public double MinHitClamp;          // p.ej. 0.05
            public bool ForzarCritico;          // Para pruebas
            public bool ForzarImpacto;          // Para pruebas
            public bool ReducePenetracionEnCritico; // Si true aplica factor a penetración en golpes críticos
            public double FactorPenetracionCritico; // 0..1
        }

        public struct Result
        {
            public int FinalDamage;
            public bool FueEvadido;
            public bool FueCritico;
            public double HitChanceUtilizada;
            public double DefensaEfectiva;
            public int AfterDefensa;
            public double AfterMitigacion;
        }

        public static Result Calcular(in Request req, RandomService rng)
        {
            var res = new Result
            {
                FinalDamage = 0,
                FueEvadido = false,
                FueCritico = false,
                HitChanceUtilizada = 0,
                DefensaEfectiva = 0,
                AfterDefensa = 0,
                AfterMitigacion = 0
            };

            // 1. BaseDamage sanity
            int db = Math.Max(0, req.BaseDamage);
            if (db <= 0)
            {
                res.FinalDamage = 0;
                return res;
            }

            // 2. Hit / Evasión
            double hitChance = req.PrecisionBase + req.PrecisionExtra - req.EvasionObjetivo;
            hitChance = Math.Clamp(hitChance, req.MinHitClamp, 1.0);
            res.HitChanceUtilizada = hitChance;
            if (!req.ForzarImpacto)
            {
                if (rng.NextDouble() >= hitChance)
                {
                    res.FueEvadido = true; // fail
                    return res;
                }
            }

            // 3. Penetración sobre defensa efectiva
            int defensaBase = req.EsMagico ? req.Objetivo.DefensaMagica : req.Objetivo.Defensa;
            double defEff = defensaBase * (1.0 - Math.Clamp(req.Penetracion, 0.0, 1.0));
            if (defEff < 0)
                defEff = 0;
            res.DefensaEfectiva = defEff;

            // 4. Resta defensa
            double afterDef = db - defEff;
            if (afterDef < 1)
                afterDef = 1; // mínimo si impactó
            res.AfterDefensa = (int)Math.Round(afterDef, MidpointRounding.AwayFromZero);

            // 5. Mitigación porcentual
            double afterMit = res.AfterDefensa * (1.0 - Math.Clamp(req.MitigacionPorcentual, 0.0, 0.99));
            res.AfterMitigacion = afterMit;

            // 6. Crítico
            bool esCrit = req.ForzarCritico || (rng.NextDouble() < Math.Clamp(req.CritChance, 0.0, 0.99));
            res.FueCritico = esCrit;
            if (esCrit)
            {
                // Ajustar penetración si corresponde (recalcular defensa efectiva diferencial solo para componente penetrada)
                if (req.ReducePenetracionEnCritico && req.FactorPenetracionCritico < 1.0 && req.Penetracion > 0)
                {
                    double penReducida = Math.Clamp(req.Penetracion * Math.Clamp(req.FactorPenetracionCritico, 0.0, 1.0), 0.0, 1.0);
                    int defensaBaseCrit = req.EsMagico ? req.Objetivo.DefensaMagica : req.Objetivo.Defensa;
                    double defEffCrit = defensaBaseCrit * (1.0 - penReducida);
                    if (defEffCrit < 0)
                        defEffCrit = 0;
                    double afterDefCrit = db - defEffCrit;
                    if (afterDefCrit < 1)
                        afterDefCrit = 1;
                    double afterMitCrit = afterDefCrit * (1.0 - Math.Clamp(req.MitigacionPorcentual, 0.0, 0.99));
                    afterMit = afterMitCrit; // reemplaza la base previa antes de crítico
                }
                // Aplicar multiplicador crítico normalizado: base + (extra * F)
                double mult = Math.Max(1.0, req.CritMultiplier);
                double f = Math.Clamp(req.CritScalingFactor <= 0 ? 1.0 : req.CritScalingFactor, 0.0, 1.0);
                // Daño crítico = afterMit * (1 + (mult-1)*f)
                afterMit *= 1.0 + ((mult - 1.0) * f);
            }

            // 7. Vulnerabilidad / Elemental
            if (req.VulnerabilidadMult > 1.0)
            {
                afterMit *= req.VulnerabilidadMult;
            }

            // 8. Redondeo y mínimo final
            int final = (int)Math.Round(afterMit, MidpointRounding.AwayFromZero);
            if (final < 1)
                final = 1;
            res.FinalDamage = final;
            return res;
        }
    }
}
