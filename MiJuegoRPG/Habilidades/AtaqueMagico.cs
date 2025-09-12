using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Habilidades
{
    public class AtaqueMagico : Habilidad
    {
        public int DanioMagico { get; set; }
        public int CostoMana { get; set; }

        public AtaqueMagico(int danioMagico, int costoMana) : base("Ataque Mágico", costoMana)
        {
            DanioMagico = danioMagico;
            CostoMana = costoMana;
        }

        public override void Usar(Personaje.Personaje usuario)
        {
            if (usuario.GastarMana(CostoMana))
            {
                Console.WriteLine($"{usuario.Nombre} lanza un ataque mágico y hace {DanioMagico} de daño mágico.");
            }
            else
            {
                Console.WriteLine($"{usuario.Nombre} no tiene suficiente maná para lanzar el ataque mágico.");
            }
        }

        public override void Usar(Personaje.Personaje usuario, ICombatiente objetivo)
        {
            if (usuario.GastarMana(CostoMana))
            {
                objetivo.RecibirDanioMagico(DanioMagico);
                Console.WriteLine($"{usuario.Nombre} lanza un ataque mágico a {objetivo.Nombre} y le hace {DanioMagico} de daño mágico.");
            }
            else
            {
                Console.WriteLine($"{usuario.Nombre} no tiene suficiente maná para lanzar el ataque mágico.");
            }
        }
    }
}
