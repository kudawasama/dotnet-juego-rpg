using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Objetos
{
    public static class GestorPociones
    {
    public static string RutaPocionesJson = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("pociones.json");
        public static List<Pocion> PocionesDisponibles = new List<Pocion>();

        public static void GuardarPocionSiNoExiste(Pocion pocion)
        {
            if (BuscarPocionPorNombre(pocion.Nombre) != null)
                return;
            List<PocionJson> pocionesJson = new List<PocionJson>();
            if (File.Exists(RutaPocionesJson))
            {
                string jsonString = File.ReadAllText(RutaPocionesJson);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var lista = JsonSerializer.Deserialize<List<PocionJson>>(jsonString, options);
                if (lista != null)
                    pocionesJson = lista;
            }
            pocionesJson.Add(new PocionJson
            {
                Nombre = pocion.Nombre,
                Curacion = pocion.Curacion,
                Rareza = pocion.Rareza,
                Categoria = pocion.Categoria
            });
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            opciones.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            File.WriteAllText(RutaPocionesJson, JsonSerializer.Serialize(pocionesJson, opciones));
            Console.WriteLine($"Poción '{pocion.Nombre}' agregada automáticamente a pociones.json");
        }

        public static Pocion? BuscarPocionPorNombre(string nombre)
        {
            return PocionesDisponibles.Find(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        public static void CargarPociones(string rutaArchivo)
        {
            if (!Path.IsPathRooted(rutaArchivo))
            {
                rutaArchivo = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath(rutaArchivo);
            }
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    // Fallback: DatosJuego/pociones/pociones.json
                    var alternativa = MiJuegoRPG.Motor.Servicios.PathProvider.PocionesPath(Path.GetFileName(rutaArchivo));
                    if (File.Exists(alternativa))
                    {
                        rutaArchivo = alternativa;
                    }
                    else
                    {
                        Console.WriteLine($"Error al cargar pociones: No existe el archivo '{rutaArchivo}'");
                        return;
                    }
                }
                string jsonString = File.ReadAllText(rutaArchivo);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var pocionesJson = JsonSerializer.Deserialize<List<PocionJson>>(jsonString, options);
                PocionesDisponibles.Clear();
                if (pocionesJson != null)
                {
                    foreach (var pocion in pocionesJson)
                    {
                        PocionesDisponibles.Add(new Pocion(
                            pocion.Nombre,
                            pocion.Curacion,
                            pocion.Rareza,
                            pocion.Categoria
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar pociones: {ex.Message}");
            }
        }
    }

    public class PocionJson
    {
        public required string Nombre { get; set; }
        public int Curacion { get; set; }
        public string Rareza { get; set; } = "Normal";
        public required string Categoria { get; set; }
    }
}
