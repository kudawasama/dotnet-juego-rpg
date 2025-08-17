using System;

namespace MiJuegoRPG.Motor
{
    public static class AvisosAventura
    {
        public static void MostrarAviso(string tipo, string nombre, string descripcion = "")
        {
            Console.WriteLine("\n=== AVISOS DE AVENTURA ===");
            Console.WriteLine($"¡Has desbloqueado {tipo}: {nombre}!");
            if (!string.IsNullOrEmpty(descripcion))
                Console.WriteLine(descripcion);
            Console.WriteLine("==========================\n");
        }
    }
}
