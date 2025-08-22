using MiJuegoRPG.Motor;
using MiJuegoRPG.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Herramientas;

class Program
{
    static void Main(string[] args)
    {
        // Genera todos los archivos de regiones del mapa automáticamente al inicio
        //GeneradorSectores.CrearMapaCompleto(@"C:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa");

        // Cambia la ruta según la ubicación real de tus sectores
        //string rutaSectores = @"c:\Users\jose.cespedes\Documents\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa";
        //ValidadorSectores.ValidarSectores(rutaSectores);

        // Cambia la ruta según la ubicación real de tus sectores
        string rutaSectores = @"c:\Users\jose.cespedes\Documents\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa";
        ReparadorSectores.RepararSectores(rutaSectores);

        // Aquí puedes agregar la lógica para iniciar el juego
        try
        {
            Juego juego = new Juego();

            Console.WriteLine("¡Bienvenido a Mi Juego RPG!");
            Console.WriteLine("1. Crear personaje nuevo");
            Console.WriteLine("2. Cargar personaje guardado");
            Console.WriteLine("0. Salir");
            Console.Write("Selecciona una opción: ");
            string opcion = Console.ReadLine() ?? "1";

            switch (opcion)
            {
                case "2":
                    juego.CargarPersonaje();
                    if (juego.jugador == null)
                    {
                        Console.WriteLine("No se pudo cargar el personaje. Se creará uno nuevo.");
                        juego.CrearPersonaje();
                    }
                    break;
                case "1":
                    juego.CrearPersonaje();
                    break;
                case "0":
                    Console.WriteLine("¡Hasta pronto!");
                    return;
                default:
                    juego.CrearPersonaje();
                    break;
            }

            juego.Iniciar();

            // Preguntar si quiere guardar solo si el personaje fue creado o cargado correctamente
            if (juego.jugador != null)
            {
                Console.WriteLine("\n¿Deseas guardar tu personaje? (s/n):");
                string respuesta = Console.ReadLine() ?? string.Empty;

                if (respuesta.ToLower() == "s" || respuesta.ToLower() == "si")
                {
                    juego.GuardarPersonaje();
                    Console.WriteLine("¡Personaje guardado exitosamente!");
                }
            }
            else
            {
                Console.WriteLine("No hay personaje para guardar.");
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
