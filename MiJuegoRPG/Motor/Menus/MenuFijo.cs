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
                juego.Ui.WriteLine("\n=== Menú Fijo ===");
                juego.Ui.WriteLine("1. Estado del personaje");
                juego.Ui.WriteLine("2. Guardar personaje");
                juego.Ui.WriteLine("3. Volver al menú principal");
                juego.Ui.WriteLine("0. Salir del juego");
                string opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1":
                        if (juego.jugador != null) juego.MostrarEstadoPersonaje(juego.jugador);
                        else juego.Ui.WriteLine("No hay personaje cargado.");
                        InputService.Pausa();
                        break;
                    case "2":
                        juego.GuardarPersonaje();
                        juego.Ui.WriteLine("¡Personaje guardado exitosamente!");
                        InputService.Pausa();
                        break;
                    case "3":
                        salir = true;
                        return;
                    case "0":
                        salir = true;
                        juego.Ui.WriteLine("¡Gracias por jugar!");
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
