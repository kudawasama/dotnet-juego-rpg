using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuFueraCiudad
    {
        
        private Juego juego;
        public MenuFueraCiudad(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuFueraCiudad(ref bool salir)
        {
            // Recuperación pasiva de energía antes de mostrar menú
            if (juego.jugador != null)
            {
                juego.energiaService.RecuperacionPasiva(juego.jugador);
            }

            string opcion = "";
            while (!salir)
            {
                Console.WriteLine(juego.FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {juego.mapa.UbicacionActual.Nombre}");
                Console.WriteLine("=== Menú Fuera de Ciudad ===");
                Console.WriteLine("1. Explorar");
                Console.WriteLine("2. Recolectar");
                Console.WriteLine("5. Viajar");
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1": juego.ExplorarSector(); break;
                    case "2": juego.MostrarMenuRecoleccion(); break;
                    case "5":
                        juego.MostrarMenuRutas();
                        return;
                    case "9": juego.MostrarMenuFijo(ref salir); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
