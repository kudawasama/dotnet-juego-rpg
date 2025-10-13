namespace MiJuegoRPG.Habilidades
{
    public class CondicionData
    {
        public string Tipo { get; set; } = string.Empty; // Nivel | Mision | NvHabilidad | NvJugador | Ataque | etc.
        public string Accion { get; set; } = string.Empty;
        public int? Cantidad { get; set; } = null;
        public string Restriccion { get; set; } = string.Empty;
    }
}