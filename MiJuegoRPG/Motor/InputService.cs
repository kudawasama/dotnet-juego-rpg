using System;

namespace MiJuegoRPG.Motor
{
    public static class InputService
    {
        // Modo pruebas: evita pausas que bloqueen la ejecución de tests automatizados
        public static bool TestMode { get; set; } = false;

        public static string LeerOpcion(string mensaje = "Selecciona una opción: ")
        {
            var ui = Juego.ObtenerInstanciaActual()?.Ui;
            if (ui != null) return ui.ReadOption(mensaje);
            Console.Write(mensaje);
            var s = Console.ReadLine() ?? string.Empty;
            return s.Trim();
        }

        public static int LeerNumero(string mensaje = "Ingresa un número: ")
        {
            var ui = Juego.ObtenerInstanciaActual()?.Ui;
            if (ui != null) return ui.ReadNumber(mensaje);
            while (true)
            {
                Console.Write(mensaje);
                var s = Console.ReadLine();
                if (int.TryParse(s, out int n))
                    return n;
                Console.WriteLine("Por favor, ingresa un número válido.");
            }
        }

        public static void Pausa(string mensaje = "Presiona cualquier tecla para continuar...")
        {
            if (TestMode) return;
            var ui = Juego.ObtenerInstanciaActual()?.Ui;
            if (ui != null) { ui.Pause(mensaje); return; }
            Console.WriteLine(mensaje);
            Console.ReadKey(true);
            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        public static void Flush()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);
        }
    }
}
