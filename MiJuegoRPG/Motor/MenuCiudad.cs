using System;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MenuCiudad
    {
        public void MostrarMenuPrincipal()
        {
            while (true)
            {
                //Console.Clear();
                Console.WriteLine("=== Menú Principal de la Ciudad ===");
                Console.WriteLine($"Ubicación actual: {juego.mapa.UbicacionActual.Nombre}");
                Console.WriteLine("1. Explorar sector actual");
                Console.WriteLine("2. Viajar a otro sector");
                Console.WriteLine("3. Entrenar");
                Console.WriteLine("4. Ir a la Tienda");
                Console.WriteLine("5. Gestionar Inventario");
                Console.WriteLine("6. Guardar/Cargar Personaje");
                Console.WriteLine("7. Ver misiones y hablar con NPCs");
                Console.WriteLine("8. Ver mapa");
                Console.WriteLine("9. Salir del juego");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        MostrarMenuExplorarSector();
                        break;
                    case "2":
                        MostrarMenuViajarMapa();
                        break;
                    case "3":
                        juego.Entrenar();
                        break;
                    case "4":
                        juego.IrATienda();
                        break;
                    case "5":
                        juego.GestionarInventario();
                        break;
                    case "6":
                        juego.MostrarMenuGuardado();
                        break;
                    case "7":
                        juego.MostrarMenuMisionesNPC();
                        break;
                    case "8":
                        juego.mapa.MostrarMapa();
                        Console.WriteLine("Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                    case "9":
                        Environment.Exit(0);
                        return;
                    default:
                        Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }
        private void MostrarMenuExplorarSector()
        {
            var sector = juego.mapa.UbicacionActual;
            Console.WriteLine($"=== {sector.Nombre} ===");
            Console.WriteLine(sector.Region);
            Console.WriteLine(sector.Descripcion);
            Console.WriteLine("Enemigos posibles: " + (sector.Enemigos.Count > 0 ? string.Join(", ", sector.Enemigos) : "Ninguno"));
            // Mostrar eventos o ubicaciones internas si existen
            if (sector.Eventos != null && sector.Eventos.Count > 0)
            {
                Console.WriteLine("Eventos/Ubicaciones internas disponibles:");
                for (int i = 0; i < sector.Eventos.Count; i++)
                {
                    Console.WriteLine($"- {sector.Eventos[i]}");
                }
            }
            if (sector.Conexiones.Count > 0)
            {
                Console.WriteLine("Sectores disponibles para explorar:");
                for (int i = 0; i < sector.Conexiones.Count; i++)
                {
                    var destinoId = sector.Conexiones[i];
                    var destino = juego.mapa.ObtenerSectores().Find(s => s.Id == destinoId);
                    var nombreMostrar = destino != null ? destino.Nombre : destinoId;
                    Console.WriteLine($"{i + 1}. {nombreMostrar} ({destinoId})");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Elige una opción: ");
                var opcion = Console.ReadLine();
                int seleccion;
                if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= sector.Conexiones.Count)
                {
                    var destinoId = sector.Conexiones[seleccion - 1];
                    var destino = juego.mapa.ObtenerSectores().Find(s => s.Id == destinoId);
                    if (destino != null)
                    {
                        juego.mapa.MoverseA(destino.Id);
                        Console.WriteLine($"Viajaste a {destino.Nombre}.");
                        Console.WriteLine(destino.Descripcion);
                        if (destino.Tipo == null || !destino.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                        {
                            var menuFueraCiudad = new MenuFueraCiudad(juego);
                            menuFueraCiudad.MostrarMenuFueraCiudad();
                        }
                        else
                        {
                            Console.WriteLine("Presiona cualquier tecla para continuar...");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Sector no encontrado. Presiona cualquier tecla para volver...");
                        Console.ReadKey();
                    }
                }
                else if (seleccion == 0)
                {
                    // Volver al menú principal
                    return;
                }
                else
                {
                    Console.WriteLine("Opción no válida. Presiona cualquier tecla para volver...");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("No hay sectores conectados disponibles para explorar.");
                Console.WriteLine("Presiona cualquier tecla para volver al menú principal...");
                Console.ReadKey();
            }
        }

        private void MostrarMenuViajarMapa()
        {
            //Console.Clear();
            Console.WriteLine($"=== Viajar desde {juego.mapa.UbicacionActual.Nombre} ===");
            var adyacentes = juego.mapa.ObtenerSectoresAdyacentes();
            if (adyacentes.Count == 0)
            {
                Console.WriteLine("No hay sectores adyacentes disponibles.");
                Console.WriteLine("Presiona cualquier tecla para volver...");
                Console.ReadKey();
                return;
            }
            for (int i = 0; i < adyacentes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {adyacentes[i].Nombre} ({adyacentes[i].Id})");
            }
            Console.WriteLine("0. Volver");
            Console.Write("Elige tu destino: ");
            var opcion = Console.ReadLine();
            if (int.TryParse(opcion, out int seleccion) && seleccion > 0 && seleccion <= adyacentes.Count)
            {
                var destino = adyacentes[seleccion - 1];
                juego.mapa.MoverseA(destino.Id);
                Console.WriteLine($"Viajaste a {destino.Nombre}.");
                Console.WriteLine(destino.Descripcion);
                // Si el destino no es ciudad, mostrar menú fuera de ciudad
                if (destino.Tipo == null || !destino.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                {
                    var menuFueraCiudad = new MenuFueraCiudad(juego);
                    menuFueraCiudad.MostrarMenuFueraCiudad();
                }
                else
                {
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    

        private Juego juego;

        public MenuCiudad(Juego juego)
        {
            this.juego = juego;
        }

    // Eliminado MostrarMenu() duplicado


    // Métodos delegados directamente a Juego

            // Este es el menú de guardado que ya tenías, pero encapsulado en un método.
    // Menú de guardado/carga ahora se gestiona desde Juego

    // Métodos de guardado/carga eliminados, delegados a Juego
    }
}