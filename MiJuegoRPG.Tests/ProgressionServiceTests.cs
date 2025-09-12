using System;
using System.Collections.Generic;
using MiJuegoRPG.Dominio;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Personaje;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class ProgressionServiceTests
    {
        private static Personaje.Personaje CrearPjBasico(int nivel = 1)
        {
            var pj = new Personaje.Personaje("Test");
            pj.Nivel = nivel;
            pj.AtributosBase = new AtributosBase();
            // Inicializar experiencia de atributos requerida por ProgressionService.AplicarEntrenamiento
            foreach (Atributo atr in Enum.GetValues(typeof(Atributo)))
                pj.ExperienciaAtributos[atr] = new ExpAtributo { Progreso = 0, Requerida = 1.0 };
            return pj;
        }

        [Fact]
        public void AplicarExpExploracion_AumentaPercepcion_YBonusAgilidad_enPrimeraVisita()
        {
            var svc = new ProgressionService(rutaConfig: null) { Verbose = false };
            var pj = CrearPjBasico();
            double per0 = pj.AtributosBase.Percepcion;
            double agi0 = pj.AtributosBase.Agilidad;

            svc.AplicarExpExploracion(pj, primeraVisita: true);

            Assert.True(pj.AtributosBase.Percepcion > per0);
            Assert.True(pj.AtributosBase.Agilidad > agi0);
        }

        [Fact]
        public void AplicarEntrenamiento_IncrementaProgreso_YSubeAtributoCuandoSuperaRequerida()
        {
            var svc = new ProgressionService(rutaConfig: null) { Verbose = false };
            var pj = CrearPjBasico();
            double fuerza0 = pj.AtributosBase.Fuerza;
            // Forzar requerimiento bajo para probar subida
            pj.ExperienciaAtributos[Atributo.Fuerza].Requerida = 0.0001;

            svc.AplicarEntrenamiento(pj, Atributo.Fuerza, minutos: 1);

            Assert.True(pj.AtributosBase.Fuerza >= fuerza0); // puede subir o acumular
            // Dado Requerida muy baja, debería producir subida y reinicio de progreso
            Assert.Equal(0, pj.ExperienciaAtributos[Atributo.Fuerza].Progreso, 5);
            Assert.True(pj.ExperienciaAtributos[Atributo.Fuerza].Requerida > 0.0001);
        }

        [Theory]
        [InlineData(TipoRecoleccion.Recolectar, Atributo.Percepcion, Atributo.Inteligencia)]
        [InlineData(TipoRecoleccion.Minar, Atributo.Fuerza, Atributo.Resistencia)]
        [InlineData(TipoRecoleccion.Talar, Atributo.Fuerza, Atributo.Destreza)]
        public void AplicarExpRecoleccion_AumentaAtributosEsperados(TipoRecoleccion tipo, Atributo a1, Atributo a2)
        {
            var svc = new ProgressionService(rutaConfig: null) { Verbose = false };
            var pj = CrearPjBasico();

            double v1_0 = ObtenerValor(pj, a1);
            double v2_0 = ObtenerValor(pj, a2);

            svc.AplicarExpRecoleccion(pj, tipo);

            Assert.True(ObtenerValor(pj, a1) > v1_0);
            Assert.True(ObtenerValor(pj, a2) > v2_0);
        }

        private static double ObtenerValor(Personaje.Personaje pj, Atributo atr)
        {
            var a = pj.AtributosBase;
            return atr switch
            {
                Atributo.Fuerza => a.Fuerza,
                Atributo.Inteligencia => a.Inteligencia,
                Atributo.Destreza => a.Destreza,
                Atributo.Resistencia => a.Resistencia,
                Atributo.Defensa => a.Defensa,
                Atributo.Vitalidad => a.Vitalidad,
                Atributo.Agilidad => a.Agilidad,
                Atributo.Suerte => a.Suerte,
                Atributo.Percepcion => a.Percepcion,
                _ => 0
            };
        }
    }
}
