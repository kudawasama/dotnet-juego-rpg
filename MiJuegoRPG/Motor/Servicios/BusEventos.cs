using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Bus simple en memoria (no persistente) para pub/sub local.
    /// </summary>
    public class BusEventos
    {
        private static BusEventos? instancia;
        private readonly Dictionary<Type, List<Delegate>> suscriptores = new();

        /// <summary>
        /// Gets instancia singleton del bus de eventos.
        /// </summary>
        public static BusEventos Instancia => instancia ??= new BusEventos();

        /// <summary>
        /// Registra un manejador para tipo de evento.
        /// </summary>
        /// <typeparam name="T">Tipo de evento que implementa IEventoJuego.</typeparam>
        /// <param name="handler">Acción a ejecutar cuando se publique el evento.</param>
        public void Suscribir<T>(Action<T> handler)
            where T : IEventoJuego
        {
            var t = typeof(T);
            if (!suscriptores.TryGetValue(t, out var lista))
            {
                lista = new List<Delegate>();
                suscriptores[t] = lista;
            }
            lista.Add(handler);
        }

        /// <summary>
        /// Invoca todos los handlers del tipo.
        /// </summary>
        /// <typeparam name="T">Tipo de evento que implementa IEventoJuego.</typeparam>
        /// <param name="ev">Instancia del evento a publicar.</param>
        public void Publicar<T>(T ev)
            where T : IEventoJuego
        {
            var t = typeof(T);
            if (suscriptores.TryGetValue(t, out var lista))
            {
                foreach (var d in lista)
                {
                    try
                    {
                        ((Action<T>)d)(ev);
                    }
                    catch { /* swallow */ }
                }
            }
        }
    }
}
