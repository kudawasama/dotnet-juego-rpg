using MiJuegoRPG.Motor;
using MiJuegoRPG.Interfaces;
using System;
using System.IO;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Juego juego = new Juego();
            juego.Iniciar();
            
            // Después de que termine el juego, preguntar si quiere guardar
            Console.WriteLine("\n¿Deseas guardar tu personaje? (s/n):");
            string respuesta = Console.ReadLine() ?? string.Empty;
            
            if (respuesta?.ToLower() == "s" || respuesta?.ToLower() == "si")
            {
                juego.GuardarPersonaje();
                Console.WriteLine("¡Personaje guardado exitosamente!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el juego: {ex.Message}");
        }
        
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}
