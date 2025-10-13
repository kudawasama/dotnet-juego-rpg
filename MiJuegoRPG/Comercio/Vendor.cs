namespace MiJuegoRPG.Comercio
{
    public class Vendor
    {
        public string Id { get; init; } = "";
        public string Nombre { get; init; } = "";
        public string Ubicacion { get; init; } = "";
        public List<StockItem> Stock { get; } = new();
    }
}