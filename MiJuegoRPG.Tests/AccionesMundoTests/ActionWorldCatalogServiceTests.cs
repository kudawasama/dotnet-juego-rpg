// <copyright file="ActionWorldCatalogServiceTests.cs" company="Kudawasama">
// Copyright (c) Kudawasama. All rights reserved.
// </copyright>

namespace MiJuegoRPG.Tests.AccionesMundoTests
{
    using Xunit;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.Personaje;

    /// <summary>
    /// Tests unitarios para ActionWorldCatalogService.
    /// Valida carga de acciones/acciones_mundo.json, defaults y requisitos.
    /// Sin RNG (lectura de configuración).
    /// </summary>
    [Collection("Sequential")]
    public class ActionWorldCatalogServiceTests
    {
        /// <summary>
        /// Dado: acciones_mundo.json cargado correctamente.
        /// Cuando: se consulta una acción sin costes explícitos.
        /// Entonces: debe retornar defaults: energia=1, tiempo=1, cooldown=0.
        /// </summary>
        [Fact]
        public void AccionSinCostes_RetornaDefaults()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo(); // Lee DatosJuego/acciones/acciones_mundo.json

            // Act
            var accion = service.ObtenerAccion("dialogar"); // Acción básica sin costes definidos

            // Assert
            Assert.NotNull(accion);
            Assert.Equal(1, accion.CosteEnergia);
            Assert.Equal(1, accion.CosteTiempoMin);
            Assert.Equal(0, accion.CooldownMin);
        }

        /// <summary>
        /// Dado: acciones_mundo.json con 'robar_intento' definido.
        /// Cuando: se consulta la acción.
        /// Entonces: debe retornar los costes configurados (energia=8, tiempo=3, cooldown=60).
        /// </summary>
        [Fact]
        public void RobarIntento_RetornaCostesConfigurados()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo();

            // Act
            var accion = service.ObtenerAccion("robar_intento");

            // Assert
            Assert.NotNull(accion);
            Assert.Equal(8, accion.CosteEnergia);
            Assert.Equal(3, accion.CosteTiempoMin);
            Assert.Equal(60, accion.CooldownMin);
            Assert.Equal("robo_intento", accion.Consecuencias?.DelitoId);
        }

        /// <summary>
        /// Dado: acciones_mundo.json con acción que requiere clase 'ladron'.
        /// Cuando: se valida elegibilidad con personaje clase 'guerrero'.
        /// Entonces: debe retornar false (no cumple requisitos).
        /// </summary>
        [Fact]
        public void AccionConRequisitoClase_PersonajeNoCumple_NoEsElegible()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo();
            var personaje = new Personaje("Heroe");
            personaje.Clase = new Clase { Nombre = "Guerrero" };

            // Act
            var accion = service.ObtenerAccion("robar_intento");
            var elegible = service.CumpleRequisitos(accion, personaje);

            // Assert
            Assert.False(elegible, "Guerrero no debe poder ejecutar acción de Ladrón");
        }

        /// <summary>
        /// Dado: acciones_mundo.json con acción que requiere Destreza >= 15.
        /// Cuando: se valida con personaje Destreza = 10.
        /// Entonces: debe retornar false (atributos insuficientes).
        /// </summary>
        [Fact]
        public void AccionConRequisitoAtributo_PersonajeNoCumple_NoEsElegible()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo();
            var personaje = new Personaje("Heroe");
            personaje.Clase = new Clase { Nombre = "Ladron" }; // Cumple clase
            personaje.AtributosBase.Destreza = 10; // NO cumple destreza

            // Act
            var accion = service.ObtenerAccion("robar_intento");
            var elegible = service.CumpleRequisitos(accion, personaje);

            // Assert
            Assert.False(elegible, "Destreza 10 < 15 requerido");
        }

        /// <summary>
        /// Dado: acciones_mundo.json cargado.
        /// Cuando: se consulta una acción inexistente.
        /// Entonces: debe retornar null y log warning (no crash).
        /// </summary>
        [Fact]
        public void AccionInexistente_RetornaNullYLogWarn()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo();

            // Act
            var accion = service.ObtenerAccion("accion_fantasma_xyz");

            // Assert
            Assert.Null(accion);
            // Logger debe haber emitido un WARN; validar con mock o log capture si disponible
        }

        /// <summary>
        /// Dado: catálogo de acciones cargado.
        /// Cuando: se listan todas las acciones.
        /// Entonces: debe contener al menos las acciones documentadas (robar_intento, dialogar, etc.).
        /// </summary>
        [Fact]
        public void ListarAcciones_ContieneAccionesDocumentadas()
        {
            // Arrange
            var service = new ActionWorldCatalogService();
            service.CargarCatalogo();

            // Act
            var acciones = service.ListarAcciones();

            // Assert
            Assert.NotEmpty(acciones);
            Assert.Contains(acciones, a => a.Id == "robar_intento");
            Assert.Contains(acciones, a => a.Id == "dialogar" || a.Tipo == "social");
        }
    }
}
