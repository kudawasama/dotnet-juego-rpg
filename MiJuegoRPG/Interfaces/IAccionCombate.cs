namespace MiJuegoRPG.Interfaces
{
    public interface IAccionCombate
    {
        string Nombre
        {
            get;
        }
        // Ejecuta la acción y retorna un resultado estandarizado para impresión/UI y lógica
        ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo);
        // Requisitos mínimos (mana/recursos) — por ahora no usados, reservados para 17.3
        int CostoMana => 0;
        // Cooldown en turnos para volver a usar la acción (0 = sin cooldown)
        int CooldownTurnos => 0;
        // Coste en Puntos de Acción (1 por defecto para mantener compatibilidad)
        int CostoPA => 1;
    }
}
