using System;
using System.Linq;
using System.Threading;
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
            Console.WriteLine("2. Inteligencia");
            Console.WriteLine("3. Agilidad");
            Console.WriteLine("4. Defensa");
            Console.WriteLine("5. Vitalidad");
            Console.WriteLine("6. Suerte");
            Console.WriteLine("7. Resistencia");
            Console.WriteLine("8. Destreza");
            Console.WriteLine("9. Percepción");
            Console.WriteLine("0. Salir");
            Console.WriteLine("V. Alternar verbosidad progreso"); // Nueva opción para verbosidad, explicacion: permite ver más detalles sobre el progreso del entrenamiento
            var opcion = Console.ReadLine();
            if (opcion?.Equals("v", StringComparison.OrdinalIgnoreCase) == true)
            {
                var svc = juego.ProgressionService;
                svc.Verbose = !svc.Verbose;
                Console.WriteLine($"Verbose ahora: {svc.Verbose}");
                return;
            }
            if (!int.TryParse(opcion, out int sel) || sel < 0 || sel > 9)
            {
                Console.WriteLine("Opción no válida.");
                return;
            }
            if (sel == 0)
            {
                Console.WriteLine("Saliendo...");
                return;
            }
            var mapping = new (int key, MiJuegoRPG.Dominio.Atributo atr)[] {
                (1, MiJuegoRPG.Dominio.Atributo.Fuerza),
                (2, MiJuegoRPG.Dominio.Atributo.Inteligencia),
                (3, MiJuegoRPG.Dominio.Atributo.Agilidad),
                (4, MiJuegoRPG.Dominio.Atributo.Defensa),
                (5, MiJuegoRPG.Dominio.Atributo.Vitalidad),
                (6, MiJuegoRPG.Dominio.Atributo.Suerte),
                (7, MiJuegoRPG.Dominio.Atributo.Resistencia),
                (8, MiJuegoRPG.Dominio.Atributo.Destreza),
                (9, MiJuegoRPG.Dominio.Atributo.Percepcion)
            };
            var atrSel = mapping.First(m => m.key == sel).atr;
            Console.WriteLine($"¿Cuántos minutos deseas entrenar {atrSel}? (0 para cancelar)");
            var minStr = Console.ReadLine();
            if (!int.TryParse(minStr, out int minutos) || minutos <= 0)
            {
                Console.WriteLine("Entrenamiento cancelado.");
                return;
            }
            bool cancelado = false;
            var progService = juego.ProgressionService;
            for (int i = 0; i < minutos && !cancelado; i++)
            {
                for (int s = 0; s < 60 && !cancelado; s++)
                {
                    if (juego.Jugador != null)
                        progService?.AplicarEntrenamiento(juego.Jugador, atrSel, 1);
                    DateTime tiempoActual = juego.FechaInicio.AddSeconds((juego.MinutosMundo * 60) + s);
                    Console.WriteLine($"Reloj mundial: [{tiempoActual:dd-MM-yyyy // HH:mm:ss}]");
                    for (int t = 0; t < 10; t++)
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
            Console.WriteLine(cancelado ? "Entrenamiento cancelado por el usuario." : $"Entrenamiento finalizado. Tiempo total en el mundo: {juego.MinutosMundo} minutos.");
            Console.WriteLine("Presiona cualquier tecla para volver...");
            Console.ReadKey();
        }
    }
}
