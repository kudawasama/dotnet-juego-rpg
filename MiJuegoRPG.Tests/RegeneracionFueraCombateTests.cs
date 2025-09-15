using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class RegeneracionFueraCombateTests
    {
        [Fact]
        public void FueraCombate_RecuperaAlMenosUno_SiNoEstaALltope()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.Estadisticas.Mana = 10;
            pj.ManaActual = 0;
            pj.Estadisticas.RegeneracionMana = 0.0; // usar solo base

            var rec = service.RegenerarManaFueraCombate(pj);

            Assert.True(rec >= 1);
            Assert.Equal(rec, pj.ManaActual);
        }

        [Fact]
        public void FueraCombate_RespetaTopePorTick()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.Estadisticas.Mana = 100;
            pj.ManaActual = 0;
            pj.Estadisticas.RegeneracionMana = 9999; // forzar valor alto

            var rec = service.RegenerarManaFueraCombate(pj);

            // Por defecto, progression.json establece 3.0 como tope
            Assert.InRange(rec, 1, 3);
            Assert.Equal(rec, pj.ManaActual);
        }

        [Fact]
        public void FueraCombate_NoSuperaManaMaximo()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.Estadisticas.Mana = 5; // max 5
            pj.ManaActual = 4;        // a 1 del tope
            pj.Estadisticas.RegeneracionMana = 9999;

            var rec = service.RegenerarManaFueraCombate(pj);

            Assert.InRange(rec, 0, 1);
            Assert.Equal(pj.ManaMaxima, pj.ManaActual);
        }
    }
}
