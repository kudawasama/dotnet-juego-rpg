using System.Linq;
using Xunit;
using MiJuegoRPG.Motor.Servicios.Repos;
using MiJuegoRPG.Objetos;
using System.IO;

namespace MiJuegoRPG.Tests
{
    public class MaterialRepositoryTests
    {
        [Fact]
        public void MaterialRepository_LoadsAndResolvesByNombre()
        {
            // Arrange: asegurar existe archivo materiales.json (si no, crear mínimo temporal)
            var ruta = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("materiales.json");
            if (!File.Exists(ruta))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
                File.WriteAllText(ruta, "[ { \"Nombre\": \"Mineral de Hierro\", \"Rareza\": \"Normal\", \"Categoria\": \"herrero\" } ]");
            }

            var repo = new MaterialRepository();

            // Act
            var all = repo.GetAll();
            Assert.True(all.Count > 0);
            var primero = all.First();
            var fetched = repo.GetByNombre(primero.Nombre);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(primero.Nombre, fetched!.Nombre);
            // Adaptador dominio
            var dom = repo.ToDomain(primero.Nombre);
            Assert.NotNull(dom);
            Assert.Equal(primero.Nombre, dom!.Nombre);
        }

        [Fact]
        public void MaterialRepository_CargaJerarquica_SinOverlay()
        {
            // Asegurar ausencia temporal de overlay renombrando si existe
            var ruta = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("materiales.json");
            string? backup = null;
            if (File.Exists(ruta))
            {
                backup = ruta + ".bak_test";
                File.Move(ruta, backup, true);
            }
            try
            {
                var repo = new MaterialRepository();
                var all = repo.GetAll();
                Assert.True(all.Count > 0); // De la base jerárquica
                Assert.Contains(all, m => m.Nombre.Equals("Mineral de Hierro", System.StringComparison.OrdinalIgnoreCase));
                var hierro = repo.GetByNombre("Mineral de Hierro");
                Assert.NotNull(hierro);
                Assert.False(string.IsNullOrWhiteSpace(hierro!.Rareza));
            }
            finally
            {
                // Restaurar overlay si estaba
                if (backup != null && File.Exists(backup))
                {
                    if (File.Exists(ruta)) File.Delete(ruta);
                    File.Move(backup, ruta);
                }
            }
        }

        [Fact]
        public void MaterialRepository_Overlay_Sobrescribe_Base()
        {
            var ruta = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("materiales.json");
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            // Definir overlay con rareza distinta para un material base conocido
            File.WriteAllText(ruta, "[ { \"Nombre\": \"Mineral de Hierro\", \"Rareza\": \"Legendario\", \"Categoria\": \"test\" } ]");
            var repo = new MaterialRepository();
            var hierro = repo.GetByNombre("Mineral de Hierro");
            Assert.NotNull(hierro);
            Assert.Equal("Legendaria", hierro!.Rareza); // normalizador
            Assert.Equal("test", hierro.Categoria);
        }
    }
}
