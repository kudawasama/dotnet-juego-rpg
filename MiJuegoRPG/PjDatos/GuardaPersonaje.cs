//grardar el personaje creado, almacenar caracteristicas del pérsonaje con su avance y poder cargar personaje guardado

using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Personaje;
using System.Collections.Generic;
using System.Linq;

namespace MiJuegoRPG.PjDatos
{
    public class GuardaPersonaje
    {
        // Nueva ruta como solicitaste
        private static readonly string RUTA_POR_DEFECTO = 
            @"C:\Users\jose.cespedes\Desktop\Programacion\MiJuegoRPG\MiJuegoRPG\PjDatos\Saves.json";

        // Clase para manejar múltiples personajes
        public class DatosGuardado
        {
            public List<MiJuegoRPG.Personaje.Personaje> Personajes { get; set; } = new List<MiJuegoRPG.Personaje.Personaje>();
            public DateTime FechaUltimaModificacion { get; set; } = DateTime.Now;
        }

        // Guardar personaje (lo agrega a la lista existente)
        public static void GuardarPersonaje(MiJuegoRPG.Personaje.Personaje personaje, string? rutaArchivo = null)
        {
            rutaArchivo ??= RUTA_POR_DEFECTO;
            
            // Crear directorio si no existe
            string directorio = Path.GetDirectoryName(rutaArchivo) ?? string.Empty;
            if (!string.IsNullOrEmpty(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            // Cargar datos existentes o crear nuevos
            DatosGuardado datos = CargarDatos(rutaArchivo);
            
            // Buscar si ya existe un personaje con el mismo nombre
            var personajeExistente = datos.Personajes.FirstOrDefault(p => p.Nombre == personaje.Nombre);
            
            if (personajeExistente != null)
            {
                // Actualizar personaje existente
                var indice = datos.Personajes.IndexOf(personajeExistente);
                datos.Personajes[indice] = personaje;
                Console.WriteLine($"Personaje '{personaje.Nombre}' actualizado.");
            }
            else
            {
                // Agregar nuevo personaje
                datos.Personajes.Add(personaje);
                Console.WriteLine($"Nuevo personaje '{personaje.Nombre}' guardado.");
            }

            datos.FechaUltimaModificacion = DateTime.Now;

            // Guardar archivo
            var opciones = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(datos, opciones);
            File.WriteAllText(rutaArchivo, json);
            
            Console.WriteLine($"Datos guardados en: {rutaArchivo}");
            Console.WriteLine($"Total de personajes: {datos.Personajes.Count}");
        }

        // Cargar todos los personajes
        public static List<MiJuegoRPG.Personaje.Personaje> CargarTodosLosPersonajes(string? rutaArchivo = null)
        {
            rutaArchivo ??= RUTA_POR_DEFECTO;
            
            DatosGuardado datos = CargarDatos(rutaArchivo);
            return datos.Personajes;
        }

        // Cargar personaje específico por nombre
        public static MiJuegoRPG.Personaje.Personaje? CargarPersonaje(string nombrePersonaje, string? rutaArchivo = null)
        {
            var personajes = CargarTodosLosPersonajes(rutaArchivo);
            return personajes.FirstOrDefault(p => p.Nombre.Equals(nombrePersonaje, StringComparison.OrdinalIgnoreCase));
        }

        // Mostrar lista de personajes guardados
        public static void MostrarPersonajesGuardados(string? rutaArchivo = null)
        {
            var personajes = CargarTodosLosPersonajes(rutaArchivo);
            
            if (personajes.Count == 0)
            {
                Console.WriteLine("No hay personajes guardados.");
                return;
            }

            Console.WriteLine("\n=== PERSONAJES GUARDADOS ===");
            for (int i = 0; i < personajes.Count; i++)
            {
                var p = personajes[i];
                Console.WriteLine($"{i + 1}. {p.Nombre} - Nivel {p.Nivel} - Clase: {p.Clase.Nombre}");
            }
        }

        // Eliminar personaje
        public static bool EliminarPersonaje(string nombrePersonaje, string? rutaArchivo = null)
        {
            rutaArchivo ??= RUTA_POR_DEFECTO;
            
            DatosGuardado datos = CargarDatos(rutaArchivo);
            var personaje = datos.Personajes.FirstOrDefault(p => p.Nombre.Equals(nombrePersonaje, StringComparison.OrdinalIgnoreCase));
            
            if (personaje != null)
            {
                datos.Personajes.Remove(personaje);
                datos.FechaUltimaModificacion = DateTime.Now;
                
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(datos, opciones);
                File.WriteAllText(rutaArchivo, json);
                
                Console.WriteLine($"Personaje '{nombrePersonaje}' eliminado.");
                return true;
            }
            
            Console.WriteLine($"No se encontró el personaje '{nombrePersonaje}'.");
            return false;
        }

        // Método privado para cargar datos del archivo
        private static DatosGuardado CargarDatos(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                return new DatosGuardado();
            }

            try
            {
                string json = File.ReadAllText(rutaArchivo);
                var datos = JsonSerializer.Deserialize<DatosGuardado>(json);
                return datos ?? new DatosGuardado();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
                return new DatosGuardado();
            }
        }
    }
}

// Ejemplo de clase Personaje
public class Personaje
{
    public required string Nombre { get; set; }
    public int Nivel { get; set; }
    public int Poder { get; set; }
    // Agrega más propiedades según sea necesario
}
