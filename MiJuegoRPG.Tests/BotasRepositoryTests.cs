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
    public class BotasRepositoryTests
    {
        private readonly string _pjDatosDir;
        private readonly string _botasBaseDir;

        public BotasRepositoryTests()
        {
            _pjDatosDir = PathProvider.PjDatosDir();
            _botasBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "botas");
            Directory.CreateDirectory(_pjDatosDir);
            Directory.CreateDirectory(_botasBaseDir);
        }

        [Fact]
        public void CargaJerarquica_NoVacia()
        {
            if (!Directory.EnumerateFiles(_botasBaseDir, "*.json", SearchOption.AllDirectories).Any())
            {
                File.WriteAllText(Path.Combine(_botasBaseDir, "botas_repo_test.json"),
                    "[{ \"Nombre\": \"Botas Repo Test\", \"Defensa\": 5, \"Nivel\": 1, \"TipoObjeto\": \"Botas\"}]");
            }
            var repo = new BotasRepository();
            var todas = repo.Todas();
            Assert.NotNull(todas);
            Assert.True(todas.Count > 0, "Se esperaba al menos 1 botas cargadas");
        }

        [Fact]
        public void Overlay_ReemplazaPorNombre()
        {
            var baseFile = Path.Combine(_botasBaseDir, "botas_overlay_base.json");
            File.WriteAllText(baseFile,
                "[{ \"Nombre\": \"Botas Overlay\", \"Defensa\": 10, \"Nivel\": 2, \"TipoObjeto\": \"Botas\" }]");
            var overlayPath = Path.Combine(_pjDatosDir, "botas_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new BotasData { Nombre = "Botas Overlay", Defensa = 25, Nivel = 2, TipoObjeto = "Botas", Rareza = "Raro" }
            }));
            var repo = new BotasRepository();
            var item = repo.Todas().FirstOrDefault(a => a.Nombre.Equals("Botas Overlay", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(item);
            Assert.Equal(25, item!.Defensa);
            Assert.Equal("Rara", item.Rareza);
        }

        [Theory]
        [InlineData("Comun", "Comun")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        public void Rareza_Normalizada(string input, string esperado)
        {
            var overlayPath = Path.Combine(_pjDatosDir, "botas_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new BotasData { Nombre = "Botas RZ", Defensa=3, Nivel=1, TipoObjeto="Botas", Rareza = input }
            }));
            var repo = new BotasRepository();
            var item = repo.Todas().First(a => a.Nombre == "Botas RZ");
            Assert.Equal(esperado, item.Rareza);
        }
    }
}
