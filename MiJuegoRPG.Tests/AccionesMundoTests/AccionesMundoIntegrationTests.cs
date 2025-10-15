// <copyright file="AccionesMundoIntegrationTests.cs" company="Kudawasama">
// Copyright (c) Kudawasama. All rights reserved.
// </copyright>

namespace MiJuegoRPG.Tests.AccionesMundoTests
{
    using Xunit;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.Personaje;

    /// <summary>
    /// Tests de integración para Acciones de Mundo.
    /// Valida flujos completos: políticas + catálogo + delitos + energía/tiempo.
    /// Usa RandomService.SetSeed para determinismo en detecciones.
    /// </summary>
    [Collection("Sequential")]
    public class AccionesMundoIntegrationTests
    {
        /// <summary>
        /// Escenario: Robar en Ruta (permitido con riesgo 25%).
        /// Dado: RNG forzado a éxito (no detectado).
        /// Cuando: se ejecuta la acción.
        /// Entonces: debe consumir recursos, NO aplicar delito y retornar éxito.
        /// </summary>
        [Fact]
        public void RobarEnRuta_ExitoNoDetectado_ConsumeRecursosSinDelito()
        {
            // Arrange - Inyectar servicios
            var rng = new RandomService();
            rng.SetSeed(999); // Semilla que garantiza NO detección (< 0.25)
            var policyService = new ZonePolicyService();
            policyService.CargarPoliticas();
            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();
            var delitosService = new DelitosService(rng);
            delitosService.CargarDelitos();
            var executor = new WorldActionExecutor(policyService, catalogService, delitosService, rng);

            var personaje = new Personaje("Ladrón");
            personaje.Clase = new Clase { Nombre = "Ladron" };
            personaje.AtributosBase.Destreza = 20; // Cumple requisito
            personaje.Estadisticas.Energia = 20;
            personaje.Oro = 100;
            personaje.ReputacionesFaccion["guardia"] = 0;

            var mundoContext = new MundoContext { MinutosMundo = 100 };

            // Act
            var resultado = executor.EjecutarAccion("robar_intento", personaje, "Ruta", mundoContext);

            // Assert
            Assert.True(resultado.Exito, "Acción debe ejecutarse con éxito");
            Assert.Equal(12, personaje.Estadisticas.Energia); // 20 - 8
            Assert.Equal(103, mundoContext.MinutosMundo); // 100 + 3
            Assert.Equal(0, personaje.ReputacionesFaccion["guardia"]); // Sin penalización
            Assert.False(resultado.FueDetectado);
        }

        /// <summary>
        /// Escenario: Robar en Ruta (permitido con riesgo 25%).
        /// Dado: RNG forzado a detección.
        /// Cuando: se ejecuta la acción.
        /// Entonces: debe consumir recursos, aplicar delito (reputación -5, multa) y marcar detección.
        /// </summary>
        [Fact]
        public void RobarEnRuta_Detectado_AplicaDelitoYConsecuencias()
        {
            // Arrange
            var rng = new RandomService();
            rng.SetSeed(123); // Semilla que garantiza detección (>= 0.25)
            var policyService = new ZonePolicyService();
            policyService.CargarPoliticas();
            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();
            var delitosService = new DelitosService(rng);
            delitosService.CargarDelitos();
            var executor = new WorldActionExecutor(policyService, catalogService, delitosService, rng);

            var personaje = new Personaje("Ladrón");
            personaje.Clase = new Clase { Nombre = "Ladron" };
            personaje.AtributosBase.Destreza = 20;
            personaje.Estadisticas.Energia = 20;
            personaje.Oro = 100;
            personaje.ReputacionesFaccion["guardia"] = 0;

            var mundoContext = new MundoContext { MinutosMundo = 100 };

            // Act
            var resultado = executor.EjecutarAccion("robar_intento", personaje, "Ruta", mundoContext);

            // Assert
            Assert.True(resultado.Exito, "Acción se ejecutó aunque fue detectado");
            Assert.Equal(12, personaje.Estadisticas.Energia); // Recursos consumidos
            Assert.Equal(103, mundoContext.MinutosMundo);
            Assert.True(resultado.FueDetectado);
            Assert.Equal(-5, personaje.ReputacionesFaccion["guardia"]); // Delito aplicado
            Assert.InRange(100 - personaje.Oro, 10, 30); // Multa aplicada
        }

