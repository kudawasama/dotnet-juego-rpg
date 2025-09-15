using System.Linq;
using MiJuegoRPG.Interfaces;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class UsarPocionAccionTests
    {
        class PjBasico : MiJuegoRPG.Personaje.Personaje
        {
            public PjBasico(string nombre) : base(nombre) { }
        }

        [Fact]
        public void UsarPocion_CuraYConsumeDelInventario()
        {
            var pj = new PjBasico("Heroe");
            pj.VidaMaxima = 100;
            pj.Vida = 50;
            var pocion = new MiJuegoRPG.Objetos.Pocion("Poción Pequeña", 30);
            pj.Inventario.AgregarObjeto(pocion, 2, pj);

            // Selecciona la primera poción
            var accion = new MiJuegoRPG.Motor.Acciones.UsarPocionAccion(0);
            var res = accion.Ejecutar(pj, pj);

            Assert.Contains("Usaste", res.Mensajes[0]);
            Assert.Equal(80, pj.Vida); // 50 + 30
            // Inventario decrementado
            var entry = pj.Inventario.NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == "Poción Pequeña");
            Assert.NotNull(entry);
            Assert.Equal(1, entry!.Cantidad);
        }

        [Fact]
        public void UsarPocion_NoExcedeVidaMaxima_YAgotaStack()
        {
            var pj = new PjBasico("Heroe");
            pj.VidaMaxima = 100;
            pj.Vida = 95;
            var pocion = new MiJuegoRPG.Objetos.Pocion("Poción Pequeña", 30);
            pj.Inventario.AgregarObjeto(pocion, 1, pj);

            var accion = new MiJuegoRPG.Motor.Acciones.UsarPocionAccion(0);
            var res = accion.Ejecutar(pj, pj);

            Assert.Equal(100, pj.Vida); // No sobrecura
            Assert.DoesNotContain(pj.Inventario.NuevosObjetos, o => o.Objeto.Nombre == "Poción Pequeña");
        }
    }
}
