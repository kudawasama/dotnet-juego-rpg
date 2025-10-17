using MiJuegoRPG.Interfaces;
using Xunit;

namespace MiJuegoRPG.Tests
{
    // Combatiente de prueba determinista
    class DummyFighter : ICombatiente
    {
        public string Nombre { get; set; } = "Dummy";
        public int Vida { get; set; } = 50;
        public int VidaMaxima { get; set; } = 50;
        public int Defensa { get; set; } = 0;
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;

        public int AtacarFisico(ICombatiente objetivo)
        {
            // Daño plano 10
            objetivo.RecibirDanioFisico(10);
            return 10;
        }

        public int AtacarMagico(ICombatiente objetivo)
        {
            // Daño plano 12
            objetivo.RecibirDanioMagico(12);
            return 12;
        }

        public void RecibirDanioFisico(int danioFisico)
        {
            int real = danioFisico - Defensa;
            if (real < 1)
                real = 1;
            Vida -= real;
            if (Vida < 0)
                Vida = 0;
        }

        public void RecibirDanioMagico(int danioMagico)
        {
            int real = danioMagico - DefensaMagica;
            if (real < 1)
                real = 1;
            Vida -= real;
            if (Vida < 0)
                Vida = 0;
        }
    }

    // Objetivo evasivo determinista para probar 'miss' sin RNG
    class EvasivoDummy : ICombatiente, IEvadible
    {
        public string Nombre { get; set; } = "Evasivo";
        public int Vida { get; set; } = 50;
        public int VidaMaxima { get; set; } = 50;
        public int Defensa { get; set; } = 0;
        public int DefensaMagica { get; set; } = 0;
        public bool EstaVivo => Vida > 0;

        public bool IntentarEvadir(bool esAtaqueMagico) => true; // siempre evade
        public int AtacarFisico(ICombatiente objetivo)
        {
            objetivo.RecibirDanioFisico(1);
            return 1;
        }
        public int AtacarMagico(ICombatiente objetivo)
        {
            objetivo.RecibirDanioMagico(1);
            return 1;
        }
        public void RecibirDanioFisico(int danioFisico)
        {
            Vida = System.Math.Max(0, Vida - danioFisico);
        }
        public void RecibirDanioMagico(int danioMagico)
        {
            Vida = System.Math.Max(0, Vida - danioMagico);
        }
    }

    public class AccionesCombateTests
    {
        [Fact]
        public void AtaqueFisicoAccion_AplicaDanoYMensajes()
        {
            var atacante = new DummyFighter { Nombre = "Atacante" };
            var objetivo = new DummyFighter { Nombre = "Objetivo" };
            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();

            var res = accion.Ejecutar(atacante, objetivo);

            Assert.Equal("Ataque Físico", res.NombreAccion);
            Assert.Equal(atacante, res.Ejecutor);
            Assert.Equal(objetivo, res.Objetivo);
            Assert.True(res.DanioReal >= 1);
            Assert.Contains("Atacante", res.Mensajes[0]);
            Assert.Contains("Objetivo", res.Mensajes[0]);
            Assert.Equal(40, objetivo.Vida); // 50 - 10
        }

        [Fact]
        public void AtaqueMagicoAccion_AplicaDanoYMarcaMagico()
        {
            var atacante = new DummyFighter { Nombre = "Mage" };
            var objetivo = new DummyFighter { Nombre = "Goblin" };
            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion();

            var res = accion.Ejecutar(atacante, objetivo);

            Assert.Equal("Ataque Mágico", res.NombreAccion);
            Assert.True(res.EsMagico);
            Assert.Equal(38, objetivo.Vida); // 50 - 12
            Assert.Contains("Ataque Mágico", res.Mensajes[0]);
            Assert.Contains("12", res.Mensajes[0]);
        }

        [Fact]
        public void AtaqueMagicoAccion_UsaResolver_CalculaDanioRealPorDeltaDeVida()
        {
            var atacante = new DummyFighter { Nombre = "Mage" };
            var objetivo = new DummyFighter { Nombre = "Mob" };
            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion();

            int vidaAntes = objetivo.Vida;
            var res = accion.Ejecutar(atacante, objetivo);
            int esperado = vidaAntes - objetivo.Vida;

            Assert.Equal(esperado, res.DanioReal);
            Assert.False(res.FueEvadido);
            Assert.False(res.FueCritico); // DummyFighter no activa crítico del resolver
        }

        [Fact]
        public void AtaqueFisicoAccion_NoCriticoConDummy_NoEvasion()
        {
            var atacante = new DummyFighter { Nombre = "A" };
            var objetivo = new DummyFighter { Nombre = "B" };
            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();

            var res = accion.Ejecutar(atacante, objetivo);

            // DummyFighter no es Personaje, por lo tanto el resolver no evalúa crítico (pCrit=0)
            Assert.False(res.FueCritico);
            // No hay evasión modelada en DummyFighter, por lo que la bandera debe mantenerse falsa
            Assert.False(res.FueEvadido);
        }

        [Fact]
        public void AtaqueFisico_PersonajeContraEvasivo_NoHaceDano()
        {
            // Arrange: Personaje atacante y objetivo que siempre evade
            var pj = new MiJuegoRPG.Personaje.Personaje("Heroe");
            var objetivo = new EvasivoDummy();
            int vidaAntes = objetivo.Vida;

            // Act: usar la acción de ataque físico que llama al resolver (que delega en AtacarFisico del ejecutor)
            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
            var res = accion.Ejecutar(pj, objetivo);

            // Assert: por evasión, no se aplica daño (vida igual) y el resolver marca evasión
            Assert.Equal(vidaAntes, objetivo.Vida);
            Assert.True(res.DanioReal >= 0);
            Assert.True(res.FueEvadido);
        }

        [Fact]
        public void AtaqueFisico_PrecisionToggle_AlFallarNoHayDano()
        {
            // Activar chequeo de precisión global
            GameplayToggles.PrecisionCheckEnabled = true;
            try
            {
                // Personaje con precisión 0 garantiza fallo
                var pj = new MiJuegoRPG.Personaje.Personaje("Heroe");
                pj.Estadisticas.Precision = 0.0; // forzar fallo
                var objetivo = new DummyFighter { Nombre = "Target" };
                int vidaAntes = objetivo.Vida;

                // Semilla determinista (por si otros RNGs aparecen en el futuro)
                MiJuegoRPG.Motor.Servicios.RandomService.Instancia.SetSeed(123);

                var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                var res = accion.Ejecutar(pj, objetivo);

                Assert.Equal(vidaAntes, objetivo.Vida);
                Assert.True(res.FueEvadido); // se marca como evadido/fallo
                Assert.Equal(0, res.DanioReal);
            }
            finally
            {
                // Desactivar para no impactar otras pruebas
                GameplayToggles.PrecisionCheckEnabled = false;
            }
        }

        [Fact]
        public void AtaqueFisico_CriticoForzado_SeMarcaCritico()
        {
            // Arrange
            var pj = new MiJuegoRPG.Personaje.Personaje("CritHero");
            // Forzar crítico: CritChance >= 1.0 hará pCrit=1.0 en resolver
            pj.Estadisticas.CritChance = 1.0;
            var objetivo = new DummyFighter { Nombre = "Dummy" };

            // Semilla determinista
            MiJuegoRPG.Motor.Servicios.RandomService.Instancia.SetSeed(1);

            var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
            var res = accion.Ejecutar(pj, objetivo);

            Assert.True(res.FueCritico);
            Assert.True(res.DanioReal >= 1); // daño lo calcula AtacarFisico actual
        }
    }
}
