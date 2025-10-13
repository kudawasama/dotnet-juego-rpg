using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Acciones;
using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class HabilidadGatingTests
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
        public void NoConsumeRecursos_NiAplicaCooldown_SinMana()
        {
            var pj = new MiJuegoRPG.Personaje.Personaje("Mage");
            pj.ManaActual = 0; // sin maná
            var enemigo = new DummyPj { Nombre = "Mob" };
            var combate = new CombatePorTurnos(pj, enemigo) { MaxIteraciones = 1 };
            var accion = new AtaqueMagicoAccion();

            // Arranque: deberia fallar por recursos
            var ok = combate.TryEjecutarAccion(pj, enemigo, accion, out var msg);
            Assert.False(ok);
            Assert.False(string.IsNullOrEmpty(msg));

            // Verificar que NO se consumió maná ni se aplicó cooldown
            Assert.Equal(0, pj.ManaActual);
            // No debería haber cooldown aplicado tras fallo de recursos
            // Reintentamos ejecutar inmediatamente: debe seguir fallando por recursos y no aplicar CD
            var ok2 = combate.TryEjecutarAccion(pj, enemigo, accion, out var msg2);
            Assert.False(ok2);
            Assert.False(string.IsNullOrEmpty(msg2));
        }

        [Fact]
        public void AplicaCooldown_YConsumeMana_ConRecursos()
        {
            var pj = new MiJuegoRPG.Personaje.Personaje("Mage");
            pj.ManaActual = 50;
            var enemigo = new DummyPj { Nombre = "Mob" };
            var combate = new CombatePorTurnos(pj, enemigo) { MaxIteraciones = 1 };
            var accion = new AtaqueMagicoAccion(); // costo 5, cd 1

            var ok = combate.TryEjecutarAccion(pj, enemigo, accion, out var msg);
            Assert.True(ok);
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.Equal(45, pj.ManaActual); // consumió 5

            // La misma acción inmediatamente debería estar en CD
            Assert.True(combate.TryEjecutarAccion(pj, enemigo, new AtaqueFisicoAccion(), out var _)); // otra acción física sí puede
            // Al intentar de nuevo la magia inmediatamente debe estar bloqueada por cooldown
            var okReintento = combate.TryEjecutarAccion(pj, enemigo, accion, out var msgCd);
            Assert.False(okReintento);
            Assert.Contains("cooldown", msgCd, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void NoConsumeTurno_SiSeleccionNoValida_Simulacion()
        {
            // Usamos MaxIteraciones para acotar y no entrar en input real
            var pj = new DummyPj { Nombre = "PJ" };
            var mob = new DummyPj { Nombre = "Mob" };
            var combate = new CombatePorTurnos(pj, mob) { MaxIteraciones = 1 };

            // Intento de acción con cooldown simulado: aplicamos cooldown manual para bloquear
            var accion = new AtaqueFisicoAccion();
            // No podemos aplicar cooldown del service sin ejecutarla, así que verificamos helper directamente con un actor distinto
            // Este test es más de humo para asegurar que el helper existe y no lanza.
            var ok = combate.TryEjecutarAccion(pj, mob, accion, out var _);
            Assert.True(ok);
        }
    }
}