        /// <summary>
        /// Escenario: Robar en Ciudad (bloqueado por política).
        /// Cuando: se intenta ejecutar la acción.
        /// Entonces: debe bloquearse SIN consumir recursos ni aplicar consecuencias.
        /// </summary>
        [Fact]
        public void RobarEnCiudad_Bloqueado_NoConsumeRecursosNiAplicaDelito()
        {
            // Arrange
            var rng = new RandomService();
            rng.SetSeed(999);
            var policyService = new ZonePolicyService();
            policyService.CargarPoliticas();
            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();
            var delitosService = new DelitosService(rng);
            delitosService.CargarDelitos();
            var executor = new WorldActionExecutor(policyService, catalogService, delitosService, rng);

            var personaje = new Personaje("Ladrón");
            personaje.Clase = new Clase { Nombre = "Ladron" };
            personaje.AtributosBase.Destreza = 20;
            personaje.Estadisticas.Energia = 20;
            personaje.Oro = 100;
            personaje.ReputacionesFaccion["guardia"] = 0;

            var mundoContext = new MundoContext { MinutosMundo = 100 };

            // Act
            var resultado = executor.EjecutarAccion("robar_intento", personaje, "Ciudad", mundoContext);

            // Assert
            Assert.False(resultado.Exito, "Acción debe estar bloqueada por política");
            Assert.Contains("no permitida", resultado.Mensaje.ToLowerInvariant());
            Assert.Equal(20, personaje.Estadisticas.Energia); // NO consumió
            Assert.Equal(100, mundoContext.MinutosMundo); // Tiempo NO avanzó
            Assert.Equal(0, personaje.ReputacionesFaccion["guardia"]); // Sin penalización
            Assert.Equal(100, personaje.Oro); // Sin multa
        }

        /// <summary>
        /// Escenario: Requisitos no cumplidos (clase incorrecta).
        /// Cuando: guerrero intenta ejecutar acción de ladrón.
        /// Entonces: debe bloquearse con mensaje de requisitos.
        /// </summary>
        [Fact]
        public void AccionConRequisitos_ClaseIncorrecta_SeBloqueaConMensaje()
        {
            // Arrange
            var rng = new RandomService();
            var policyService = new ZonePolicyService();
            policyService.CargarPoliticas();
            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();
            var delitosService = new DelitosService(rng);
            var executor = new WorldActionExecutor(policyService, catalogService, delitosService, rng);

            var personaje = new Personaje("Guerrero");
            personaje.Clase = new Clase { Nombre = "Guerrero" }; // NO es Ladrón
            personaje.Estadisticas.Energia = 20;

            // Act
            var resultado = executor.EjecutarAccion("robar_intento", personaje, "Ruta");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("requisitos", resultado.Mensaje.ToLowerInvariant());
            Assert.Equal(20, personaje.Estadisticas.Energia); // No consumió
        }

        /// <summary>
        /// Escenario: Cooldown aplicado tras primera ejecución.
        /// Cuando: se intenta ejecutar acción inmediatamente después.
        /// Entonces: debe bloquearse por cooldown activo.
        /// </summary>
        [Fact]
        public void AccionConCooldown_EjecucionRepetida_SeBloqueaPorCooldown()
        {
            // Arrange
            var rng = new RandomService();
            rng.SetSeed(999); // No detección
            var policyService = new ZonePolicyService();
            policyService.CargarPoliticas();
            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();
            var delitosService = new DelitosService(rng);
            var executor = new WorldActionExecutor(policyService, catalogService, delitosService, rng);

            var personaje = new Personaje("Ladrón");
            personaje.Clase = new Clase { Nombre = "Ladron" };
            personaje.AtributosBase.Destreza = 20;
            personaje.Estadisticas.Energia = 50; // Suficiente para 2 intentos

            var mundoContext = new MundoContext { MinutosMundo = 100 };

            // Act - Primera ejecución
            var resultado1 = executor.EjecutarAccion("robar_intento", personaje, "Ruta", mundoContext);
            // Act - Segunda ejecución inmediata (mismo minuto lógico)
            mundoContext.MinutosMundo = 103; // Avanzó solo 3 min, cooldown es 60
            var resultado2 = executor.EjecutarAccion("robar_intento", personaje, "Ruta", mundoContext);

            // Assert
            Assert.True(resultado1.Exito, "Primera ejecución debe ser exitosa");
            Assert.False(resultado2.Exito, "Segunda ejecución debe bloquearse");
            Assert.Contains("cooldown", resultado2.Mensaje.ToLowerInvariant());
            Assert.Equal(42, personaje.Estadisticas.Energia); // Solo consumió primera (50 - 8)
        }
    }
}
