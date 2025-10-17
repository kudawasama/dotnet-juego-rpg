using System;
using Xunit;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Personaje;
using PersonajeEnt = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Tests
{
    public class AccionRegistryIntegrationTests
    {
        [Fact]
        public void DesbloqueaHabilidadDemoPorAcciones()
        {
            var pj = new PersonajeEnt("Tester");

            // Registrar acciones requeridas
            AccionRegistry.Instancia.RegistrarAccion("ExplorarSector", pj);
            AccionRegistry.Instancia.RegistrarAccion("ExplorarSector", pj);
            AccionRegistry.Instancia.RegistrarAccion("ObservarNPC", pj);

            // Verificar que la habilidad se desbloqueó
            Assert.Contains(pj.Habilidades.Values, h => h.Id == "habilidad_acciones_demo");

            // Verificar progreso registrado
            Assert.True(pj.ProgresoAccionesPorHabilidad.TryGetValue("habilidad_acciones_demo", out var progreso));
            Assert.Equal(2, progreso["ExplorarSector"]);
            Assert.Equal(1, progreso["ObservarNPC"]);
        }
    }
}
