using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AtaqueFisicoAccion : IAccionCombate
    {
        public string Nombre => "Ataque Físico";

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            // Usamos el cálculo existente del ejecutor para no duplicar fórmulas
            int danio = ejecutor.AtacarFisico(objetivo);
            var res = new ResultadoAccion
            {
                NombreAccion = Nombre,
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio, // AtacarFisico ya aplica reducción; aquí usamos el mismo valor como base
                DanioReal = danio,
                EsMagico = false,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };
            res.Mensajes.Add($"{ejecutor.Nombre} usa {Nombre} sobre {objetivo.Nombre} y causa {danio} de daño.");
            return res;
        }
    }
}
