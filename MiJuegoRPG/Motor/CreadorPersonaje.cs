using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor  // Debe ser este espacio de nombres
{
    public class CreadorPersonaje
    {
        public static MiJuegoRPG.Personaje.Personaje Crear()
        {
            Console.WriteLine("Nombre de tu personaje:");
            string nombre = Console.ReadLine() ?? "Héroe Sin Nombre";

            Console.WriteLine("Elige una clase:");
            Console.WriteLine("1. Guerrero");
            Console.WriteLine("2. Mago");
            Console.WriteLine("3. Ladrón");


            
            int eleccion = int.Parse(Console.ReadLine() ?? "1");
            Clase clase;
            // AtributosBase(fuerza, inteligencia, agilidad, vitalidad, suerte, resistencia, sabiduria, carisma, destreza, fe, liderazgo, percepcion, persuasión, voluntad, defensa)
            switch (eleccion)
            {
                case 2:
                    var atributosMago = new AtributosBase(2, 10, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5);
                    clase = new Clase("Mago", atributosMago, new Estadisticas(atributosMago));
                    break;
                case 3:
                    var atributosLadron = new AtributosBase(4, 3, 8, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5);
                    clase = new Clase("Ladrón", atributosLadron, new Estadisticas(atributosLadron));
                    break;
                default:
                    var atributosGuerrero = new AtributosBase(10, 2, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5);
                    clase = new Clase("Guerrero", atributosGuerrero, new Estadisticas(atributosGuerrero));
                    break;
            }

            var personaje = new MiJuegoRPG.Personaje.Personaje(nombre);
            personaje.Clase = clase;
            return personaje;
        }



        public static void GuardarPersonaje(MiJuegoRPG.Personaje.Personaje personaje, string? rutaArchivo = null)
        {
            try
            {
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(personaje, opciones);
                
                // Usar ruta por defecto si no se proporciona una
                if (rutaArchivo == null)
                {
                    var dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
                    string rutaProyecto = dir?.Parent?.Parent?.FullName ?? AppDomain.CurrentDomain.BaseDirectory;
                    rutaArchivo = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "Saves.json");
                }
                
                // Crear directorio si no existe
                string directorio = rutaArchivo != null ? Path.GetDirectoryName(rutaArchivo) ?? string.Empty : string.Empty;
                if (!string.IsNullOrEmpty(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                if (rutaArchivo == null)
                {
                    throw new ArgumentNullException(nameof(rutaArchivo), "La ruta del archivo no puede ser nula.");
                }
                
                File.WriteAllText(rutaArchivo, json);
                Console.WriteLine($"Personaje guardado en: {rutaArchivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
                throw;
            }
        }

        public static MiJuegoRPG.Personaje.Personaje CargarPersonaje(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException("El archivo de personaje no existe.", rutaArchivo);
            }

            string json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json) 
                   ?? throw new InvalidOperationException("No se pudo deserializar el personaje.");
        }

        public static void MostrarPersonaje(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"Nombre: {personaje.Nombre}");
            Console.WriteLine($"Clase: {(personaje.Clase != null ? personaje.Clase.Nombre : "Sin clase")}");
            if (personaje.Atributos != null)
            {
                Console.WriteLine($"Atributos: Fuerza={personaje.Atributos?.Fuerza}, Inteligencia={personaje.Atributos?.Inteligencia}, Agilidad={personaje.Atributos?.Agilidad}");
            }
            else
            {
                Console.WriteLine("Atributos: No disponibles");
            }
            Console.WriteLine($"Vida: {personaje.VidaActual}/{personaje.VidaMaxima}");
        }

    }
}
