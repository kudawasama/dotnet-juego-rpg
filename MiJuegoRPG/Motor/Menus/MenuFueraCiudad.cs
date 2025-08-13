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
            string opcion = "";
            while (!salir)
            {
                Console.WriteLine(juego.FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {juego.ubicacionActual.Nombre}");
                Console.WriteLine("=== Menú Fuera de Ciudad ===");
                Console.WriteLine("1. Explorar");
                Console.WriteLine("2. Recolectar");
                Console.WriteLine("3. Volver a la ciudad");
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("Selecciona una opción: ");
                var key = Console.ReadKey(true);
                opcion = key.KeyChar.ToString();
                switch (opcion)
                {
                    case "1": juego.ExplorarSector(); break;
                    case "2": juego.MostrarMenuRecoleccion(); break;
                    case "3":
                        var ciudadDesbloqueada = juego.estadoMundo.Ubicaciones.Find(u => u.Tipo == "Ciudad" && u.Desbloqueada);
                        if (ciudadDesbloqueada != null)
                        {
                            juego.ubicacionActual = ciudadDesbloqueada;
                            Console.WriteLine("Has regresado a la ciudad.");
                        }
                        else
                            Console.WriteLine("No tienes acceso a ninguna ciudad desbloqueada en este momento.");
                        juego.MostrarMenuFijo(ref salir);
                        break;
                    case "9": juego.MostrarMenuFijo(ref salir); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        juego.MostrarMenuFijo(ref salir);
                        break;
                }
            }
        }
    }
}
