// <copyright file="DelitosServiceTests.cs" company="Kudawasama">
// Copyright (c) Kudawasama. All rights reserved.
// </copyright>

namespace MiJuegoRPG.Tests.AccionesMundoTests
{
    using Xunit;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.Personaje;

    /// <summary>
    /// Tests unitarios para DelitosService.
    /// Valida carga de config/delitos.json, aplicación de consecuencias y acumulación.
    /// Usa RNG determinista con semilla fija para multas aleatorias.
    /// </summary>
    [Collection("Sequential")]
    public class DelitosServiceTests
    {
        /// <summary>
        /// Dado: config/delitos.json cargado correctamente.
        /// Cuando: se aplica el delito 'robo_intento' a un personaje.
        /// Entonces: debe reducir reputación con guardia en -5.
        /// </summary>
        [Fact]
        public void RoboIntento_AplicaConsequencias_ReputacionGuardiaMenos5()
        {
            // Arrange
            var service = new DelitosService();
            service.CargarDelitos(); // Lee DatosJuego/config/delitos.json
            var personaje = new Personaje("Criminal");
            personaje.ReputacionesFaccion["guardia"] = 0; // Neutral

            // Act
            service.AplicarDelito("robo_intento", personaje);

            // Assert
            Assert.Equal(-5, personaje.ReputacionesFaccion["guardia"]);
        }

        /// <summary>
        /// Dado: config/delitos.json con 'robo_intento' multa [10, 30] oro.
        /// Cuando: se aplica el delito con RNG semilla fija.
        /// Entonces: debe aplicar multa determinista dentro del rango.
        /// </summary>
        [Fact]
        public void RoboIntento_AplicaMulta_RangoDeterminista()
        {
            // Arrange
            var rng = new RandomService();
            rng.SetSeed(12345); // Determinismo
            var service = new DelitosService(rng);
            service.CargarDelitos();
            var personaje = new Personaje("Ladrón");
            personaje.Oro = 100;

            // Act
            service.AplicarDelito("robo_intento", personaje);

            // Assert
            var multaAplicada = 100 - personaje.Oro;
            Assert.InRange(multaAplicada, 10, 30);
        }

        /// <summary>
        /// Dado: delito 'robo_intento' aplicado 3 veces.
        /// Cuando: se acumulan consecuencias.
        /// Entonces: reputación debe bajar -15 total (3 x -5).
        /// </summary>
        [Fact]
        public void MultiplesDelitos_AcumulanConsecuencias()
        {
            // Arrange
            var service = new DelitosService();
            service.CargarDelitos();
            var personaje = new Personaje("Reincidente");
            personaje.ReputacionesFaccion["guardia"] = 10; // Positiva inicial

            // Act
            service.AplicarDelito("robo_intento", personaje);
            service.AplicarDelito("robo_intento", personaje);
            service.AplicarDelito("robo_intento", personaje);

            // Assert
            Assert.Equal(-5, personaje.ReputacionesFaccion["guardia"]); // 10 - 15 = -5
        }

        /// <summary>
        /// Dado: delito 'robo_intento' con flag alerta_ciudad=true.
        /// Cuando: se aplica el delito.
        /// Entonces: debe marcar estado de alerta en el personaje o contexto.
        /// </summary>
        [Fact]
        public void RoboIntento_MarcaAlertaCiudad()
        {
            // Arrange
            var service = new DelitosService();
            service.CargarDelitos();
            var personaje = new Personaje("Fugitivo");

            // Act
            var resultado = service.AplicarDelito("robo_intento", personaje);

            // Assert
            Assert.True(resultado.AlertaCiudad, "robo_intento debe activar alerta de ciudad");
        }

        /// <summary>
        /// Dado: config/delitos.json con 'hechiceria_en_ciudad'.
        /// Cuando: se aplica el delito.
        /// Entonces: debe reducir reputación global en -2.
        /// </summary>
        [Fact]
        public void HechiceriaEnCiudad_ReduceReputacionGlobal()
        {
            // Arrange
            var service = new DelitosService();
            service.CargarDelitos();
            var personaje = new Personaje("Mago");
            personaje.Reputacion = 50; // Inicial

            // Act
            service.AplicarDelito("hechiceria_en_ciudad", personaje);

            // Assert
            Assert.Equal(48, personaje.Reputacion); // 50 - 2
        }

        /// <summary>
        /// Dado: delito inexistente en config.
        /// Cuando: se intenta aplicar.
        /// Entonces: debe log warning y no afectar al personaje.
        /// </summary>
        [Fact]
        public void DelitoInexistente_LogWarnYNoAfecta()
        {
            // Arrange
            var service = new DelitosService();
            service.CargarDelitos();
            var personaje = new Personaje("Inocente");
            var reputacionInicial = personaje.Reputacion;

            // Act
            var resultado = service.AplicarDelito("delito_inexistente_xyz", personaje);

            // Assert
            Assert.Null(resultado); // No hay resultado por delito desconocido
            Assert.Equal(reputacionInicial, personaje.Reputacion);
        }
    }
}
