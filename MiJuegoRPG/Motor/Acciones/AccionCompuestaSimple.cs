using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    /// <summary>
    /// Acción que delega la ejecución en un callback pero expone Nombre/Costo/Cooldown configurables.
    /// Útil para mapear habilidades a acciones existentes ajustando recursos.
    /// </summary>
    public class AccionCompuestaSimple : IAccionCombate
    {
        private readonly Func<ICombatiente, ICombatiente, ResultadoAccion> _ejecutar;
        private readonly string _nombre;
        private readonly int _costo;
        private readonly int _cooldown;

        public AccionCompuestaSimple(string nombre, int costoMana, int cooldown, Func<ICombatiente, ICombatiente, ResultadoAccion> ejecutar)
        {
            _nombre = nombre;
            _costo = costoMana;
            _cooldown = cooldown;
            _ejecutar = ejecutar;
        }

        public string Nombre => _nombre;
        public int CostoMana => _costo;
        public int CooldownTurnos => _cooldown;

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo) => _ejecutar(ejecutor, objetivo);
    }
}
