namespace MiJuegoRPG.Objetos
{
    /// <summary>
    /// Referencia ligera a una habilidad otorgada por un equipo.
    /// </summary>
    public class HabilidadOtorgadaRef
    {
        public string Id { get; set; } = string.Empty;
        public int NivelMinimo { get; set; } = 1;
    }
}