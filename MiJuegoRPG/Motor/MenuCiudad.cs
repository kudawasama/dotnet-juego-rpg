using System;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MenuCiudad
    {
        private Juego juego;

        public MenuCiudad(Juego juego)
        {
            this.juego = juego;
        }

        public void MostrarMenu()
        {
            Console.Clear(); // Limpia la consola para que el menú se vea más limpio
            Console.WriteLine("=== CIUDAD INICIAL ===");
            Console.WriteLine("¿Qué deseas hacer?");
            Console.WriteLine("1. Aventura (Explorar el sector)");
            Console.WriteLine("2. Entrenar");
            Console.WriteLine("3. Ir a la Tienda");
            Console.WriteLine("4. Gestionar Inventario");
            Console.WriteLine("5. Guardar/Cargar Personaje");
            Console.WriteLine("6. Salir del juego");

            Console.Write("Elige una opción: ");
            var opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    Console.WriteLine("¡Te aventuras en lo desconocido!");
                    juego.ExplorarSector(); // Llamada al método que crearemos en Juego.cs
                    break;
                case "2":
                    Console.WriteLine("Te diriges a la Escuela de Entrenamiento.");
                    juego.Entrenar(); // Llamada al método que crearemos en Juego.cs
                    break;
                case "3":
                    Console.WriteLine("Entras a la Tienda del Mercader.");
                    juego.IrATienda(); // Llamada al método que crearemos en Juego.cs
                    break;
                case "4":
                    Console.WriteLine("Revisas tu inventario.");
                    juego.GestionarInventario(); // Llamada al método que crearemos en Juego.cs
                    break;
                case "5":
                    // El menú de Guardar/Cargar es un submenú
                    juego.MostrarMenuGuardado(); // Llamada al método que crearemos en Juego.cs
                    break;
                case "6":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }


        public void ExplorarSector()
        {
            Console.WriteLine("Explorando... ¡Se ha iniciado un combate!");
            juego.ComenzarCombate(); // Llama al método que ya habíamos acordado
        }

        public void Entrenar()
            {
                Console.WriteLine("Entrenamiento aún no implementado.");
            }

        public void IrATienda()
            {
                Console.WriteLine("Tienda aún no implementada.");
            }

        public void GestionarInventario()
            {
                Console.WriteLine("Inventario aún no implementado.");
            }

            // Este es el menú de guardado que ya tenías, pero encapsulado en un método.
        public void MostrarMenuGuardado()
            {
                Console.WriteLine("Menú de Guardado y Carga:");
                Console.WriteLine("1. Guardar personaje");
                Console.WriteLine("2. Cargar personaje");
                Console.WriteLine("3. Volver");
                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        GuardarPersonaje();
                        break;
                    case "2":
                        CargarPersonaje();
                        break;
                    case "3":
                        return; // Vuelve al menú principal
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }

        private void GuardarPersonaje()
        {
            Console.WriteLine("Funcionalidad de guardar personaje aún no implementada.");
            // Aquí puedes agregar la lógica para guardar el personaje
        }

        private void CargarPersonaje()
        {
            Console.WriteLine("Funcionalidad de cargar personaje aún no implementada.");
            // Aquí puedes agregar la lógica para cargar el personaje
        }
    }
}