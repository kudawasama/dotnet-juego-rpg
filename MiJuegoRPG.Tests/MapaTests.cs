using System.Collections.Generic;
using MiJuegoRPG.Motor;
using MiJuegoRPG.PjDatos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class MapaTests
    {
        private static Dictionary<string, SectorData> CrearMapaBasico()
        {
            var s1 = new SectorData { Id = "A", Nombre = "Ciudad Central", CiudadPrincipal = true, Conexiones = new List<string> { "B", "C" } };
            var s2 = new SectorData { Id = "B", Nombre = "Pradera", Conexiones = new List<string> { "A" } };
            var s3 = new SectorData { Id = "C", Nombre = "Bosque", Conexiones = new List<string> { "A" } };
            return new Dictionary<string, SectorData>
            {
                [s1.Id] = s1,
                [s2.Id] = s2,
                [s3.Id] = s3,
            };
        }

        [Fact]
        public void Constructor_EscogeCiudadPrincipalComoUbicacionInicial_YLaMarcaDescubierta()
        {
            var map = new Mapa(CrearMapaBasico());
            Assert.Equal("A", map.UbicacionActual.Id);
            Assert.True(map.SectoresDescubiertos.ContainsKey("A") && map.SectoresDescubiertos["A"]);
        }

        [Fact]
        public void ObtenerSectoresAdyacentes_DevuelveConexionesValidas()
        {
            var map = new Mapa(CrearMapaBasico());
            var ady = map.ObtenerSectoresAdyacentes();
            Assert.Collection(ady,
                it => Assert.Equal("B", it.Id),
                it => Assert.Equal("C", it.Id));
        }

        [Fact]
        public void MoverseA_SoloPermiteConexionesDesdeUbicacionActual_YMarcaDescubrimiento()
        {
            var map = new Mapa(CrearMapaBasico());
            // Movimiento válido A -> B
            Assert.True(map.MoverseA("B"));
            Assert.Equal("B", map.UbicacionActual.Id);
            Assert.True(map.SectoresDescubiertos.ContainsKey("B") && map.SectoresDescubiertos["B"]);

            // Movimiento inválido B -> C (no hay conexión directa)
            Assert.False(map.MoverseA("C"));
            Assert.Equal("B", map.UbicacionActual.Id);

            // Regreso válido B -> A y luego a C
            Assert.True(map.MoverseA("A"));
            Assert.True(map.MoverseA("C"));
            Assert.Equal("C", map.UbicacionActual.Id);
            Assert.True(map.SectoresDescubiertos.ContainsKey("C") && map.SectoresDescubiertos["C"]);
        }
    }
}
