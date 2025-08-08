


namespace MiJuegoRPG.Habilidades
{
    public abstract class Habilidad
    {
        public string Nombre { get; set; }
        public int Costo { get; set; }

        public Habilidad(string nombre, int costo)
        {
            Nombre = nombre;
            Costo = costo;
        }

        public abstract void Usar();
    }
}