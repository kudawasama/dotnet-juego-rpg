using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MiJuegoRPG.Objetos
{
    /// <summary>
    /// Configuración dinámica de rarezas, cargada desde JSON.
    /// Permite que los archivos rareza_pesos.json y rareza_perfeccion.json sean la fuente de verdad.
    /// </summary>
    public class RarezaConfig
    {
        /// <summary>
        /// Instancia singleton global de la configuración de rarezas.
        /// Debe inicializarse al cargar el juego (por ejemplo, en el loader principal).
        /// </summary>
        public static RarezaConfig? Instancia { get; private set; }

        /// <summary>
        /// Inicializa la instancia global (sobrescribe si ya existe).
        /// </summary>
        public static void SetInstancia(RarezaConfig config)
        {
            Instancia = config;
        }

        public Dictionary<string, double> Pesos { get; private set; } = new();
        public Dictionary<string, (int min, int max)> RangosPerfeccion { get; private set; } = new();

        /// <summary>
        /// Carga la configuración de rarezas desde los archivos JSON especificados.
        /// </summary>
        /// <param name="rutaPesos">Ruta a rareza_pesos.json</param>
        /// <param name="rutaRangos">Ruta a rareza_perfeccion.json</param>
        public void Cargar(string rutaPesos, string rutaRangos)
        {
            // Cargar pesos
            var jsonPesos = File.ReadAllText(rutaPesos);
            Pesos = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonPesos) ?? new();

            // Cargar rangos de perfección
            var jsonRangos = File.ReadAllText(rutaRangos);
            var temp = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonRangos) ?? new();
            RangosPerfeccion.Clear();
            foreach (var kv in temp)
            {
                if (kv.Value.Length == 2)
                    RangosPerfeccion[kv.Key] = (kv.Value[0], kv.Value[1]);
            }
        }

        /// <summary>
        /// Devuelve true si la rareza existe en la configuración cargada.
        /// </summary>
        public bool RarezaValida(string rareza) => Pesos.ContainsKey(rareza) && RangosPerfeccion.ContainsKey(rareza);
    }
}
