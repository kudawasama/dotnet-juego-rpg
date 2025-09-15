using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Motor.Acciones;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class RecursosAccionesTests
    {
        [Fact]
        public void TieneRecursos_DeberiaFallar_SiNoHayManaSuficiente()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.ManaActual = 0; // sin maná
            var accion = new AtaqueMagicoAccion(); // costo 5

            var ok = service.TieneRecursos(pj, accion, out var mensaje);

            Assert.False(ok);
            Assert.Equal("No tienes maná suficiente.", mensaje);
        }

        [Fact]
        public void TieneRecursos_DeberiaPasar_ConManaSuficiente()
        {
            var service = new ActionRulesService();
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            pj.ManaActual = 100; // suficiente maná
            var accion = new AtaqueMagicoAccion(); // costo 5

            var ok = service.TieneRecursos(pj, accion, out var mensaje);

            Assert.True(ok);
            Assert.True(string.IsNullOrEmpty(mensaje));
        }
    }
}
