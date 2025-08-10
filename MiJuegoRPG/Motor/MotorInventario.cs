using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MotorInventario
    {
        private Juego juego;
        public MotorInventario(Juego juego)
        {
            this.juego = juego;
        }
        public void GestionarInventario()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("=== Inventario ===");
            if (juego.jugador.Inventario.NuevosObjetos.Count == 0)
            {
                Console.WriteLine("Tu inventario está vacío.");
            }
            else
            {
                for (int i = 0; i < juego.jugador.Inventario.NuevosObjetos.Count; i++)
                {
                    var obj = juego.jugador.Inventario.NuevosObjetos[i];
                    Console.WriteLine($"{i + 1}. {obj.Nombre}");
                }
                Console.WriteLine("¿Quieres usar una poción? Ingresa el número o presiona Enter para salir.");
                var opcion = Console.ReadLine();
                int seleccion;
                if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= juego.jugador.Inventario.NuevosObjetos.Count)
                {
                    var obj = juego.jugador.Inventario.NuevosObjetos[seleccion - 1];
                    if (obj is Pocion pocion)
                    {
                        juego.jugador.Vida = Math.Min(juego.jugador.Vida + pocion.Curacion, juego.jugador.VidaMaxima);
                        juego.jugador.Inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                        Console.WriteLine($"Usaste {pocion.Nombre} y recuperaste {pocion.Curacion} puntos de vida.");
                    }
                    else
                    {
                        Console.WriteLine($"No puedes usar {obj.Nombre}.");
                    }
                }
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }
    }
}
