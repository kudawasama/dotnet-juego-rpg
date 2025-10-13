namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Reglas de botín (drops) para enemigos.
    /// </summary>
    public class DropRuleData
    {
        // Tipo: "Material" | "Arma" | "Pocion" | "Objeto" (genérico)
        public string Tipo { get; set; } = "Material";

        // Id o Nombre del ítem (de momento por nombre para compatibilidad con Gestores existentes)
        public string Nombre { get; set; } = string.Empty;

        // Rareza opcional (como texto) para ítems; se parsea a MiJuegoRPG.Objetos.Rareza
        public string? Rareza
        {
            get; set;
        }

        // Chance 0..1
        public double Chance { get; set; } = 0.05;

        // Cantidades
        public int CantidadMin { get; set; } = 1;

        public int CantidadMax { get; set; } = 1;

        // Si true, dropea como máximo una vez por partida (requiere soporte futuro en GuardadoService)
        public bool UniqueOnce { get; set; } = false;
    }
}
