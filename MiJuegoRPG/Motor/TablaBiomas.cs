using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Tabla estática para manejo de biomas de recolección.
    /// </summary>
    public static class TablaBiomas
    {
        /// <summary>
        /// Gets diccionario de biomas indexado por tipo de bioma.
        /// </summary>
        public static Dictionary<string, BiomaRecoleccion> Biomas { get; } = new();

        /// <summary>
        /// Carga biomas desde un archivo JSON.
        /// </summary>
        /// <param name="ruta">Ruta del archivo JSON.</param>
        public static void CargarDesdeJson(string ruta)
        {
            var json = System.IO.File.ReadAllText(ruta);
            var lista = System.Text.Json.JsonSerializer.Deserialize<List<BiomaRecoleccion>>(json);
            Biomas.Clear();
            if (lista != null)
            {
                foreach (var bioma in lista)
                {
                    if (bioma.TipoBioma != null)
                        Biomas[bioma.TipoBioma] = bioma;
                }
            }
        }

        /// <summary>
        /// Genera nodos de recolección para un bioma específico.
        /// </summary>
        /// <param name="tipoBioma">Tipo de bioma.</param>
        /// <param name="rng">Random obsoleto, mantenido por compatibilidad.</param>
        /// <returns>Lista de nodos de recolección.</returns>
        public static List<NodoRecoleccion> GenerarNodosParaBioma(string tipoBioma, Random? rng = null)
        {
            // Usar el servicio centralizado (el parámetro rng se mantiene por compatibilidad, no se usa)
            var randomSvc = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            if (string.IsNullOrWhiteSpace(tipoBioma))
                return new List<NodoRecoleccion>();
            if (!Biomas.TryGetValue(tipoBioma, out var bioma))
                return new List<NodoRecoleccion>();
            var nodos = new List<NodoRecoleccion>();
            nodos.AddRange(bioma.NodosComunes ?? new List<NodoRecoleccion>());
            // Probabilidad de agregar un nodo raro
            foreach (var nodoRaro in bioma.NodosRaros ?? new List<NodoRecoleccion>())
            {
                if (randomSvc.NextDouble() < 0.3) // 30% de probabilidad
                    nodos.Add(nodoRaro);
            }
            return nodos;
        }
    }
}