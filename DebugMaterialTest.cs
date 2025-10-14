using System;
using System.IO;
using MiJuegoRPG.Motor.Servicios.Repos;
using MiJuegoRPG.Motor.Servicios;

class DebugMaterialTest
{
    static void Main()
    {
        Console.WriteLine("=== DEBUG MATERIAL REPOSITORY ===");

        // Test del normalizador
        Console.WriteLine($"Normalizar 'Legendario': '{RarezaNormalizer.Normalizar("Legendario")}'");

        // Test de paths
        var overlayPath = PathProvider.PjDatosPath("materiales.json");
        Console.WriteLine($"Overlay path: {overlayPath}");
        Console.WriteLine($"Overlay exists: {File.Exists(overlayPath)}");

        if (File.Exists(overlayPath))
        {
            var content = File.ReadAllText(overlayPath);
            Console.WriteLine($"Overlay content: {content}");
        }

        // Test del repository
        var repo = new MaterialRepository();
        var hierro = repo.GetByNombre("Mineral de Hierro");

        if (hierro != null)
        {
            Console.WriteLine($"Material encontrado:");
            Console.WriteLine($"  Nombre: {hierro.Nombre}");
            Console.WriteLine($"  Rareza: {hierro.Rareza}");
            Console.WriteLine($"  Categoria: {hierro.Categoria}");
        }
        else
        {
            Console.WriteLine("Material NO encontrado");
        }

        // Listar todos los materiales
        var todos = repo.GetAll();
        Console.WriteLine($"Total materiales cargados: {todos.Count}");

        foreach (var mat in todos)
        {
            if (mat.Nombre.Contains("Hierro"))
            {
                Console.WriteLine($"Hierro found: {mat.Nombre} - {mat.Rareza} - {mat.Categoria}");
            }
        }
    }
}
