using MiJuegoRPG.Interfaces;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class EfectosCombateTests
    {
        class Dummy : ICombatiente
        {
            public string Nombre { get; set; } = "Dummy";
            public int Vida { get; set; } = 30;
            public int VidaMaxima { get; set; } = 30;
            public int Defensa { get; set; } = 0;
            public int DefensaMagica { get; set; } = 0;
            public bool EstaVivo => Vida > 0;
            public int AtacarFisico(ICombatiente objetivo) { objetivo.RecibirDanioFisico(1); return 1; }
            public int AtacarMagico(ICombatiente objetivo) { objetivo.RecibirDanioMagico(1); return 1; }
            public void RecibirDanioFisico(int d) { Vida = System.Math.Max(0, Vida - d); }
            public void RecibirDanioMagico(int d) { Vida = System.Math.Max(0, Vida - d); }
        }

        [Fact]
        public void EfectoVeneno_TickReduceVida_YExpira()
        {
            var objetivo = new Dummy { Vida = 20, VidaMaxima = 30 };
            var veneno = new MiJuegoRPG.Motor.Acciones.EfectoVeneno(danioPorTurno: 3, duracionTurnos: 2);

            // Turno 1
            foreach (var _ in veneno.Tick(objetivo)) { }
            Assert.Equal(17, objetivo.Vida);
            Assert.True(veneno.AvanzarTurno());

            // Turno 2
            foreach (var _ in veneno.Tick(objetivo)) { }
            Assert.Equal(14, objetivo.Vida);
            Assert.False(veneno.AvanzarTurno());
        }
    }
}
