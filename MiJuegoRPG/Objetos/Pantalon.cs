using System;

namespace MiJuegoRPG.Objetos
{
    public class Pantalon : Objeto
    {
        public string TipoObjeto { get; set; } = "Pantalon";
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Pantalon(string nombre, int defensa, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Pantalon", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensa, perfeccion);
            Perfeccion = perfeccion;
        }

        public Pantalon() : base("", Rareza.Normal, "Pantalon") { }

        private int CalcularDefensa(int defensaBase, int perfeccion)
        {
            return (int)(defensaBase * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el pantalón {this.Nombre}.");
        }
    }
}
