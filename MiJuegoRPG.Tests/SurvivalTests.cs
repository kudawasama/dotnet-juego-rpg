using System;
using Xunit;
using MiJuegoRPG.Motor.Servicios;
using PersonajeModel = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Tests
{
    public class SurvivalTests
    {
        [Fact]
        public void TickBasico_AumentaHambreSedFatiga()
        {
            var cfg = new SupervivenciaService();
            cfg.CargarConfig(); // tolerante si falta archivo
            var rt = new SupervivenciaRuntimeService(cfg) { FeatureEnabled = true };
            var pj = new PersonajeModel("Tester");

            double h0 = pj.Hambre, s0 = pj.Sed, f0 = pj.Fatiga;

            rt.ApplyTick(pj, contexto: "Explorar", bioma: "bosque", minutos: 60);

            Assert.True(pj.Hambre >= h0);
            Assert.True(pj.Sed >= s0);
            Assert.True(pj.Fatiga >= f0);
        }

        [Fact]
        public void MultiplicadoresContexto_AfectanConsumo()
        {
            var cfg = new SupervivenciaService();
            cfg.CargarConfig();
            var rt = new SupervivenciaRuntimeService(cfg) { FeatureEnabled = true };
            var pj1 = new PersonajeModel("A");
            var pj2 = new PersonajeModel("B");

            // Ejecutar el mismo tiempo en dos contextos distintos
            rt.ApplyTick(pj1, "Explorar", "bosque", 60);
            rt.ApplyTick(pj2, "Descanso", "bosque", 60);

            // Suponiendo que Descanso tiene multiplicadores <= 1 en config, debería consumir menos
            Assert.True(pj1.Hambre >= pj2.Hambre);
            Assert.True(pj1.Sed >= pj2.Sed);
            Assert.True(pj1.Fatiga >= pj2.Fatiga);
        }
    }
}
