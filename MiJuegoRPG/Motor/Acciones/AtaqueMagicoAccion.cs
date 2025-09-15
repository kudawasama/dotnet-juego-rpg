using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AtaqueMagicoAccion : IAccionCombate
    {
        public string Nombre => "Ataque Mágico";
        public int CostoMana => 5;
        public int CooldownTurnos => 1;

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            int danio = ejecutor.AtacarMagico(objetivo);
            var res = new ResultadoAccion
            {
                NombreAccion = Nombre,
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danio,
                EsMagico = true,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };
            res.Mensajes.Add($"{ejecutor.Nombre} lanza {Nombre} sobre {objetivo.Nombre} y causa {danio} de daño mágico.");
            return res;
        }
    }
}
