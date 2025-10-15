// <copyright file="ZonePolicyServiceTests.cs" company="Kudawasama">
// Copyright (c) Kudawasama. All rights reserved.
// </copyright>

namespace MiJuegoRPG.Tests.AccionesMundoTests
{
    using Xunit;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.PjDatos;

    /// <summary>
    /// Tests unitarios para ZonePolicyService.
    /// Valida carga de config/zonas_politicas.json y resolución de políticas por tipo de zona.
    /// Sin RNG (puramente lectura de configuración).
    /// </summary>
    [Collection("Sequential")]
    public class ZonePolicyServiceTests
    {
        /// <summary>
        /// Dado: config/zonas_politicas.json cargado correctamente.
        /// Cuando: se consulta si 'robar_intento' está permitido en 'Ciudad'.
        /// Entonces: debe retornar permitido=false (bloqueado).
        /// </summary>
        [Fact]
        public void RobarIntento_EnCiudad_EstaBloqueado()
        {
            // Arrange
            var service = new ZonePolicyService();
            service.CargarPoliticas(); // Lee DatosJuego/config/zonas_politicas.json

            // Act
            var politica = service.ObtenerPolitica("Ciudad", "robar_intento");

            // Assert
            Assert.NotNull(politica);
            Assert.False(politica.Permitido, "Robar en Ciudad debe estar bloqueado por política");
            Assert.Equal("robo_intento", politica.DelitoId);
        }

        /// <summary>
        /// Dado: config/zonas_politicas.json cargado correctamente.
        /// Cuando: se consulta si 'robar_intento' está permitido en 'Ruta'.
        /// Entonces: debe retornar permitido=true (permitido con riesgo).
        /// </summary>
        [Fact]
        public void RobarIntento_EnRuta_EstaPermitido()
        {
            // Arrange
            var service = new ZonePolicyService();
            service.CargarPoliticas();

            // Act
            var politica = service.ObtenerPolitica("Ruta", "robar_intento");

            // Assert
            Assert.NotNull(politica);
            Assert.True(politica.Permitido, "Robar en Ruta debe estar permitido");
            // Puede tener o no riesgo; validar campo opcional
        }

        /// <summary>
        /// Dado: config/zonas_politicas.json cargado.
        /// Cuando: se consulta una acción NO definida en la política de zona.
        /// Entonces: debe retornar fallback seguro (permitido sin riesgo ni delito).
        /// </summary>
        [Fact]
        public void AccionNoDefinida_RetornaFallbackSeguro()
        {
            // Arrange
            var service = new ZonePolicyService();
            service.CargarPoliticas();

            // Act
            var politica = service.ObtenerPolitica("Ciudad", "accion_inexistente_xyz");

            // Assert
            Assert.NotNull(politica);
            Assert.True(politica.Permitido, "Acción no definida debe usar fallback permitido");
            Assert.Null(politica.DelitoId);
            Assert.False(politica.Risky);
        }

        /// <summary>
        /// Dado: config/zonas_politicas.json cargado.
        /// Cuando: se consulta una acción en 'ParteCiudad' (subtipo).
        /// Entonces: debe resolver política específica si existe, o heredar de 'Ciudad'.
        /// </summary>
        [Fact]
        public void ParteCiudad_ResuelvePoliticaEspecifica_OHereda()
        {
            // Arrange
            var service = new ZonePolicyService();
            service.CargarPoliticas();

            // Act - ParteCiudad puede tener política específica para robar_intento (permitido con riesgo)
            var politica = service.ObtenerPolitica("ParteCiudad", "robar_intento");

            // Assert
            Assert.NotNull(politica);
            // Según diseño en Resumen_Datos.md, ParteCiudad permite robar con riesgo
            Assert.True(politica.Permitido, "ParteCiudad debe permitir robar con riesgo");
            Assert.True(politica.Risky);
        }

        /// <summary>
        /// Dado: config/zonas_politicas.json con múltiples zonas.
        /// Cuando: se carga el servicio.
        /// Entonces: debe cargar políticas para Ciudad, ParteCiudad, Ruta sin errores.
        /// </summary>
        [Fact]
        public void CargarPoliticas_CargaMultiplesZonas_SinErrores()
        {
            // Arrange & Act
            var service = new ZonePolicyService();
            var exception = Record.Exception(() => service.CargarPoliticas());

            // Assert
            Assert.Null(exception);
            // Validar que existen al menos 3 tipos de zona documentados
            Assert.True(service.TieneZona("Ciudad"));
            Assert.True(service.TieneZona("ParteCiudad"));
            Assert.True(service.TieneZona("Ruta"));
        }
    }
}
