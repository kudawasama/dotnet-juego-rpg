using System;

namespace MiJuegoRPG.Objetos
{
    public class Accesorio : Objeto
    {
        public string TipoObjeto { get; set; } = "Accesorio";
        public int BonificacionAtaque { get; set; }
        public int BonificacionDefensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Accesorio(string nombre, int bonifAtaque, int bonifDefensa, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Accesorio", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            BonificacionAtaque = CalcularBonificacion(bonifAtaque, perfeccion);
            BonificacionDefensa = CalcularBonificacion(bonifDefensa, perfeccion);
            Perfeccion = perfeccion;
        }

        public Accesorio() : base("", Rareza.Normal, "Accesorio") { }

        private int CalcularBonificacion(int baseValor, int perfeccion)
        {
            return (int)(baseValor * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            // Implementación de ejemplo: equipa el accesorio
            Console.WriteLine($"{personaje.Nombre} equipa el accesorio {this.Nombre}.");
        }
    }
}
