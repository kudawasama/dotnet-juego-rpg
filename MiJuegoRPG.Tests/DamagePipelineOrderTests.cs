using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Enemigos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    // Atacante mínimo que delega en los métodos del objetivo para aplicar daño
    class FlatCaster : ICombatiente
    {
        public string Nombre { get; set; } = "Caster";
        public int Vida { get; set; } = 100;
        public int VidaMaxima { get; set; } = 100;
        public int Defensa { get; set; } = 0;
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;

        public int AtacarFisico(ICombatiente objetivo)
        {
            // daño físico plano 100 para hacer visibles los pasos de mitigación
            objetivo.RecibirDanioFisico(100);
            return 100;
        }

        public int AtacarMagico(ICombatiente objetivo)
        {
            // daño mágico plano 100 para facilitar cálculos de porcentaje
            objetivo.RecibirDanioMagico(100);
            return 100;
        }

        public void RecibirDanioFisico(int danioFisico)
        {
            int real = danioFisico - Defensa;
            if (real < 1) real = 1;
            Vida -= real;
            if (Vida < 0) Vida = 0;
        }

        public void RecibirDanioMagico(int danioMagico)
        {
            int real = danioMagico - DefensaMagica;
            if (real < 1) real = 1;
            Vida -= real;
            if (Vida < 0) Vida = 0;
        }
    }

    public class DamagePipelineOrderTests
    {
        [Fact]
        public void Magico_Aplica_Defensa_Mitigacion_Resistencia_Vulnerabilidad()
        {
            // Arrange: enemigo con defensa mágica 10, mitigación 20%, resistencia 30% a magia, vulnerabilidad 1.5x a magia
            // Orden esperado con input 100:
            // 1) Defensa: 100 - 10 = 90
            // 2) Mitigación: 90 * (1 - 0.20) = 72
            // 3) Resistencia: 72 * (1 - 0.30) = 50.4 -> 50 (redondeo away from zero en código)
            // 4) Vulnerabilidad: 50 * 1.5 = 75
            var enemigo = new EnemigoEstandar("Dummy", vidaBase: 1, ataqueBase: 0, defensaBase: 0, defensaMagicaBase: 10, nivel: 0, experienciaRecompensa: 0, oroRecompensa: 0);
            enemigo.MitigacionMagicaPorcentaje = 0.20;
            enemigo.EstablecerMitigacionElemental("magia", 0.30);
            enemigo.EstablecerVulnerabilidadElemental("magia", 1.5);
            enemigo.Vida = enemigo.VidaMaxima = 1000; // alto para evitar muerte prematura

            var caster = new FlatCaster { Nombre = "Mage" };
            var resolver = new DamageResolver();

            int vidaAntes = enemigo.Vida;
            var res = resolver.ResolverAtaqueMagico(caster, enemigo);
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal("Ataque Mágico", res.NombreAccion);
            Assert.Equal(aplicado, res.DanioReal);
            Assert.Equal(75, res.DanioReal); // coincide con el orden esperado
            Assert.False(res.FueEvadido);
        }

        [Fact]
        public void Fisico_Aplica_Defensa_Luego_Mitigacion()
        {
            // Input 100; defensa 15; mitigación 10% -> (100-15)=85; 85*(1-0.10)=76.5 -> 77
            var enemigo = new EnemigoEstandar("Dummy", vidaBase: 1, ataqueBase: 0, defensaBase: 15, defensaMagicaBase: 0, nivel: 0, experienciaRecompensa: 0, oroRecompensa: 0);
            enemigo.MitigacionFisicaPorcentaje = 0.10;
            enemigo.Vida = enemigo.VidaMaxima = 1000;

            var caster = new FlatCaster { Nombre = "Guerrero" };
            var resolver = new DamageResolver();

            int vidaAntes = enemigo.Vida;
            var res = resolver.ResolverAtaqueFisico(caster, enemigo);
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(aplicado, res.DanioReal);
            Assert.Equal(77, res.DanioReal);
            Assert.False(res.FueEvadido);
        }

        [Fact]
        public void Magico_SinVulnerabilidad_Aplica_Defensa_Mitigacion_Resistencia()
        {
            // 100 -> (def 5) 95 -> (mit 25%) 71.25 -> (res 50%) 35.625 -> 36
            var enemigo = new EnemigoEstandar("Dummy", vidaBase: 1, ataqueBase: 0, defensaBase: 0, defensaMagicaBase: 5, nivel: 0, experienciaRecompensa: 0, oroRecompensa: 0);
            enemigo.MitigacionMagicaPorcentaje = 0.25;
            enemigo.EstablecerMitigacionElemental("magia", 0.50);
            enemigo.Vida = enemigo.VidaMaxima = 1000;

            var caster = new FlatCaster();
            var resolver = new DamageResolver();

            int vidaAntes = enemigo.Vida;
            var res = resolver.ResolverAtaqueMagico(caster, enemigo);
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(aplicado, res.DanioReal);
            Assert.Equal(36, res.DanioReal);
            Assert.False(res.FueEvadido);
        }

        [Fact]
        public void Magico_SoloVulnerabilidad_Aplica_Defensa_Y_Vulnerabilidad()
        {
            // Sin mitigación ni resistencia elemental; con vulnerabilidad 1.2x
            // 100 -> (def 20) 80 -> *1.2 = 96
            var enemigo = new EnemigoEstandar("Dummy", vidaBase: 1, ataqueBase: 0, defensaBase: 0, defensaMagicaBase: 20, nivel: 0, experienciaRecompensa: 0, oroRecompensa: 0);
            enemigo.EstablecerVulnerabilidadElemental("magia", 1.2);
            enemigo.Vida = enemigo.VidaMaxima = 1000;

            var caster = new FlatCaster();
            var resolver = new DamageResolver();

            int vidaAntes = enemigo.Vida;
            var res = resolver.ResolverAtaqueMagico(caster, enemigo);
            int aplicado = vidaAntes - enemigo.Vida;

            Assert.Equal(aplicado, res.DanioReal);
            Assert.Equal(96, res.DanioReal);
            Assert.False(res.FueEvadido);
        }
    }
}
