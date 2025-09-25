using System;

namespace MiJuegoRPG.Objetos
{
    public class Casco : Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public int Perfeccion { get; set; }
        public string TipoObjeto { get; set; } = "Casco";
        public int Defensa { get; set; }
        public int Nivel { get; set; }

        public Casco(string nombre, int defensaBase, int nivel = 1, string rareza = "Normal", string categoria = "Cabeza", int perfeccion = 50)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Defensa = CalcularDefensa(defensaBase, nivel, rareza);
            Perfeccion = perfeccion;
        }

    public Casco() : base("", "Normal", "Cabeza") { }

        /// <summary>
        /// Calcula la defensa del casco escalando por nivel y rareza dinámica.
        /// Usa el multiplicador de rareza desde RarezaConfig (JSON), no hardcode.
        /// </summary>
        /// <param name="defensaBase">Defensa base del casco.</param>
        /// <param name="nivel">Nivel del objeto.</param>
        /// <param name="rareza">Rareza (string, dinámica).</param>
        /// <returns>Defensa final escalada y ajustada por rareza.</returns>
        private int CalcularDefensa(int defensaBase, int nivel, string rareza)
        {
            var random = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            int defensaEscalada = defensaBase + (int)(defensaBase * (nivel - 1) * 0.5);
            double mult = 1.0;
            // Obtener multiplicador de rareza desde RarezaConfig
            var rarezaConfig = MiJuegoRPG.Objetos.RarezaConfig.Instancia;
            if (rarezaConfig != null && rarezaConfig.Multiplicadores.TryGetValue(rareza, out var m))
                mult = m;
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
