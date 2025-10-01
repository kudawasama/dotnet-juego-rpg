using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Objetos
{
    public class Material : Objeto
    {
        // Constructor sin parámetros requerido para deserialización System.Text.Json
        public Material() : base(string.Empty, "Comun", "Material") { }

        public Material(string nombre, string rareza = "Rota", string categoria = "Material") : base(nombre, rareza, categoria)
        {
            if (string.IsNullOrWhiteSpace(Rareza)) Rareza = "Comun";
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} ha obtenido el material {Nombre}.");
        }
    }
}
