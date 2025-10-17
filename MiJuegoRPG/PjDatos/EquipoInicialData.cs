namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Configuración de equipo inicial para enemigos.
    /// </summary>
    public class EquipoInicialData
    {
        public string? Arma
        {
            get; set;
        }

        // Reservado a futuro: Armadura/Casco/Botas/etc por nombre
        public string? Armadura
        {
            get; set;
        }

        public string? Casco
        {
            get; set;
        }

        public string? Botas
        {
            get; set;
        }

        public string? Accesorio
        {
            get; set;
        }
    }
}
