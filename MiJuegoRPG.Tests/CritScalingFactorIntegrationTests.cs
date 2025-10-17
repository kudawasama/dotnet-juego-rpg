using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    class CritIntDummy : ICombatiente
    {
        public string Nombre { get; set; } = "CritIntDummy";
        public int Vida { get; set; } = 5000;
        public int VidaMaxima { get; set; } = 5000;
        public int Defensa { get; set; } = 50; // para la prueba
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;
        public int AtacarFisico(ICombatiente objetivo) => 0;
        public int AtacarMagico(ICombatiente objetivo) => 0;
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

    public class CritScalingFactorIntegrationTests
    {
        [Fact]
        public void Pipeline_Completo_Calcula_Danio_Esperado_Con_CritScaling_Vulnerabilidad()
        {
            /* Escenario:
               BaseDamage = 200, Defensa = 50, Penetracion = 25% -> defEff = 50*(1-0.25)=37.5
               AfterDef = 200 - 37.5 = 162.5 -> redondeo away-from-zero = 163
               Mitigacion 20% => 163 * 0.8 = 130.4 (se guarda en Result.AfterMitigacion)
               CritMultiplier = 1.5, F=0.65 => mult efectivo = 1 + (0.5*0.65) = 1.325
               AfterCrit = 130.4 * 1.325 = 172.78
               Vulnerabilidad 1.3 => 172.78 * 1.3 = 224.614 -> redondeo away-from-zero = 225
            */
            var atk = new CritIntDummy { Defensa = 0 }; // atacante sin defensa relevante
            var def = new CritIntDummy();
            var req = new DamagePipeline.Request
            {
                Atacante = atk,
                Objetivo = def,
                BaseDamage = 200,
                EsMagico = false,
                PrecisionBase = 1.0,
                PrecisionExtra = 0,
                EvasionObjetivo = 0,
                Penetracion = 0.25,
                MitigacionPorcentual = 0.20,
                CritChance = 1.0,
                CritMultiplier = 1.5,
                CritScalingFactor = 0.65,
                VulnerabilidadMult = 1.3,
                MinHitClamp = 0.01,
                ForzarCritico = true,
                ForzarImpacto = true,
                ReducePenetracionEnCritico = false,
                FactorPenetracionCritico = 1.0
            };

            var rng = RandomService.Instancia;
            rng.SetSeed(123);
            var res = DamagePipeline.Calcular(in req, rng);

            Assert.True(res.FueCritico);
            Assert.Equal(163, res.AfterDefensa); // paso defensa redondeado
            Assert.Equal(130.4, res.AfterMitigacion, 5); // valor antes de crítico y vulnerabilidad
            Assert.Equal(225, res.FinalDamage); // resultado final esperado
        }
    }
}
