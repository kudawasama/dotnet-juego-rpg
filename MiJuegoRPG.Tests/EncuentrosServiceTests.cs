using System;
using Xunit;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Tests
{
    public class EncuentrosServiceTests
    {
        public EncuentrosServiceTests()
        {
            // Silenciar salida de UI para pruebas y crear una instancia de juego para controlar la hora del mundo
            Juego.UiFactory = () => new MiJuegoRPG.Motor.Servicios.SilentUserInterface();
        }

        [Fact]
        public void MiniJefe_NoAparece_SiNoCumpleMinKills()
        {
            // Arrange
            var juego = new Juego();
            var svc = new EncuentrosService { HoraActualProvider = () => 22 }; // Noche
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "TestBioma",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, Param = "lider_manada:lobo", MinKills = 12, HoraMin = 20, HoraMax = 4 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");

            int? GetKills(string clave) => 0; // No cumple MinKills

            // Act
            var res = svc.Resolver("TestBioma", pj, clave => GetKills(clave));

            // Assert
            Assert.NotEqual(TipoEncuentro.MiniJefe, res.Tipo);
        }

        [Fact]
        public void MiniJefe_Aparece_SiCumpleMinKills_YEnVentanaHoraria()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(1234);
            var svc = new EncuentrosService { HoraActualProvider = () => 23 };
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "TestBioma2",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 1 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, Param = "alfa_lobo:lobo", MinKills = 12, HoraMin = 20, HoraMax = 4 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string clave) => 20; // Cumple y excede

            // Act: probamos varias veces para estabilizar con el peso alto
            TipoEncuentro? visto = null;
            for (int i = 0; i < 10; i++)
            {
                var res = svc.Resolver("TestBioma2", pj, c => GetKills(c));
                if (res.Tipo == TipoEncuentro.MiniJefe) { visto = res.Tipo; break; }
            }

            // Assert
            Assert.Equal(TipoEncuentro.MiniJefe, visto);
        }

        [Fact]
        public void VentanasHorarias_Directa_Y_CruceMedianoche()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(42);
            var svcDia = new EncuentrosService { HoraActualProvider = () => 10 };
            svcDia.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaHora",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    // Válido solo de 08 a 18
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100, HoraMin = 8, HoraMax = 18 },
                    // Válido 22..02 (cruza medianoche)
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, HoraMin = 22, HoraMax = 2 }
                }
            });
            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string clave) => 0;

            // Día (10:00): debe salir CombateComunes
            var dia = svcDia.Resolver("BiomaHora", pj, c => GetKills(c));
            Assert.Equal(TipoEncuentro.CombateComunes, dia.Tipo);

            // Noche (23:00): debe salir MiniJefe (sin MinKills requeridos)
            var svcNoche = new EncuentrosService { HoraActualProvider = () => 23 };
            svcNoche.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaHora",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100, HoraMin = 8, HoraMax = 18 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, HoraMin = 22, HoraMax = 2 }
                }
            });
            var noche = svcNoche.Resolver("BiomaHora", pj, c => GetKills(c));
            Assert.Equal(TipoEncuentro.MiniJefe, noche.Tipo);

            // Cruce (02:00): sigue válido MiniJefe por ventana 22..02
            var svcMadrugada = new EncuentrosService { HoraActualProvider = () => 2 };
            svcMadrugada.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaHora",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100, HoraMin = 8, HoraMax = 18 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, HoraMin = 22, HoraMax = 2 }
                }
            });
            var madrugada = svcMadrugada.Resolver("BiomaHora", pj, c => GetKills(c));
            Assert.Equal(TipoEncuentro.MiniJefe, madrugada.Tipo);
        }

        [Fact]
        public void MiniJefe_Aparece_ConChanceUno_YMinKillsCumplidos()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(7);
            var svc = new EncuentrosService { HoraActualProvider = () => 12 };
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaChance1",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 1, Param = "lider_manada:lobo", MinKills = 13, Chance = 1.0 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string clave) => 20; // cumple

            // Act
            var res = svc.Resolver("BiomaChance1", pj, c => GetKills(c));

            // Assert
            Assert.Equal(TipoEncuentro.MiniJefe, res.Tipo);
        }

        [Fact]
        public void MiniJefe_NoAparece_ConChanceCero_AunqueCumplaMinKills()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(7);
            var svc = new EncuentrosService { HoraActualProvider = () => 12 };
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaChance0",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100 },
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 100, Param = "lider_manada:lobo", MinKills = 13, Chance = 0.0 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string clave) => 999; // cumple de sobra

            // Act (varias veces por si acaso)
            bool vistoMini = false;
            for (int i = 0; i < 10; i++)
            {
                var res = svc.Resolver("BiomaChance0", pj, c => GetKills(c));
                if (res.Tipo == TipoEncuentro.MiniJefe) { vistoMini = true; break; }
            }

            // Assert
            Assert.False(vistoMini);
        }

        [Fact]
        public void Cooldown_BloqueaRepeticion_Inmediata_DeEncuentroConChance()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(123);
            var now = new DateTime(2025, 1, 1, 12, 0, 0);
            var svc = new EncuentrosService
            {
                // Ambos proveedores usan la misma referencia temporal mutable
                FechaActualProvider = () => now,
                HoraActualProvider = () => now.Hour
            };
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaCooldown",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    // Evento con chance garantizada y cooldown de 60 minutos
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 1, Chance = 1.0, CooldownMinutos = 60, Param = "prueba" },
                    // Fallback sin chance
                    new EntradaEncuentro { Tipo = TipoEncuentro.CombateComunes, Peso = 100 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string _) => 0;

            // Act 1: Primera resolución debe disparar el MiniJefe
            var res1 = svc.Resolver("BiomaCooldown", pj, GetKills);

            // Act 2: Inmediatamente después, el cooldown debe bloquear al MiniJefe → sale el fallback
            var res2 = svc.Resolver("BiomaCooldown", pj, GetKills);

            // Assert
            Assert.Equal(TipoEncuentro.MiniJefe, res1.Tipo);
            Assert.Equal(TipoEncuentro.CombateComunes, res2.Tipo);
        }

        [Fact]
        public void Cooldown_Expira_YPermite_NuevoDisparo()
        {
            // Arrange
            var juego = new Juego();
            RandomService.Instancia.SetSeed(456);
            var now = new DateTime(2025, 1, 1, 12, 0, 0);
            var svc = new EncuentrosService
            {
                FechaActualProvider = () => now,
                HoraActualProvider = () => now.Hour
            };
            svc.RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "BiomaCooldown2",
                Entradas = new System.Collections.Generic.List<EntradaEncuentro>
                {
                    new EntradaEncuentro { Tipo = TipoEncuentro.MiniJefe, Peso = 1, Chance = 1.0, CooldownMinutos = 30, Param = "prueba2" },
                    new EntradaEncuentro { Tipo = TipoEncuentro.Materiales, Peso = 100 }
                }
            });

            var pj = new MiJuegoRPG.Personaje.Personaje("Tester");
            int? GetKills(string _) => 0;

            // Act: primer disparo (MiniJefe)
            var res1 = svc.Resolver("BiomaCooldown2", pj, GetKills);

            // Avanzamos el tiempo 29 minutos (sigue en cooldown)
            now = now.AddMinutes(29);
            var res2 = svc.Resolver("BiomaCooldown2", pj, GetKills);

            // Avanzamos el tiempo 2 minutos más (31 total, cooldown 30 → debería expirar)
            now = now.AddMinutes(2);
            var res3 = svc.Resolver("BiomaCooldown2", pj, GetKills);

            // Assert
            Assert.Equal(TipoEncuentro.MiniJefe, res1.Tipo);
            Assert.Equal(TipoEncuentro.Materiales, res2.Tipo); // aún bloqueado
            Assert.Equal(TipoEncuentro.MiniJefe, res3.Tipo);   // cooldown expirado
        }
    }
}
