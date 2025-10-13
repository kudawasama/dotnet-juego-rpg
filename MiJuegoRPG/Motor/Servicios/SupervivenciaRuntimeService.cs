using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Aplica progresión de hambre/sed/fatiga/temperatura en runtime a partir de la configuración.
    /// Protegido por bandera FeatureEnabled para no afectar el juego hasta que se integre en bucles.
    /// </summary>
    public class SupervivenciaRuntimeService
    {
        public bool FeatureEnabled { get; set; } = false; // bandera global segura
        private readonly SupervivenciaService cfg;

        public SupervivenciaRuntimeService(SupervivenciaService cfg)
        {
            this.cfg = cfg;
        }

        /// <summary>
        /// Aplica un tick de supervivencia.
        /// contexto: p.ej. "Explorar", "Viajar", "Entrenar", "Combate", "Descanso".
        /// bioma: nombre del bioma actual, debe existir en config para aplicar reglas; si no, se usan defaults.
        /// minutos: cantidad de minutos simulados.
        /// </summary>
        public void ApplyTick(Personaje.Personaje pj, string contexto, string bioma, int minutos)
        {
            if (!FeatureEnabled || pj == null || minutos <= 0)
                return;

            var cfg = this.cfg.Config;
            var tasas = cfg.Tasas;
            var multCtx = this.cfg.ObtenerMultiplicadores(contexto);
            var reglas = this.cfg.ObtenerReglasBioma(bioma);

            double horas = minutos / 60.0;
            // Consumos base por hora ajustados por contexto
            double dHambre = tasas.HambrePorHora * multCtx.Hambre * horas;
            double dSed = tasas.SedPorHora * multCtx.Sed * horas;
            double dFatiga = tasas.FatigaPorHora * multCtx.Fatiga * horas;

            // Ajustes por bioma (multiplicadores específicos)
            dHambre *= reglas.HambreMultiplier <= 0 ? 1.0 : reglas.HambreMultiplier;
            dSed *= reglas.SedMultiplier <= 0 ? 1.0 : reglas.SedMultiplier;
            dFatiga *= reglas.FatigaMultiplier <= 0 ? 1.0 : reglas.FatigaMultiplier;

            pj.Hambre += dHambre;
            pj.Sed += dSed;
            pj.Fatiga += dFatiga;

            // Temperatura: aproximar hacia TempDia/TempNoche según hora del mundo si estuviera disponible.
            // Por ahora: relajar hacia el promedio simple para no romper sin hora de mundo.
            double targetTemp = (reglas.TempDia + reglas.TempNoche) / 2.0;
            if (targetTemp == 0 && reglas.TempDia == 0 && reglas.TempNoche == 0)
                targetTemp = pj.TempActual; // sin datos, no mover
            double rate = Math.Max(0, cfg.Tasas.TempRecuperacionPorHora) * horas;
            pj.TempActual = pj.TempActual + ((targetTemp - pj.TempActual) * Math.Min(1.0, rate));

            pj.ClampEstadosSupervivencia();

            // Avisos por transición de umbral (OK/ADVERTENCIA/CRÍTICO)
            try
            {
                var (etH, etS, etF) = this.cfg.EtiquetasHSF(pj.Hambre, pj.Sed, pj.Fatiga);
                if (!string.Equals(etH, pj.UltHambreEstado, StringComparison.Ordinal))
                {
                    BusEventos.Instancia.Publicar(new EventoSupervivenciaUmbralCruzado("Hambre", pj.UltHambreEstado, etH, pj.Hambre));
                    pj.UltHambreEstado = etH;
                }
                if (!string.Equals(etS, pj.UltSedEstado, StringComparison.Ordinal))
                {
                    BusEventos.Instancia.Publicar(new EventoSupervivenciaUmbralCruzado("Sed", pj.UltSedEstado, etS, pj.Sed));
                    pj.UltSedEstado = etS;
                }
                if (!string.Equals(etF, pj.UltFatigaEstado, StringComparison.Ordinal))
                {
                    BusEventos.Instancia.Publicar(new EventoSupervivenciaUmbralCruzado("Fatiga", pj.UltFatigaEstado, etF, pj.Fatiga));
                    pj.UltFatigaEstado = etF;
                }
            }
            catch { }
        }
    }
}
