namespace MiJuegoRPG.PjDatos
{
    // Clase para almacenar los datos de un arma.
    public class ArmaData
    {
        public required string Nombre { get; set; }
        public int Daño { get; set; }
        public int NivelRequerido { get; set; }
        public int Valor { get; set; }
        public required string Tipo { get; set; } // Ejemplo: "Espada", "Arco", "Bastón"
        public string Rareza { get; set; } = "Comun";
        public int Perfeccion { get; set; } = 50;
    }
}
        