namespace MiJuegoRPG.Comercio
{
    using MiJuegoRPG.Objetos;

    internal static class MapExtensions
    {
        public static Objeto ToDomain(this ArmaDto dto)
            => new Arma(dto.Nombre, (int)dto.DañoBase, nivel: 1, rareza: MiJuegoRPG.Objetos.RarezaHelper.Normalizar(dto.Rareza), categoria: "Arma");

        public static Objeto ToDomain(this PocionDto dto)
            => new Pocion(dto.Nombre, dto.Curacion, MiJuegoRPG.Objetos.RarezaHelper.Normalizar(dto.Rareza), "Consumible");
    }
}