using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Servicio ligero para gestionar reputación global y por facción.
    /// Extensible: efectos por umbral, eventos, facciones múltiples.
    /// </summary>
    public class ReputacionService
    {
        private readonly Juego juego;
        public bool Verbose { get; set; } = true;
        public ReputacionService(Juego juego)
        {
            this.juego = juego;
        }

        /// <summary>
        /// Modifica la reputación global del jugador y dispara reevaluación de clases.
        /// </summary>
        public void ModificarReputacion(int delta)
        {
            if (juego.jugador == null) return;
            juego.jugador.Reputacion += delta;
            if (Verbose) Console.WriteLine($"[Reputación] Cambio global: {(delta >= 0 ? "+" : "")}{delta} => {juego.jugador.Reputacion}");
            // Reevaluar clases por posibles desbloqueos dependientes de reputación
            try { juego.claseService.Evaluar(juego.jugador); } catch { }
        }

        /// <summary>
        /// Ajusta reputación de una facción y opcionalmente reputación global.
        /// </summary>
        public void ModificarReputacionFaccion(string faccion, int delta, bool afectarGlobal = false, double factorGlobal = 0.25)
        {
            if (juego.jugador == null || string.IsNullOrWhiteSpace(faccion)) return;
            var pj = juego.jugador;
            pj.ReputacionesFaccion.TryGetValue(faccion, out var actual);
            pj.ReputacionesFaccion[faccion] = actual + delta;
            if (Verbose) Console.WriteLine($"[Reputación] Facción '{faccion}': {(delta >= 0 ? "+" : "")}{delta} => {pj.ReputacionesFaccion[faccion]}");
            if (afectarGlobal && delta != 0)
            {
                int mod = (int)Math.Round(delta * factorGlobal);
                if (mod != 0) ModificarReputacion(mod);
            }
            else
            {
                // Reevaluar clases igualmente (por si en el futuro hay ReputacionMinima específica por facción)
                try { juego.claseService.Evaluar(pj); } catch { }
            }
        }

        /// <summary>
        /// Devuelve reputación por facción (0 si no existe).
        /// </summary>
        public int ObtenerReputacionFaccion(string faccion)
        {
            if (juego.jugador == null) return 0;
            juego.jugador.ReputacionesFaccion.TryGetValue(faccion, out var v);
            return v;
        }
    }
}
