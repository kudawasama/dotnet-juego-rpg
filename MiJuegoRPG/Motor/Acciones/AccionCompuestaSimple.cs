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
        private readonly Func<ICombatiente, ICombatiente, ResultadoAccion> ejecutar;
        private readonly string nombre;
        private readonly int costo;
        private readonly int cooldown;

        public AccionCompuestaSimple(string nombre, int costoMana, int cooldown, Func<ICombatiente, ICombatiente, ResultadoAccion> ejecutar)
        {
            this.nombre = nombre;
            costo = costoMana;
            this.cooldown = cooldown;
            this.ejecutar = ejecutar;
        }

        public string Nombre => nombre;
        public int CostoMana => costo;
        public int CooldownTurnos => cooldown;

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo) => ejecutar(ejecutor, objetivo);
    }
}
