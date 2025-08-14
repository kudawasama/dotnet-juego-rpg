namespace MiJuegoRPG.Comercio
{
    using System.Text.Json;
    using MiJuegoRPG.Personaje;
    using MiJuegoRPG.Objetos;

    public record StockItem(Objeto Item, int Cantidad);

    public class Vendor
    {
        public string Id { get; init; } = "";
        public string Nombre { get; init; } = "";
        public string Ubicacion { get; init; } = "";
        public List<StockItem> Stock { get; } = new();
    }

    public class ShopService
    {
        private readonly IPriceService _precios;
        private readonly Dictionary<string, Vendor> _vendors = new();

        public ShopService(IPriceService precios)
        {
            _precios = precios;
            CargarVendors();
        }

        private static string BuscarDataRoot()
        {
            var candidatos = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego"),
                Path.Combine(AppContext.BaseDirectory, "DatosJuego"),
                Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego")
            };
            foreach (var c in candidatos) if (Directory.Exists(c)) return c;
            throw new DirectoryNotFoundException("No se encontró carpeta PjDatos cerca del ejecutable.");
        }

        private void CargarVendors()
        {

            var npcPath = Path.Combine(AppContext.BaseDirectory, "DatosJuego", "npcs", "npc.json");
            var armasPath = Path.Combine(AppContext.BaseDirectory, "DatosJuego", "Equipo", "armas.json");
            var pocionesPath = Path.Combine(AppContext.BaseDirectory, "DatosJuego", "pociones", "pociones.json");

            var npcs = JsonSerializer.Deserialize<List<NpcDto>>(File.ReadAllText(npcPath)) ?? new();
            var armas = JsonSerializer.Deserialize<List<ArmaDto>>(File.ReadAllText(armasPath)) ?? new();
            var pociones = JsonSerializer.Deserialize<List<PocionDto>>(File.ReadAllText(pocionesPath)) ?? new();

            foreach (var npc in npcs.Where(n => string.Equals(n.Tipo, "Mercader", StringComparison.OrdinalIgnoreCase)))
            {
                var v = new Vendor { Id = npc.Id, Nombre = npc.Nombre, Ubicacion = npc.Ubicacion };
                foreach (var a in armas.Take(5)) v.Stock.Add(new StockItem(a.ToDomain(), 3));
                foreach (var p in pociones.Take(3)) v.Stock.Add(new StockItem(p.ToDomain(), 10));
                _vendors[v.Ubicacion] = v;
            }
        }

        public Vendor? GetVendorPorUbicacion(string? ubicacion)
            => ubicacion != null && _vendors.TryGetValue(ubicacion, out var v) ? v : null;

        public bool Comprar(Personaje pj, Vendor v, int index, int cantidad, out string msg)
        {
            if (index < 0 || index >= v.Stock.Count) { msg = "Ítem inválido."; return false; }
            var s = v.Stock[index];
            if (cantidad <= 0 || cantidad > s.Cantidad) { msg = "Cantidad no disponible."; return false; }

            var precioUnit = _precios.PrecioDe(s.Item);
            var total = precioUnit * cantidad;
            if (pj.Oro < total) { msg = $"Oro insuficiente. Necesitas {total}."; return false; }

            pj.Oro -= total;
            pj.Inventario.AgregarObjeto(s.Item, cantidad);
            // StockItem es inmutable, así que reemplazamos el objeto
            v.Stock[index] = new StockItem(s.Item, s.Cantidad - cantidad);
            msg = $"Has comprado {cantidad} x {s.Item.Nombre} por {total} oro.";
            return true;
        }

        public bool Vender(Personaje pj, Vendor v, int indexInventario, int cantidad, out string msg)
        {
            var inv = pj.Inventario.NuevosObjetos;
            if (indexInventario < 0 || indexInventario >= inv.Count) { msg="Ítem inválido."; return false; }
            var oc = inv[indexInventario];
            if (cantidad <= 0 || cantidad > oc.Cantidad) { msg="Cantidad inválida."; return false; }

            var total = _precios.PrecioReventa(oc.Objeto) * cantidad;
            pj.Oro += total;
            // RemoverObjeto puede llamarse QuitarObjeto si así se llama en tu Inventario
            pj.Inventario.QuitarObjeto(oc.Objeto, cantidad);
            var i = v.Stock.FindIndex(s => s.Item.Nombre == oc.Objeto.Nombre);
            if (i >= 0) v.Stock[i] = new StockItem(v.Stock[i].Item, v.Stock[i].Cantidad + cantidad);
            else v.Stock.Add(new StockItem(oc.Objeto, cantidad));

            msg = $"Has vendido {cantidad} x {oc.Objeto.Nombre} por {total} oro.";
            return true;
        }
    }

    // DTOs públicos y fuera de ShopService
    public record NpcDto(string Id, string Nombre, string Tipo, string Ubicacion);
    public record ArmaDto(string Nombre, int DañoBase, int Perfeccion, string Rareza);
    public record PocionDto(string Nombre, int Curacion, string Rareza);

    static class MapExtensions
    {
        public static Objeto ToDomain(this ArmaDto dto)
            => new Arma(dto.Nombre, dto.DañoBase, dto.Perfeccion, ParseRareza(dto.Rareza), "Arma");

        public static Objeto ToDomain(this PocionDto dto)
            => new Pocion(dto.Nombre, dto.Curacion, ParseRareza(dto.Rareza), "Consumible");

        private static Rareza ParseRareza(string r)
            => Enum.TryParse<Rareza>(r, true, out var rr) ? rr : Rareza.Normal;
    }
}
