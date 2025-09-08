using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class NodoRecoleccion
    {
    public string? Nombre { get; set; }
    public List<(string Nombre, int Cantidad)>? Materiales { get; set; }
    public int Cooldown { get; set; }
    public string? Tipo { get; set; }
    public string? Requiere { get; set; } // Herramienta u objeto requerido
    }

    public class BiomaRecoleccion
    {
        public string? TipoBioma { get; set; }
        public List<NodoRecoleccion>? NodosComunes { get; set; }
        public List<NodoRecoleccion>? NodosRaros { get; set; }
    }

    public static class TablaBiomas
    {
        public static Dictionary<string, BiomaRecoleccion> Biomas = new();
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

        public static List<NodoRecoleccion> GenerarNodosParaBioma(string tipoBioma, Random? rng = null)
        {
            // Si no se pasa RNG, usar el servicio centralizado
            rng ??= new Random((int)DateTime.UtcNow.Ticks & 0x0000FFFF); // fallback mínimo
            var randomSvc = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
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
