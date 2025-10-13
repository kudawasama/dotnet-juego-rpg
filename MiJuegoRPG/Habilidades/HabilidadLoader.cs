using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Habilidades
{
    public static class HabilidadLoader
    {
        public static List<HabilidadData> CargarTodas(string carpeta)
        {
            var habilidades = new List<HabilidadData>();
            var archivos = Directory.GetFiles(carpeta, "*.json", SearchOption.AllDirectories);
            foreach (var archivo in archivos)
            {
                var json = File.ReadAllText(archivo);
                // Permitir que un archivo contenga lista o un único objeto (se normaliza a lista)
                List<HabilidadData>? lista = null;
                try
                {
                    lista = JsonSerializer.Deserialize<List<HabilidadData>>(json, JsonOptions());
                }
                catch
                {
                    try
                    {
                        var single = JsonSerializer.Deserialize<HabilidadData>(json, JsonOptions());
                        if (single != null)
                            lista = new List<HabilidadData> { single };
                    }
                    catch { /* tolerante: archivo inválido, se ignora */ }
                }
                if (lista != null)
                {
                    // Normalizar IDs duplicadas y whitespace
                    foreach (var h in lista)
                    {
                        if (h == null)
                            continue;
                        h.Id = h.Id?.Trim() ?? string.Empty;
                        h.Nombre = string.IsNullOrWhiteSpace(h.Nombre) ? h.Id : h.Nombre.Trim();
                        h.Tipo = h.Tipo?.Trim() ?? string.Empty;
                        h.Categoria = h.Categoria?.Trim() ?? string.Empty;
                        h.Descripcion = h.Descripcion?.Trim() ?? string.Empty;
                        h.AccionId = h.AccionId?.Trim();
                        // Limpieza de evoluciones nulas
                        if (h.Evoluciones == null)
                            h.Evoluciones = new List<EvolucionData>();
                        foreach (var evo in h.Evoluciones)
                        {
                            if (evo == null)
                                continue;
                            evo.Id = evo.Id?.Trim() ?? string.Empty;
                            evo.Nombre = string.IsNullOrWhiteSpace(evo.Nombre) ? evo.Id : evo.Nombre.Trim();
                            if (evo.Condiciones == null)
                                evo.Condiciones = new List<CondicionData>();
                        }
                    }
                    habilidades.AddRange(lista.Where(x => x != null));
                }
            }
            return habilidades;
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }
    }
}
