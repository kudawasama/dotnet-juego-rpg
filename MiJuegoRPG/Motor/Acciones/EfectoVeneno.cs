using System.Collections.Generic;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class EfectoVeneno : IEfecto
    {
        public string Nombre => "Veneno";
        public bool EsBenefico => false;
        public int TurnosRestantes { get; private set; }
        private readonly int _danioPorTurno;
        private readonly bool _magico;

        public EfectoVeneno(int danioPorTurno, int duracionTurnos, bool magico = true)
        {
            _danioPorTurno = danioPorTurno;
            TurnosRestantes = duracionTurnos;
            _magico = magico;
        }

        public IEnumerable<string> Tick(ICombatiente objetivo)
        {
            int vidaAntes = objetivo.Vida;
            if (_magico) objetivo.RecibirDanioMagico(_danioPorTurno);
            else objetivo.RecibirDanioFisico(_danioPorTurno);
            yield return $"{objetivo.Nombre} sufre {_danioPorTurno} de daño por Veneno ({vidaAntes} → {objetivo.Vida} HP).";
        }

        public bool AvanzarTurno()
        {
            if (TurnosRestantes > 0) TurnosRestantes--;
            return TurnosRestantes > 0;
        }
    }
}
