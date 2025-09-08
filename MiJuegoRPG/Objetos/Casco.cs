using System;

namespace MiJuegoRPG.Objetos
{
    public class Casco : Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public int Perfeccion { get; set; }
        public string TipoObjeto { get; set; } = "Casco";
        public int Defensa { get; set; }
        public int Nivel { get; set; }

        public Casco(string nombre, int defensaBase, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Cabeza", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensaBase, nivel, rareza);
            Perfeccion = perfeccion;
        }

        public Casco() : base("", Rareza.Normal, "Cabeza") { }

        private int CalcularDefensa(int defensaBase, int nivel, Rareza rareza)
        {
            var random = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            int defensaEscalada = defensaBase + (int)(defensaBase * (nivel - 1) * 0.5);
            double mult = 1.0;
            // Puedes agregar lógica de multiplicadores por rareza si lo deseas
            int defensaAleatoria = random.Next((int)(defensaEscalada * 0.9), (int)(defensaEscalada * 1.1) + 1);
            return (int)(defensaAleatoria * mult);
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el casco {Nombre} ({Rareza}, {Categoria}, Defensa: {Defensa}, Nivel: {Nivel}).");
        }
        // Implementación de bonificador de estadística
        public double ObtenerBonificador(string estadistica)
        {
            if (estadistica == "DefensaFisica" || estadistica == "Defensa")
                return Defensa;
            return 0;
        }
    }
}
