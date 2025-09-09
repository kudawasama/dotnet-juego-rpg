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
            if (!Directory.Exists(carpeta))
            {
                Console.WriteLine($"[MapaLoader] Carpeta de mapa no encontrada: {carpeta}");
                return new Mapa(sectores);
            }
            var archivos = Directory.GetFiles(carpeta, "*.json", SearchOption.AllDirectories);
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            int vacios = 0, invalidos = 0, cargados = 0;
            bool debugIds = true; // Debug específico para diagnosticar sectores faltantes (8_22, 8_24)
            foreach (var archivo in archivos)
            {
                string json = File.ReadAllText(archivo);
                if (string.IsNullOrWhiteSpace(json))
                {
                    vacios++;
                    continue; // ignorar archivos vacíos (antes causaban excepción del usuario)
                }
                bool agregado = false;
                // Intento 1: archivo representa un único sector
                try
                {
                    var sector = JsonSerializer.Deserialize<SectorData>(json, opciones);
                    if (sector != null && !string.IsNullOrWhiteSpace(sector.Id))
                    {
                        if (!sectores.ContainsKey(sector.Id))
                        {
                            sectores.Add(sector.Id, sector);
                            cargados++; agregado = true;
                            if (debugIds && (sector.Id == "8_22" || sector.Id == "8_24"))
                                Console.WriteLine($"[MapaLoader][DEBUG] Cargado sector {sector.Id} desde {archivo} (modo objeto)");
                        }
                        else
                        {
                            // Preferir el primero, ignorar duplicado
                        }
                    }
                }
                catch { /* se intenta modo lista */ }
                if (!agregado)
                {
                    try
                    {
                        var lista = JsonSerializer.Deserialize<List<SectorData>>(json, opciones);
                        if (lista != null)
                        {
                            foreach (var sector in lista)
                            {
                                if (sector != null && !string.IsNullOrWhiteSpace(sector.Id) && !sectores.ContainsKey(sector.Id))
                                {
                                    sectores.Add(sector.Id, sector); cargados++; agregado = true;
                                    if (debugIds && (sector.Id == "8_22" || sector.Id == "8_24"))
                                        Console.WriteLine($"[MapaLoader][DEBUG] Cargado sector {sector.Id} desde {archivo} (modo lista)");
                                }
                            }
                        }
                        if (!agregado) invalidos++;
                    }
                    catch
                    {
                        invalidos++;
                        if (debugIds && (archivo.EndsWith("8_22.json") || archivo.EndsWith("8_24.json")))
                            Console.WriteLine($"[MapaLoader][DEBUG] Falló deserialización para {archivo}");
                    }
                }
            }
            if (vacios > 0 || invalidos > 0)
            {
                Console.WriteLine($"[MapaLoader] Cargados: {cargados} | Vacíos ignorados: {vacios} | Inválidos: {invalidos}");
                if (vacios > 0)
                    Console.WriteLine("[MapaLoader] Sugerencia: Ejecuta con --reparar-sectores para autocompletar archivos vacíos.");
            }
            if (debugIds)
            {
                Console.WriteLine("[MapaLoader][DEBUG] ¿Existe 8_22?: " + sectores.ContainsKey("8_22") + " | ¿Existe 8_24?: " + sectores.ContainsKey("8_24"));
            }
            return new Mapa(sectores);
        }
    }
}
