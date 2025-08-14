namespace MiJuegoRPG.Personaje
{
    public class FuenteBonificador
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "Atributo" o "Estadistica"
        public string Clave { get; set; } // Ej: "Fuerza", "Ataque"
        public double Valor { get; set; }
        public FuenteBonificador(string nombre, string tipo, string clave, double valor)
        {
            Nombre = nombre;
            Tipo = tipo;
            Clave = clave;
            Valor = valor;
        }
    }
}
