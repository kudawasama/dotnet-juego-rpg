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

        // DEBUG OFF: Reparación opcional de sectores desactivada temporalmente para no ejecutarse al inicio
        /*
        // Reparación opcional de sectores (solo si se pasa argumento --reparar-sectores)
        if (args != null && Array.Exists(args, a => a.Equals("--reparar-sectores", StringComparison.OrdinalIgnoreCase)))
        {
            var raiz = Juego.ObtenerRutaRaizProyecto();
            var rutaSectores = Path.Combine(raiz, "MiJuegoRPG", "DatosJuego", "mapa", "SectoresMapa");
            if (Directory.Exists(rutaSectores))
            {
                Console.WriteLine($"[INIT] Reparando sectores en: {rutaSectores}");
                ReparadorSectores.RepararSectores(rutaSectores);
            }
            else
            {
                Console.WriteLine($"[WARN] Ruta de sectores no encontrada: {rutaSectores} (se omite reparación)");
            }
        }
        */

        // Aquí puedes agregar la lógica para iniciar el juego
        try
        {
            Juego juego = new Juego();
            var ui = juego.Ui;

            ui.WriteLine("¡Bienvenido a Mi Juego RPG!");
            ui.WriteLine("1. Crear personaje nuevo");
            ui.WriteLine("2. Cargar personaje guardado");
            ui.WriteLine("0. Salir");
            string opcion = MiJuegoRPG.Motor.InputService.LeerOpcion("Selecciona una opción: ") ?? "1";

            switch (opcion)
            {
                case "2":
                    juego.CargarPersonaje();
                    if (juego.jugador == null)
                    {
                        ui.WriteLine("No se pudo cargar el personaje. Se creará uno nuevo.");
                        juego.CrearPersonaje();
                    }
                    break;
                case "1":
                    juego.CrearPersonaje();
                    break;
                case "0":
                    ui.WriteLine("¡Hasta pronto!");
                    return;
                default:
                    juego.CrearPersonaje();
                    break;
            }

            juego.Iniciar();

            // Preguntar si quiere guardar solo si el personaje fue creado o cargado correctamente
            if (juego.jugador != null)
            {
                var respuesta = MiJuegoRPG.Motor.InputService.LeerOpcion("\n¿Deseas guardar tu personaje? (s/n): ") ?? string.Empty;
                if (respuesta.Equals("s", StringComparison.OrdinalIgnoreCase) || respuesta.Equals("si", StringComparison.OrdinalIgnoreCase))
                {
                    juego.GuardarPersonaje();
                    ui.WriteLine("¡Personaje guardado exitosamente!");
                }
            }
            else
            {
                ui.WriteLine("No hay personaje para guardar.");
            }
            ui.WriteLine("Presiona cualquier tecla para salir...");
            MiJuegoRPG.Motor.InputService.Pausa();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el juego: {ex.Message}");
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
