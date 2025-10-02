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
            #if TEST_MODE
            Console.WriteLine("[TEST_MODE] Guardado de personaje omitido (Sqlite deshabilitado).");
            #else
            var db = new MiJuegoRPG.PjDatos.PersonajeSqliteService();
            db.Guardar(jugador);
            Console.WriteLine($"Personaje guardado exitosamente en la base de datos como '{jugador.Nombre}'.");
            #endif
        }

        public static Personaje.Personaje? CargarPersonaje()
        {
            try
            {
                #if TEST_MODE
                Console.WriteLine("[TEST_MODE] Carga de personaje omitida (Sqlite deshabilitado).");
                var nombres = new System.Collections.Generic.List<string>();
                #else
                var db = new MiJuegoRPG.PjDatos.PersonajeSqliteService();
                var nombres = db.ListarNombres();
                #endif
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
                    #if TEST_MODE
                    return null;
                    #else
                    return db.Cargar(nombres[seleccion - 1]);
                    #endif
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
