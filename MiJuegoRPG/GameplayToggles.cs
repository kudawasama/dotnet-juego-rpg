// Bandera global para activar el chequeo de precisión (hit chance)
public static class GameplayToggles
{
    public static bool PrecisionCheckEnabled { get; set; } = false;

    // Toggle para activar la penetración en el pipeline (no intrusivo por defecto)
    public static bool PenetracionEnabled { get; set; } = false;

    // Toggle de verbosidad de combate (muestra detalle didáctico del cálculo)
    public static bool CombatVerbose { get; set; } = false;
}
