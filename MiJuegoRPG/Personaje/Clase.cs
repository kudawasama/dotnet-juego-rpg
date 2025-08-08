namespace MiJuegoRPG.Personaje
{
    public class Clase
    {
        public string Nombre { get; set; }
        public AtributosBase Atributos { get; set; }
        public Estadisticas Estadisticas { get; set; }

        public Clase(string nombre, AtributosBase atributos, Estadisticas estadisticas)
        {
            Nombre = nombre;
            Atributos = atributos;
            Estadisticas = estadisticas;
        }
    }
}
