namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Condición para evolución de habilidad.
    /// Tipo ejemplos: "NvHabilidad", "NvJugador", "Ataque" (u otros contadores).
    /// </summary>
    public class CondicionEvolucion
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad
        {
            get; set;
        }
    }
}