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
        }
    }
}
