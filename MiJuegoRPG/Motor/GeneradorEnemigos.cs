using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public static class GeneradorEnemigos
    {
        public static ICombatiente GenerarEnemigoAleatorio(CreadorPersonaje jugador)
        {
            // Lógica para elegir un enemigo basado en el nivel del jugador
            // Por ahora, solo devolveremos un Goblin por defecto
            
            Console.WriteLine("Generando enemigo aleatorio...");
            
            // Más adelante podemos hacer esto dinámico
            return new Goblin(); 
        }

        public static void IniciarCombate(CreadorPersonaje jugador, ICombatiente enemigo)
        {
            if (enemigo == null)
            {
                Console.WriteLine("No se pudo generar un enemigo para el combate.");
                return;
            }

            Console.Clear();
            Console.WriteLine($"¡Un {enemigo.Nombre} salvaje ha aparecido!");

            if (jugador is ICombatiente jugadorCombatiente)
            {
                var combate = new CombatePorTurnos(jugadorCombatiente, enemigo);
                combate.IniciarCombate();
            }
            else
            {
                Console.WriteLine("El jugador no es un combatiente válido.");
            }
            
            Console.WriteLine("\nEl combate ha terminado. Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}