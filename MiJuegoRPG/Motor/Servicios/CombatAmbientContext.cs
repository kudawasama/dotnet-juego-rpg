using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Contexto ambiental de combate para pasar metadatos del atacante al receptor
    /// sin cambiar firmas públicas. Se limpia automáticamente por el llamador.
    /// </summary>
    public static class CombatAmbientContext
    {
        /// <summary>
        /// Penetración del atacante (0..1). Usada para reducir defensa efectiva
        /// del objetivo antes de aplicar mitigaciones y resistencias.
        /// </summary>
        [ThreadStatic]
        public static double? AttackerPenetracion;

        /// <summary>
        /// Obtiene la penetración efectiva (clamp defensivo 0..0.9) y la limpia opcionalmente.
        /// </summary>
        public static double GetPenetracion(bool clear = false)
        {
            double val = Math.Clamp(AttackerPenetracion ?? 0.0, 0.0, 0.9);
            if (clear) AttackerPenetracion = null;
            return val;
        }

        /// <summary>
        /// Helper para ejecutar una acción con una penetración temporal establecida.
        /// </summary>
        public static T WithPenetracion<T>(double pen, Func<T> action)
        {
            var prev = AttackerPenetracion;
            try
            {
                AttackerPenetracion = Math.Clamp(pen, 0.0, 0.9);
                return action();
            }
            finally
            {
                AttackerPenetracion = prev;
            }
        }
    }
}
