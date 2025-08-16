using System;
using System.Collections.Generic;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MotorRutas
    {
        private Juego juego;
        public MotorRutas(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuRutas()
        {
            var ubicacionActual = juego.ubicacionActual;
            var estadoMundo = juego.estadoMundo;
            Console.Clear();
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine($"Rutas disponibles desde {ubicacionActual.Nombre}:");
            int i = 1;
            foreach (var ruta in ubicacionActual.Rutas)
            {
                Console.WriteLine($"{i}. {ruta.Nombre} {(ruta.Desbloqueada ? "(Desbloqueada)" : "(Bloqueada)")}");
                i++;
            }
            Console.WriteLine($"{i}. Volver");
            var opcion = Console.ReadLine();
            int seleccion;
            if (int.TryParse(opcion, out seleccion))
            {
                if (seleccion > 0 && seleccion <= ubicacionActual.Rutas.Count)
                {
                    var rutaElegida = ubicacionActual.Rutas[seleccion - 1];
                    if (rutaElegida.Desbloqueada)
                    {
                        var nuevaUbicacion = estadoMundo.Ubicaciones.Find(u => u.Nombre == rutaElegida.Destino);
                        if (nuevaUbicacion != null)
                        {
                            juego.ubicacionActual = nuevaUbicacion;
                            Console.WriteLine($"Viajaste a {juego.ubicacionActual.Nombre}.");
                            Console.WriteLine(juego.ubicacionActual.Descripcion);
                            Console.WriteLine("Presiona cualquier tecla para ver los sectores y eventos disponibles...");
                            Console.ReadKey();
                            juego.MostrarMenuPorUbicacion();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("No se encontró la ubicación de destino. No tienes acceso a esa zona.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("La ruta está bloqueada. No tienes acceso a esa zona.");
                    }
                }
                else if (seleccion == i)
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Opción no válida. No tienes acceso a esa opción.");
                }
            }
            else
            {
                Console.WriteLine("Opción no válida. No tienes acceso a esa opción.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}
