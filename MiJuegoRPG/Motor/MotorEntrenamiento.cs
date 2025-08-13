using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MotorEntrenamiento
    {
        private Juego juego;
        public MotorEntrenamiento(Juego juego)
        {
            this.juego = juego;
        }
        public void Entrenar()
        {
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine("¿Qué atributo deseas entrenar?");
            Console.WriteLine("1. Fuerza");
            Console.WriteLine("2. Magia");
            Console.WriteLine("3. Agilidad");
            Console.WriteLine("4. Defensa");
            Console.WriteLine("5. Inteligencia");
            Console.WriteLine("6. Vitalidad");
            Console.WriteLine("7. Suerte");
            Console.WriteLine("8. Resistencia");
            Console.WriteLine("9. Destreza");
            Console.WriteLine("0. Salir");
            var opcion = Console.ReadLine();
            int seleccion;
            if (int.TryParse(opcion, out seleccion) && seleccion >= 1 && seleccion <= 9)
            {
                string atributo = seleccion switch
                {
                    1 => "fuerza",
                    2 => "magia",
                    3 => "agilidad",
                    4 => "defensa",
                    5 => "inteligencia",
                    6 => "vitalidad",
                    7 => "suerte",
                    8 => "resistencia",
                    9 => "destreza",
                    _ => ""
                };
                Console.WriteLine($"¿Cuántos minutos deseas entrenar {atributo}? (0 para cancelar)");
                var minStr = Console.ReadLine();
                if (int.TryParse(minStr, out int minutos) && minutos > 0)
                {
                    bool cancelado = false;
                    for (int i = 0; i < minutos && !cancelado; i++) // Entrenamiento por minutos
                    {
                        for (int s = 0; s < 60 && !cancelado; s++) // Entrenamiento por segundos
                        {
                            juego.jugador?.Entrenar(atributo);
                            DateTime tiempoActual = juego.FechaInicio.AddSeconds(juego.MinutosMundo * 60 + s); 
                            Console.Clear();
                            int valorBase = 0;
                            if (juego.jugador != null)
                            {
                                switch (atributo.ToLower())
                                {
                                    case "fuerza": valorBase = juego.jugador.AtributosBase.Fuerza; break;
                                    case "inteligencia": valorBase = juego.jugador.AtributosBase.Inteligencia; break;
                                    case "destreza": valorBase = juego.jugador.AtributosBase.Destreza; break;
                                    case "magia": valorBase = (int)juego.jugador.ExpMagia; break;
                                    case "suerte": valorBase = juego.jugador.AtributosBase.Suerte; break;
                                    case "defensa": valorBase = juego.jugador.AtributosBase.Defensa; break;
                                    case "vitalidad": valorBase = juego.jugador.AtributosBase.Vitalidad; break;
                                    case "agilidad": valorBase = juego.jugador.AtributosBase.Agilidad; break;
                                    case "resistencia": valorBase = juego.jugador.AtributosBase.Resistencia; break;
                                }
                            }
                            Console.WriteLine($"Entrenando {atributo}... Progreso: {(i * 60 + s + 1)}/{minutos * 60} segundos | Valor actual: {valorBase} (Entrenamiento Avanzando)");
                            Console.WriteLine($"Reloj mundial: [{tiempoActual:dd-MM-yyyy // HH:mm:ss}]");
                            Console.WriteLine("Presiona 'c' para cancelar el entrenamiento.");
                            for (int t = 0; t < 10; t++) // Espera 1 segundo en 10 intervalos de 100ms para detectar tecla
                            {
                                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.C)
                                {
                                    cancelado = true;
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                        }
                        juego.MinutosMundo++;
                    }
                    if (cancelado)
                        Console.WriteLine("Entrenamiento cancelado por el usuario.");
                    else
                        Console.WriteLine($"Entrenamiento finalizado. Tiempo total en el mundo: {juego.MinutosMundo} minutos.");
                }
                else
                {
                    Console.WriteLine("Entrenamiento cancelado.");
                }
            }
            else if (seleccion == 0)
            {
                Console.WriteLine("Saliendo del entrenamiento...");
            }
            else
            {
                Console.WriteLine("Opción no válida.");
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }
    }
}
