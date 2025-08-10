using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor
{
    public static class MapaLoader
    {
        public static Mapa CargarMapaCompleto(string carpeta)
        {
            var sectores = new Dictionary<string, SectorData>();
            var archivos = Directory.GetFiles(carpeta, "*.json");
            foreach (var archivo in archivos)
            {
                var json = File.ReadAllText(archivo);
                var lista = JsonSerializer.Deserialize<List<SectorData>>(json);
                if (lista != null)
                {
                    foreach (var sector in lista)
                    {
                        if (sector != null && !sectores.ContainsKey(sector.Id))
                            sectores.Add(sector.Id, sector);
                    }
                }
            }
            return new Mapa(sectores);
        }
    }
}
