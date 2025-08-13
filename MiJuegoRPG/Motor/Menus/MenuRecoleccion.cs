using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Acciones;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuRecoleccion
    {
        private Juego juego;
        public MenuRecoleccion(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuRecoleccion(ref bool salir)
        {
            string opcion = "";
            var accionRecoleccion = new AccionRecoleccion(juego);
            while (!salir)
            {
                Console.WriteLine("=== Menú de Recolección ===");
                Console.WriteLine("1. Buscar materiales");
                Console.WriteLine("2. Volver");
                Console.Write("Selecciona una opción: ");
                var key = Console.ReadKey(true);
                opcion = key.KeyChar.ToString();
                switch (opcion)
                {
                    case "1":
                        accionRecoleccion.RealizarAccionRecoleccion("material");
                        break;
                    case "2":
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    }
}
