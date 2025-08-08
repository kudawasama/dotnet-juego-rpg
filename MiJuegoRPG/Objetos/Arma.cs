using MiJuegoRPG.Objetos;

namespace Objetos
{
    public class Arma : Objeto
    {
        public int Daño { get; set; }

        public Arma(string nombre, int daño) : base(nombre)
        {
            Daño = daño;
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el arma {Nombre}.");
        }
    }
}

