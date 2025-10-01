using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Objetos
{
    /// <summary>
    /// Helper centralizado para operar con rarezas (string) tras migración dinámica.
    /// Mantiene diccionarios de multiplicadores y conversión legacy (enum -> string).
    /// </summary>
    public static class RarezaHelper
    {
        // Normalización a formato TitleCase simple (primera letra mayúscula, resto minúsculas)
        public static string Normalizar(string? rareza)
        {
            if (string.IsNullOrWhiteSpace(rareza)) return "Normal";
            rareza = rareza.Trim();
            return char.ToUpperInvariant(rareza[0]) + (rareza.Length > 1 ? rareza.Substring(1).ToLowerInvariant() : string.Empty);
        }

        public static double MultiplicadorBase(string rareza)
        {
            var r = Normalizar(rareza);
            var cfg = RarezaConfig.Instancia;
            if (cfg == null) return 1.0;
            return cfg.ObtenerMeta(r).BaseStatMult;
        }

        public static double MultiplicadorPrecio(string rareza)
        {
            var r = Normalizar(rareza);
            var cfg = RarezaConfig.Instancia;
            if (cfg == null) return 1.0;
            return cfg.ObtenerMeta(r).PriceMult;
        }

        public static double MultiplicadorDrop(string rareza)
        {
            // Usamos el multiplicador base (estadístico) como factor para mantener monotonicidad.
            var cfg = RarezaConfig.Instancia;
            if (cfg == null) return 0.5;
            return cfg.ObtenerMeta(Normalizar(rareza)).BaseStatMult;
        }

        // Conversión legacy enum -> string centralizada (para pasos graduales)
        public static string FromEnum(Rareza legacy) => legacy.ToString();
    }
}
