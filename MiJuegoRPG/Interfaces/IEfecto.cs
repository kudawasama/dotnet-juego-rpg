using System.Collections.Generic;

namespace MiJuegoRPG.Interfaces
{
    public interface IEfecto
    {
        string Nombre
        {
            get;
        }
        bool EsBenefico
        {
            get;
        }
        int TurnosRestantes
        {
            get;
        }
        // Aplica el tick del efecto al objetivo y devuelve mensajes para UI
        IEnumerable<string> Tick(ICombatiente objetivo);
        // Reduce duración en 1 turno; retorna true si sigue activo
        bool AvanzarTurno();
    }
}
