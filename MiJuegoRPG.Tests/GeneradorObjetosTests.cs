using System;
using System.Collections.Generic;
using System.Reflection;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.PjDatos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class GeneradorObjetosTests
    {
        /// <summary>
        /// Verifica que: (a) la rareza elegida esté dentro del CSV permitido y (b) la perfección resultante
        /// respete la intersección entre el rango por rareza y el rango del ítem.
        /// </summary>
        [Fact]
        public void Accesorio_RarezasCSV_FiltraYRespetaInterseccionPerfeccion()
        {
            // Arrange: accesorio con CSV de rarezas y rango de perfección acotado
            var item = new AccesorioData
            {
                Nombre = "Prueba CSV",
                BonificacionAtaque = 10,
                BonificacionDefensa = 0,
                Nivel = 1,
                TipoObjeto = "Accesorio",
                // CSV de rarezas permitidas restringido a 'Superior' para evitar sesgo por pesos
                Rareza = "Superior",
                RarezasPermitidasCsv = "Superior",
                // Forzamos un rango de perfección que intersecta con Normal(50..50) y Superior(51..60)
                PerfeccionMin = 52,
                PerfeccionMax = 55,
                Perfeccion = 52
            };

            // Inyectar lista en el campo estático privado 'accesoriosDisponibles'
            SetStaticPrivateList("accesoriosDisponibles", new List<AccesorioData> { item });
            GeneradorObjetos.UsaSeleccionPonderadaRareza = true;

            // Semilla fija para determinismo
            RandomService.Instancia.SetSeed(12345);

            // Act
            var acc = GeneradorObjetos.GenerarAccesorioAleatorio(nivelJugador: 1);

            // Assert: la rareza elegida debe ser Superior (única permitida)
            Assert.Equal(MiJuegoRPG.Objetos.Rareza.Superior, acc.Rareza);
            // Perfección debe estar dentro de la intersección [52..55]
            Assert.InRange(acc.Perfeccion, 52, 55);
        }

        /// <summary>
        /// Con Perfeccion=50 y Rareza=Normal, el valor final debe ser igual al base (factor 50/50 = 1).
        /// </summary>
        [Fact]
        public void Accesorio_BaseNormal50_NoCambiaValorBase()
        {
            // Arrange: accesorio con perfección fija en 50 y rareza Normal
            var item = new AccesorioData
            {
                Nombre = "Base50",
                BonificacionAtaque = 14,
                BonificacionDefensa = 6,
                Nivel = 1,
                TipoObjeto = "Accesorio",
                Rareza = "Normal",
                Perfeccion = 50,
                PerfeccionMin = 50,
                PerfeccionMax = 50
            };

            SetStaticPrivateList("accesoriosDisponibles", new List<AccesorioData> { item });
            GeneradorObjetos.UsaSeleccionPonderadaRareza = true;
            RandomService.Instancia.SetSeed(777);

            // Act
            var acc = GeneradorObjetos.GenerarAccesorioAleatorio(nivelJugador: 1);

            // Assert: valores deben coincidir con los base
            Assert.Equal(14, acc.BonificacionAtaque);
            Assert.Equal(6, acc.BonificacionDefensa);
            Assert.Equal(50, acc.Perfeccion);
            Assert.Equal(MiJuegoRPG.Objetos.Rareza.Normal, acc.Rareza);
        }

        private static void SetStaticPrivateList(string fieldName, object value)
        {
            var t = typeof(GeneradorObjetos);
            var f = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(f);
            f!.SetValue(null, value);
        }
    }
}
