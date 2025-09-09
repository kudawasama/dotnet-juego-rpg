using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios
{
    // Evento base simple
    public interface IEventoJuego { } // Marcador común para todos los eventos del juego

    // Eventos dominio (nombres en español)
    public class EventoAtributoSubido : IEventoJuego // Disparado al subir un atributo base
    {
        public string Atributo { get; }
        public double NuevoValor { get; }
        public EventoAtributoSubido(string atributo, double nuevoValor)
        { Atributo = atributo; NuevoValor = nuevoValor; }
    }

    public class BusEventos // Bus simple en memoria (no persistente) para pub/sub local
    {
        private readonly Dictionary<Type, List<Delegate>> _suscriptores = new();
        private static BusEventos? _instancia;
        public static BusEventos Instancia => _instancia ??= new BusEventos();

    public void Suscribir<T>(Action<T> handler) where T : IEventoJuego // Registra un manejador para tipo de evento
        {
            var t = typeof(T);
            if (!_suscriptores.TryGetValue(t, out var lista))
            {
                lista = new List<Delegate>();
                _suscriptores[t] = lista;
            }
            lista.Add(handler);
        }

    public void Publicar<T>(T ev) where T : IEventoJuego // Invoca todos los handlers del tipo
        {
            var t = typeof(T);
            if (_suscriptores.TryGetValue(t, out var lista))
            {
                foreach (var d in lista)
                {
                    try { ((Action<T>)d)(ev); } catch { /* swallow */ }
                }
            }
        }
    }

    // Eventos adicionales
    public class EventoNivelSubido : IEventoJuego // Subida de nivel de personaje
    {
        public int Nivel { get; }
        public EventoNivelSubido(int nivel) { Nivel = nivel; }
    }
    public class EventoMisionCompletada : IEventoJuego // Misión completada (Id + Nombre)
    {
        public string Id { get; }
        public string Nombre { get; }
        public EventoMisionCompletada(string id, string nombre) { Id = id; Nombre = nombre; }
    }
}