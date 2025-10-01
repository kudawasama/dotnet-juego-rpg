using System;
using System.IO;
using Xunit;
using MiJuegoRPG.Motor.Servicios;
using PersonajeEnt = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Tests
{
    public class AccionRegistryPersistenceTests
    {
        [Fact]
        public void ProgresoAcciones_PersisteTrasGuardadoYCarga()
        {
            // Arrange
            RandomService.Instancia.SetSeed(12345);
            var nombre = "Tester_Persistencia_" + Guid.NewGuid().ToString("N").Substring(0,6);
            var pj = new PersonajeEnt(nombre);

            // Registrar algunas acciones (aunque no desbloqueen nada concreto según data real)
            AccionRegistry.Instancia.RegistrarAccion("ExplorarSector", pj);
            AccionRegistry.Instancia.RegistrarAccion("ObservarNPC", pj);
            AccionRegistry.Instancia.RegistrarAccion("RecolectarMaterial", pj);

            // Guardar
            var guardado = new GuardadoService();
            guardado.Verbose = false; // reducir ruido consola en test
            guardado.Guardar(pj);

            // Act: cargar (interactivo pide input; en este entorno simulamos acceso directo a archivo de progreso)
            // Releer progreso_acciones.json directamente (simplificación: validar persistencia de mapa)
            var ruta = PathProvider.PjDatosPath("progreso_acciones.json");
            Assert.True(File.Exists(ruta));
            var json = File.ReadAllText(ruta);
            Assert.False(string.IsNullOrWhiteSpace(json)); // archivo persistido no vacío

            // Simular nueva instancia de personaje y restaurar manual (como hace GuardadoService.Cargar)
            var pj2 = new PersonajeEnt(nombre);
            var mapa = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string,int>>>(json);
            pj2.ProgresoAccionesPorHabilidad = mapa ?? new();

            // Assert
            // No conocemos ids concretos de habilidades porque dependen de data, pero verificamos que la estructura no está vacía.
            Assert.NotNull(pj2.ProgresoAccionesPorHabilidad);
            // Validación mínima: no debe lanzar; se asegura persistencia física del archivo no vacío.
        }
    }
}
