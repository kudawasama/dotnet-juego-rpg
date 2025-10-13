namespace MiJuegoRPG.PjDatos
{
    public class FatigaReduce
    {
        // La propiedad no puede llamarse igual que la clase; mapeamos el nombre JSON para compatibilidad
        [System.Text.Json.Serialization.JsonPropertyName("FatigaReduce")]
        public double Valor
        {
            get; set;
        }
    }
}