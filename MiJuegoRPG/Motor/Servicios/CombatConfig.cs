using System;
using System.IO;
using System.Text.Json;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Configuración de combate (fase 1: PA + parámetros básicos de pipeline futuro).
    /// Carga desde DatosJuego/config/combat_config.json si existe; si no, usa defaults.
    /// </summary>
    public class CombatConfig
    {
        public int BasePA { get; set; } = 2;
        public int PAMax { get; set; } = 6;
        public int PAMin { get; set; } = 1;
        public int AgilityDivisor { get; set; } = 30;
        public int DexterityDivisor { get; set; } = 40;
        public int LevelDivisor { get; set; } = 10;
        // Ajuste inicial post benchmark: reducir multiplicador crítico para acercar media pipeline a legacy
        public double CritMultiplier { get; set; } = 1.35;
        public double CritCap { get; set; } = 0.50;
        public double PenetracionMax { get; set; } = 0.9;
        public double MinHit { get; set; } = 0.05;
        public bool ModoAcciones { get; set; } = false; // Flag para activar nuevo sistema (OFF por defecto)
        public bool UseNewDamagePipelineShadow { get; set; } = false; // Ejecuta el nuevo pipeline en modo sombra (no aplica daño, solo compara)
        public bool UseNewDamagePipelineLive { get; set; } = false; // (Reservado) Cuando sea true reemplazará el cálculo legacy
        public bool ReducePenetracionEnCritico { get; set; } = true; // Si true, reduce la penetración efectiva en golpes críticos
        public double FactorPenetracionCritico { get; set; } = 0.80; // Ajuste tuning: menos castigo a la penetración en críticos (antes 0.75)
        public bool UseCritDiminishingReturns { get; set; } = true; // Aplica DR a CritChance
        public double CritChanceHardCap { get; set; } = 0.60; // Límite duro recomendado
        public double CritDiminishingK { get; set; } = 50.0; // Parámetro K de la curva Stat/(Stat+K)
        public double CritScalingFactor { get; set; } = 0.65; // Factor F para escalar (CritMultiplier-1); tuning desde sweep

        public static CombatConfig LoadOrDefault()
        {
            try
            {
                var path = PathProvider.ConfigPath("combat_config.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var cfg = JsonSerializer.Deserialize<CombatConfig>(json);
                    if (cfg != null)
                        return cfg;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[CombatConfig] Error cargando config: {ex.Message}. Usando valores por defecto.");
            }
            return new CombatConfig();
        }
    }
}
