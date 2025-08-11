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
                if (int.TryParse(minStr, out int minutos) && minutos > 0) // Verifica si la entrada es un número válido y mayor que 0
                {
                    for (int i = 0; i < minutos; i++)
                    {
                        juego.jugador?.Entrenar(atributo); // Entrena el atributo seleccionado
                        juego.MinutosMundo++;
                        Console.WriteLine($"Reloj mundial: {juego.FormatoRelojMundo} | Minuto {i+1} de entrenamiento");
                        Thread.Sleep(60000); // Espera 1 minuto real por cada minuto entrenado
                    }
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
