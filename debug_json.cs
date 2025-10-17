using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Motor.Servicios;

class DebugJson
{
    static void Main()
    {
        Console.WriteLine("=== Debug JSON Acciones ===");

        var catalogService = new ActionWorldCatalogService();
        catalogService.CargarCatalogo();

        var accion = catalogService.ObtenerAccion("robar_intento");
        Console.WriteLine($"Acción: {accion.Id}");
        Console.WriteLine($"Tipo: {accion.Tipo}");
        Console.WriteLine($"CosteEnergia: {accion.CosteEnergia}");
        Console.WriteLine($"Energia (alias): {accion.Energia}");
        Console.WriteLine($"CosteTiempoMin: {accion.CosteTiempoMin}");
        Console.WriteLine($"Tiempo (alias): {accion.Tiempo}");
        Console.WriteLine($"Cooldown: {accion.Cooldown}");

        Console.WriteLine("\n=== Verificar JSON Raw ===");
        var jsonPath = Path.Combine("MiJuegoRPG", "DatosJuego", "config", "acciones_mundo.json");
        if (File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            Console.WriteLine($"JSON encontrado: {json.Substring(0, Math.Min(200, json.Length))}...");
        }
        else
        {
            Console.WriteLine($"JSON NO encontrado en: {jsonPath}");
            Console.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
        }
    }
}
