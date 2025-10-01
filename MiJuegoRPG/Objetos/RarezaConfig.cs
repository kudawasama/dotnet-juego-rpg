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
    public Dictionary<string, RarezaMeta> Metas { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

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

            ConstruirMetas();
        }

        /// <summary>
        /// Devuelve true si la rareza existe en la configuración cargada.
        /// </summary>
        public bool RarezaValida(string rareza) => Pesos.ContainsKey(rareza) && RangosPerfeccion.ContainsKey(rareza);

        /// <summary>
        /// Construye las métricas derivadas para cada rareza (meta-modelo) basadas en pesos y rangos de perfección.
        /// </summary>
        private void ConstruirMetas()
        {
            Metas.Clear();
            if (Pesos.Count == 0)
                return;

            double totalWeight = 0;
            foreach (var w in Pesos.Values) totalWeight += w;
            if (totalWeight <= 0) totalWeight = 1;

            // Precalcular scarcity para normalizar
            double maxScarcity = 1.0;
            var scarcityTemps = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in Pesos)
            {
                double prob = kv.Value / totalWeight;
                if (prob <= 0) prob = 1.0 / (totalWeight * 10); // evitar div 0
                double scarcityRaw = 1.0 / prob; // más grande = más escaso
                scarcityTemps[kv.Key] = scarcityRaw;
                if (scarcityRaw > maxScarcity) maxScarcity = scarcityRaw;
            }

            double logMax = Math.Log(maxScarcity + 1);
            const double PRICE_K = 0.6; // factor de influencia de escasez en precio (ajustable)
            foreach (var kv in Pesos)
            {
                var name = kv.Key;
                Pesos.TryGetValue(name, out var w);
                RangosPerfeccion.TryGetValue(name, out var rango);
                int min = rango.min == 0 && rango.max == 0 ? 50 : rango.min;
                int max = rango.min == 0 && rango.max == 0 ? 50 : rango.max;
                if (max < min) max = min;
                double perfAvg = (min + max) / 2.0;
                double baseStatMult = perfAvg / 100.0; // escalar 0..1
                if (baseStatMult <= 0) baseStatMult = 0.5; // fallback
                double prob = w / totalWeight;
                if (prob < 0) prob = 0;
                double scarcityRaw = scarcityTemps[name];
                double scarcityNorm = 0;
                if (logMax > 0) scarcityNorm = Math.Log(scarcityRaw + 1) / logMax; // 0..1
                if (scarcityNorm > 1) scarcityNorm = 1;
                double priceMult = baseStatMult * (1.0 + PRICE_K * scarcityNorm);
                // clamp de seguridad
                if (priceMult < 0.05) priceMult = 0.05;
                if (priceMult > 2.0) priceMult = 2.0;

                Metas[name] = new RarezaMeta
                {
                    Nombre = name,
                    Peso = w,
                    PerfMin = min,
                    PerfMax = max,
                    PerfAvg = perfAvg,
                    BaseStatMult = baseStatMult,
                    Prob = prob,
                    ScarcityRaw = scarcityRaw,
                    ScarcityNorm = scarcityNorm,
                    PriceMult = priceMult
                };
            }
        }

        public RarezaMeta ObtenerMeta(string rareza)
        {
            if (Metas.TryGetValue(rareza, out var meta)) return meta;
            // Fallback dinámico (no rompe)
            var rNorm = rareza;
            meta = new RarezaMeta
            {
                Nombre = rNorm,
                Peso = 1,
                PerfMin = 50,
                PerfMax = 50,
                PerfAvg = 50,
                BaseStatMult = 0.5,
                Prob = 0,
                ScarcityRaw = 1,
                ScarcityNorm = 0,
                PriceMult = 0.5
            };
            Metas[rNorm] = meta;
            return meta;
        }
    }

    public class RarezaMeta
    {
        public string Nombre { get; set; } = string.Empty;
        public double Peso { get; set; }
        public int PerfMin { get; set; }
        public int PerfMax { get; set; }
        public double PerfAvg { get; set; }
        public double BaseStatMult { get; set; }
        public double Prob { get; set; }
        public double ScarcityRaw { get; set; }
        public double ScarcityNorm { get; set; }
        public double PriceMult { get; set; }
    }
}
