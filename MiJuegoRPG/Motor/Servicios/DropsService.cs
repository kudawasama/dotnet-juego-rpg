using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Persistencia ligera para drops únicos (UniqueOnce) por partida.
    /// Guarda/lee un conjunto de claves en PjDatos/drops_unicos.json.
    /// Clave: e:{enemigoIdOrNombre}|i:{itemName}.
    /// </summary>
    public static class DropsService
    {
        private static readonly object Lock = new object();
        private static HashSet<string> uniqueDrops = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static bool cargado = false;
        private static string RutaArchivo => PathProvider.PjDatosPath("drops_unicos.json");

        public static string ClaveUnique(string enemigoIdOrNombre, string itemName)
        {
            enemigoIdOrNombre = enemigoIdOrNombre?.Trim() ?? string.Empty;
            itemName = itemName?.Trim() ?? string.Empty;
            return $"e:{enemigoIdOrNombre}|i:{itemName}";
        }

        public static void Cargar()
        {
            lock (Lock)
            {
                if (cargado)
                    return;
                try
                {
                    var ruta = RutaArchivo;
                    if (File.Exists(ruta))
                    {
                        var json = File.ReadAllText(ruta);
                        var arr = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                        uniqueDrops = new HashSet<string>(arr.Where(s => !string.IsNullOrWhiteSpace(s)), StringComparer.OrdinalIgnoreCase);
                    }
                    cargado = true;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[DropsService] No se pudo cargar drops únicos: {ex.Message}");
                    cargado = true; // evitar reintentos continuos
                }
            }
        }

        public static void Guardar()
        {
            lock (Lock)
            {
                try
                {
                    var ruta = RutaArchivo;
                    var dir = Path.GetDirectoryName(ruta);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    var arr = uniqueDrops.ToList();
                    var json = JsonSerializer.Serialize(arr, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ruta, json);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[DropsService] No se pudo guardar drops únicos: {ex.Message}");
                }
            }
        }

        public static bool Marcado(string clave)
        {
            Cargar();
            lock (Lock)
            {
                return uniqueDrops.Contains(clave);
            }
        }

        /// <summary>
        /// Marca la clave si no existe. Devuelve true si se añadió (no existía), false si ya estaba marcada.
        /// </summary>
        /// <returns></returns>
        public static bool MarcarSiNoExiste(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                return false;
            Cargar();
            lock (Lock)
            {
                if (uniqueDrops.Contains(clave))
                    return false;
                uniqueDrops.Add(clave);
                return true;
            }
        }

        // Para GuardadoService
        public static List<string> ExportarKeys()
        {
            Cargar();
            lock (Lock)
            {
                return uniqueDrops.ToList();
            }
        }

        public static void ImportarKeys(IEnumerable<string> claves)
        {
            lock (Lock)
            {
                uniqueDrops = new HashSet<string>(claves?.Where(c => !string.IsNullOrWhiteSpace(c)) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                cargado = true;
            }
        }

        // --- Utilidades de administración ---
        public static int Count()
        {
            Cargar();
            lock (Lock)
            {
                return uniqueDrops.Count;
            }
        }

        public static IReadOnlyCollection<string> KeysSnapshot()
        {
            Cargar();
            lock (Lock)
            {
                return uniqueDrops.ToList().AsReadOnly();
            }
        }

        public static int ClearAll()
        {
            Cargar();
            lock (Lock)
            {
                int n = uniqueDrops.Count;
                uniqueDrops.Clear();
                // Persistimos inmediatamente para reflejar limpieza
                try
                {
                    Guardar();
                }
                catch { }
                return n;
            }
        }
    }
}
