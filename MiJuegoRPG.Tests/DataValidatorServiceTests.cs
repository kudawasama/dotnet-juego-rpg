using System.IO;
using MiJuegoRPG.Motor.Servicios.Validacion;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class DataValidatorServiceTests
    {
        [Fact]
        public void ValidarReferenciasBasicas_NoRevienta_SinDatosFaltantes()
        {
            // Act
            var res = DataValidatorService.ValidarReferenciasBasicas();

            // Assert: no debe lanzar y debe retornar un resultado válido (con o sin advertencias)
            Assert.NotNull(res);
        }

        [Fact]
        public void ValidarEnemigosBasico_NoRevienta()
        {
            var res = DataValidatorService.ValidarEnemigosBasico();
            Assert.NotNull(res);
        }
    }
}
