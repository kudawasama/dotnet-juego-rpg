using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Personaje
{
    public class ObjetoConCantidad
    {
        public Objeto Objeto
        {
            get; set;
        }
        public int Cantidad
        {
            get; set;
        }

        public ObjetoConCantidad(Objeto objeto, int cantidad = 1)
        {
            Objeto = objeto;
            Cantidad = cantidad;
        }
    }
}
