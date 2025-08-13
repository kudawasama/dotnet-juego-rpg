namespace MiJuegoRPG.PjDatos
{
    public class BotasData
    {
        public required string Nombre { get; set; }
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public required string TipoObjeto { get; set; }
        public string Rareza { get; set; } = "Comun";
        public int Perfeccion { get; set; } = 50;
    }
}
