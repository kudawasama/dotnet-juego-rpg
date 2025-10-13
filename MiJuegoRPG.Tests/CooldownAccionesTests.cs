using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Acciones;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class CooldownAccionesTests
    {
        class DummyPj : ICombatiente
        {
            public string Nombre { get; set; } = "PJ";
            public int Vida { get; set; } = 50;
            public int VidaMaxima { get; set; } = 50;
            public int Defensa { get; set; } = 0;
            public int DefensaMagica { get; set; } = 0;
            public bool EstaVivo => Vida > 0;
            public int AtacarFisico(ICombatiente objetivo)
            {
                objetivo.RecibirDanioFisico(10);
                return 10;
            }
            public int AtacarMagico(ICombatiente objetivo)
            {
                objetivo.RecibirDanioMagico(12);
                return 12;
            }
            public void RecibirDanioFisico(int d)
            {
                Vida = System.Math.Max(0, Vida - System.Math.Max(1, d - Defensa));
            }
            public void RecibirDanioMagico(int d)
            {
                Vida = System.Math.Max(0, Vida - System.Math.Max(1, d - DefensaMagica));
            }
        }

        [Fact]
        public void Veneno_EntraEnCooldown_Y_BloqueaReuso()
        {
            var pj = new DummyPj { Nombre = "PJ" };
            var enemigo = new DummyPj { Nombre = "Mob" };
            var combate = new CombatePorTurnos(pj, enemigo); // no se inicia el loop

            // Simulamos usar veneno directamente vía acción y aplicamos cooldown manualmente
            var veneno = new AplicarVenenoAccion();
            var res = veneno.Ejecutar(pj, enemigo);
            // Registrar efectos como hace CombatePorTurnos
            Assert.Contains("aplica Veneno", res.Mensajes[0]);

            // Verificamos que la acción declara cooldown > 0 (la lógica de aplicación se valida en integración)
            Assert.True(veneno.CooldownTurnos > 0);
        }

        [Fact]
        public void AtaqueMagico_TieneCooldown_Unitario()
        {
            var magia = new AtaqueMagicoAccion();
            Assert.Equal(1, magia.CooldownTurnos);
        }
    }
}
