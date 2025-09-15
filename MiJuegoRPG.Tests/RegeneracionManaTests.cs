using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class RegeneracionManaTests
    {
        [Fact]
        public void RegenerarManaTurno_AcumulaFraccionYEntregaEnteros()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            // Preparar un personaje con ManaMaxima > 0 y ManaActual bajo
            pj.Estadisticas.Mana = 10; // => ManaMaxima = 10
            pj.ManaActual = 0;
            // Forzar una estadística de regeneración baja para ver fracciones
            pj.Estadisticas.RegeneracionMana = 0.0; // regen = base (0.2) => 5 turnos ~ 1 punto

            int totalRec = 0;
            for (int i = 0; i < 5; i++)
            {
                totalRec += service.RegenerarManaTurno(pj);
            }

            Assert.Equal(1, totalRec); // 0.2 * 5 = 1.0 → debe sumar 1 punto en total
            Assert.Equal(1, pj.ManaActual);
        }

        [Fact]
        public void RegenerarManaTurno_RespetaTopePorTurno()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.Estadisticas.Mana = 100; // tope alto
            pj.ManaActual = 0;
            // Poner una regeneración alta para que el cálculo crudo supere el tope
            pj.Estadisticas.RegeneracionMana = 9999; // regen cruda enorme → debe clamp a MaxPorTurno (1.0)

            var rec = service.RegenerarManaTurno(pj);
            Assert.True(rec <= 1);
            Assert.Equal(rec, pj.ManaActual);
        }

        [Fact]
        public void RegenerarManaTurno_NoSuperaManaMaximo()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            // Forzar estado cercano al máximo
            pj.Estadisticas.Mana = 5; // => ManaMaxima = 5
            pj.ManaActual = pj.ManaMaxima - 1; // 4
            // Forzar que este turno otorgue 1 para llegar al máximo
            pj.Estadisticas.RegeneracionMana = 9999;

            // Regenerar, a lo sumo debería ganar 1
            var rec = service.RegenerarManaTurno(pj);
            Assert.InRange(rec, 0, 1);
            Assert.Equal(pj.ManaMaxima, pj.ManaActual);
        }
    }
}
