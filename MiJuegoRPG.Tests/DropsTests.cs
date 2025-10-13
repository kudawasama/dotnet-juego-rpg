using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Objetos;
using Xunit;
using PJ = MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Tests
{
    public class DropsTests
    {
        class DummyEnemy : Enemigo
        {
            public DummyEnemy() : base("Dummy", 10, 1, 0, 0, 1, 0, 0) { }
        }

        [Fact]
        public void UniqueOnce_SeMarcaYNoRepite()
        {
            // Arrange
            var juego = new Juego();
            var pj = new PJ.Personaje("Tester");
            var e = new DummyEnemy();
            e.IdData = "dummy-1";
            var material = new Material("Insignia", "Normal", "Material");
            e.ObjetosDrop.Add(material);
            e.ProbabilidadesDrop[material.Nombre] = 1.0; // siempre cae
            e.DropsUniqueOnce.Add(material.Nombre);

            // Limpiar estado drops únicos
            DropsService.ImportarKeys(Array.Empty<string>());

            // Act 1: primer kill debe dar 1x y marcar
            RandomService.Instancia.SetSeed(1);
            e.Vida = 0;
            e.DarRecompensas(pj);

            // Assert 1
            Assert.Contains(pj.Inventario.NuevosObjetos, o => o.Objeto.Nombre == material.Nombre && o.Cantidad == 1);
            Assert.True(DropsService.Marcado(DropsService.ClaveUnique(e.IdData, material.Nombre)));

            // Act 2: segunda kill no debe dar nada por UniqueOnce
            RandomService.Instancia.SetSeed(1);
            e.Vida = 0;
            e.DarRecompensas(pj);

            // Assert 2: sigue habiendo 1 en total
            int total = 0;
            foreach (var oc in pj.Inventario.NuevosObjetos)
            {
                if (oc.Objeto.Nombre == material.Nombre)
                    total += oc.Cantidad;
            }

            Assert.Equal(1, total);
        }

        [Fact]
        public void CantidadesRespetaRangoYClamps()
        {
            var juego = new Juego();
            var pj = new PJ.Personaje("Tester");
            var e = new DummyEnemy();

            var matComun = new Material("Madera", "Pobre", "Material");
            e.ObjetosDrop.Add(matComun);
            e.ProbabilidadesDrop[matComun.Nombre] = 1.0; // forzar drop
            e.RangoCantidadDrop[matComun.Nombre] = (1, 10); // debe clamp a 5 por ser Pobre

            // Reset inventario/drops
            pj.Inventario.NuevosObjetos.Clear();
            DropsService.ImportarKeys(Array.Empty<string>());

            RandomService.Instancia.SetSeed(2);
            e.Vida = 0;
            e.DarRecompensas(pj);

            // Debe estar entre 1 y 5
            var oc = Assert.Single(pj.Inventario.NuevosObjetos);
            Assert.True(oc.Cantidad >= 1 && oc.Cantidad <= 5, $"Cantidad fuera de rango: {oc.Cantidad}");

            // Legendario clamp 3
            var e2 = new DummyEnemy();
            var matRaro = new Material("Gema", "Legendaria", "Material");
            e2.ObjetosDrop.Add(matRaro);
            e2.ProbabilidadesDrop[matRaro.Nombre] = 1.0;
            e2.RangoCantidadDrop[matRaro.Nombre] = (2, 10); // clamp a 3

            var pj2 = new PJ.Personaje("Tester2");
            RandomService.Instancia.SetSeed(3);
            e2.Vida = 0;
            e2.DarRecompensas(pj2);

            var oc2 = Assert.Single(pj2.Inventario.NuevosObjetos);
            Assert.True(oc2.Cantidad >= 2 && oc2.Cantidad <= 3, $"Cantidad fuera de rango/clamp: {oc2.Cantidad}");
        }
    }
}
