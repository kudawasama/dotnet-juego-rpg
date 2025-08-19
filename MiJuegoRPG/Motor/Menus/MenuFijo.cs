using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuFijo
    {
        private Juego juego;
        public MenuFijo(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuFijo(ref bool salir)
        {
            while (true)
            {
                Console.WriteLine("\n=== Menú Fijo ===");
                Console.WriteLine("1. Estado del personaje");
                Console.WriteLine("2. Guardar personaje");
                Console.WriteLine("3. Volver al menú principal");
                Console.WriteLine("0. Salir del juego");
                string opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1":
                        if (juego.jugador != null) juego.MostrarEstadoPersonaje(juego.jugador);
                        else Console.WriteLine("No hay personaje cargado.");
                        InputService.Pausa();
                        break;
                    case "2":
                        juego.GuardarPersonaje();
                        Console.WriteLine("¡Personaje guardado exitosamente!");
                        InputService.Pausa();
                        break;
                    case "3":
                        // Al volver, mostrar el menú adecuado según la ubicación actual
                        if (juego.ubicacionActual != null && juego.ubicacionActual.Tipo != null && juego.ubicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                        {
                            juego.MostrarMenuCiudad(ref salir);
                        }
                        else if (juego.ubicacionActual != null && !string.IsNullOrWhiteSpace(juego.ubicacionActual.Tipo))
                        {
                            juego.MostrarMenuFueraCiudad(ref salir);
                        }
                        else
                        {
                            salir = true;
                        }
                        return;
                    case "0":
                        salir = true;
                        Console.WriteLine("¡Gracias por jugar!");
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
