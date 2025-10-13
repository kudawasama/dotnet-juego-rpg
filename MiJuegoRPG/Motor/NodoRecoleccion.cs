using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Representa un nodo de recolección con materiales, cooldowns y configuraciones.
    /// </summary>
    public class NodoRecoleccion
    {
        /// <summary>
        /// Gets or sets el nombre del nodo de recolección.
        /// </summary>
        public string? Nombre
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets la lista de materiales disponibles en este nodo.
        /// </summary>
        public List<MaterialCantidad>? Materiales
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets cooldown en segundos definido por datos (0 = sin cooldown).
        /// </summary>
        public int Cooldown
        {
            get; set;
        } // Compat: valor efectivo si se usa directamente

        /// <summary>
        /// Gets or sets nuevo: cooldown base para escalados futuros (si Cooldown==0 se usará este).
        /// </summary>
        public int? CooldownBase
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets el tipo de nodo de recolección.
        /// </summary>
        public string? Tipo
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets herramienta u objeto requerido para usar este nodo.
        /// </summary>
        public string? Requiere
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets nuevo: Rareza del nodo (Comun, Raro, Epico). Afectará a balance/drop más adelante.
        /// </summary>
        public string? Rareza
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets nuevo: Producción mínima de cada material (si se define rango dinámico).
        /// </summary>
        public int? ProduccionMin
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets nuevo: Producción máxima de cada material.
        /// </summary>
        public int? ProduccionMax
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets runtime: no serializar.
        /// </summary>
        [JsonIgnore]
        public DateTime? UltimoUso
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets métrica ligera de usos fallidos recientes.
        /// </summary>
        [JsonIgnore]
        public int UsosFallidosRecientes
        {
            get; set;
        }

        /// <summary>
        /// Obtiene el cooldown efectivo (Cooldown directo o CooldownBase).
        /// </summary>
        /// <returns>El cooldown efectivo en segundos.</returns>
        public int CooldownEfectivo()
        {
            if (Cooldown > 0)
                return Cooldown;
            if (CooldownBase.HasValue && CooldownBase.Value > 0)
                return CooldownBase.Value;
            return 0;
        }

        /// <summary>
        /// Verifica si el nodo está actualmente en cooldown.
        /// </summary>
        /// <returns>True si está en cooldown, false en caso contrario.</returns>
        public bool EstaEnCooldown()
        {
            var cd = CooldownEfectivo();
            if (cd <= 0 || UltimoUso == null)
                return false;
            return (DateTime.UtcNow - UltimoUso.Value).TotalSeconds < cd;
        }

        /// <summary>
        /// Obtiene los segundos restantes de cooldown.
        /// </summary>
        /// <returns>Segundos restantes, 0 si no está en cooldown.</returns>
        public int SegundosRestantesCooldown()
        {
            if (!EstaEnCooldown())
                return 0;
            var cd = CooldownEfectivo();
            var restante = cd - (int)(DateTime.UtcNow - UltimoUso!.Value).TotalSeconds;
            return restante < 0 ? 0 : restante;
        }
    }
}