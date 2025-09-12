namespace MiJuegoRPG.PjDatos
{
    public class EnemigoData
    {
        // Corrección: Asignamos un valor predeterminado para evitar el error.
        public string Nombre { get; set; } = string.Empty;

        public int VidaBase { get; set; }
        public int AtaqueBase { get; set; }
        public int DefensaBase { get; set; }
        public int DefensaMagicaBase { get; set; }
        public int Nivel { get; set; }
        public int ExperienciaRecompensa { get; set; }
        public int OroRecompensa { get; set; }

    public Familia Familia { get; set; }
    public Rareza Rareza { get; set; }
    public Categoria Categoria { get; set; }

    // Permite asignar el nombre de un arma desde el JSON
    public string ArmaNombre { get; set; } = string.Empty;
    }
}   