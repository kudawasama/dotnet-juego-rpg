using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Objetos;


namespace MiJuegoRPG.Objetos
{
    // Valores recomendados para balanceo:
    // Daño mínimo: 1
    // Daño máximo: 100
    // Nivel mínimo: 1
    // Nivel máximo: 20
    // Rareza: Rota < Pobre < Normal < Superior < Rara < Legendaria < Ornamentada
    // Categoria: UnaMano, DosManos, Daga, Baston, Arco, Escudo, Otro

    public static class GestorArmas
    {
        public static string RutaArmasJson = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName ?? AppDomain.CurrentDomain.BaseDirectory,
            "MiJuegoRPG", "PjDatos", "armas.json");
        public static List<Arma> ArmasDisponibles = new List<Arma>();

        public static void CargarArmas(string rutaArchivo)
        {
            // Mostrar la ruta recibida
            Console.WriteLine($"[GestorArmas] Ruta recibida: {rutaArchivo}");
            // Forzar ruta a la carpeta del proyecto
            var dirProyecto = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? "");
            string rutaBase = dirProyecto?.FullName ?? Environment.CurrentDirectory;
            rutaArchivo = Path.Combine(rutaBase, "MiJuegoRPG", "PjDatos", Path.GetFileName(rutaArchivo));
            Console.WriteLine($"[GestorArmas] Ruta forzada: {rutaArchivo}");
            // Mostrar la ruta final usada
            Console.WriteLine($"[GestorArmas] Ruta final usada: {rutaArchivo}");
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var armasJson = JsonSerializer.Deserialize<List<ArmaJson>>(jsonString, options);
                ArmasDisponibles.Clear();
                if (armasJson != null)
                {
                    foreach (var arma in armasJson)
                    {
                        ArmasDisponibles.Add(new Arma(
                            arma.Nombre,
                            arma.Daño,
                            arma.NivelRequerido,
                            arma.Rareza,
                            arma.Categoria
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar armas: {ex.Message}");
            }
        }

        public static Arma? BuscarArmaPorNombre(string nombre)
        {
            return ArmasDisponibles.Find(a => a.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        // Si el arma no existe, la agrega automáticamente al archivo JSON
        public static void GuardarArmaSiNoExiste(Arma arma)
        {
            if (BuscarArmaPorNombre(arma.Nombre) != null)
                return;
            // Cargar lista actual
            List<ArmaJson> armasJson = new List<ArmaJson>();
            if (File.Exists(RutaArmasJson))
            {
                string jsonString = File.ReadAllText(RutaArmasJson);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var lista = JsonSerializer.Deserialize<List<ArmaJson>>(jsonString, options);
                if (lista != null)
                    armasJson = lista;
            }
            // Agregar nueva arma
            armasJson.Add(new ArmaJson
            {
                Nombre = arma.Nombre,
                Daño = arma.Daño,
                NivelRequerido = arma.Nivel,
                Rareza = arma.Rareza,
                Categoria = arma.Categoria
            });
            // Guardar en JSON
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            opciones.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            File.WriteAllText(RutaArmasJson, JsonSerializer.Serialize(armasJson, opciones));
            Console.WriteLine($"Arma '{arma.Nombre}' agregada automáticamente a armas.json");
        }
    }

    public class ArmaJson
    {
        public required string Nombre { get; set; }
        public int Daño { get; set; }
        public int NivelRequerido { get; set; }
        public Rareza Rareza { get; set; }
        public required string Categoria { get; set; }
    }
}
