//crear enemigo nombre Jefe Goblin desde la clase padre enemigo con opcion de aumentar sus estadisticas
using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Enemigos
{
    public class GranGoblin : Enemigo
    {
        public GranGoblin() : base("Gran Goblin", 80, 15, 8, 8, 2, 50, 25)
            {
                // Vida: 80, Ataque: 15, Defensa: 8, DefensaMágica: 8, Nivel: 2, Experiencia: 50, Oro: 25
            }

        public override int AtacarFisico(ICombatiente objetivo)
        {
            Console.WriteLine($"¡El {Nombre} ruge ferozmente y lanza un ataque devastador!");
            objetivo.RecibirDanioFisico(Ataque);
            return Ataque;
        }
    }
}