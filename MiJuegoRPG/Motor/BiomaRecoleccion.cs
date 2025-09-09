using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor
{
    public class NodoRecoleccion
    {
        public string? Nombre { get; set; }
        public List<(string Nombre, int Cantidad)>? Materiales { get; set; }
        /// <summary>Cooldown en segundos definido por datos (0 = sin cooldown).</summary>
        public int Cooldown { get; set; } // Compat: valor efectivo si se usa directamente
        /// <summary>Nuevo: cooldown base para escalados futuros (si Cooldown==0 se usará este).</summary>
        public int? CooldownBase { get; set; }
        public string? Tipo { get; set; }
        public string? Requiere { get; set; } // Herramienta u objeto requerido
        /// <summary>Nuevo: Rareza del nodo (Comun, Raro, Epico). Afectará a balance/drop más adelante.</summary>
        public string? Rareza { get; set; }
        /// <summary>Nuevo: Producción mínima de cada material (si se define rango dinámico).</summary>
        public int? ProduccionMin { get; set; }
        /// <summary>Nuevo: Producción máxima de cada material.</summary>
        public int? ProduccionMax { get; set; }
        [JsonIgnore]
        public DateTime? UltimoUso { get; set; } // Runtime: no serializar
        [JsonIgnore]
        public int UsosFallidosRecientes { get; set; } // métrica ligera
        /// <summary>Obtiene el cooldown efectivo (Cooldown directo o CooldownBase).</summary>
        public int CooldownEfectivo()
        {
            if (Cooldown > 0) return Cooldown;
            if (CooldownBase.HasValue && CooldownBase.Value > 0) return CooldownBase.Value;
            return 0;
        }
        public bool EstaEnCooldown()
        {
            var cd = CooldownEfectivo();
            if (cd <= 0 || UltimoUso == null) return false;
            return (DateTime.UtcNow - UltimoUso.Value).TotalSeconds < cd;
        }
        public int SegundosRestantesCooldown()
        {
            if (!EstaEnCooldown()) return 0;
            var cd = CooldownEfectivo();
            var restante = cd - (int)(DateTime.UtcNow - UltimoUso!.Value).TotalSeconds;
            return restante < 0 ? 0 : restante;
        }
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
