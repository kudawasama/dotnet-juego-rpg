using System;

namespace MiJuegoRPG.Objetos
{
    public class Botas : Objeto
    {
        public string TipoObjeto { get; set; } = "Botas";
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Botas(string nombre, int defensa, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Botas", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensa, perfeccion);
            Perfeccion = perfeccion;
        }

        public Botas() : base("", Rareza.Normal, "Botas") { }

        private int CalcularDefensa(int defensaBase, int perfeccion)
        {
            return (int)(defensaBase * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa las botas {this.Nombre}.");
        }
    }
}
