
namespace MiJuegoRPG.Habilidades
{
    public class AtaqueFisico : Habilidad
    {
        public int DanioFisico { get; set; }

        public AtaqueFisico(int danioFisico) : base("Ataque Físico", 5)
        {
            DanioFisico = danioFisico;
        }

        public override void Usar()
        {
            Console.WriteLine($"Realizas un ataque físico y haces {DanioFisico} de daño.");
        }
    }
}
