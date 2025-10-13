using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    class CritDummy : ICombatiente
    {
        public string Nombre { get; set; } = "CritDummy";
        public int Vida { get; set; } = 1000;
        public int VidaMaxima { get; set; } = 1000;
        public int Defensa { get; set; } = 0;
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;
        public int AtacarFisico(ICombatiente objetivo)
        {
            return 0;
        }
        public int AtacarMagico(ICombatiente objetivo)
        {
            return 0;
        }
        public void RecibirDanioFisico(int d)
        {
            Vida -= d;
            if (Vida < 0)
                Vida = 0;
        }
        public void RecibirDanioMagico(int d)
        {
            Vida -= d;
            if (Vida < 0)
                Vida = 0;
        }
    }

    public class CritScalingFactorTests
    {
        [Fact]
        public void Critico_Aplica_Solo_Porcion_Extra_Segun_F()
        {
            // Arrange: baseDamage=100, sin defensa ni mitigación.
            // multiplier=1.50, F=0.60 => daño crit = 100 * (1 + (1.5-1)*0.6) = 100 * (1 + 0.5*0.6) = 100 * 1.30 = 130
            var atk = new CritDummy();
            var def = new CritDummy();
            var req = new DamagePipeline.Request
            {
                Atacante = atk,
                Objetivo = def,
                BaseDamage = 100,
                EsMagico = false,
                PrecisionBase = 1.0,
                PrecisionExtra = 0,
                EvasionObjetivo = 0,
                Penetracion = 0,
                MitigacionPorcentual = 0,
                CritChance = 1.0, // forzar crit natural
                CritMultiplier = 1.5,
                CritScalingFactor = 0.60,
                VulnerabilidadMult = 1.0,
                MinHitClamp = 0.01,
                ForzarCritico = true,
                ForzarImpacto = true,
                ReducePenetracionEnCritico = false,
                FactorPenetracionCritico = 1.0
            };

            var rng = RandomService.Instancia;
            rng.SetSeed(12345);
            var res = DamagePipeline.Calcular(in req, rng);

            Assert.True(res.FueCritico);
            Assert.Equal(130, res.FinalDamage);
        }

        [Fact]
        public void CritScalingFactor_Cero_O_Negativo_Fallback_A_1_Completo()
        {
            // F <=0 debe comportarse como F=1 => daño crit = base * mult completo.
            // base 80, mult 1.40 => esperado 112.
            var atk = new CritDummy();
            var def = new CritDummy();
            var req = new DamagePipeline.Request
            {
                Atacante = atk,
                Objetivo = def,
                BaseDamage = 80,
                EsMagico = false,
                PrecisionBase = 1.0,
                PrecisionExtra = 0,
                EvasionObjetivo = 0,
                Penetracion = 0,
                MitigacionPorcentual = 0,
                CritChance = 1.0,
                CritMultiplier = 1.4,
                CritScalingFactor = 0.0, // fuerza fallback
                VulnerabilidadMult = 1.0,
                MinHitClamp = 0.01,
                ForzarCritico = true,
                ForzarImpacto = true,
                ReducePenetracionEnCritico = false,
                FactorPenetracionCritico = 1.0
            };

            var rng = RandomService.Instancia;
            rng.SetSeed(999);
            var res = DamagePipeline.Calcular(in req, rng);
            Assert.True(res.FueCritico);
            Assert.Equal(112, res.FinalDamage); // 80 * 1.4
        }

        [Fact]
        public void Critico_Con_Reduccion_Penetracion_Recalcula_Defensa_Antes_De_Multiplicar()
        {
            // Defensa 40, penetración 50% normal => defEff=20 -> afterDef=80
            // Mitigación 0.
            // Con FactorPenCrit=0.5 => pen efectiva en crítico = 25% => defEffCrit = 30 -> afterDefCrit=70
            // mult=1.5 F=0.5 => crit mult efectivo = 1 + 0.5*0.5 = 1.25 -> 70 * 1.25 = 87.5 => 88
            var atk = new CritDummy();
            var def = new CritDummy { Defensa = 40 };
            var req = new DamagePipeline.Request
            {
                Atacante = atk,
                Objetivo = def,
                BaseDamage = 100,
                EsMagico = false,
                PrecisionBase = 1.0,
                PrecisionExtra = 0,
                EvasionObjetivo = 0,
                Penetracion = 0.50,
                MitigacionPorcentual = 0,
                CritChance = 1.0,
                CritMultiplier = 1.5,
                CritScalingFactor = 0.5,
                VulnerabilidadMult = 1.0,
                MinHitClamp = 0.01,
                ForzarCritico = true,
                ForzarImpacto = true,
                ReducePenetracionEnCritico = true,
                FactorPenetracionCritico = 0.5
            };

            var rng = RandomService.Instancia;
            rng.SetSeed(42);
            var res = DamagePipeline.Calcular(in req, rng);
            Assert.True(res.FueCritico);
            Assert.Equal(88, res.FinalDamage);
        }
    }
}
