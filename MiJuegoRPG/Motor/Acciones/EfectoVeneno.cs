using System.Collections.Generic;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class EfectoVeneno : IEfecto
    {
        public string Nombre => "Veneno";
        public bool EsBenefico => false;
        public int TurnosRestantes
        {
            get; private set;
        }
        private readonly int danioPorTurno;
        private readonly bool magico;

        public EfectoVeneno(int danioPorTurno, int duracionTurnos, bool magico = true)
        {
            this.danioPorTurno = danioPorTurno;
            TurnosRestantes = duracionTurnos;
            this.magico = magico;
        }

        public IEnumerable<string> Tick(ICombatiente objetivo)
        {
            int vidaAntes = objetivo.Vida;
            if (magico)
                objetivo.RecibirDanioMagico(danioPorTurno);
            else
                objetivo.RecibirDanioFisico(danioPorTurno);
            yield return $"{objetivo.Nombre} sufre {danioPorTurno} de daño por Veneno ({vidaAntes} → {objetivo.Vida} HP).";
        }

        public bool AvanzarTurno()
        {
            if (TurnosRestantes > 0)
                TurnosRestantes--;
            return TurnosRestantes > 0;
        }
    }
}
