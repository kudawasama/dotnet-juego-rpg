using System;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MenuFueraCiudad
    {
        private Juego juego;
        private MenusJuego menusJuego;
        public MenuFueraCiudad(Juego juego, MenusJuego menusJuego)
        {
            this.juego = juego;
            this.menusJuego = menusJuego;
        }

        public void MostrarMenuFueraCiudad()
        {
            while (true)
            {
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
                Console.WriteLine("8. Menú principal fijo");
                Console.WriteLine("9. Salir del juego");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        juego.AvanzarTiempo(1);
                        juego.ExplorarSector();
                        break;
                    case "2":
                        juego.AvanzarTiempo(1);
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
                        juego.AvanzarTiempo(1);
                        var ciudadDesbloqueada = juego.estadoMundo.Ubicaciones.Find(u => u.Tipo == "Ciudad" && u.Desbloqueada);
                        if (ciudadDesbloqueada != null)
                        {
                            juego.ubicacionActual = ciudadDesbloqueada;
                            Console.WriteLine($"Regresaste a {ciudadDesbloqueada.Nombre}.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("No tienes acceso a ninguna ciudad desbloqueada en este momento.");
                            Console.WriteLine("¿Qué deseas hacer?");
                            Console.WriteLine("1. Volver al menú principal fijo");
                            Console.WriteLine("2. Explorar el sector actual");
                            var subopcion = Console.ReadLine();
                            if (subopcion == "1")
                                menusJuego.MostrarMenuPrincipalFijo();
                            else
                                juego.ExplorarSector();
                        }
                        break;
                    case "5":
                        juego.AvanzarTiempo(1);
                        juego.GestionarInventario();
                        break;
                    case "6":
                        juego.AvanzarTiempo(1);
                        juego.MostrarMenuGuardado();
                        break;
                    case "7":
                        juego.AvanzarTiempo(1);
                        juego.mapa.MostrarMapa();
                        Console.WriteLine("Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                    case "8":
                        juego.AvanzarTiempo(1);
                        menusJuego.MostrarMenuPrincipalFijo();
                        break;
                    case "9":
                        juego.AvanzarTiempo(1);
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
