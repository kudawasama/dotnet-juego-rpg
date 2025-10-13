using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Objetos
{
    public class Pocion : Objeto
    {
        public string TipoObjeto { get; set; } = "Pocion";
        public int Curacion
        {
            get; set;
        }

        public Pocion(string nombre, int curacion, string rareza = "Normal", string categoria = "Consumible")
            : base(nombre, rareza, categoria)
        {
            Curacion = curacion;
        }

        public Pocion()
            : base("", "Normal", "Consumible") { }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            personaje.Vida += Curacion;
            Console.WriteLine($"{personaje.Nombre} ha usado {Nombre} y ha recuperado {Curacion} puntos de vida.");
        }
    }
}

