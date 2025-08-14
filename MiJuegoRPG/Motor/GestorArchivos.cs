using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public static class GestorArchivos
    {
        public static void GuardarPersonaje(Personaje.Personaje jugador)
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para guardar.");
                return;
            }
            var db = new MiJuegoRPG.PjDatos.PersonajeSqliteService();
            db.Guardar(jugador);
            Console.WriteLine($"Personaje guardado exitosamente en la base de datos como '{jugador.Nombre}'.");
        }

        public static Personaje.Personaje? CargarPersonaje()
        {
            try
            {
                var db = new MiJuegoRPG.PjDatos.PersonajeSqliteService();
                var nombres = db.ListarNombres();
                if (nombres.Count == 0)
                {
                    Console.WriteLine("No hay personajes guardados en la base de datos.");
                    return null;
                }
                Console.WriteLine("Personajes disponibles:");
                for (int i = 0; i < nombres.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {nombres[i]}");
                }
                Console.Write("Elige el número del personaje a cargar: ");
                if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= nombres.Count)
                {
                    return db.Cargar(nombres[seleccion - 1]);
                }
                else
                {
                    Console.WriteLine("Selección inválida. No se cargó ningún personaje.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el personaje: {ex.Message}");
            }
            return null;
        }
    }
}
