using System;

namespace MiJuegoRPG.Objetos
{
    public class Armadura : Objeto
    {
        public string TipoObjeto { get; set; } = "Armadura";
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Armadura(string nombre, int defensa, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Armadura", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensa, perfeccion);
            Perfeccion = perfeccion;
        }

        public Armadura() : base("", Rareza.Normal, "Armadura") { }

        private int CalcularDefensa(int defensaBase, int perfeccion)
        {
            // Defensa ajustada por perfección (ejemplo: defensaBase * (perfeccion / 50.0))
            return (int)(defensaBase * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            // Implementación de ejemplo: equipa la armadura
            Console.WriteLine($"{personaje.Nombre} equipa la armadura {this.Nombre}.");
        }
    }
}
