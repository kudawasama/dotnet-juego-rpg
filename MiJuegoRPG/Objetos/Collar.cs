using System;

namespace MiJuegoRPG.Objetos
{
    public class Collar : Objeto
    {
        public string TipoObjeto { get; set; } = "Collar";
        public int BonificacionDefensa { get; set; }
        public int BonificacionEnergia { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Collar(string nombre, int bonifDefensa, int bonifEnergia, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Collar", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            BonificacionDefensa = CalcularBonificacion(bonifDefensa, perfeccion);
            BonificacionEnergia = CalcularBonificacion(bonifEnergia, perfeccion);
            Perfeccion = perfeccion;
        }

        public Collar() : base("", Rareza.Normal, "Collar") { }

        private int CalcularBonificacion(int baseValor, int perfeccion)
        {
            return (int)(baseValor * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el collar {this.Nombre}.");
        }
    }
}
