using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Servicio puro para cálculo de Puntos de Acción (PA) por turno.
    /// Fórmula: BasePA + floor(Agilidad / AgilityDivisor) + floor(Destreza / DexterityDivisor) + floor(Nivel / LevelDivisor)
    ///          + sum(equipo.BonusPA) + sum(buffs) - sum(debuffs), luego clamp [PAMin, PAMax].
    /// (Buffs / debuffs PA aún no implementados: retornan 0 en esta fase.)
    /// </summary>
    public static class ActionPointService
    {
        public static int ComputePA(Personaje.Personaje p, CombatConfig cfg)
        {
            if (p == null)
                return cfg.BasePA;
            int basePA = cfg.BasePA;
            int fromAgi = (int)(p.Atributos.Agilidad / cfg.AgilityDivisor);
            int fromDex = (int)(p.Atributos.Destreza / cfg.DexterityDivisor);
            int fromLvl = (int)(p.Nivel / cfg.LevelDivisor);

            // Bonus de equipo: si en el futuro se añaden propiedades especificas, aquí se mapearán.
            int equipBonus = 0; // placeholder (no hay estructura estandarizada BonusPA todavía)
            int buffs = 0;      // Fase 1: sin sistema de buffs PA consolidado
            int debuffs = 0;    // Fase 1: idem

            int raw = basePA + fromAgi + fromDex + fromLvl + equipBonus + buffs - debuffs;
            if (raw < cfg.PAMin)
                raw = cfg.PAMin;
            if (raw > cfg.PAMax)
                raw = cfg.PAMax;
            return raw;
        }
    }
}
