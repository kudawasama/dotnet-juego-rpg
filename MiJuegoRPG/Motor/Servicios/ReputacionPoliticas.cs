using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MiJuegoRPG.Motor.Servicios
{
    public static class ReputacionPoliticas
    {
        private class BandaRepConfig
        {
            public string Nombre { get; set; } = string.Empty;
            public int Min
            {
                get; set;
            }
            public int Max
            {
                get; set;
            }
            public string Color { get; set; } = "Gray";
        }

        private class PoliticaBloqueo
        {
            public List<string> BloqueaFac { get; set; } = new();
            public List<string> BloqueaGlobal { get; set; } = new();
        }

        private class Politicas
        {
            public PoliticaBloqueo NPC { get; set; } = new();
            public PoliticaBloqueo Tienda { get; set; } = new();
        }

        private static List<BandaRepConfig>? bandas;
        private static Politicas? politicas;
        private static bool cargado = false;

        private static void Cargar()
        {
            if (cargado)
                return;
            cargado = true;
            try
            {
                // Cargar bandas
                var bandasPath = BuscarRuta(new[]
                {
                    PathProvider.ConfigPath("reputacion_bandas.json"),
                    Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego","config","reputacion_bandas.json"),
                    Path.Combine(AppContext.BaseDirectory, "DatosJuego","config","reputacion_bandas.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego","config","reputacion_bandas.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "DatosJuego","config","reputacion_bandas.json")
                });
                if (!string.IsNullOrEmpty(bandasPath) && File.Exists(bandasPath))
                {
                    var json = File.ReadAllText(bandasPath);
                    bandas = JsonSerializer.Deserialize<List<BandaRepConfig>>(json);
                }
            }
            catch { }

            try
            {
                // Cargar políticas
                var politicasPath = BuscarRuta(new[]
                {
                    PathProvider.ConfigPath("reputacion_politicas.json"),
                    Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego","config","reputacion_politicas.json"),
                    Path.Combine(AppContext.BaseDirectory, "DatosJuego","config","reputacion_politicas.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego","config","reputacion_politicas.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "DatosJuego","config","reputacion_politicas.json")
                });
                if (!string.IsNullOrEmpty(politicasPath) && File.Exists(politicasPath))
                {
                    var json = File.ReadAllText(politicasPath);
                    politicas = JsonSerializer.Deserialize<Politicas>(json);
                }
            }
            catch { }

            // Fallback por defecto si no hay archivos
            bandas ??= new List<BandaRepConfig>
            {
                new() { Nombre = "Perseguido", Min = -9999, Max = -151, Color = "DarkRed" },
                new() { Nombre = "Hostil",     Min = -150,  Max = -51,  Color = "Red" },
                new() { Nombre = "Tenso",      Min = -50,   Max = -1,   Color = "DarkYellow" },
                new() { Nombre = "Neutral",    Min = 0,     Max = 49,   Color = "Gray" },
                new() { Nombre = "Amistoso",   Min = 50,    Max = 99,   Color = "Green" },
                new() { Nombre = "Aliado",     Min = 100,   Max = 9999, Color = "Cyan" }
            };
            politicas ??= new Politicas
            {
                NPC = new PoliticaBloqueo
                {
                    BloqueaFac = new List<string> { "Hostil", "Perseguido" },
                    BloqueaGlobal = new List<string> { "Perseguido" }
                },
                Tienda = new PoliticaBloqueo
                {
                    BloqueaFac = new List<string> { "Hostil", "Perseguido" },
                    BloqueaGlobal = new List<string> { "Perseguido" }
                }
            };
        }

        private static string? BuscarRuta(IEnumerable<string> candidatos)
        {
            foreach (var r in candidatos)
            {
                if (File.Exists(r))
                    return r;
            }
            return null;
        }

        private static ConsoleColor ParseColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return ConsoleColor.Gray;
            return Enum.TryParse<ConsoleColor>(color, true, out var c) ? c : ConsoleColor.Gray;
        }

        public static (string nombre, ConsoleColor color) BandaPorValor(int valor)
        {
            Cargar();
            foreach (var b in bandas!)
            {
                if (valor >= b.Min && valor <= b.Max)
                {
                    return (b.Nombre, ParseColor(b.Color));
                }
            }
            // Seguridad: si no cae en ningún rango, retornar Neutral
            return ("Neutral", ConsoleColor.Gray);
        }

        public static bool DebeBloquearNPC(int repFac, int repGlobal)
        {
            Cargar();
            var bandaFac = BandaPorValor(repFac).nombre;
            var bandaGlobal = BandaPorValor(repGlobal).nombre;
            return politicas!.NPC.BloqueaFac.Any(b => string.Equals(b, bandaFac, StringComparison.OrdinalIgnoreCase))
                || politicas!.NPC.BloqueaGlobal.Any(b => string.Equals(b, bandaGlobal, StringComparison.OrdinalIgnoreCase));
        }

        public static bool DebeBloquearTienda(int repFac, int repGlobal)
        {
            Cargar();
            var bandaFac = BandaPorValor(repFac).nombre;
            var bandaGlobal = BandaPorValor(repGlobal).nombre;
            return politicas!.Tienda.BloqueaFac.Any(b => string.Equals(b, bandaFac, StringComparison.OrdinalIgnoreCase))
                || politicas!.Tienda.BloqueaGlobal.Any(b => string.Equals(b, bandaGlobal, StringComparison.OrdinalIgnoreCase));
        }
    }
}
