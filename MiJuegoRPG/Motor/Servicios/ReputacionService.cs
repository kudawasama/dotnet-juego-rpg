using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.Motor.Servicios;

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
        // Configuración de umbrales (12.3)
        private List<int> bandasGlobal = new(); // valores de corte ordenados ascendentemente
        private Dictionary<string, List<int>> bandasPorFaccion = new();
        private Dictionary<int, string> mensajesGlobal = new();
        private Dictionary<string, Dictionary<int, string>> mensajesPorFaccion = new();
        public ReputacionService(Juego juego)
        {
            this.juego = juego;
            CargarConfigUmbrales();
        }

        /// <summary>
        /// Modifica la reputación global del jugador y dispara reevaluación de clases.
        /// </summary>
        public void ModificarReputacion(int delta)
        {
            if (juego.jugador == null) return;
            int anterior = juego.jugador.Reputacion;
            int nuevo = anterior + delta;
            juego.jugador.Reputacion = nuevo;
            if (Verbose) Logger.Info($"[Reputación] Cambio global: {(delta >= 0 ? "+" : "")}{delta} => {nuevo}");
            DetectarCruceGlobal(anterior, nuevo);
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
            pj.ReputacionesFaccion.TryGetValue(faccion, out var anterior);
            int nuevo = anterior + delta;
            pj.ReputacionesFaccion[faccion] = nuevo;
            if (Verbose) Logger.Info($"[Reputación] Facción '{faccion}': {(delta >= 0 ? "+" : "")}{delta} => {nuevo}");
            DetectarCruceFaccion(faccion, anterior, nuevo);
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

        // 12.3: Umbrales
        private record ConfigBandas(List<int>? Global, Dictionary<string, List<int>>? Facciones, Dictionary<string, string>? MensajesGlobal, Dictionary<string, Dictionary<string, string>>? MensajesPorFaccion);
        private void CargarConfigUmbrales()
        {
            try
            {
                string ruta = PathProvider.CombineData("reputacion_umbrales.json");
                if (!File.Exists(ruta)) return; // opcional
                var json = File.ReadAllText(ruta);
                var cfg = JsonSerializer.Deserialize<ConfigBandas>(json);
                if (cfg == null) return;
                bandasGlobal = (cfg.Global ?? new List<int>()).Distinct().OrderBy(x => x).ToList();
                mensajesGlobal.Clear();
                if (cfg.MensajesGlobal != null)
                {
                    foreach (var kv in cfg.MensajesGlobal)
                        if (int.TryParse(kv.Key, out int clave)) mensajesGlobal[clave] = kv.Value;
                }
                bandasPorFaccion.Clear(); mensajesPorFaccion.Clear();
                if (cfg.Facciones != null)
                {
                    foreach (var kv in cfg.Facciones)
                        bandasPorFaccion[kv.Key] = kv.Value.Distinct().OrderBy(x => x).ToList();
                }
                if (cfg.MensajesPorFaccion != null)
                {
                    foreach (var fac in cfg.MensajesPorFaccion)
                    {
                        var inner = new Dictionary<int, string>();
                        foreach (var kv in fac.Value)
                            if (int.TryParse(kv.Key, out int clave)) inner[clave] = kv.Value;
                        mensajesPorFaccion[fac.Key] = inner;
                    }
                }
                if (Verbose) Logger.Info($"[Reputación] Umbrales cargados: Global({bandasGlobal.Count}) Facciones({bandasPorFaccion.Count})");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Reputación] Error cargando umbrales: {ex.Message}");
            }
        }

        private static int? BandaActual(List<int> bandas, int valor)
        {
            if (bandas == null || bandas.Count == 0) return null;
            int idx = 0;
            while (idx < bandas.Count && valor >= bandas[idx]) idx++;
            return idx; // índice de banda entre 0..bandas.Count
        }

        private void DetectarCruceGlobal(int anterior, int nuevo)
        {
            if (bandasGlobal.Count == 0) return;
            var bPrev = BandaActual(bandasGlobal, anterior);
            var bNew = BandaActual(bandasGlobal, nuevo);
            if (bPrev != bNew)
            {
                int bandaKey = ObtenerClaveBanda(bandasGlobal, nuevo);
                mensajesGlobal.TryGetValue(bandaKey, out var msg);
                var ev = new EventoReputacionUmbralGlobal(bandaKey.ToString(), anterior, nuevo, (bNew ?? 0) > (bPrev ?? 0), msg ?? "");
                try { BusEventos.Instancia.Publicar(ev); } catch { }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    try { MiJuegoRPG.Motor.AvisosAventura.MostrarAviso("Reputación", "Nuevo estado", msg); } catch { Console.WriteLine($"[Reputación] {msg}"); }
                }
            }
        }

        private void DetectarCruceFaccion(string faccion, int anterior, int nuevo)
        {
            if (!bandasPorFaccion.TryGetValue(faccion, out var bandas) || bandas.Count == 0) return;
            var bPrev = BandaActual(bandas, anterior);
            var bNew = BandaActual(bandas, nuevo);
            if (bPrev != bNew)
            {
                int bandaKey = ObtenerClaveBanda(bandas, nuevo);
                mensajesPorFaccion.TryGetValue(faccion, out var dictMsg);
                string? msg = null;
                if (dictMsg != null && dictMsg.TryGetValue(bandaKey, out var val)) msg = val;
                var ev = new EventoReputacionUmbralFaccion(faccion, bandaKey.ToString(), anterior, nuevo, (bNew ?? 0) > (bPrev ?? 0), msg ?? "");
                try { BusEventos.Instancia.Publicar(ev); } catch { }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    try { MiJuegoRPG.Motor.AvisosAventura.MostrarAviso("Reputación Facción", faccion, msg); } catch { Console.WriteLine($"[Reputación|{faccion}] {msg}"); }
                }
            }
        }

        private static int ObtenerClaveBanda(List<int> bandas, int valor)
        {
            // Devuelve el umbral más alto <= valor, o el más bajo si por debajo de todos.
            int clave = bandas[0];
            foreach (var b in bandas)
            {
                if (valor >= b) clave = b; else break;
            }
            return clave;
        }
    }
}
