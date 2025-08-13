using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

class FixTipoObjetoJson
{
    static void Main(string[] args)
    {
        string path = @"c:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\PjDatos\PjGuardados\Grid.json";
        string json = File.ReadAllText(path);
        var root = JsonNode.Parse(json);
        bool changed = false;

        // Corrige NuevosObjetos
        var nuevos = root?["Inventario"]?["NuevosObjetos"]?.AsArray();
        if (nuevos != null)
        {
            foreach (var objCant in nuevos)
            {
                var obj = objCant?["Objeto"];
                if (obj != null && obj["TipoObjeto"] == null)
                {
                    if (obj["Daño"] != null) obj["TipoObjeto"] = "Arma";
                    else if (obj["Defensa"] != null && obj["Categoria"]?.ToString() == "Cabeza") obj["TipoObjeto"] = "Casco";
                    else if (obj["Defensa"] != null) obj["TipoObjeto"] = "Armadura";
                    else obj["TipoObjeto"] = "Material";
                    changed = true;
                }
            }
        }
        // Corrige Equipo
        var equipo = root?["Inventario"]?["Equipo"];
        if (equipo != null)
        {
            foreach (var slot in equipo.AsObject())
            {
                var obj = slot.Value;
                if (obj != null && obj["TipoObjeto"] == null)
                {
                    if (obj["Daño"] != null) obj["TipoObjeto"] = "Arma";
                    else if (obj["Defensa"] != null && obj["Categoria"]?.ToString() == "Cabeza") obj["TipoObjeto"] = "Casco";
                    else if (obj["Defensa"] != null) obj["TipoObjeto"] = "Armadura";
                    else obj["TipoObjeto"] = "Material";
                    changed = true;
                }
            }
        }
        if (changed)
        {
            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Grid.json corregido con TipoObjeto en todos los objetos.");
        }
        else
        {
            Console.WriteLine("No se detectaron objetos sin TipoObjeto.");
        }
    }
}
