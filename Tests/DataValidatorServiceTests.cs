using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Validacion;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class DataValidatorServiceTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly string _datosJuego;

        public DataValidatorServiceTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "RPGTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            // Crear estructura mínima
            _datosJuego = Path.Combine(_tempRoot, "MiJuegoRPG", "DatosJuego");
            Directory.CreateDirectory(_datosJuego);
            // Requeridos por validator (algunos vacíos permitidos)
            Directory.CreateDirectory(Path.Combine(_datosJuego, "mapa"));
            Directory.CreateDirectory(Path.Combine(_datosJuego, "misiones"));
            Directory.CreateDirectory(Path.Combine(_datosJuego, "npcs"));
            Directory.CreateDirectory(Path.Combine(_datosJuego, "enemigos"));
            Directory.CreateDirectory(Path.Combine(_datosJuego, "Equipo"));
            Directory.CreateDirectory(Path.Combine(_datosJuego, "pociones"));
            PathProvider.OverrideRootForTests(_tempRoot); // redirigir
        }

        [Fact]
        public void Armas_DuplicadoNombre_EsError()
        {
            // armas.json array
            var armasPath = Path.Combine(_datosJuego, "Equipo", "armas.json");
            var armasJson = "[ { \"Nombre\": \"Espada Oxidada\", \"Rareza\": \"Normal\", \"Perfeccion\": 90 }, { \"Nombre\": \"Espada Oxidada\", \"Rareza\": \"Rara\", \"Perfeccion\": 95 } ]";
            File.WriteAllText(armasPath, armasJson);

            var res = DataValidatorService.ValidarArmasBasico();
            Assert.True(res.Errores >= 1, "Debe detectar nombre duplicado");
            Assert.Contains(res.Mensajes, m => m.Contains("Espada Oxidada") && m.Contains("duplicado"));
        }

        [Fact]
        public void Armas_PerfeccionMayor100_Warn()
        {
            var armasPath = Path.Combine(_datosJuego, "Equipo", "armas.json");
            var armasJson = "[ { \"Nombre\": \"Hoja Perfecta\", \"Rareza\": \"Rara\", \"Perfeccion\": 150 } ]";
            File.WriteAllText(armasPath, armasJson);

            var res = DataValidatorService.ValidarArmasBasico();
            Assert.Equal(0, res.Errores);
            Assert.True(res.Advertencias >= 1);
            Assert.Contains(res.Mensajes, m => m.Contains("Hoja Perfecta") && m.Contains("Perfeccion >100"));
        }

        [Fact]
        public void Pociones_DuplicadoNombre_Error()
        {
            var pocionesDir = Path.Combine(_datosJuego, "pociones");
            Directory.CreateDirectory(pocionesDir);
            var pocionesPath = Path.Combine(pocionesDir, "pociones.json");
            File.WriteAllText(pocionesPath, "[ { \"Nombre\": \"Pocion Pequeña\", \"Rareza\": \"Normal\" }, { \"Nombre\": \"Pocion Pequeña\", \"Rareza\": \"Rara\" } ]");
            // armas.json requerido para evitar advertencia irrelevante en otras pruebas
            File.WriteAllText(Path.Combine(_datosJuego, "Equipo", "armas.json"), "[]");

            var res = DataValidatorService.ValidarPocionesBasico();
            Assert.True(res.Errores >= 1);
            Assert.Contains(res.Mensajes, m => m.Contains("Pocion Pequeña") && m.Contains("duplicado"));
        }

        [Fact]
        public void EquipoNoArma_PerfeccionFueraRango_Error()
        {
            var eqDir = Path.Combine(_datosJuego, "Equipo", "armaduras");
            Directory.CreateDirectory(eqDir);
            var file = Path.Combine(eqDir, "armadura_x.json");
            File.WriteAllText(file, "{ \"Nombre\": \"Armadura Rota\", \"Perfeccion\": 250, \"Rareza\": \"Normal\" }");
            File.WriteAllText(Path.Combine(_datosJuego, "Equipo", "armas.json"), "[]");

            var res = DataValidatorService.ValidarEquipoNoArmaBasico();
            Assert.True(res.Errores >= 1);
            Assert.Contains(res.Mensajes, m => m.Contains("Armadura Rota") && m.Contains("Perfeccion fuera"));
        }

        [Fact]
        public void EquipoNoArma_RangoInvertidoNivel_Error()
        {
            var eqDir = Path.Combine(_datosJuego, "Equipo", "botas");
            Directory.CreateDirectory(eqDir);
            var file = Path.Combine(eqDir, "botas_x.json");
            File.WriteAllText(file, "{ \"Nombre\": \"Botas Extrañas\", \"Perfeccion\": 80, \"Rareza\": \"Normal\", \"NivelMin\": 50, \"NivelMax\": 10 }");
            File.WriteAllText(Path.Combine(_datosJuego, "Equipo", "armas.json"), "[]");

            var res = DataValidatorService.ValidarEquipoNoArmaBasico();
            Assert.True(res.Errores >= 1);
            Assert.Contains(res.Mensajes, m => m.Contains("Botas Extrañas") && m.Contains("NivelMin>NivelMax"));
        }

        [Fact]
        public void EquipoNoArma_RarezaDesconocida_Warn()
        {
            var eqDir = Path.Combine(_datosJuego, "Equipo", "cascos");
            Directory.CreateDirectory(eqDir);
            var file = Path.Combine(eqDir, "casco_x.json");
            File.WriteAllText(file, "{ \"Nombre\": \"Casco Misterioso\", \"Perfeccion\": 70, \"Rareza\": \"Mistica\" }");
            File.WriteAllText(Path.Combine(_datosJuego, "Equipo", "armas.json"), "[]");

            var res = DataValidatorService.ValidarEquipoNoArmaBasico();
            Assert.True(res.Advertencias >= 1);
            Assert.Contains(res.Mensajes, m => m.Contains("Casco Misterioso") && m.Contains("Rareza desconocida"));
        }

        public void Dispose()
        {
            try { PathProvider.OverrideRootForTests(null); } catch { }
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, true); } catch { }
        }
    }
}
