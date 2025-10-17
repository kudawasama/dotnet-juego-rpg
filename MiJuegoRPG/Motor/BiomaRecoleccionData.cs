using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Representa un bioma con sus nodos de recolección comunes y raros.
    /// </summary>
    public class BiomaRecoleccion
    {
        /// <summary>
        /// Gets or sets el tipo de bioma.
        /// </summary>
        public string? TipoBioma
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets la lista de nodos comunes del bioma.
        /// </summary>
        public List<NodoRecoleccion>? NodosComunes
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets la lista de nodos raros del bioma.
        /// </summary>
        public List<NodoRecoleccion>? NodosRaros
        {
            get; set;
        }
    }
}