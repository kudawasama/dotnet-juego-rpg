namespace MiJuegoRPG.Objetos
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// Configuración dinámica de rarezas, cargada desde JSON.
    /// Permite que los archivos rareza_pesos.json y rareza_perfeccion.json sean la fuente de verdad.
    /// </summary>
    public class RarezaConfig
    {
        /// <summary>
        /// Gets the global singleton instance of the rarity configuration.
        /// Debe inicializarse al cargar el juego (por ejemplo, en el loader principal).
        /// </summary>
        public static RarezaConfig? Instancia { get; private set; }

        public Dictionary<string, double> Pesos { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, (int Min, int Max)> RangosPerfeccion { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, RarezaMeta> Metas { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Inicializa la instancia global (sobrescribe si ya existe).
        /// </summary>
        /// <param name="config">Configuración a establecer como instancia global.</param>
        public static void SetInstancia(RarezaConfig config)
        {
            Instancia = config;
        }

        /// <summary>
        /// Carga la configuración de rarezas desde los archivos JSON especificados.
        /// </summary>
        /// <param name="rutaPesos">Ruta a rareza_pesos.json.</param>
        /// <param name="rutaRangos">Ruta a rareza_perfeccion.json.</param>
        public void Cargar(string rutaPesos, string rutaRangos)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            // Cargar pesos
            var jsonPesos = File.ReadAllText(rutaPesos);
            var pesosTmp = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonPesos, options) ?? new();
            Pesos = new Dictionary<string, double>(pesosTmp, StringComparer.OrdinalIgnoreCase);

            // Cargar rangos de perfección
            var jsonRangos = File.ReadAllText(rutaRangos);
            var temp = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonRangos, options) ?? new();
            var rangos = new Dictionary<string, (int Min, int Max)>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in temp)
            {
                if (kv.Value.Length == 2)
                {
                    rangos[kv.Key] = (kv.Value[0], kv.Value[1]);
                }
            }

            RangosPerfeccion = rangos;

            ConstruirMetas();
        }

        /// <summary>
        /// Devuelve true si la rareza existe en la configuración cargada.
        /// </summary>
        /// <param name="rareza">Nombre de la rareza a validar.</param>
        /// <returns>True si existe en Pesos y RangosPerfeccion; de lo contrario false.</returns>
        public bool RarezaValida(string rareza)
        {
            var key = (rareza ?? string.Empty).Trim();
            return Pesos.ContainsKey(key) && RangosPerfeccion.ContainsKey(key);
        }

        /// <summary>
        /// Obtiene la meta de rareza (métricas derivadas) para el nombre indicado.
        /// </summary>
        /// <param name="rareza">Nombre de la rareza.</param>
        /// <returns>Instancia de RarezaMeta correspondiente (o fallback si no existe).</returns>
        public RarezaMeta ObtenerMeta(string rareza)
        {
            var key = (rareza ?? string.Empty).Trim();
            if (Metas.TryGetValue(key, out var meta))
            {
                return meta;
            }

            // Fallback dinámico (no rompe)
            var rNorm = key;
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
                PriceMult = 0.5,
            };
            Metas[rNorm] = meta;
            return meta;
        }

        /// <summary>
        /// Construye las métricas derivadas para cada rareza (meta-modelo) basadas en pesos y rangos de perfección.
        /// </summary>
        private void ConstruirMetas()
        {
            Metas.Clear();

            // Unión de claves: considerar rarezas presentes en Pesos y/o Rangos
            var todas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in Pesos.Keys)
            {
                todas.Add(k);
            }

            foreach (var k in RangosPerfeccion.Keys)
            {
                todas.Add(k);
            }

            if (todas.Count == 0)
            {
                return;
            }

            // Sumar pesos (clamp >= 0)
            double totalWeight = 0;
            foreach (var k in todas)
            {
                Pesos.TryGetValue(k, out var w);
                if (w < 0)
                {
                    w = 0;
                }

                totalWeight += w;
            }

            if (totalWeight <= 0)
            {
                totalWeight = 1;
            }

            // Precalcular scarcity normalizada
            double maxScarcity = 1.0;
            var scarcityTemps = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in todas)
            {
                Pesos.TryGetValue(k, out var w);
                if (w < 0)
                {
                    w = 0;
                }

                double probW = w / totalWeight;
                if (probW <= 0)
                {
                    probW = 1.0 / (totalWeight * 10); // evitar div 0
                }

                double scarcityRaw = 1.0 / probW; // mayor = más escaso
                scarcityTemps[k] = scarcityRaw;
                if (scarcityRaw > maxScarcity)
                {
                    maxScarcity = scarcityRaw;
                }
            }

            double logMax = Math.Log(maxScarcity + 1);
            const double PRICE_K = 0.6; // factor de influencia de escasez en precio (ajustable)
            foreach (var k in todas)
            {
                Pesos.TryGetValue(k, out var w);
                if (w < 0)
                {
                    w = 0;
                }

                RangosPerfeccion.TryGetValue(k, out var rango);

                int min = (rango.Min == 0 && rango.Max == 0) ? 50 : rango.Min;
                int max = (rango.Min == 0 && rango.Max == 0) ? 50 : rango.Max;
                if (max < min)
                {
                    max = min;
                }

                double perfAvg = (min + max) / 2.0;
                double baseStatMult = perfAvg / 100.0; // escalar 0..1
                if (baseStatMult <= 0)
                {
                    baseStatMult = 0.5; // fallback
                }

                double prob = w <= 0 ? 0 : w / totalWeight;
                double scarcityRaw = scarcityTemps[k];
                double scarcityNorm = 0;
                if (logMax > 0)
                {
                    scarcityNorm = Math.Log(scarcityRaw + 1) / logMax; // 0..1
                }

                if (scarcityNorm > 1)
                {
                    scarcityNorm = 1;
                }

                double priceMult = baseStatMult * (1.0 + (PRICE_K * scarcityNorm));

                // clamp de seguridad
                if (priceMult < 0.05)
                {
                    priceMult = 0.05;
                }

                if (priceMult > 2.0)
                {
                    priceMult = 2.0;
                }

                Metas[k] = new RarezaMeta
                {
                    Nombre = k,
                    Peso = w,
                    PerfMin = min,
                    PerfMax = max,
                    PerfAvg = perfAvg,
                    BaseStatMult = baseStatMult,
                    Prob = prob,
                    ScarcityRaw = scarcityRaw,
                    ScarcityNorm = scarcityNorm,
                    PriceMult = priceMult,
                };
            }
        }
    }

    // RarezaMeta se movió a su propio archivo para cumplir SA1402 (un tipo por archivo).
}
