using System;
using System.IO;
using System.Linq;
using MiJuegoRPG.Motor.Servicios.Repos;
using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class ArmaRepositoryTests
    {
        [Fact]
        public void ArmaRepository_CargaJerarquica_NoVacio_SiDirExiste()
        {
            var dir = PathProvider.ArmasDir();
            if (!Directory.Exists(dir))
            {
                // Crear fixture mínima temporal
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "ArmaPrueba.json"), "{ \"Nombre\": \"Espada Prueba\", \"Tipo\": \"Espada\", \"Daño\": 5 }");
            }
            var repo = new ArmaRepository();
            var todas = repo.Todas();
            Assert.NotEmpty(todas);
        }

        [Fact]
        public void ArmaRepository_Overlay_Sobrescribe_Danio()
        {
            var dir = PathProvider.ArmasDir();
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "ArmaOverlayBase.json"), "{ \"Nombre\": \"Arco Test\", \"Tipo\": \"Arco\", \"Daño\": 3 }");
            // Overlay
            Directory.CreateDirectory(PathProvider.PjDatosDir());
            File.WriteAllText(PathProvider.PjDatosPath("armas_overlay.json"), "[{ \"Nombre\": \"Arco Test\", \"Tipo\": \"Arco\", \"Daño\": 10 }]");

            var repo = new ArmaRepository();
            var arco = repo.Todas().First(a => a.Nombre == "Arco Test");
            Assert.Equal(10, arco.Daño); // overlay reemplaza daño
        }
    }
}