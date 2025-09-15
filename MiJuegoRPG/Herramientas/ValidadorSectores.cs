using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Herramientas
{
    // Nota: usamos PjDatos.SectorData real del juego para validar con el mismo contrato.

    public static class ValidadorSectores
    {
        public static void ValidarSectores(string rutaBase)
        {
            var sectores = new Dictionary<string, SectorData>();
            foreach (var file in Directory.GetFiles(rutaBase, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var sector = JsonSerializer.Deserialize<SectorData>(json);
                    if (sector == null)
                    {
                        Console.WriteLine($"[ERROR] No se pudo deserializar: {file}");
                        continue;
                    }
                    sectores[sector.Id] = sector;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {file}: {ex.Message}");
                }
            }

            // Validar campos obligatorios y conexiones
            foreach (var kv in sectores)
            {
                var s = kv.Value;
                if (string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Nombre) || s.Conexiones == null)
                {
                    Console.WriteLine($"[ERROR] Sector con campos faltantes: {s.Id} ({s.Nombre})");
                }
                if (s.Conexiones != null)
                {
                    foreach (var conn in s.Conexiones)
                    {
                        if (!sectores.ContainsKey(conn))
                        {
                            Console.WriteLine($"[ERROR] Conexión inválida en sector {s.Id} -> {conn}");
                        }
                        else
                        {
                            // Bidireccionalidad
                            var otro = sectores[conn];
                            if (otro.Conexiones == null || !otro.Conexiones.Contains(s.Id))
                            {
                                Console.WriteLine($"[WARN] Conexión no bidireccional: {s.Id} -> {conn} (falta {conn} -> {s.Id})");
                            }
                        }
                    }
                }
            }

            // BFS desde CiudadPrincipal (o fallback primer sector)
            var start = sectores.Values.FirstOrDefault(x => x.CiudadPrincipal) ?? sectores.Values.FirstOrDefault();
            if (start != null)
            {
                var visitados = new HashSet<string>();
                var q = new Queue<string>();
                q.Enqueue(start.Id);
                visitados.Add(start.Id);
                while (q.Count > 0)
                {
                    var cur = q.Dequeue();
                    var s = sectores[cur];
                    if (s.Conexiones == null) continue;
                    foreach (var nxt in s.Conexiones)
                    {
                        if (!sectores.ContainsKey(nxt)) continue;
                        if (visitados.Add(nxt)) q.Enqueue(nxt);
                    }
                }

                var inalcanzables = sectores.Keys.Where(id => !visitados.Contains(id)).ToList();
                if (inalcanzables.Count > 0)
                {
                    Console.WriteLine($"[WARN] Sectores inalcanzables desde '{start.Id}': {inalcanzables.Count}");
                    // imprimir algunas muestras para debug
                    foreach (var id in inalcanzables.Take(50))
                        Console.WriteLine($"    - {id}");
                }
            }

            Console.WriteLine($"Validación completada. Sectores revisados: {sectores.Count}");
        }
    }
}
