using System;
using System.Collections.Generic;
using System.IO;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using Xunit;
using PJ = MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Tests
{
    public class GeneradorEnemigosTests : IDisposable
    {
        private readonly string _tempDir;
        public GeneradorEnemigosTests()
        {
            // Aislar E/S de archivos durante las pruebas para evitar leer JSONs del repo
            _tempDir = Path.Combine(Path.GetTempPath(), "MiJuegoRPGTests_IO_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            MiJuegoRPG.Objetos.GestorArmas.RutaArmasJson = Path.Combine(_tempDir, "armas.json");
            MiJuegoRPG.Objetos.GestorPociones.RutaPocionesJson = Path.Combine(_tempDir, "pociones.json");
            MiJuegoRPG.Objetos.GestorMateriales.RutaMaterialesJson = Path.Combine(_tempDir, "materiales.json");

            // Limpiar catálogos en memoria para estado limpio de tests
            MiJuegoRPG.Objetos.GestorArmas.ArmasDisponibles.Clear();
            MiJuegoRPG.Objetos.GestorPociones.PocionesDisponibles.Clear();
            MiJuegoRPG.Objetos.GestorMateriales.MaterialesDisponibles.Clear();

            // Silenciar UI y evitar pausas/bloqueos durante pruebas
            MiJuegoRPG.Motor.Juego.UiFactory = () => new MiJuegoRPG.Motor.Servicios.SilentUserInterface();
            MiJuegoRPG.Motor.InputService.TestMode = true;

            // No persistir drops durante tests
            MiJuegoRPG.Motor.GeneradorEnemigos.DesactivarPersistenciaDrops = true;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDir))
                {
                    Directory.Delete(_tempDir, recursive: true);
                }
            }
            catch
            {
                // Ignorar errores de limpieza en CI/Windows cuando archivos están bloqueados
            }
        }

        [Fact]
        public void GenerarEnemigoAleatorio_UsaRandomServiceConSemilla_Determinista()
        {
            // Arrange: crear el juego primero (su inicialización puede consumir RNG)
            var juego = new Juego();
            // Setear semilla DESPUÉS de construir el juego para resultado reproducible
            RandomService.Instancia.SetSeed(12345);
            var pj = new PJ.Personaje("Tester");
            pj.Nivel = 3; // filtra enemigos apropiados (<= nivel+2)

            // Act
            var e1 = GeneradorEnemigos.GenerarEnemigoAleatorio(pj);

            // Reset semilla y repetir (sin reconstruir juego para mantener el mismo entorno)
            RandomService.Instancia.SetSeed(12345);
            var e2 = GeneradorEnemigos.GenerarEnemigoAleatorio(pj);

            // Assert: mismo enemigo esperado por determinismo
            Assert.Equal(e1.Nombre, e2.Nombre);
            Assert.Equal(e1.Nivel, e2.Nivel);
        }

        [Fact]
        public void GenerarEnemigoAleatorio_FiltraPorNivelDelJugador()
        {
            var juego = new Juego();
            var pjBajo = new PJ.Personaje("Low");
            pjBajo.Nivel = 1; // solo enemigos hasta nivel 3 deberían ser elegibles
            RandomService.Instancia.SetSeed(1);
            var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(pjBajo);
            Assert.True(enemigo.Nivel <= pjBajo.Nivel + 2);
        }
    }
}
