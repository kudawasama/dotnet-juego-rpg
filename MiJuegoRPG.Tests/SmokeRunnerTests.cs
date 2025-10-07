namespace MiJuegoRPG.Tests
{
    using System;
    using System.IO;
    using FluentAssertions;
    using MiJuegoRPG.Motor.Servicios;
    using Xunit;

    public class SmokeRunnerTests
    {
        [Fact]
        public void RunCombateSmoke_ShouldReturnZeroAndPrintOk()
        {
            // Capturar salida de consola
            var original = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                var code = SmokeRunner.RunCombateSmoke();
                code.Should().Be(0);

                var output = sw.ToString();
                output.Should().Contain("SMOKE COMBATE");
                output.Should().Contain("SMOKE OK");
            }
            finally
            {
                // Restaurar salida y toggles para no contaminar otras pruebas
                Console.SetOut(original);
                GameplayToggles.PrecisionCheckEnabled = false;
                GameplayToggles.PenetracionEnabled = false;
                GameplayToggles.CombatVerbose = false;
            }
        }
    }
}
