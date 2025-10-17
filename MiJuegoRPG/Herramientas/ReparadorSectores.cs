using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Herramientas
{
    public class SectorDataNuevo
    {
        public required string Id
        {
            get; set;
        }
        public required string Nombre
        {
            get; set;
        }
        public required string Bioma
        {
            get; set;
        }
        public required string ParteCiudad
        {
            get; set;
        }
        public required bool CiudadPrincipal
        {
            get; set;
        }
        public required bool EsCentroCiudad
        {
            get; set;
        }
        public List<string> NodosRecoleccion { get; set; } = new();
        public Dictionary<string, object> Requisitos { get; set; } = new();
        public List<string> Enemigos { get; set; } = new();
        public List<string> Conexiones { get; set; } = new();
        public List<string> Eventos { get; set; } = new();
        public required string Descripcion
        {
            get; set;
        }
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
                    try
                    {
                        sector = JsonSerializer.Deserialize<SectorDataNuevo>(json);
                    }
                    catch { }
                    if (sector == null)
                    {
                        var nombreArchivo = Path.GetFileNameWithoutExtension(file);
                        sector = new SectorDataNuevo
                        {
                            Id = nombreArchivo,
                            Nombre = $"Sector {nombreArchivo}",
                            Bioma = "Campo",
                            ParteCiudad = "",
                            CiudadPrincipal = false,
                            EsCentroCiudad = false,
                            NodosRecoleccion = new List<string>(),
                            Requisitos = new Dictionary<string, object>(),
                            Enemigos = new List<string>(),
                            Conexiones = new List<string>(),
                            Eventos = new List<string>(),
                            Descripcion = "Sector autocompletado."
                        };
                        var opciones = new JsonSerializerOptions { WriteIndented = true };
                        File.WriteAllText(file, JsonSerializer.Serialize(sector, opciones));
                        Console.WriteLine($"[REPARADO] {file}");
                    }
                    else
                    {
                        // Si ya es válido, no tocarlo. Esto permite que GeneradorConexiones complete conexiones sin conflictos.
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
