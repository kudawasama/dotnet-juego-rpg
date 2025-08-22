using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MiJuegoRPG.Herramientas
{
    public class SectorData
    {
        public required string Id { get; set; }
        public required string Nombre { get; set; }
        public required string Tipo { get; set; }
        public required string Descripcion { get; set; }
        public required int NivelMinimo { get; set; }
        public required int NivelMaximo { get; set; }
        public required List<string> Enemigos { get; set; }
        public required List<string> Conexiones { get; set; }
    }

    public static class ValidadorSectores
    {
        public static void ValidarSectores(string rutaBase)
        {
            var sectores = new Dictionary<string, SectorData>();
            foreach (var file in Directory.GetFiles(rutaBase, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var sector = JsonSerializer.Deserialize<SectorData>(json);
                    if (sector == null)
                    {
                        Console.WriteLine($"[ERROR] No se pudo deserializar: {file}");
                        continue;
                    }
                    sectores[sector.Id] = sector;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {file}: {ex.Message}");
                }
            }

            // Validar campos obligatorios y conexiones
            foreach (var kv in sectores)
            {
                var s = kv.Value;
                if (string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Nombre) || s.Conexiones == null)
                {
                    Console.WriteLine($"[ERROR] Sector con campos faltantes: {s.Id} ({s.Nombre})");
                }
                if (s.Conexiones != null)
                {
                    foreach (var conn in s.Conexiones)
                    {
                        if (!sectores.ContainsKey(conn))
                        {
                            Console.WriteLine($"[ERROR] Conexión inválida en sector {s.Id} -> {conn}");
                        }
                    }
                }
            }
            Console.WriteLine($"Validación completada. Sectores revisados: {sectores.Count}");
        }
    }
}
