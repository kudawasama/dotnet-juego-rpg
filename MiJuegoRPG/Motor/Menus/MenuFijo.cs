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
            string opcion = "";
            var menusJuego = new MenusJuego(juego);
            Console.WriteLine("=== Menú Fijo ===");
            Console.WriteLine("1. Inventario");
            Console.WriteLine("2. Guardar partida");
            Console.WriteLine("3. Cargar partida");
            Console.WriteLine("0. Volver");
            opcion = InputService.LeerOpcion();
            switch (opcion)
            {
                case "1": menusJuego.MostrarInventario(); break;
                case "2": menusJuego.GuardarPartida(); break;
                case "3": menusJuego.CargarPartida(); break;
                case "0": return;
                default:
                    Console.WriteLine("Opción no válida.");
                    InputService.Pausa();
                    break;
            }
        }
    }
}
