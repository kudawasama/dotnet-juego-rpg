using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AtaqueFisicoAccion : IAccionCombate
    {
        public string Nombre => "Ataque Físico";

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            var resolver = new MiJuegoRPG.Motor.Servicios.DamageResolver();
            var res = resolver.ResolverAtaqueFisico(ejecutor, objetivo);
            return res;
        }
    }
}
