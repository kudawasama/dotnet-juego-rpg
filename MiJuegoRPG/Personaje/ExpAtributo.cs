namespace MiJuegoRPG.Personaje
{
    /// <summary>
    /// Representa el progreso de experiencia de un atributo individual.
    /// Progreso: experiencia acumulada actual.
    /// Requerida: experiencia necesaria para subir el atributo (escala multiplicando al subir, p.ej. *1.2).
    /// </summary>
    public class ExpAtributo
    {
        public double Progreso { get; set; } = 0.0;
        public double Requerida { get; set; } = 1.0;
    }
}
