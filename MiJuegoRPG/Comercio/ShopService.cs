namespace MiJuegoRPG.Comercio
{
    using System.Text.Json;
    using MiJuegoRPG.Personaje;
    using MiJuegoRPG.Objetos;
    using MiJuegoRPG.Motor; // Para acceder a Juego.ObtenerInstanciaActual()

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
    private static Dictionary<string, string>? _faccionPorUbicacionCache; // carga perezosa desde JSON

        public ShopService(IPriceService precios)
        {
            _precios = precios;
            CargarVendors();
        }

        private static string BuscarDataRoot()
            => MiJuegoRPG.Motor.Servicios.PathProvider.DatosJuegoDir();

        private void CargarVendors()
        {

            var npcPath = MiJuegoRPG.Motor.Servicios.PathProvider.NpcsPath("npc.json");
            var armasPath = MiJuegoRPG.Motor.Servicios.PathProvider.EquipoPath("armas.json");
            var pocionesPath = MiJuegoRPG.Motor.Servicios.PathProvider.PocionesPath("pociones.json");

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
        {
            if (ubicacion != null && _vendors.TryGetValue(ubicacion, out var directo)) return directo;
            // Búsqueda tolerante: por Id/Nombre de la ubicación actual (mezcla de datos en npc.json)
            try
            {
                var juego = Juego.ObtenerInstanciaActual();
                var sector = juego?.mapa?.UbicacionActual;
                if (sector == null) return null;
                // Buscar vendor cuyo Ubicacion coincida con cualquiera de las variantes
                var candidatos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ubicacion ?? string.Empty,
                    sector.Id ?? string.Empty,
                    sector.Nombre ?? string.Empty
                };
                foreach (var v in _vendors.Values)
                {
                    if (candidatos.Contains(v.Ubicacion)) return v;
                }
            }
            catch { }
            return null;
        }

        public bool Comprar(Personaje pj, Vendor v, int index, int cantidad, out string msg)
        {
            // Verificación de atención por reputación en cada operación
            if (!PuedeAtender(pj, v, out var motivo)) { msg = motivo; return false; }
            if (index < 0 || index >= v.Stock.Count) { msg = "Ítem inválido."; return false; }
            var s = v.Stock[index];
            if (cantidad <= 0 || cantidad > s.Cantidad) { msg = "Cantidad no disponible."; return false; }

            var precioUnit = GetPrecioCompra(pj, v, s.Item);
            var total = precioUnit * cantidad;
            if (pj.Oro < total) { msg = $"Oro insuficiente. Necesitas {total}."; return false; }

            pj.Oro -= total;
            pj.Inventario.AgregarObjeto(s.Item, cantidad);
            // StockItem es inmutable, así que reemplazamos el objeto
            v.Stock[index] = new StockItem(s.Item, s.Cantidad - cantidad);
            msg = $"Has comprado {cantidad} x {s.Item.Nombre} por {total} oro.";
            // Reputación: compras favorecen a la facción local
            try
            {
                var juego = Juego.ObtenerInstanciaActual();
                if (juego?.reputacionService != null)
                {
                    int puntos = Math.Max(0, total / 100); // +1 reputación por cada 100 de oro gastado
                    if (puntos > 0)
                    {
                        var fac = FaccionPorUbicacion(v.Ubicacion);
                        if (!string.IsNullOrWhiteSpace(fac)) juego.reputacionService.ModificarReputacionFaccion(fac, puntos, afectarGlobal: false);
                    }
                }
            }
            catch { }
            return true;
        }

        public bool Vender(Personaje pj, Vendor v, int indexInventario, int cantidad, out string msg)
        {
            // Verificación de atención por reputación en cada operación
            if (!PuedeAtender(pj, v, out var motivo)) { msg = motivo; return false; }
            var inv = pj.Inventario.NuevosObjetos;
            if (indexInventario < 0 || indexInventario >= inv.Count) { msg="Ítem inválido."; return false; }
            var oc = inv[indexInventario];
            if (cantidad <= 0 || cantidad > oc.Cantidad) { msg="Cantidad inválida."; return false; }

            var precioUnit = GetPrecioVenta(pj, v, oc.Objeto);
            var total = precioUnit * cantidad;
            pj.Oro += total;
            // RemoverObjeto puede llamarse QuitarObjeto si así se llama en tu Inventario
            pj.Inventario.QuitarObjeto(oc.Objeto, cantidad);
            var i = v.Stock.FindIndex(s => s.Item.Nombre == oc.Objeto.Nombre);
            if (i >= 0) v.Stock[i] = new StockItem(v.Stock[i].Item, v.Stock[i].Cantidad + cantidad);
            else v.Stock.Add(new StockItem(oc.Objeto, cantidad));

            msg = $"Has vendido {cantidad} x {oc.Objeto.Nombre} por {total} oro.";
            // Reputación: ventas también aportan ligeramente a la facción local
            try
            {
                var juego = Juego.ObtenerInstanciaActual();
                if (juego?.reputacionService != null)
                {
                    int puntos = Math.Max(0, total / 200); // +1 reputación por cada 200 de oro vendido
                    if (puntos > 0)
                    {
                        var fac = FaccionPorUbicacion(v.Ubicacion);
                        if (!string.IsNullOrWhiteSpace(fac)) juego.reputacionService.ModificarReputacionFaccion(fac, puntos, afectarGlobal: false);
                    }
                }
            }
            catch { }
            return true;
        }

        // Gating suave: si reputación de facción es muy negativa, el vendedor no atiende.
        public bool PuedeAtender(Personaje pj, Vendor v, out string motivo)
        {
            motivo = string.Empty;
            var fac = FaccionPorUbicacion(v.Ubicacion);
            int repFac = 0; if (!string.IsNullOrWhiteSpace(fac)) pj.ReputacionesFaccion.TryGetValue(fac, out repFac);
            int repGlobal = pj.Reputacion;
            if (MiJuegoRPG.Motor.Servicios.ReputacionPoliticas.DebeBloquearTienda(repFac, repGlobal))
            {
                motivo = string.IsNullOrWhiteSpace(fac)
                    ? "Tu reputación global es nefasta. El comerciante se niega a atenderte."
                    : $"La {fac} te considera indeseable. Este comerciante se niega a atenderte.";
                return false;
            }
            return true;
        }

        // Helpers públicos de precio con reputación aplicada (fuente única de verdad)
        public int GetPrecioCompra(Personaje pj, Vendor v, Objeto item)
        {
            var basePrice = _precios.PrecioDe(item);
            return AplicarDescuentoReputacionCompra(basePrice, pj, v.Ubicacion);
        }

        public int GetPrecioVenta(Personaje pj, Vendor v, Objeto item)
        {
            var basePrice = _precios.PrecioReventa(item);
            return AplicarDescuentoReputacionVenta(basePrice, pj, v.Ubicacion);
        }

        // Reglas de modificación por reputación: ±10% global, ±15% facción
        private static int AplicarDescuentoReputacionCompra(int precioBase, Personaje pj, string ubicacion)
        {
            var faccion = FaccionPorUbicacion(ubicacion);
            int repGlobal = pj.Reputacion;
            int repFaccion = 0;
            if (!string.IsNullOrWhiteSpace(faccion)) pj.ReputacionesFaccion.TryGetValue(faccion, out repFaccion);

            double modGlobal = Math.Clamp(repGlobal / 1000.0, -0.10, 0.10);
            double modFaccion = Math.Clamp(repFaccion / 500.0, -0.15, 0.15);
            double factor = 1.0 - (modGlobal + modFaccion);
            int precio = (int)Math.Max(1, Math.Round(precioBase * factor));
            return precio;
        }

        private static int AplicarDescuentoReputacionVenta(int precioBase, Personaje pj, string ubicacion)
        {
            var faccion = FaccionPorUbicacion(ubicacion);
            int repGlobal = pj.Reputacion;
            int repFaccion = 0;
            if (!string.IsNullOrWhiteSpace(faccion)) pj.ReputacionesFaccion.TryGetValue(faccion, out repFaccion);

            double modGlobal = Math.Clamp(repGlobal / 1000.0, -0.10, 0.10);
            double modFaccion = Math.Clamp(repFaccion / 500.0, -0.15, 0.15);
            // Para venta, el beneficio es menor: 50% del efecto
            double factor = 1.0 + (modGlobal + modFaccion) * 0.5;
            int precio = (int)Math.Max(1, Math.Round(precioBase * factor));
            return precio;
        }

    internal static string FaccionPorUbicacion(string ubicacion)
        {
            // 1) Intentar mapa data-driven desde DatosJuego/facciones_ubicacion.json (cacheado)
            _faccionPorUbicacionCache ??= CargarFaccionesUbicacion();
            if (_faccionPorUbicacionCache != null && _faccionPorUbicacionCache.TryGetValue(ubicacion, out var fac) && !string.IsNullOrWhiteSpace(fac))
                return fac;
            // 2) Fallback mínimo (compatibilidad)
            return ubicacion switch
            {
                "8_23" => "Guardia de Bairan",
                _ => string.Empty
            };
        }

        private static Dictionary<string, string>? CargarFaccionesUbicacion()
        {
            try
            {
                var candidatos = new[]
                {
                    MiJuegoRPG.Motor.Servicios.PathProvider.CombineData("facciones_ubicacion.json"),
                    Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego","facciones_ubicacion.json"),
                    Path.Combine(AppContext.BaseDirectory, "DatosJuego","facciones_ubicacion.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego","facciones_ubicacion.json")
                };
                foreach (var ruta in candidatos)
                {
                    if (File.Exists(ruta))
                    {
                        var json = File.ReadAllText(ruta);
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        if (dict != null) return dict;
                    }
                }
            }
            catch { }
            return null;
        }
    }

    // DTOs públicos y fuera de ShopService
    public record NpcDto(string Id, string Nombre, string Tipo, string Ubicacion);
    public record ArmaDto(string Nombre, int DañoBase, int Perfeccion, string Rareza);
    public record PocionDto(string Nombre, int Curacion, string Rareza);

    static class MapExtensions
    {
        public static Objeto ToDomain(this ArmaDto dto)
            => new Arma(dto.Nombre, dto.DañoBase, nivel:1, rareza: MiJuegoRPG.Objetos.RarezaHelper.Normalizar(dto.Rareza), categoria: "Arma");

        public static Objeto ToDomain(this PocionDto dto)
            => new Pocion(dto.Nombre, dto.Curacion, MiJuegoRPG.Objetos.RarezaHelper.Normalizar(dto.Rareza), "Consumible");
    }
}
