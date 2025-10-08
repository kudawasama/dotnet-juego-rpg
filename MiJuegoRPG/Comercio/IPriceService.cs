namespace MiJuegoRPG.Comercio
{
    using MiJuegoRPG.Objetos;

    public interface IPriceService
    {
        int PrecioDe(Objeto item);

        int PrecioReventa(Objeto item);
    }
}
