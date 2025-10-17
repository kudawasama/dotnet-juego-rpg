using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Motor.Servicios;

namespace DebugApp
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine($"Working Directory: {Directory.GetCurrentDirectory()}");

            Console.WriteLine("\n=== Test Manual Carga ===");

            // Test manual de carga de archivo
            var accionesPathCorrecta = Path.Combine("..", "MiJuegoRPG", "DatosJuego", "config", "acciones_mundo.json");
            Console.WriteLine($"Archivo acciones existe: {File.Exists(accionesPathCorrecta)}");

            if (File.Exists(accionesPathCorrecta))
            {
                var json = File.ReadAllText(accionesPathCorrecta);
                Console.WriteLine($"JSON length: {json.Length}");
                Console.WriteLine($"JSON preview: {json.Substring(0, Math.Min(100, json.Length))}...");

                try
                {
                    var config = JsonSerializer.Deserialize<AccionesMundoConfig>(json);
                    Console.WriteLine($"Deserialización exitosa. Acciones count: {config?.Acciones?.Count ?? 0}");

                    if (config?.Acciones?.ContainsKey("robar_intento") == true)
                    {
                        var accionJson = config.Acciones["robar_intento"];
                        Console.WriteLine($"robar_intento encontrada:");
                        Console.WriteLine($"  ID: '{accionJson.Id}'");
                        Console.WriteLine($"  Tipo: '{accionJson.Tipo}'");
                        Console.WriteLine($"  CosteEnergia: {accionJson.CosteEnergia}");
                        Console.WriteLine($"  Energia (alias): {accionJson.Energia}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializando: {ex.Message}");
                }
            }

            Console.WriteLine("\n=== Test Servicio ===");

            var catalogService = new ActionWorldCatalogService();
            catalogService.CargarCatalogo();

            var accion = catalogService.ObtenerAccion("robar_intento");
            Console.WriteLine($"Servicio - ID: '{accion.Id}'");
            Console.WriteLine($"Servicio - Tipo: '{accion.Tipo}'");
            Console.WriteLine($"Servicio - CosteEnergia: {accion.CosteEnergia}");
            Console.WriteLine($"Servicio - Energia (alias): {accion.Energia}");
        }
    }
}
