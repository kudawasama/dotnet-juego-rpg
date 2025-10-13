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
    public class PantalonesRepositoryTests
    {
        private readonly string _pjDatosDir;
        private readonly string _pantalonesBaseDir;
        public PantalonesRepositoryTests()
        {
            _pjDatosDir = PathProvider.PjDatosDir();
            _pantalonesBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "pantalones");
            Directory.CreateDirectory(_pjDatosDir);
            Directory.CreateDirectory(_pantalonesBaseDir);
        }

        [Fact]
        public void CargaJerarquica_NoVacia()
        {
            if (!Directory.EnumerateFiles(_pantalonesBaseDir, "*.json", SearchOption.AllDirectories).Any())
            {
                File.WriteAllText(Path.Combine(_pantalonesBaseDir, "pantalon_test_repo.json"),
                    "[{ \"Nombre\": \"Pantalon Test Repo\", \"Defensa\": 4, \"Nivel\": 1, \"TipoObjeto\": \"Pantalon\"}]");
            }
            var repo = new PantalonesRepository();
            Assert.True(repo.Todas().Count > 0);
        }

        [Fact]
        public void Overlay_ReemplazaPorNombre()
        {
            var baseFile = Path.Combine(_pantalonesBaseDir, "pantalon_overlay_base.json");
            File.WriteAllText(baseFile,
                "[{ \"Nombre\": \"Pantalon Overlay\", \"Defensa\": 10, \"Nivel\": 2, \"TipoObjeto\": \"Pantalon\" }]");
            var overlayPath = Path.Combine(_pjDatosDir, "pantalones_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new PantalonData { Nombre = "Pantalon Overlay", Defensa = 22, Nivel = 2, TipoObjeto = "Pantalon", Rareza = "Raro" }
            }));
            var repo = new PantalonesRepository();
            var item = repo.Todas().First(p => p.Nombre == "Pantalon Overlay");
            Assert.Equal(22, item.Defensa);
            Assert.Equal("Rara", item.Rareza);
        }

        [Theory]
        [InlineData("Comun", "Comun")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        public void Rareza_Normalizada(string input, string esperado)
        {
            var overlayPath = Path.Combine(_pjDatosDir, "pantalones_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new PantalonData { Nombre = "Pantalon RZ", Defensa=3, Nivel=1, TipoObjeto="Pantalon", Rareza = input }
            }));
            var repo = new PantalonesRepository();
            var item = repo.Todas().First(p => p.Nombre == "Pantalon RZ");
            Assert.Equal(esperado, item.Rareza);
        }
    }
}
