using System;

namespace MiJuegoRPG.Objetos
{
    public class Cinturon : Objeto
    {
        public string TipoObjeto { get; set; } = "Cinturon";
        public int BonificacionCarga { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Cinturon(string nombre, int bonifCarga, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Cinturon", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            BonificacionCarga = CalcularBonificacion(bonifCarga, perfeccion);
            Perfeccion = perfeccion;
        }

        public Cinturon() : base("", Rareza.Normal, "Cinturon") { }

        private int CalcularBonificacion(int baseValor, int perfeccion)
        {
            return (int)(baseValor * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el cinturón {this.Nombre}.");
        }
    }
}
