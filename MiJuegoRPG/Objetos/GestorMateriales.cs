using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Objetos
{
    public static class GestorMateriales
    {
    public static string RutaMaterialesJson = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("materiales.json");
        public static List<Material> MaterialesDisponibles = new List<Material>();

        public static void GuardarMaterialSiNoExiste(Material material)
        {
            if (BuscarMaterialPorNombre(material.Nombre) != null)
                return;
            List<MaterialJson> materialesJson = new List<MaterialJson>();
            if (File.Exists(RutaMaterialesJson))
            {
                string jsonString = File.ReadAllText(RutaMaterialesJson);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var lista = JsonSerializer.Deserialize<List<MaterialJson>>(jsonString, options);
                if (lista != null)
                    materialesJson = lista;
            }
            materialesJson.Add(new MaterialJson
            {
                Nombre = material.Nombre,
                Rareza = material.Rareza,
                Categoria = material.Categoria
            });
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            opciones.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            File.WriteAllText(RutaMaterialesJson, JsonSerializer.Serialize(materialesJson, opciones));
            Console.WriteLine($"Material '{material.Nombre}' agregado automáticamente a materiales.json");
        }

        public static Material? BuscarMaterialPorNombre(string nombre)
        {
            return MaterialesDisponibles.Find(m => m.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        public static void CargarMateriales(string rutaArchivo)
        {
            if (!Path.IsPathRooted(rutaArchivo))
            {
                rutaArchivo = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath(rutaArchivo);
            }
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    // Fallback razonable: usar PjDatos/materiales.json tal cual (ya resuelto) o silencio
                    Console.WriteLine($"Error al cargar materiales: No existe el archivo '{rutaArchivo}'");
                    return;
                }
                string jsonString = File.ReadAllText(rutaArchivo);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var materialesJson = JsonSerializer.Deserialize<List<MaterialJson>>(jsonString, options);
                MaterialesDisponibles.Clear();
                if (materialesJson != null)
                {
                    foreach (var material in materialesJson)
                    {
                        MaterialesDisponibles.Add(new Material(
                            material.Nombre,
                            material.Rareza,
                            material.Categoria
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar materiales: {ex.Message}");
            }
        }
    }

    public class MaterialJson
    {
        public required string Nombre { get; set; }
        public Rareza Rareza { get; set; }
        public required string Categoria { get; set; }
    }
}
