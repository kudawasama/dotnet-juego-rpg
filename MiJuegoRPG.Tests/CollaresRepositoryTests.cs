using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Motor.Servicios.Repos;
using MiJuegoRPG.PjDatos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class CollaresRepositoryTests
    {
        private readonly string _pjDatosDir;
        private readonly string _collaresBaseDir;
        public CollaresRepositoryTests()
        {
            _pjDatosDir = PathProvider.PjDatosDir();
            _collaresBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "collares");
            Directory.CreateDirectory(_pjDatosDir);
            Directory.CreateDirectory(_collaresBaseDir);
        }

        [Fact]
        public void CargaJerarquica_NoVacia()
        {
            if (!Directory.EnumerateFiles(_collaresBaseDir, "*.json", SearchOption.AllDirectories).Any())
            {
                File.WriteAllText(Path.Combine(_collaresBaseDir, "collar_test_repo.json"),
                    "[{ \"Nombre\": \"Collar Test Repo\", \"BonificacionDefensa\": 2, \"BonificacionEnergia\": 5, \"Nivel\": 1, \"TipoObjeto\": \"Collar\"}]");
            }
            var repo = new CollaresRepository();
            Assert.True(repo.Todas().Count > 0);
        }

        [Fact]
        public void Overlay_ReemplazaPorNombre()
        {
            var baseFile = Path.Combine(_collaresBaseDir, "collar_overlay_base.json");
            File.WriteAllText(baseFile,
                "[{ \"Nombre\": \"Collar Overlay\", \"BonificacionDefensa\": 3, \"BonificacionEnergia\": 10, \"Nivel\": 2, \"TipoObjeto\": \"Collar\" }]");
            var overlayPath = Path.Combine(_pjDatosDir, "collares_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CollarData { Nombre = "Collar Overlay", BonificacionDefensa = 8, BonificacionEnergia=25, Nivel = 2, TipoObjeto = "Collar", Rareza = "Raro" }
            }));
            var repo = new CollaresRepository();
            var item = repo.Todas().First(c => c.Nombre == "Collar Overlay");
            Assert.Equal(8, item.BonificacionDefensa);
            Assert.Equal(25, item.BonificacionEnergia);
            Assert.Equal("Rara", item.Rareza);
        }

        [Theory]
        [InlineData("Comun", "Comun")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        public void Rareza_Normalizada(string input, string esperado)
        {
            var overlayPath = Path.Combine(_pjDatosDir, "collares_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CollarData { Nombre = "Collar RZ", BonificacionDefensa=1, BonificacionEnergia=2, Nivel=1, TipoObjeto="Collar", Rareza = input }
            }));
            var repo = new CollaresRepository();
            var item = repo.Todas().First(c => c.Nombre == "Collar RZ");
            Assert.Equal(esperado, item.Rareza);
        }
    }
}
