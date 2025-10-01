using System;
using Xunit;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Tests
{
    public class GeneradorObjetosRarezaFallbackTests
    {
        [Fact]
        public void GenerarArmaAleatoria_NoFallaConRarezaDesconocida()
        {
            RandomService.Instancia.SetSeed(98765);
            // Forzar carga de equipo (si ya cargado no afecta). Asumimos que existirá al menos un arma.
            GeneradorObjetos.CargarEquipoAuto();

            var arma = GeneradorObjetos.GenerarArmaAleatoria(1);

            Assert.NotNull(arma);
            Assert.InRange(arma.Perfeccion, 0, 100); // clamp esperado
            Assert.False(string.IsNullOrWhiteSpace(arma.Rareza));
            // No afirmamos rareza exacta porque depende de data; objetivo: ausencia de excepción y valores en rango seguro.
        }
    }
}
