using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor
{
    public class CombatePorTurnos
    {
        private ICombatiente jugador;
        private ICombatiente enemigo;

        public CombatePorTurnos(ICombatiente jugador, ICombatiente enemigo)
        {
            this.jugador = jugador;
            this.enemigo = enemigo;
        }

        public void IniciarCombate()
        {
            Console.WriteLine($"¡Comienza el combate entre {jugador.Nombre} y {enemigo.Nombre}!");

            bool turnoJugador = true;

            while (jugador.EstaVivo && enemigo.EstaVivo)
            {
                if (turnoJugador)
                {
                    Console.WriteLine($"\nTurno de {jugador.Nombre}:");
                    Console.WriteLine("1. Atacar");
                    Console.WriteLine("2. Usar poción (si tienes)");
                    Console.Write("Elige una acción: ");
                    var accion = Console.ReadLine();

                    switch (accion)
                    {
                        case "1":
                            int danio = jugador.Atacar(enemigo);
                            Console.WriteLine($"{jugador.Nombre} ataca a {enemigo.Nombre} y le hace {danio} de daño.");
                            break;
                        case "2":
                            Console.WriteLine("Aún no implementado.");
                            break;
                        default:
                            Console.WriteLine("Acción inválida, pierdes el turno.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"\nTurno de {enemigo.Nombre}:");
                    int danioEnemigo = enemigo.Atacar(jugador);
                    Console.WriteLine($"{enemigo.Nombre} ataca a {jugador.Nombre} y le hace {danioEnemigo} de daño.");
                }

                // Mostrar estado después de cada turno
                MostrarEstadoCombate();
                turnoJugador = !turnoJugador;
            }

            // Resultado del combate
            if (!jugador.EstaVivo)
            {
                Console.WriteLine($"{jugador.Nombre} ha sido derrotado.");
            }
            else
            {
                Console.WriteLine($"{enemigo.Nombre} ha sido derrotado.");
                // Dar recompensas si el jugador es un Personaje
                if (jugador is MiJuegoRPG.Personaje.Personaje personaje && enemigo is MiJuegoRPG.Enemigos.Enemigo enemigoReal)
                {
                    enemigoReal.DarRecompensas(personaje);
                }
            }
        }

        private void MostrarEstadoCombate()
        {
            Console.WriteLine($"\n=== ESTADO DEL COMBATE ===");
            Console.WriteLine($"{jugador.Nombre}: {jugador.Vida}/{jugador.VidaMaxima} HP");
            Console.WriteLine($"{enemigo.Nombre}: {enemigo.Vida}/{enemigo.VidaMaxima} HP");
        }
    }
}