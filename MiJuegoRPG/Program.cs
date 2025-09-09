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
        // === BLOQUE UTILIDADES DEBUG (comentado / protegido) ===
        // Las siguientes herramientas de generación / validación de sectores y reparación
        // se dejan dentro de un bloque de compilación condicional para evitar ejecuciones
        // accidentales en inicio estándar. Para usarlas, compilar en Debug y descomentar
        // la(s) línea(s) deseada(s), o pasar el flag correspondiente.
#if DEBUG
        // Generar todos los sectores del mapa (costo: puede sobrescribir archivos existentes)
        //GeneradorSectores.CrearMapaCompleto(@"C:\\RUTA\\A\\TU\\REPO\\MiJuegoRPG\\DatosJuego\\mapa\\SectoresMapa");

        // Validar sectores (estructura / referencias)
        //string rutaSectoresDebug = @"C:\\RUTA\\A\\TU\\REPO\\MiJuegoRPG\\DatosJuego\\mapa\\SectoresMapa";
        //ValidadorSectores.ValidarSectores(rutaSectoresDebug);

        // Reparación opcional de sectores solo en Debug si se pasa argumento --reparar-sectores
        if (args != null && Array.Exists(args, a => a.Equals("--reparar-sectores", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var raiz = Juego.ObtenerRutaRaizProyecto();
                var rutaSectores = Path.Combine(raiz, "MiJuegoRPG", "DatosJuego", "mapa", "SectoresMapa");
                if (Directory.Exists(rutaSectores))
                {
                    Console.WriteLine($"[DEBUG] Reparando sectores en: {rutaSectores}");
                    ReparadorSectores.RepararSectores(rutaSectores);
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Ruta de sectores no encontrada: {rutaSectores} (omitida reparación)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG][ERROR] Falló reparación sectores: {ex.Message}");
            }
        }
#endif
        // === FIN BLOQUE DEBUG ===

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
