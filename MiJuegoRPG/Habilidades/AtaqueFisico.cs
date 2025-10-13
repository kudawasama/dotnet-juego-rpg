namespace MiJuegoRPG.Habilidades
{
    public class AtaqueFisico : Habilidad
    {
        public int DanioFisico
        {
            get; set;
        }
        public int CostoMana
        {
            get; set;
        }

        public AtaqueFisico(int danioFisico)
            : base("Ataque Físico", 5)
        {
            DanioFisico = danioFisico;
            CostoMana = 5; // Puedes ajustar el costo por defecto
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje usuario)
        {
            if (usuario.GastarMana(CostoMana))
            {
                Console.WriteLine($"{usuario.Nombre} realiza un ataque físico y hace {DanioFisico} de daño.");
            }
            else
            {
                Console.WriteLine($"{usuario.Nombre} no tiene suficiente maná para realizar el ataque físico.");
            }
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje usuario, MiJuegoRPG.Interfaces.ICombatiente objetivo)
        {
            if (usuario.GastarMana(CostoMana))
            {
                objetivo.RecibirDanioFisico(DanioFisico);
                Console.WriteLine($"{usuario.Nombre} ataca a {objetivo.Nombre} y le hace {DanioFisico} de daño físico.");
            }
            else
            {
                Console.WriteLine($"{usuario.Nombre} no tiene suficiente maná para realizar el ataque físico.");
            }
        }
    }
}
