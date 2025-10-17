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
    public class CinturonesRepositoryTests
    {
        private readonly string _pjDatosDir;
        private readonly string _cinturonesBaseDir;
        public CinturonesRepositoryTests()
        {
            _pjDatosDir = PathProvider.PjDatosDir();
            _cinturonesBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "cinturones");
            Directory.CreateDirectory(_pjDatosDir);
            Directory.CreateDirectory(_cinturonesBaseDir);
        }

        [Fact]
        public void CargaJerarquica_NoVacia()
        {
            if (!Directory.EnumerateFiles(_cinturonesBaseDir, "*.json", SearchOption.AllDirectories).Any())
            {
                File.WriteAllText(Path.Combine(_cinturonesBaseDir, "cinturon_test_repo.json"),
                    "[{ \"Nombre\": \"Cinturon Test Repo\", \"BonificacionCarga\": 5, \"Nivel\": 1, \"TipoObjeto\": \"Cinturon\"}]");
            }
            var repo = new CinturonesRepository();
            var todos = repo.Todas();
            Assert.NotNull(todos);
            Assert.True(todos.Count > 0);
        }

        [Fact]
        public void Overlay_ReemplazaPorNombre()
        {
            var baseFile = Path.Combine(_cinturonesBaseDir, "cinturon_overlay_base.json");
            File.WriteAllText(baseFile,
                "[{ \"Nombre\": \"Cinturon Overlay\", \"BonificacionCarga\": 10, \"Nivel\": 2, \"TipoObjeto\": \"Cinturon\" }]");
            var overlayPath = Path.Combine(_pjDatosDir, "cinturones_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CinturonData { Nombre = "Cinturon Overlay", BonificacionCarga = 25, Nivel = 2, TipoObjeto = "Cinturon", Rareza = "Raro" }
            }));
            var repo = new CinturonesRepository();
            var item = repo.Todas().First(c => c.Nombre == "Cinturon Overlay");
            Assert.Equal(25, item.BonificacionCarga);
            Assert.Equal("Rara", item.Rareza);
        }

        [Theory]
        [InlineData("Comun", "Comun")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        public void Rareza_Normalizada(string input, string esperado)
        {
            var overlayPath = Path.Combine(_pjDatosDir, "cinturones_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CinturonData { Nombre = "Cinturon RZ", BonificacionCarga=3, Nivel=1, TipoObjeto="Cinturon", Rareza = input }
            }));
            var repo = new CinturonesRepository();
            var item = repo.Todas().First(c => c.Nombre == "Cinturon RZ");
            Assert.Equal(esperado, item.Rareza);
        }
    }
}
