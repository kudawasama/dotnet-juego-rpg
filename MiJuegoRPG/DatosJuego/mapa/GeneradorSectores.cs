using System;
using System.IO;
using System.Text.Json;

namespace Herramientas
{
    public class GeneradorSectores
    {
        public static void CrearMapaCompleto(string rutaBase)
        {
            int filas = 54;
            int columnas = 54;
            string sector = "Sector_0";
            string rutaSector = Path.Combine(rutaBase, sector);
            Directory.CreateDirectory(rutaSector);
            for (int fila = 0; fila <= filas; fila++)
            {
                for (int columna = 0; columna <= columnas; columna++)
                {
                    string nombreArchivo = $"{fila}_{columna}.json";
                    string rutaArchivo = Path.Combine(rutaSector, nombreArchivo);
                    var region = new
                    {
                        nombre = $"Región {fila}_{columna}",
                        bioma = "Indefinido",
                        nodosRecoleccion = new string[] {},
                        enemigos = new string[] {},
                        eventos = new string[] {}
                    };
                    string json = JsonSerializer.Serialize(region, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(rutaArchivo, json);
                }
            }
        }
    }
}
