using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Enemigos;

namespace MiJuegoRPG.Motor.Menus
{
    /// <summary>
    /// Menú de acciones avanzadas durante el combate. Permite seleccionar acciones tácticas y especiales.
    /// </summary>
    public static class MenuCombateAvanzado
    {
        /// <summary>
        /// Muestra el submenú de acciones avanzadas de combate.
        /// </summary>
        /// <param name="jugador">Instancia concreta del personaje jugador.</param>
        /// <param name="enemigo">Instancia del enemigo actual.</param>
        /// <param name="contexto">Contexto adicional (puede ser null).</param>
        public static void Mostrar(MiJuegoRPG.Personaje.Personaje jugador, MiJuegoRPG.Enemigos.Enemigo enemigo, object contexto)
        {
            Console.WriteLine("=== ACCIONES ===");
            Console.WriteLine("1. Provocar");
            Console.WriteLine("2. Preparar contraataque");
            Console.WriteLine("3. Lanzar trampa");
            Console.WriteLine("4. Usar habilidad especial");
            Console.WriteLine("5. Intercambiar equipo");
            Console.WriteLine("6. Hablar/Intimidar");
            Console.WriteLine("7. Esperar");
            Console.WriteLine("8. Analizar entorno");
            Console.WriteLine("9. Pedir ayuda");
            Console.WriteLine("10. Rendirse");
            Console.Write("Elige una acción: ");
            var input = Console.ReadLine();
            // Aquí se puede enrutar a la lógica de cada acción avanzada
            switch (input)
            {
                case "1":
                    Console.WriteLine("Intentas provocar al enemigo...");
                    break;
                case "2":
                    Console.WriteLine("Te preparas para un contraataque...");
                    break;
                case "3":
                    Console.WriteLine("Intentas lanzar una trampa...");
                    break;
                case "4":
                    Console.WriteLine("Usas una habilidad especial...");
                    break;
                case "5":
                    Console.WriteLine("Intercambias tu equipo...");
                    break;
                case "6":
                    Console.WriteLine("Intentas hablar o intimidar al enemigo...");
                    break;
                case "7":
                    Console.WriteLine("Esperas y recuperas compostura...");
                    break;
                case "8":
                    Console.WriteLine("Analizas el entorno en busca de oportunidades...");
                    break;
                case "9":
                    Console.WriteLine("Intentas pedir ayuda...");
                    break;
                case "10":
                    Console.WriteLine("Te rindes. El combate termina.");
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
    }
}
