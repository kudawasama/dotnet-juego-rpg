using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Motor.Servicios;
using Xunit;
using PersonajeModel = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Tests
{
    public class CritPenetracionInteractionTests
    {
        [Fact]
        public void Fisico_Critico_Forzado_Con_Penetracion_Aplica_Orden_Correcto()
        {
            // Arrange
            GameplayToggles.PrecisionCheckEnabled = false; // no queremos fallo por precisión
            GameplayToggles.PenetracionEnabled = true;

            var pj = new PersonajeModel("PJ");
            // Forzar crítico
            pj.Estadisticas.CritChance = 1.0; // fuerza crítico en resolver
            // Penetración 20%
            pj.Estadisticas.Penetracion = 0.20;
            // Asegurar daño base 100 para facilidad
            pj.AtributosBase.Fuerza = 100;
            pj.Estadisticas.Ataque = 0;

            var objetivo = new EnemigoEstandar("Dummy", 1, 0, 30, 0, 0, 0, 0) { Vida = 1000, VidaMaxima = 1000 };
            objetivo.MitigacionFisicaPorcentaje = 0.10;

            var resolver = new DamageResolver();
            var res = resolver.ResolverAtaqueFisico(pj, objetivo);

            // Assert: DanioReal = 68 (como en pruebas de penetración) y crítico marcado
            Assert.Equal(68, res.DanioReal);
            Assert.True(res.FueCritico);
        }

        [Fact]
        public void Magico_Critico_Forzado_Con_Penetracion_Aplica_Orden_Correcto()
        {
            // Arrange
            GameplayToggles.PenetracionEnabled = true;

            var pj = new PersonajeModel("Mage");
            pj.Estadisticas.CritChance = 1.0; // crítico forzado
            pj.Estadisticas.Penetracion = 0.25;
            // daño mágico efectivo ~100 (Inteligencia + PoderMágico), fijamos Inteligencia=100
            pj.AtributosBase.Inteligencia = 100;
            pj.Estadisticas.PoderMagico = 0;

            var objetivo = new EnemigoEstandar("Mob", 1, 0, 0, 20, 0, 0, 0) { Vida = 1000, VidaMaxima = 1000 };
            objetivo.EstablecerMitigacionElemental("magia", 0.30);
            objetivo.EstablecerVulnerabilidadElemental("magia", 1.2);

            var resolver = new DamageResolver();
            var res = resolver.ResolverAtaqueMagico(pj, objetivo);

            // Assert: DanioReal = 71 (mismo caso de penetración mágica) y crítico marcado
            Assert.Equal(71, res.DanioReal);
            Assert.True(res.FueCritico);
        }
    }
}
