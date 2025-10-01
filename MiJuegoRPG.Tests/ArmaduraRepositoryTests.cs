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
	public class ArmaduraRepositoryTests
	{
		// Nota: este comentario fuerza recompilación para evitar que el runner reutilice ensamblado en caché.
		private readonly string _pjDatosDir;
		private readonly string _armadurasBaseDir;

		public ArmaduraRepositoryTests()
		{
			_pjDatosDir = PathProvider.PjDatosDir();
			_armadurasBaseDir = Path.Combine(PathProvider.DatosJuegoDir(), "Equipo", "armaduras");
			Directory.CreateDirectory(_pjDatosDir);
			Directory.CreateDirectory(_armadurasBaseDir);
		}

		[Fact]
		public void CargaJerarquica_NoVacia()
		{
			// Arrange: asegurar al menos un archivo base mínimo si carpeta estuviera vacía en entorno CI
			if (!Directory.EnumerateFiles(_armadurasBaseDir, "*.json", SearchOption.AllDirectories).Any())
			{
				File.WriteAllText(Path.Combine(_armadurasBaseDir, "armadura_test_repo.json"),
					"[{ \"Nombre\": \"Armadura Test Repo\", \"Defensa\": 10, \"Nivel\": 1, \"TipoObjeto\": \"Armadura\"}]");
			}

			var repo = new ArmaduraRepository();

			// Act
			var todas = repo.Todas();

			// Assert
			Assert.NotNull(todas);
			Assert.True(todas.Count > 0, "Se esperaba al menos 1 armadura cargada");
		}

		[Fact]
		public void Overlay_ReemplazaPorNombre()
		{
			// Arrange base
			var baseFile = Path.Combine(_armadurasBaseDir, "armadura_overlay_base.json");
			File.WriteAllText(baseFile,
				"[{ \"Nombre\": \"Armadura Overlay\", \"Defensa\": 25, \"Nivel\": 2, \"TipoObjeto\": \"Armadura\" }]");

			// Overlay que modifica Defensa
			var overlayPath = Path.Combine(_pjDatosDir, "armaduras_overlay.json");
			File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
			{
				new ArmaduraData {
					Nombre = "Armadura Overlay", Defensa = 40, Nivel = 2, TipoObjeto = "Armadura", Rareza = "Raro" }
			}));

			var repo = new ArmaduraRepository();

			// Act
			var todas = repo.Todas();
			var item = todas.FirstOrDefault(a => a.Nombre.Equals("Armadura Overlay", StringComparison.OrdinalIgnoreCase));

			// Assert
			Assert.NotNull(item);
			Assert.Equal(40, item!.Defensa); // reemplazado por overlay
			Assert.Equal("Rara", item.Rareza); // Normalizado por RarezaNormalizer (Raro -> Rara)
		}

		[Theory]
		[InlineData("Comun","Comun")]
		[InlineData("Raro","Rara")]
		[InlineData("Epico","Epica")]
		public void Rareza_Normalizada(string input, string esperado)
		{
			var overlayPath = Path.Combine(_pjDatosDir, "armaduras_overlay.json");
			File.WriteAllText(overlayPath, JsonSerializer.Serialize(new[]
			{
				new ArmaduraData { Nombre = "Armadura RZ", Defensa=5, Nivel=1, TipoObjeto="Armadura", Rareza = input }
			}));
			var repo = new ArmaduraRepository();

			var item = repo.Todas().First(a => a.Nombre == "Armadura RZ");
			Assert.Equal(esperado, item.Rareza);
		}
	}
}