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
            // Ahora solo crea personaje con clase oculta
            return CrearSinClase();
        }

        public static void GuardarPersonaje(MiJuegoRPG.Personaje.Personaje personaje, string? rutaArchivo = null)
        {
            try
            {
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                opciones.Converters.Add(new MiJuegoRPG.Personaje.ObjetoPolimorficoConverter());
                string json = JsonSerializer.Serialize(personaje, opciones);

                // Usar ruta por defecto si no se proporciona una
                if (rutaArchivo == null)
                {
                    rutaArchivo = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("PjGuardados", "Grid.json");
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
         var opciones = new JsonSerializerOptions();
         opciones.Converters.Add(new MiJuegoRPG.Personaje.ObjetoPolimorficoConverter());
         return JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json, opciones)
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

        // Crea un personaje sin clase inicial, atributos base genéricos
        public static MiJuegoRPG.Personaje.Personaje CrearSinClase()
        {
            Console.WriteLine("Nombre de tu personaje:");
            string nombre = InputService.LeerOpcion();
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Héroe Sin Nombre";

            // Atributos base neutros
            var atributosBase = new AtributosBase(5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5);
            var clase = new Clase("Sin clase", atributosBase, new Estadisticas(atributosBase));

            var personaje = new MiJuegoRPG.Personaje.Personaje(nombre);
            personaje.Clase = clase;
            personaje.ClaseDesbloqueada = "Sin clase";
            personaje.Titulo = "Novato";
            personaje.AtributosBase = atributosBase;
            personaje.Vida = personaje.VidaMaxima;
            return personaje;
        }
    }
}
