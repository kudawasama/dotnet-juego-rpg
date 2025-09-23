using System;

namespace MiJuegoRPG.Objetos
{
    public class Botas : Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public string TipoObjeto { get; set; } = "Botas";
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Botas(string nombre, int defensa, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "Botas", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensa, perfeccion);
            Perfeccion = perfeccion;
        }

        public Botas() : base("", Rareza.Normal, "Botas") { }

        private int CalcularDefensa(int defensaBase, int perfeccion)
        {
            return (int)(defensaBase * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa las botas {this.Nombre}.");
        }

        /// <summary>
        /// Bonificador de estadísticas aportado por las botas.
        /// Suma Defensa a "Defensa", "DefensaFisica" o "Defensa Física" (case-insensitive).
        /// </summary>
        public double ObtenerBonificador(string estadistica)
        {
            if (string.IsNullOrWhiteSpace(estadistica)) return 0;
            if (estadistica.Equals("Defensa", StringComparison.OrdinalIgnoreCase) ||
                estadistica.Equals("DefensaFisica", StringComparison.OrdinalIgnoreCase) ||
                estadistica.Equals("Defensa Física", StringComparison.OrdinalIgnoreCase))
            {
                return Defensa;
            }
            return 0;
        }
    }
}
