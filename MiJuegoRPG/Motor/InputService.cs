using System;

namespace MiJuegoRPG.Motor
{
    public static class InputService
    {
        public static string LeerOpcion(string mensaje = "Selecciona una opción: ")
        {
            Console.Write(mensaje);
            var s = Console.ReadLine() ?? string.Empty;
            return s.Trim().Length > 0 ? s.Trim()[0].ToString() : string.Empty;
        }

        public static int LeerNumero(string mensaje = "Ingresa un número: ")
        {
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
