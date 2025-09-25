using System;

namespace MiJuegoRPG.Objetos
{
    public class Armadura : Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public string TipoObjeto { get; set; } = "Armadura";
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Armadura(string nombre, int defensa, int nivel = 1, string rareza = "Normal", string categoria = "Armadura", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensa, perfeccion);
            Perfeccion = perfeccion;
        }

    public Armadura() : base("", "Normal", "Armadura") { }

        private int CalcularDefensa(int defensaBase, int perfeccion)
        {
            // Defensa ajustada por perfección (ejemplo: defensaBase * (perfeccion / 50.0))
            return (int)(defensaBase * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            // Implementación de ejemplo: equipa la armadura
            Console.WriteLine($"{personaje.Nombre} equipa la armadura {this.Nombre}.");
        }

        /// <summary>
        /// Bonificador de estadísticas aportado por la armadura.
        /// Aporta Defensa a claves comunes: "Defensa", "DefensaFisica" o "Defensa Física" (case-insensitive).
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
