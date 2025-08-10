using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor
{
    public class CombatePorTurnos
    {
        private ICombatiente jugador;
        private List<ICombatiente> enemigos;

        // Constructor para combate múltiple
        public CombatePorTurnos(ICombatiente jugador, List<ICombatiente> enemigos)
        {
            this.jugador = jugador;
            this.enemigos = enemigos;
        }

        // Constructor para combate clásico (uno a uno)
        public CombatePorTurnos(ICombatiente jugador, ICombatiente enemigo)
        {
            this.jugador = jugador;
            this.enemigos = new List<ICombatiente> { enemigo };
        }

        public void IniciarCombate()
        {

            Console.WriteLine($"\n¡Un {string.Join(", ", enemigos.Select(e => e.Nombre))} salvaje ha aparecido!");
            Console.WriteLine($"¡Comienza el combate entre {jugador.Nombre} y {string.Join(", ", enemigos.Select(e => e.Nombre))}!");
            Console.WriteLine("----------------------------------------");
            MostrarEstadoCombate();
            Console.WriteLine("----------------------------------------\n");

            bool turnoJugador = true;
            int turno = 0;
            while (jugador.EstaVivo && enemigos.Any(e => e.EstaVivo))
            {
                if (turnoJugador)
                {
                    Console.WriteLine($"\nTurno de {jugador.Nombre}:");
                    Console.WriteLine("1. Atacar");
                    Console.WriteLine("2. Usar poción (si tienes)");
                    Console.WriteLine("3. Huir");
                    Console.Write("Elige una acción: ");
                    var accion = Console.ReadLine();

                    switch (accion)
                    {
                        case "1":
                            // Elegir enemigo vivo
                            var enemigosVivos = enemigos.Where(e => e.EstaVivo).ToList();
                            if (enemigosVivos.Count == 1)
                            {
                                var objetivo = enemigosVivos[0];
                                int danio = jugador.Atacar(objetivo);
                                Console.WriteLine($"{jugador.Nombre} ataca a {objetivo.Nombre} y le hace {danio} de daño.");
                            }
                            else
                            {
                                Console.WriteLine("Elige enemigo a atacar:");
                                for (int i = 0; i < enemigosVivos.Count; i++)
                                    Console.WriteLine($"{i + 1}. {enemigosVivos[i].Nombre} ({enemigosVivos[i].Vida}/{enemigosVivos[i].VidaMaxima} HP)");
                                var sel = Console.ReadLine();
                                int idx;
                                if (int.TryParse(sel, out idx) && idx > 0 && idx <= enemigosVivos.Count)
                                {
                                    var objetivo = enemigosVivos[idx - 1];
                                    int danio = jugador.Atacar(objetivo);
                                    Console.WriteLine($"{jugador.Nombre} ataca a {objetivo.Nombre} y le hace {danio} de daño.");
                                }
                                else
                                {
                                    Console.WriteLine("Selección inválida, pierdes el turno.");
                                }
                            }
                            break;
                        case "2":
                            Console.WriteLine("Aún no implementado.");
                            break;
                        case "3":
                            Console.WriteLine($"{jugador.Nombre} intenta huir...");
                            if (new Random().Next(100) < 60) // 60% probabilidad de huir
                            {
                                Console.WriteLine("¡Has logrado huir del combate!");
                                Console.WriteLine("¿Qué deseas hacer?");
                                Console.WriteLine("1. Buscar otro combate");
                                Console.WriteLine("2. Volver al sector anterior");
                                var opcionHuir = Console.ReadLine();
                                if (opcionHuir == "1")
                                {
                                    // Iniciar otro combate en el mismo sector
                                    var juego = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual();
                                    if (juego != null && jugador is MiJuegoRPG.Personaje.Personaje pj)
                                    {
                                        var enemigoNuevo = MiJuegoRPG.Motor.GeneradorEnemigos.GenerarEnemigoAleatorio(pj);
                                        var nuevoCombate = new CombatePorTurnos(jugador, enemigoNuevo);
                                        nuevoCombate.IniciarCombate();
                                    }
                                }
                                // Si elige 2 o cualquier otra opción, termina el combate y vuelve al sector
                                return; 
                            }
                            else
                            {
                                Console.WriteLine("No lograste huir, pierdes el turno.");
                            }
                            break;
                        default:
                            Console.WriteLine("Acción inválida, pierdes el turno.");
                            break;
                    }
                }
                else
                {
                    // Cada enemigo vivo ataca al jugador
                    foreach (var enemigo in enemigos.Where(e => e.EstaVivo))
                    {
                        Console.WriteLine($"\nTurno de {enemigo.Nombre}:");
                        int danioEnemigo = enemigo.Atacar(jugador);
                        Console.WriteLine($"{enemigo.Nombre} ataca a {jugador.Nombre} y le hace {danioEnemigo} de daño.");
                    }
                }

                // Mostrar estado después de cada turno
                MostrarEstadoCombate();
                turnoJugador = !turnoJugador;
                turno++;
            }

            // Resultado del combate
            if (!jugador.EstaVivo)
            {
                Console.WriteLine($"{jugador.Nombre} ha sido derrotado.");
            }
            else
            {
                Console.WriteLine($"Has derrotado a todos los enemigos!");
                // Dar recompensas por cada enemigo derrotado
                foreach (var enemigo in enemigos)
                {
                    if (!enemigo.EstaVivo && jugador is MiJuegoRPG.Personaje.Personaje personaje && enemigo is MiJuegoRPG.Enemigos.Enemigo enemigoReal)
                    {
                        enemigoReal.DarRecompensas(personaje);
                    }
                }
            }
        }

        private void MostrarEstadoCombate()
        {
            Console.WriteLine($"\n=== ESTADO DEL COMBATE ===");
            Console.WriteLine($"{jugador.Nombre}: {jugador.Vida}/{jugador.VidaMaxima} HP");
            foreach (var enemigo in enemigos)
            {
                Console.WriteLine($"{enemigo.Nombre}: {enemigo.Vida}/{enemigo.VidaMaxima} HP");
            }
            Console.WriteLine("----------------------------------------\n");
        }
    }
}