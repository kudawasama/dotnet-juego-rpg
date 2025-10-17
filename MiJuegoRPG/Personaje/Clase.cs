using System;

namespace MiJuegoRPG.Personaje
{
    public class Clase
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public AtributosBase Atributos { get; set; } = new AtributosBase();
        public Estadisticas Estadisticas
        {
            get; set;
        }

        public Clase(string nombre, AtributosBase atributos, Estadisticas estadisticas)
        {
            Nombre = nombre;
            Atributos = atributos;
            Estadisticas = estadisticas;
        }

        public Clase()
        {
            Estadisticas = new Estadisticas(new AtributosBase());
        }
        // Puedes agregar más propiedades según tu sistema de clases
    }
}
