namespace MiJuegoRPG.PjDatos
{
    public class CollarData
    {
        public required string Nombre { get; set; }
        public int BonificacionDefensa { get; set; }
        public int BonificacionEnergia { get; set; }
        public int Nivel { get; set; }
        public required string TipoObjeto { get; set; }
        public string Rareza { get; set; } = "Comun";
        public int Perfeccion { get; set; } = 50;
    }
}
