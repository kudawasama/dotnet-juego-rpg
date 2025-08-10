using System;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MenuFueraCiudad
    {
        private Juego juego;
        public MenuFueraCiudad(Juego juego)
        {
            this.juego = juego;
        }

        public void MostrarMenuFueraCiudad()
        {
            while (true)
            {
                //Console.Clear();
                var sector = juego.mapa.UbicacionActual;
                Console.WriteLine($"=== {sector.Nombre} ===");
                Console.WriteLine(sector.Region);
                Console.WriteLine(sector.Descripcion);
                Console.WriteLine("1. Explorar sector");
                Console.WriteLine("2. Batallar");
                Console.WriteLine("3. Recolectar recursos");
                Console.WriteLine("4. Volver a la ciudad");
                Console.WriteLine("5. Gestionar Inventario");
                Console.WriteLine("6. Guardar/Cargar Personaje");
                Console.WriteLine("7. Ver mapa");
                Console.WriteLine("8. Salir del juego");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        juego.ExplorarSector();
                        break;
                    case "2":
                        juego.ComenzarCombate();
                        break;
                    case "3":
                        while (true)
                        {
                            Console.WriteLine("--- Recolección de Recursos ---");
                            Console.WriteLine("1. Recolectar hierbas");
                            Console.WriteLine("2. Minar");
                            Console.WriteLine("3. Talar");
                            Console.WriteLine("0. Volver");
                            Console.Write("Elige qué acción realizar: ");
                            var opcionRecoleccion = Console.ReadLine();
                            switch (opcionRecoleccion)
                            {
                                case "1":
                                    juego.RealizarAccionRecoleccion("Recolectar");
                                    break;
                                case "2":
                                    juego.RealizarAccionRecoleccion("Minar");
                                    break;
                                case "3":
                                    juego.RealizarAccionRecoleccion("Talar");
                                    break;
                                case "0":
                                    return;
                                default:
                                    Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
                                    Console.ReadKey();
                                    break;
                            }
                        }
                    case "4":
                        // Volver a la ciudad más cercana
                        // Aquí podrías implementar lógica para buscar la ciudad más cercana
                        Console.WriteLine("Regresando a la ciudad...");
                        // ...
                        return;
                    case "5":
                        juego.GestionarInventario();
                        break;
                    case "6":
                        juego.MostrarMenuGuardado();
                        break;
                    case "7":
                        juego.mapa.MostrarMapa();
                        Console.WriteLine("Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                    case "8":
                        Environment.Exit(0);
                        return;
                    default:
                        Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
