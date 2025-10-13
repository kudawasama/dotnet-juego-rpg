using System;
using System.Collections.Generic;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.PjDatos;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class RecoleccionServiceTests
    {
        private static (Juego juego, Mapa mapa, SectorData sector, NodoRecoleccion nodo) CrearJuegoConSectorYNodo(int cooldown)
        {
            var juego = new Juego();
            var nodo = new NodoRecoleccion { Nombre = "NodoA", Cooldown = cooldown };
            var sector = new SectorData
            {
                Id = "S1",
                Nombre = "Sector Test",
                CiudadPrincipal = true,
                Conexiones = new List<string>(),
                NodosRecoleccion = new List<NodoRecoleccion> { nodo }
            };
            var mapa = new Mapa(new Dictionary<string, SectorData> { [sector.Id] = sector });
            juego.Mapa = mapa; // Sobre-escribimos el mapa del juego por el de prueba
            return (juego, mapa, sector, nodo);
        }

        [Fact]
        public void AlEntrarSector_AplicaCooldownsVigentes()
        {
            var (juego, mapa, sector, nodo) = CrearJuegoConSectorYNodo(cooldown: 60);
            var svc = juego.RecoleccionService;
            var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Importar cooldown con último uso hace 30s (debe seguir en cooldown)
            var data = new Dictionary<string, Dictionary<string, long>>
            {
                [sector.Id] = new Dictionary<string, long> { [nodo.Nombre!] = ahora - 30 }
            };
            svc.ImportarCooldownsMultiSector(data);

            // Al entrar sector, debe aplicar UltimoUso al nodo
            svc.AlEntrarSector(sector.Id);

            Assert.True(nodo.UltimoUso.HasValue);
            Assert.True(nodo.EstaEnCooldown());
            Assert.InRange(nodo.SegundosRestantesCooldown(), 28, 60);
        }

        [Fact]
        public void AlEntrarSector_LimpiaCooldownsExpirados()
        {
            var (juego, mapa, sector, nodo) = CrearJuegoConSectorYNodo(cooldown: 30);
            var svc = juego.RecoleccionService;
            var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Último uso hace 100s (ya expirado)
            var data = new Dictionary<string, Dictionary<string, long>>
            {
                [sector.Id] = new Dictionary<string, long> { [nodo.Nombre!] = ahora - 100 }
            };
            svc.ImportarCooldownsMultiSector(data);

            svc.AlEntrarSector(sector.Id);

            Assert.False(nodo.UltimoUso.HasValue); // no debería aplicarse
            var export = svc.ExportarCooldownsMultiSector();
            Assert.False(export.TryGetValue(sector.Id, out var dic) && dic.ContainsKey(nodo.Nombre!));
        }
    }
}
