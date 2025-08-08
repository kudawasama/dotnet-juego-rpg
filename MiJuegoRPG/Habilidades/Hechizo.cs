
using MiJuegoRPG.Habilidades;
namespace MiJuegoRPG.Habilidades
{
    public class Hechizo : Habilidad
    {
        public int DanioMagico { get; set; }

        public Hechizo(int danioMagico) : base("Hechizo", 10)
        {
            DanioMagico = danioMagico;
        }

        public override void Usar()
        {
            Console.WriteLine($"Lanzas un hechizo y haces {DanioMagico} de daño mágico.");
        }
    }
}