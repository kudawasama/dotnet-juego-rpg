namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Modelo auxiliar para efectos aplicados por objetos (armas, armaduras, etc.)
    /// </summary>
    public class EfectoData
    {
        public string? Tipo
        {
            get; set;
        } // OnHit | Aura | Uso
        public string? Nombre
        {
            get; set;
        }
        public double? Probabilidad
        {
            get; set;
        }
        public int? Potencia
        {
            get; set;
        }
        public int? DuracionTurnos
        {
            get; set;
        }
    }
}
