namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Representa una cantidad específica de un material para recolección.
    /// </summary>
    public class MaterialCantidad
    {
        /// <summary>
        /// Gets or sets el nombre del material.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets la cantidad del material.
        /// </summary>
        public int Cantidad
        {
            get; set;
        }
    }
}