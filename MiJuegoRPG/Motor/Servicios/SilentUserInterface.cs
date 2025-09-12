using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Implementación de IU silenciosa para pruebas: no escribe en consola ni bloquea.
    /// </summary>
    public class SilentUserInterface : IUserInterface
    {
        public void Write(string text) { /* no-op */ }
        public void WriteLine(string text = "") { /* no-op */ }
        public string ReadLine() => string.Empty;
        public string ReadOption(string prompt = "Selecciona una opción: ") => string.Empty;
        public int ReadNumber(string prompt = "Ingresa un número: ") => 0;
        public bool Confirm(string prompt = "¿Confirmar? (s/n): ") => true;
        public void SetColor(ConsoleColor? foreground = null, ConsoleColor? background = null) { /* no-op */ }
        public void ResetColor() { /* no-op */ }
        public void Pause(string message = "Presiona cualquier tecla para continuar...") { /* no-op */ }
    }
}
