using System.Linq;
using MiJuegoRPG.Interfaces;
using Xunit;

namespace MiJuegoRPG.Tests
{
    // Combatiente mínimo determinista para pruebas (10 físico, 12 mágico, sin evasión ni crítico)
    class VerboseDummyFighter : ICombatiente
    {
        public string Nombre { get; set; } = "Dummy";
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

    public class CombatVerboseMessageTests
    {
        [Fact]
        public void AtaqueFisico_Verbose_IncluyeDetalleDefensaYMitigacion()
        {
            // Arrange
            bool prevVerbose = GameplayToggles.CombatVerbose;
            bool prevPen = GameplayToggles.PenetracionEnabled;
            bool prevPrec = GameplayToggles.PrecisionCheckEnabled;
            GameplayToggles.CombatVerbose = true;
            GameplayToggles.PenetracionEnabled = false;
            GameplayToggles.PrecisionCheckEnabled = false;
            try
            {
                var atacante = new VerboseDummyFighter { Nombre = "Atacante" };
                var objetivo = new MiJuegoRPG.Enemigos.EnemigoEstandar(
                    nombre: "Mob",
                    vidaBase: 100,
                    ataqueBase: 5,
                    defensaBase: 5,
                    defensaMagicaBase: 3,
                    nivel: 1,
                    experienciaRecompensa: 0,
                    oroRecompensa: 0
                );
                objetivo.MitigacionFisicaPorcentaje = 0.10; // 10% para asegurar presencia de la sección

                // Act
                var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                var res = accion.Ejecutar(atacante, objetivo);

                // Assert básico
                Assert.False(res.FueEvadido);
                // Debe existir una línea con el detalle didáctico
                var detalle = res.Mensajes.FirstOrDefault(m => m.Contains("Daño final:"));
                Assert.False(string.IsNullOrWhiteSpace(detalle));
                Assert.Contains("Base", detalle!);
                Assert.Contains("Defensa efectiva", detalle!);
                Assert.Contains("Mitigación", detalle!);
                Assert.Contains("Daño final:", detalle!);
            }
            finally
            {
                GameplayToggles.CombatVerbose = prevVerbose;
                GameplayToggles.PenetracionEnabled = prevPen;
                GameplayToggles.PrecisionCheckEnabled = prevPrec;
            }
        }

        [Fact]
        public void AtaqueMagico_Verbose_IncluyeDefensaMitigacionResistenciaYVulnerabilidad()
        {
            // Arrange
            bool prevVerbose = GameplayToggles.CombatVerbose;
            bool prevPen = GameplayToggles.PenetracionEnabled;
            GameplayToggles.CombatVerbose = true;
            GameplayToggles.PenetracionEnabled = false; // no necesitamos pen para este test

            try
            {
                var atacante = new VerboseDummyFighter { Nombre = "Mage" };
                var objetivo = new MiJuegoRPG.Enemigos.EnemigoEstandar(
                    nombre: "Mob",
                    vidaBase: 100,
                    ataqueBase: 5,
                    defensaBase: 2,
                    defensaMagicaBase: 7,
                    nivel: 1,
                    experienciaRecompensa: 0,
                    oroRecompensa: 0
                );
                objetivo.MitigacionMagicaPorcentaje = 0.10; // 10%
                objetivo.EstablecerMitigacionElemental("magia", 0.30); // Resistencia 30%
                objetivo.EstablecerVulnerabilidadElemental("magia", 1.20); // Vulnerabilidad +20%

                // Act
                var accion = new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion();
                var res = accion.Ejecutar(atacante, objetivo);

                // Assert
                Assert.False(res.FueEvadido);
                var detalle = res.Mensajes.FirstOrDefault(m => m.Contains("Daño final:"));
                Assert.False(string.IsNullOrWhiteSpace(detalle));
                Assert.Contains("Base", detalle!);
                Assert.Contains("Defensa mágica efectiva", detalle!);
                Assert.Contains("Mitigación", detalle!);
                Assert.Contains("Resistencia magia", detalle!);
                Assert.Contains("Vulnerabilidad", detalle!);
                Assert.Contains("Daño final:", detalle!);
            }
            finally
            {
                GameplayToggles.CombatVerbose = prevVerbose;
                GameplayToggles.PenetracionEnabled = prevPen;
            }
        }

        [Fact]
        public void AtaqueFisico_Evadido_NoAgregaDetalleAunqueVerbose()
        {
            // Arrange: activar verbose y forzar fallo por precisión
            bool prevVerbose = GameplayToggles.CombatVerbose;
            bool prevPrec = GameplayToggles.PrecisionCheckEnabled;
            GameplayToggles.CombatVerbose = true;
            GameplayToggles.PrecisionCheckEnabled = true;
            try
            {
                var pj = new MiJuegoRPG.Personaje.Personaje("Heroe");
                pj.Estadisticas.Precision = 0.0; // garantiza fallo
                var objetivo = new VerboseDummyFighter { Nombre = "Target" };

                // Determinismo por si interviene RNG en rutas futuras
                MiJuegoRPG.Motor.Servicios.RandomService.Instancia.SetSeed(12345);

                var accion = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                var res = accion.Ejecutar(pj, objetivo);

                Assert.True(res.FueEvadido);
                // En caso de evadir/fallar, no se debe añadir la línea didáctica
                Assert.DoesNotContain(res.Mensajes, m => m.Contains("Daño final:"));
            }
            finally
            {
                GameplayToggles.CombatVerbose = prevVerbose;
                GameplayToggles.PrecisionCheckEnabled = prevPrec;
            }
        }
    }
}
