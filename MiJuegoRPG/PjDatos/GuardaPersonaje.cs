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
            // Eliminado: ya no se usa Saves.json
    

        // Clase para manejar múltiples personajes
        public class DatosGuardado
        {
            public List<MiJuegoRPG.Personaje.Personaje> Personajes { get; set; } = new List<MiJuegoRPG.Personaje.Personaje>();
            public DateTime FechaUltimaModificacion { get; set; } = DateTime.Now;
        }

        // Guardar personaje (lo agrega a la lista existente)
        public static void GuardarPersonaje(MiJuegoRPG.Personaje.Personaje personaje, string? rutaArchivo = null)
        {
            // Eliminado: ya no se usa RUTA_POR_DEFECTO
            
            // Guardar cada personaje en su propio archivo .json
                string rutaCarpeta = "c:\\Users\\ASUS\\OneDrive\\Documentos\\GitHub\\dotnet-juego-rpg\\PjDatos\\PjGuardados";
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);
            string rutaArchivoFinal = Path.Combine(rutaCarpeta, personaje.Nombre + ".json");
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            opciones.Converters.Add(new MiJuegoRPG.Objetos.ObjetoJsonConverter());
            // Serializar siempre la ubicación actual
            if (personaje.UbicacionActual == null)
            {
                var bairan = MiJuegoRPG.Motor.Juego.InstanciaActual?.estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Bairan");
                if (bairan != null)
                    personaje.UbicacionActual = bairan;
            }
            string json = JsonSerializer.Serialize(personaje, opciones);
            File.WriteAllText(rutaArchivoFinal, json);
            Console.WriteLine($"Personaje '{personaje.Nombre}' guardado en: {rutaArchivoFinal}");
        }

        // Cargar todos los personajes
        public static List<MiJuegoRPG.Personaje.Personaje> CargarTodosLosPersonajes(string? rutaArchivo = null)
        {
            // Eliminado: ya no se usa RUTA_POR_DEFECTO
            
            // Cargar todos los archivos .json en PjGuardados
                string rutaCarpeta = "c:\\Users\\ASUS\\OneDrive\\Documentos\\GitHub\\dotnet-juego-rpg\\PjDatos\\PjGuardados";
            List<MiJuegoRPG.Personaje.Personaje> personajes = new List<MiJuegoRPG.Personaje.Personaje>();
            if (Directory.Exists(rutaCarpeta))
            {
                var archivos = Directory.GetFiles(rutaCarpeta, "*.json");
                foreach (var archivo in archivos)
                {
                    try
                    {
                        string json = File.ReadAllText(archivo);
                        var opciones = new JsonSerializerOptions();
                        opciones.Converters.Add(new MiJuegoRPG.Objetos.ObjetoJsonConverter());
                        var personaje = JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json, opciones);
                        if (personaje != null)
                        {
                            // Si no tiene ubicación, asignar Bairan
                            if (personaje.UbicacionActual == null)
                            {
                                var bairan = MiJuegoRPG.Motor.Juego.InstanciaActual?.estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Bairan");
                                if (bairan != null)
                                    personaje.UbicacionActual = bairan;
                            }
                            personajes.Add(personaje);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar personaje desde {archivo}: {ex.Message}");
                    }
                }
            }
            // Si no existe la carpeta, simplemente retorna lista vacía (sin mensaje de depuración)
            return personajes;
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
            // Oculta mensaje de depuración
            for (int i = 0; i < personajes.Count; i++)
            {
                var p = personajes[i];
                string nombreClase = p.Clase != null ? p.Clase.Nombre : "Sin clase";
                Console.WriteLine($"{i + 1}. {p.Nombre}");
            }
        }

        // Eliminar personaje
        public static bool EliminarPersonaje(string nombrePersonaje, string? rutaArchivo = null)
        {
            // Eliminado: ya no se usa RUTA_POR_DEFECTO
            
            // Eliminar el archivo individual del personaje
            string rutaCarpeta = "/workspaces/dotnet-juego-rpg/PjDatos/PjGuardados";
            string rutaArchivoFinal = Path.Combine(rutaCarpeta, nombrePersonaje + ".json");
            if (File.Exists(rutaArchivoFinal))
            {
                File.Delete(rutaArchivoFinal);
                Console.WriteLine($"Personaje '{nombrePersonaje}' eliminado.");
                return true;
            }
            else
            {
                Console.WriteLine($"No se encontró el personaje '{nombrePersonaje}'.");
                return false;
            }
        }

        // Método privado para cargar datos del archivo
    // Eliminado: ya no se usa Saves.json ni CargarDatos
    }
}

// Ejemplo de clase Personaje
