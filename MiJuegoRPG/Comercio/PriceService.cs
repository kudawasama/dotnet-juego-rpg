namespace MiJuegoRPG.Comercio
{
    using MiJuegoRPG.Objetos;

    public class PriceService : IPriceService
    {
        public int PrecioDe(Objeto item)
        {
            // base por categoría
            int baseCat = item.Categoria switch
            {
                "Arma" => 30,
                "Armadura" => 25,
                "Accesorio" => 20,
                "Consumible" => 10,
                _ => 15,
            };

            // multiplicador por rareza
            double multRareza = MiJuegoRPG.Objetos.RarezaHelper.MultiplicadorPrecio(item.Rareza);

            // pequeño plus por atributos comunes (si existen)
            int plus = 0;
            if (item is Arma a)
            {
                plus += Math.Max(0, a.DañoFisico + a.DañoMagico);
            }

            if (item is Armadura ar)
            {
                plus += Math.Max(0, ar.Defensa / 2);
            }

            if (item is Pocion p)
            {
                plus += Math.Max(0, p.Curacion / 2);
            }

            var precio = (int)Math.Ceiling((baseCat + plus) * multRareza);
            return Math.Max(precio, 1);
        }

        public int PrecioReventa(Objeto item) => (int)Math.Floor(PrecioDe(item) * 0.6);
    }
}
