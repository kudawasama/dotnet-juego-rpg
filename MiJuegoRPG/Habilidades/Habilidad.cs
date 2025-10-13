namespace MiJuegoRPG.Habilidades
{
    public abstract class Habilidad
    {
        public string Nombre
        {
            get; set;
        }
        public int Costo
        {
            get; set;
        }

        public Habilidad(string nombre, int costo)
        {
            Nombre = nombre;
            Costo = costo;
        }

        // Método abstracto para habilidades sin objetivo específico
        public abstract void Usar(MiJuegoRPG.Personaje.Personaje usuario);

        // Nuevo método abstracto para habilidades con objetivo
        public abstract void Usar(MiJuegoRPG.Personaje.Personaje usuario, MiJuegoRPG.Interfaces.ICombatiente objetivo);
    }
}
