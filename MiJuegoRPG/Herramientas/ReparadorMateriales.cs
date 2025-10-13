using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Herramientas
{
    public class ReparacionMaterialesResultado
    {
        public int SectoresEscaneados
        {
            get; set;
        }
        public int SectoresModificados
        {
            get; set;
        }
        public int NodosAfectados
        {
            get; set;
        }
        public int MaterialesEliminados
        {
            get; set;
        }
        public int ListasNullNormalizadas
        {
            get; set;
        }
        public string ReportePath { get; set; } = string.Empty;
    }

    public static class ReparadorMateriales
    {
        /// <summary>
        /// Repara materiales inválidos en nodos de recolección de todos los sectores.
        /// Regla: elimina materiales nulos, con Nombre vacío/espacios o Cantidad. <= 0. Si lista de materiales es null, la normaliza a [].
        /// </summary>
        /// <param name="aplicarCambios">Si true, escribe los JSON reparados; si false, solo genera reporte (dry-run).</param>
        /// <param name="rutaReporte">Ruta del archivo de reporte de cambios. Si es directorio, se crea un archivo dentro. Si es vacío, usa PjDatos/validacion.</param>
        /// <returns></returns>
        public static ReparacionMaterialesResultado Reparar(bool aplicarCambios, string? rutaReporte = null)
        {
            var resultado = new ReparacionMaterialesResultado();
            var sectoresDir = PathProvider.SectoresDir();
            var archivos = Directory.Exists(sectoresDir)
                ? Directory.GetFiles(sectoresDir, "*.json", SearchOption.AllDirectories)
                : Array.Empty<string>();

            var opcionesLectura = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var opcionesEscritura = new JsonSerializerOptions { WriteIndented = true };

            // Preparar ruta de reporte
            string reporteFinalPath = PrepararRutaReporte(rutaReporte);
            var sb = new StringBuilder();
            sb.AppendLine($"[ReparadorMateriales] Inicio {(aplicarCambios ? "WRITE" : "DRY-RUN")} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Carpeta sectores: {sectoresDir}");

            foreach (var archivo in archivos)
            {
                resultado.SectoresEscaneados++;
                try
                {
                    var json = File.ReadAllText(archivo);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        // No reparar estructura: lo hace ReparadorSectores. Aquí solo materiales.
                        continue;
                    }
                    var sector = JsonSerializer.Deserialize<SectorData>(json, opcionesLectura);
                    if (sector == null)
                        continue;

                    bool modSector = false;
                    int nodosAfectadosEnSector = 0;
                    int materialesEliminadosEnSector = 0;
                    int listasNormalizadasEnSector = 0;

                    var nodos = sector.NodosRecoleccion;
                    if (nodos != null)
                    {
                        foreach (var nodo in nodos)
                        {
                            bool modNodo = false;
                            if (nodo == null)
                                continue;
                            if (nodo.Materiales == null)
                            {
                                nodo.Materiales = new List<MaterialCantidad>();
                                listasNormalizadasEnSector++;
                                modNodo = true;
                            }
                            else
                            {
                                int antes = nodo.Materiales.Count;
                                nodo.Materiales = nodo.Materiales
                                    .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Nombre) && m.Cantidad > 0)
                                    .ToList();
                                int despues = nodo.Materiales.Count;
                                if (despues < antes)
                                {
                                    materialesEliminadosEnSector += antes - despues;
                                    modNodo = true;
                                }
                            }

                            if (modNodo)
                            {
                                nodosAfectadosEnSector++;
                                modSector = true;
                            }
                        }
                    }

                    if (modSector)
                    {
                        resultado.SectoresModificados++;
                        resultado.NodosAfectados += nodosAfectadosEnSector;
                        resultado.MaterialesEliminados += materialesEliminadosEnSector;
                        resultado.ListasNullNormalizadas += listasNormalizadasEnSector;

                        sb.AppendLine($"- {sector.Id} ({Path.GetFileName(archivo)}): nodos afectados={nodosAfectadosEnSector}, materiales eliminados={materialesEliminadosEnSector}, listas normalizadas={listasNormalizadasEnSector}");

                        if (aplicarCambios)
                        {
                            try
                            {
                                var jsonOut = JsonSerializer.Serialize(sector, opcionesEscritura);
                                File.WriteAllText(archivo, jsonOut);
                            }
                            catch (Exception exw)
                            {
                                sb.AppendLine($"  [ERROR] No se pudo escribir {archivo}: {exw.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"[WARN] {archivo}: {ex.Message}");
                }
            }

            sb.AppendLine($"Resumen: sectores escaneados={resultado.SectoresEscaneados}, modificados={resultado.SectoresModificados}, nodos afectados={resultado.NodosAfectados}, materiales eliminados={resultado.MaterialesEliminados}, listas normalizadas={resultado.ListasNullNormalizadas}");

            try
            {
                File.WriteAllText(reporteFinalPath, sb.ToString(), Encoding.UTF8);
                resultado.ReportePath = reporteFinalPath;
            }
            catch { /* best-effort */ }

            return resultado;
        }

        private static string PrepararRutaReporte(string? rutaReporte)
        {
            string baseDir;
            if (string.IsNullOrWhiteSpace(rutaReporte))
            {
                baseDir = PathProvider.PjDatosPath("validacion");
                Directory.CreateDirectory(baseDir);
                return Path.Combine(baseDir, $"materiales_reparacion_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }

            // Si es directorio, construir nombre dentro
            if (Directory.Exists(rutaReporte))
            {
                return Path.Combine(rutaReporte, $"materiales_reparacion_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }

            // Si termina con .txt u otra extensión, usar tal cual pero asegurando carpeta
            try
            {
                var dir = Path.GetDirectoryName(rutaReporte);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
            }
            catch { }
            return rutaReporte!;
        }
    }
}
