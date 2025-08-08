using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Objetos
{
    public class Pocion : Objeto
    {
        public int CantidadCuracion { get; set; }

        public Pocion(string nombre, int cantidadCuracion) : base(nombre)
        {
            CantidadCuracion = cantidadCuracion;
        }

        public Pocion(string nombre) : base(nombre)
        {
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            personaje.Curar(CantidadCuracion);
            Console.WriteLine($"{personaje.Nombre} usó {Nombre} y recuperó {CantidadCuracion} de vida.");
        }

        // Removed duplicate Usar method with incorrect type reference
    }
}

