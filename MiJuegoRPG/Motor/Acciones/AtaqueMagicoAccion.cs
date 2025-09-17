using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AtaqueMagicoAccion : IAccionCombate
    {
        public string Nombre => "Ataque Mágico";
        public int CostoMana => 5;
        public int CooldownTurnos => 1;

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            var resolver = new DamageResolver();
            var res = resolver.ResolverAtaqueMagico(ejecutor, objetivo);
            return res;
        }
    }
}
