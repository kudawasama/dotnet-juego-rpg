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
    public class CascosRepositoryTests
    {
        // Comentario fuerza recompilación
        private readonly string _pjDatosDir;
        private readonly string _cascosBaseDir;

        public CascosRepositoryTests()
        {
            _pjDatosDir = PathProvider.PjDatosDir();
            _cascosBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "cascos");
            Directory.CreateDirectory(_pjDatosDir);
            Directory.CreateDirectory(_cascosBaseDir);
        }

        [Fact]
        public void CargaJerarquica_NoVacia()
        {
            if (!Directory.EnumerateFiles(_cascosBaseDir, "*.json", SearchOption.AllDirectories).Any())
            {
                File.WriteAllText(Path.Combine(_cascosBaseDir, "casco_test_repo.json"),
                    "[{ \"Nombre\": \"Casco Test Repo\", \"Defensa\": 5, \"Nivel\": 1, \"TipoObjeto\": \"Casco\"}]");
            }
            var repo = new CascosRepository();
            var todos = repo.Todas();
            Assert.NotNull(todos);
            Assert.True(todos.Count > 0, "Se esperaba al menos 1 casco cargado");
        }

        [Fact]
        public void Overlay_ReemplazaPorNombre()
        {
            var baseFile = Path.Combine(_cascosBaseDir, "casco_overlay_base.json");
            File.WriteAllText(baseFile,
                "[{ \"Nombre\": \"Casco Overlay\", \"Defensa\": 10, \"Nivel\": 2, \"TipoObjeto\": \"Casco\" }]");
            var overlayPath = Path.Combine(_pjDatosDir, "cascos_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CascoData { Nombre = "Casco Overlay", Defensa = 20, Nivel = 2, TipoObjeto = "Casco", Rareza = "Raro" }
            }));
            var repo = new CascosRepository();
            var todos = repo.Todas();
            var item = todos.FirstOrDefault(c => c.Nombre.Equals("Casco Overlay", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(item);
            Assert.Equal(20, item!.Defensa);
            Assert.Equal("Rara", item.Rareza); // normalizado
        }

        [Theory]
        [InlineData("Comun", "Comun")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        public void Rareza_Normalizada(string input, string esperado)
        {
            var overlayPath = Path.Combine(_pjDatosDir, "cascos_overlay.json");
            File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
            {
                new CascoData { Nombre = "Casco RZ", Defensa=3, Nivel=1, TipoObjeto="Casco", Rareza = input }
            }));
            var repo = new CascosRepository();
            var item = repo.Todas().First(c => c.Nombre == "Casco RZ");
            Assert.Equal(esperado, item.Rareza);
        }
    }
}
