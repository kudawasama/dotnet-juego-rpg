using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Objetos
{
    public class Material : Objeto
    {
        public Material(string nombre, Rareza rareza = Rareza.Rota, string categoria = "Material") : base(nombre, rareza, categoria)
        {
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} ha obtenido el material {Nombre}.");
        }
    }
}
