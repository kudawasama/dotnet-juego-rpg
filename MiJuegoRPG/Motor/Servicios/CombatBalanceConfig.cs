using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Configuración de límites (caps) de combate cargada desde progression.json (sección opcional StatsCaps).
    /// Centraliza clamps para Precision, CritChance, CritMult y Penetracion.
    /// </summary>
    public static class CombatBalanceConfig
    {
        private static bool loaded = false;

        // Defaults conservadores (alineados con Docs/progression_config.md)
        public static double PrecisionMax { get; private set; } = 0.95;
        public static double CritChanceMax { get; private set; } = 0.50;
        public static double CritMultMin { get; private set; } = 1.25;
        public static double CritMultMax { get; private set; } = 1.75;
        public static double PenetracionMax { get; private set; } = 0.25; // runtime receptores clampean a 0.90 por seguridad

        private class StatsCapsDto
        {
            public double? PrecisionMax
            {
                get; set;
            }
            public double? CritChanceMax
            {
                get; set;
            }
            public double? CritMultMin
            {
                get; set;
            }
            public double? CritMultMax
            {
                get; set;
            }
            public double? PenetracionMax
            {
                get; set;
            }
        }

        private class ProgressionDto
        {
            public StatsCapsDto? StatsCaps
            {
                get; set;
            }
        }

        /// <summary>
        /// Lee caps desde DatosJuego/progression.json (solo una vez). Si faltan, mantiene defaults.
        /// </summary>
        public static void EnsureLoaded()
        {
            if (loaded)
                return;
            try
            {
                var ruta = PathProvider.CombineData("progression.json");
                if (System.IO.File.Exists(ruta))
                {
                    var json = System.IO.File.ReadAllText(ruta);
                    var cfg = System.Text.Json.JsonSerializer.Deserialize<ProgressionDto>(json);
                    var caps = cfg?.StatsCaps;
                    if (caps != null)
                    {
                        if (caps.PrecisionMax.HasValue)
                            PrecisionMax = Clamp01(caps.PrecisionMax.Value, 0.95);
                        if (caps.CritChanceMax.HasValue)
                            CritChanceMax = Clamp01(caps.CritChanceMax.Value, 0.50);
                        if (caps.CritMultMin.HasValue)
                            CritMultMin = Math.Max(1.0, caps.CritMultMin.Value);
                        if (caps.CritMultMax.HasValue)
                            CritMultMax = Math.Max(CritMultMin, caps.CritMultMax.Value);
                        if (caps.PenetracionMax.HasValue)
                            PenetracionMax = Clamp01(caps.PenetracionMax.Value, 0.25);
                    }
                }
            }
            catch
            {
                // Silencioso: dejamos defaults
            }
            finally
            {
                loaded = true;
            }
        }

        private static double Clamp01(double v, double fallback)
        {
            if (double.IsNaN(v) || double.IsInfinity(v))
                return fallback;
            return Math.Clamp(v, 0.0, 1.0);
        }

        public static double ClampPrecision(double p)
        {
            EnsureLoaded();
            return Math.Clamp(p, 0.0, PrecisionMax);
        }

        public static double ClampCritChance(double c)
        {
            EnsureLoaded();
            return Math.Clamp(c, 0.0, CritChanceMax);
        }

        /// <summary>
        /// Aplica diminishing returns a una probabilidad de crítico en escala 0..1 usando fórmula cap * (stat*100 / (stat*100 + K)).
        /// </summary>
        /// <returns></returns>
        public static double CritChanceWithDR(double critChanceRaw, double cap, double k)
        {
            if (critChanceRaw <= 0)
                return 0;
            if (k <= 0)
                return Math.Clamp(critChanceRaw, 0, cap);
            double stat = critChanceRaw * 100.0; // reinterpretar como “puntos” para suavizar
            double eff = cap * (stat / (stat + k));
            if (eff < 0)
                eff = 0;
            return Math.Min(eff, cap);
        }

        public static double ClampCritMult(double m)
        {
            EnsureLoaded();
            return Math.Clamp(m, CritMultMin, CritMultMax);
        }

        public static double ClampPenetracion(double p)
        {
            EnsureLoaded();
            return Math.Clamp(p, 0.0, PenetracionMax);
        }
    }
}
