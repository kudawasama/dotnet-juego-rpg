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
            string nombreArchivo = jugador?.Nombre ?? "personaje";
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                nombreArchivo = "personaje";
            }
            string rutaCarpeta = "/workspaces/dotnet-juego-rpg/PjDatos/PjGuardados";
            Directory.CreateDirectory(rutaCarpeta);
            string rutaGuardado = Path.Combine(rutaCarpeta, nombreArchivo + ".json");
            try
            {
                string json = JsonSerializer.Serialize(jugador, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaGuardado, json);
                Console.WriteLine($"Personaje guardado exitosamente como {nombreArchivo}.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el personaje: {ex.Message}");
            }
        }

        public static Personaje.Personaje? CargarPersonaje()
        {
            try
            {
                string rutaPj = "/workspaces/dotnet-juego-rpg/PjDatos/PjGuardados";
                var archivos = Directory.Exists(rutaPj) ? Directory.GetFiles(rutaPj, "*.json") : Array.Empty<string>();
                if (archivos.Length == 0)
                {
                    Console.WriteLine("No hay personajes guardados.");
                    return null;
                }
                Console.WriteLine("Personajes disponibles:");
                for (int i = 0; i < archivos.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(archivos[i])}");
                }
                Console.Write("Elige el número del personaje a cargar: ");
                if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= archivos.Length)
                {
                    string json = File.ReadAllText(archivos[seleccion - 1]);
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    // Si tienes convertidores personalizados, agrégalos aquí
                    return JsonSerializer.Deserialize<Personaje.Personaje>(json, options);
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
