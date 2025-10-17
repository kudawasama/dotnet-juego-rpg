// <copyright file="WorldActionExecutorTests.cs" company="Kudawasama">
// Copyright (c) Kudawasama. All rights reserved.
// </copyright>

namespace MiJuegoRPG.Tests
{
    using Xunit;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.Personaje;

    /// <summary>
    /// Tests unitarios para WorldActionExecutor.
    /// Valida consumo de Energía, avance de tiempo del mundo, bloqueos por recursos/cooldown.
    /// Determinista (no usa RNG para estos casos base).
    /// </summary>
    [Collection("Sequential")]
    public class WorldActionExecutorTests
    {
        /// <summary>
        /// Dado: personaje con Energía=20.
        /// Cuando: ejecuta acción con coste_energia=8.
        /// Entonces: Energía debe quedar en 12.
        /// </summary>
        [Fact]
        public void EjecutarAccion_ConsumeEnergia_Correctamente()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Explorador");
            personaje.Estadisticas.Energia = 20;
            var accion = new ActionWorldDef
            {
                Id = "robar_intento",
                CosteEnergia = 8,
                CosteTiempoMin = 3,
            };

            // Act
            var resultado = executor.EjecutarAccion(accion, personaje, "Ruta");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Equal(12, personaje.Estadisticas.Energia); // 20 - 8
        }

        /// <summary>
        /// Dado: personaje con Energía=5.
        /// Cuando: intenta ejecutar acción con coste_energia=8.
        /// Entonces: debe bloquearse con mensaje "Energía insuficiente".
        /// </summary>
        [Fact]
        public void EjecutarAccion_EnergiaInsuficiente_SeBloqueaConMensaje()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Cansado");
            personaje.Estadisticas.Energia = 5;
            var accion = new ActionWorldDef
            {
                Id = "robar_intento",
                CosteEnergia = 8,
            };

            // Act
            var resultado = executor.EjecutarAccion(accion, personaje, "Ruta");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Energía insuficiente", resultado.Mensaje);
            Assert.Equal(5, personaje.Estadisticas.Energia); // No se consumió
        }

        /// <summary>
        /// Dado: mundo con tiempo actual = 100 minutos.
        /// Cuando: ejecuta acción con coste_tiempo_min=3.
        /// Entonces: tiempo del mundo debe avanzar a 103 minutos.
        /// </summary>
        [Fact]
        public void EjecutarAccion_AvanzaTiempoMundo_Correctamente()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Viajero");
            personaje.Estadisticas.Energia = 20;
            var mundoContext = new MundoContext { MinutosMundo = 100 };
            var accion = new ActionWorldDef
            {
                Id = "dialogar",
                CosteEnergia = 1,
                CosteTiempoMin = 3,
            };

            // Act
            var resultado = executor.EjecutarAccion(accion, personaje, "Ciudad", mundoContext);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Equal(103, mundoContext.MinutosMundo); // 100 + 3
        }

        /// <summary>
        /// Dado: personaje con cooldown activo para 'robar_intento' (expira en minuto 150).
        /// Cuando: intenta ejecutar la acción en minuto 100.
        /// Entonces: debe bloquearse con mensaje de cooldown.
        /// </summary>
        [Fact]
        public void EjecutarAccion_CooldownActivo_SeBloqueaConMensaje()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Ladrón");
            personaje.Estadisticas.Energia = 20;
            personaje.CooldownsAccionesMundo["robar_intento"] = 150; // Expira en minuto 150
            var mundoContext = new MundoContext { MinutosMundo = 100 };
            var accion = new ActionWorldDef
            {
                Id = "robar_intento",
                CosteEnergia = 8,
                CooldownMin = 60,
            };

            // Act
            var resultado = executor.EjecutarAccion(accion, personaje, "Ruta", mundoContext);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("cooldown", resultado.Mensaje.ToLowerInvariant());
            Assert.Equal(20, personaje.Estadisticas.Energia); // No se consumió
        }

        /// <summary>
        /// Dado: personaje ejecutó acción con cooldown=60 en minuto 100.
        /// Cuando: el mundo está en minuto 161 (pasó el cooldown).
        /// Entonces: debe permitir ejecutar la acción nuevamente.
        /// </summary>
        [Fact]
        public void EjecutarAccion_CooldownExpirado_PermiteEjecucion()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Ladrón");
            personaje.Estadisticas.Energia = 20;
            personaje.CooldownsAccionesMundo["robar_intento"] = 160; // Expira en 160
            var mundoContext = new MundoContext { MinutosMundo = 161 }; // Cooldown expirado
            var accion = new ActionWorldDef
            {
                Id = "robar_intento",
                CosteEnergia = 8,
                CooldownMin = 60,
            };

            // Act
            var resultado = executor.EjecutarAccion(accion, personaje, "Ruta", mundoContext);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Equal(12, personaje.Estadisticas.Energia); // 20 - 8
            Assert.Equal(221, personaje.CooldownsAccionesMundo["robar_intento"]); // 161 + 60
        }

        /// <summary>
        /// Dado: acción sin cooldown definido (cooldown=0).
        /// Cuando: se ejecuta múltiples veces.
        /// Entonces: debe permitir ejecución repetida sin bloqueo.
        /// </summary>
        [Fact]
        public void EjecutarAccion_SinCooldown_PermiteEjecucionRepetida()
        {
            // Arrange
            var executor = new WorldActionExecutor();
            var personaje = new Personaje("Charlatán");
            personaje.Estadisticas.Energia = 50;
            var accion = new ActionWorldDef
            {
                Id = "dialogar",
                CosteEnergia = 1,
                CooldownMin = 0, // Sin cooldown
            };

            // Act
            var resultado1 = executor.EjecutarAccion(accion, personaje, "Ciudad");
            var resultado2 = executor.EjecutarAccion(accion, personaje, "Ciudad");
            var resultado3 = executor.EjecutarAccion(accion, personaje, "Ciudad");

            // Assert
            Assert.True(resultado1.Exito);
            Assert.True(resultado2.Exito);
            Assert.True(resultado3.Exito);
            Assert.Equal(47, personaje.Estadisticas.Energia); // 50 - 3
        }
    }
}
