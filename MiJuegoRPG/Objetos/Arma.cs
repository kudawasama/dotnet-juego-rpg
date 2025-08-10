using System;

namespace MiJuegoRPG.Objetos
{
    public class Arma : MiJuegoRPG.Objetos.Objeto
    {
        public string TipoObjeto { get; set; } = "Arma";
        public int Daño { get; set; }
        public int Nivel { get; set; }

        private static readonly Dictionary<Rareza, double> MultiplicadoresRareza = new Dictionary<Rareza, double>
        {
            { Rareza.Rota, 0.05 },
            { Rareza.Pobre, 0.20 },
            { Rareza.Normal, 0.50 },
            { Rareza.Superior, 0.70 },
            { Rareza.Rara, 0.85 },
            { Rareza.Legendaria, 0.95 },
            { Rareza.Ornamentada, 1.0 }
        };

        public Arma(string nombre, int dañoBase, int nivel = 1, Rareza rareza = Rareza.Normal, string categoria = "UnaMano") : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            Daño = CalcularDaño(dañoBase, nivel, rareza);
        }

        public Arma() : base("", Rareza.Normal, "UnaMano") { }

        private int CalcularDaño(int dañoBase, int nivel, Rareza rareza)
        {
            var random = new Random();
            // Daño base escalado por nivel (ejemplo: dañoBase * nivel)
            int dañoEscalado = dañoBase + (int)(dañoBase * (nivel - 1) * 0.5);
            // Multiplicador por rareza
            double mult = MultiplicadoresRareza.ContainsKey(rareza) ? MultiplicadoresRareza[rareza] : 1.0;
            // Daño aleatorio entre 90% y 110% del daño escalado
            int dañoAleatorio = random.Next((int)(dañoEscalado * 0.9), (int)(dañoEscalado * 1.1) + 1);
            return (int)(dañoAleatorio * mult);
        }

        public override void Usar(MiJuegoRPG.Personaje.Personaje personaje)
        {
            Console.WriteLine($"{personaje.Nombre} equipa el arma {Nombre} ({Rareza}, {Categoria}, Daño: {Daño}, Nivel: {Nivel}).");
        }
    }
}

