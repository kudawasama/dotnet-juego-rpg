using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ConsoleUserInterface : IUserInterface
    {
        public void Write(string text) => Console.Write(text);
        public void WriteLine(string text = "") => Console.WriteLine(text);
        public string ReadLine() => Console.ReadLine() ?? string.Empty;
        public string ReadOption(string prompt = "Selecciona una opción: ")
        {
            Write(prompt);
            return ReadLine().Trim();
        }
        public int ReadNumber(string prompt = "Ingresa un número: ")
        {
            while (true)
            {
                Write(prompt);
                var s = ReadLine();
                if (int.TryParse(s, out int n))
                    return n;
                WriteLine("Por favor, ingresa un número válido.");
            }
        }
        public bool Confirm(string prompt = "¿Confirmar? (s/n): ")
        {
            Write(prompt);
            var s = (ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
            return s == "s" || s == "si" || s == "sí" || s == "y" || s == "yes";
        }
        public void SetColor(ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground.HasValue)
                Console.ForegroundColor = foreground.Value;
            if (background.HasValue)
                Console.BackgroundColor = background.Value;
        }
        public void ResetColor() => Console.ResetColor();
        public void Pause(string message = "Presiona cualquier tecla para continuar...")
        {
            WriteLine(message);
            Console.ReadKey(true);
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }
    }
}
