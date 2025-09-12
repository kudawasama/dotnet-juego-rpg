using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Herramientas
{
    public class SectorDataNuevo
    {
        public required string Id { get; set; }
        public required string nombre { get; set; }
        public required string bioma { get; set; }
        public required string parteCiudad { get; set; }
        public required bool ciudadPrincipal { get; set; }
        public required bool esCentroCiudad { get; set; }
        public List<string> nodosRecoleccion { get; set; } = new();
        public Dictionary<string, object> Requisitos { get; set; } = new();
        public List<string> enemigos { get; set; } = new();
        public List<string> Conexiones { get; set; } = new();
        public List<string> eventos { get; set; } = new();
        public required string descripcion { get; set; }
    }

    public static class ReparadorSectores
    {
        public static void RepararSectores(string rutaBase)
        {
            foreach (var file in Directory.GetFiles(rutaBase, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    SectorDataNuevo? sector = null;
                    try { sector = JsonSerializer.Deserialize<SectorDataNuevo>(json); } catch { }
                    if (sector == null)
                    {
                        var nombreArchivo = Path.GetFileNameWithoutExtension(file);
                        sector = new SectorDataNuevo
                        {
                            Id = nombreArchivo,
                            nombre = $"Sector {nombreArchivo}",
                            bioma = "Campo",
                            parteCiudad = "",
                            ciudadPrincipal = false,
                            esCentroCiudad = false,
                            nodosRecoleccion = new List<string>(),
                            Requisitos = new Dictionary<string, object>(),
                            enemigos = new List<string>(),
                            Conexiones = new List<string>(),
                            eventos = new List<string>(),
                            descripcion = "Sector autocompletado."
                        };
                        var opciones = new JsonSerializerOptions { WriteIndented = true };
                        File.WriteAllText(file, JsonSerializer.Serialize(sector, opciones));
                        Console.WriteLine($"[REPARADO] {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {file}: {ex.Message}");
                }
            }
            Console.WriteLine("Reparación completada.");
        }
    }
}
