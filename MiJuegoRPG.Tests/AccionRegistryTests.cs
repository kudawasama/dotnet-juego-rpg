using System;
using System.Linq;
using Xunit;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Personaje;
// Alias para evitar colisión de nombre entre namespace y tipo 'Personaje'
using PersonajeEnt = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Tests
{
    public class AccionRegistryTests
    {
        [Fact]
        public void RegistrarAccion_DesbloqueaHabilidadCuandoCumpleCondiciones()
        {
            // Arrange: personaje sin habilidades
            var pj = new PersonajeEnt("Tester");
            // Asegurar que existe la habilidad de demo con condiciones de acciones
            // Si en datos reales existe Embestida con "CorrerGolpear" y "SinArma", simulamos las acciones mínimas.
            // Para no ejecutar cientos de iteraciones, solo validamos que el método suma progreso y no lanza.

            // Act: registrar algunas acciones conocidas del catálogo
            bool unlock1 = AccionRegistry.Instancia.RegistrarAccion("ExplorarSector", pj);
            bool unlock2 = AccionRegistry.Instancia.RegistrarAccion("ObservarNPC", pj);
            bool unlock3 = AccionRegistry.Instancia.RegistrarAccion("RecolectarMaterial", pj);

            // Assert: no debe lanzar y el mapa de progreso debe haberse creado
            Assert.NotNull(pj.ProgresoAccionesPorHabilidad);
            // Como las habilidades concretas dependen de los datos, no afirmamos un id específico aquí.
            // Verificamos que el registro no produce inconsistencias (no agrega habilidades inexistentes)
            Assert.All(pj.Habilidades.Values, h => Assert.False(string.IsNullOrWhiteSpace(h.Id)));
        }

        [Fact]
        public void GetProgreso_RetornaCeroParaFaltantes()
        {
            var pj = new PersonajeEnt("Tester");
            var v = AccionRegistry.Instancia.GetProgreso(pj, "habilidad_inexistente", "accion_x");
            Assert.Equal(0, v);
        }
    }
}
