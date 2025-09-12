using System;

namespace MiJuegoRPG.Interfaces
{
    public interface IUserInterface
    {
        void Write(string text);
        void WriteLine(string text = "");
        string ReadLine();
        string ReadOption(string prompt = "Selecciona una opción: ");
        int ReadNumber(string prompt = "Ingresa un número: ");
        bool Confirm(string prompt = "¿Confirmar? (s/n): ");
        void SetColor(ConsoleColor? foreground = null, ConsoleColor? background = null);
        void ResetColor();
        void Pause(string message = "Presiona cualquier tecla para continuar...");
    }
}
