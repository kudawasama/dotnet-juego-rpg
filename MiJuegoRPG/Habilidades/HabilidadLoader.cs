using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MiJuegoRPG.Habilidades
{
    public static class HabilidadLoader
    {
        public static List<HabilidadData> CargarTodas(string carpeta)
        {
            var habilidades = new List<HabilidadData>();
            var archivos = Directory.GetFiles(carpeta, "*.json");
            foreach (var archivo in archivos)
            {
                var json = File.ReadAllText(archivo);
                var lista = JsonSerializer.Deserialize<List<HabilidadData>>(json);
                if (lista != null)
                    habilidades.AddRange(lista);
            }
            return habilidades;
        }
    }

    public class HabilidadData
    {
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Dictionary<string, int> AtributosNecesarios { get; set; } = new Dictionary<string, int>();
    public List<CondicionData> Condiciones { get; set; } = new List<CondicionData>();
    public string Beneficio { get; set; } = string.Empty;
    public List<string> Mejoras { get; set; } = new List<string>();
    public bool Oculta { get; set; } = false;
    }

    public class CondicionData
    {
    public string Tipo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public int? Cantidad { get; set; } = null;
    public string Restriccion { get; set; } = string.Empty;
    }
}
