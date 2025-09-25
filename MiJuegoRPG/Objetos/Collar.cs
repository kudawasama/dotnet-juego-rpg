using System;

namespace MiJuegoRPG.Objetos
{
    public class Collar : Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public string TipoObjeto { get; set; } = "Collar";
        public int BonificacionDefensa { get; set; }
        public int BonificacionEnergia { get; set; }
        public int Nivel { get; set; }
        public int Perfeccion { get; set; }

        public Collar(string nombre, int bonifDefensa, int bonifEnergia, int nivel = 1, string rareza = "Normal", string categoria = "Collar", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            BonificacionDefensa = CalcularBonificacion(bonifDefensa, perfeccion);
            BonificacionEnergia = CalcularBonificacion(bonifEnergia, perfeccion);
            Perfeccion = perfeccion;
        }

    public Collar() : base("", "Normal", "Collar") { }

        private int CalcularBonificacion(int baseValor, int perfeccion)
        {
            return (int)(baseValor * (perfeccion / 50.0));
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el collar {this.Nombre}.");
        }

        /// <summary>
        /// Bonificador de estadísticas aportado por el collar.
        /// Aporta a Defensa y Energía/Mana según la clave solicitada.
        /// </summary>
        public double ObtenerBonificador(string estadistica)
        {
            if (string.IsNullOrWhiteSpace(estadistica)) return 0;
            if (estadistica.Equals("Defensa", StringComparison.OrdinalIgnoreCase) ||
                estadistica.Equals("DefensaFisica", StringComparison.OrdinalIgnoreCase) ||
                estadistica.Equals("Defensa Física", StringComparison.OrdinalIgnoreCase))
            {
                return BonificacionDefensa;
            }
            if (estadistica.Equals("Energia", StringComparison.OrdinalIgnoreCase) ||
                estadistica.Equals("Mana", StringComparison.OrdinalIgnoreCase))
            {
                return BonificacionEnergia;
            }
            return 0;
        }
    }
}
