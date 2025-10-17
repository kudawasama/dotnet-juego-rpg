using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Enemigos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    // Atacante con penetración configurable
    class PenCaster : ICombatiente
    {
        public string Nombre { get; set; } = "PenCaster";
        public int Vida { get; set; } = 100;
        public int VidaMaxima { get; set; } = 100;
        public int Defensa { get; set; } = 0;
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;

        public int AtacarFisico(ICombatiente objetivo)
        {
            objetivo.RecibirDanioFisico(100);
            return 100;
        }
        public int AtacarMagico(ICombatiente objetivo)
        {
            objetivo.RecibirDanioMagico(100);
            return 100;
        }
        public void RecibirDanioFisico(int d)
        {
            int real = d - Defensa;
            if (real < 1)
                real = 1;
            Vida = System.Math.Max(0, Vida - real);
        }
        public void RecibirDanioMagico(int d)
        {
            int real = d - DefensaMagica;
            if (real < 1)
                real = 1;
            Vida = System.Math.Max(0, Vida - real);
        }
    }

    public class PenetracionPipelineTests
    {
        [Fact]
        public void Fisico_Aplica_Penetracion_En_Defensa_Antes_Mitigacion()
        {
            // Arrange: defensa 30, mitigación 10%, daño 100, pen 20% ->
            // defEff = 30*(1-0.2)=24 -> (100-24)=76 -> *0.9 = 68.4 -> 68
            GameplayToggles.PenetracionEnabled = true;
            var enemigo = new EnemigoEstandar("Dummy", 1, 0, 30, 0, 0, 0, 0) { Vida = 1000, VidaMaxima = 1000 };
            enemigo.MitigacionFisicaPorcentaje = 0.10;
            var caster = new PenCaster();

            // Inyectar pen 20% via contexto usando resolver
            var resolver = new DamageResolver();
            int vidaAntes = enemigo.Vida;
            // simulamos Personaje con pen usando el contexto directamente
            CombatAmbientContext.WithPenetracion(0.20, () => { caster.AtacarFisico(enemigo); return 0; });
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(68, aplicado);
        }

        [Fact]
        public void Magico_Aplica_Penetracion_Antes_De_Resistencias_Y_Vulnerabilidad()
        {
            // defMag 20, res 30%, vuln 1.2, pen 25% -> defEff=15 -> (100-15)=85 -> *0.7=59.5 -> *1.2=71.4 -> 71
            GameplayToggles.PenetracionEnabled = true;
            var enemigo = new EnemigoEstandar("Dummy", 1, 0, 0, 20, 0, 0, 0) { Vida = 1000, VidaMaxima = 1000 };
            enemigo.EstablecerMitigacionElemental("magia", 0.30);
            enemigo.EstablecerVulnerabilidadElemental("magia", 1.2);
            var caster = new PenCaster();

            int vidaAntes = enemigo.Vida;
            CombatAmbientContext.WithPenetracion(0.25, () => { caster.AtacarMagico(enemigo); return 0; });
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(71, aplicado);
        }

        [Fact]
        public void Toggle_Off_No_Aplica_Penetracion()
        {
            // Misma configuración que primera prueba, pero sin toggle: (100-30)=70 -> *0.9=63
            GameplayToggles.PenetracionEnabled = false;
            var enemigo = new EnemigoEstandar("Dummy", 1, 0, 30, 0, 0, 0, 0) { Vida = 1000, VidaMaxima = 1000 };
            enemigo.MitigacionFisicaPorcentaje = 0.10;
            var caster = new PenCaster();

            int vidaAntes = enemigo.Vida;
            caster.AtacarFisico(enemigo);
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(63, aplicado);
        }
    }
}
