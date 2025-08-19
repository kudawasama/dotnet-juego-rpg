using System;

namespace MiJuegoRPG.Objetos
{
    public class Arma : MiJuegoRPG.Objetos.Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public int Perfeccion { get; set; }
        public string TipoObjeto { get; set; } = "Arma";
        public int DañoFisico { get; set; }
        public int DañoMagico { get; set; }
        public int Nivel { get; set; }
        public Dictionary<string, double>? BonificadorAtributos { get; set; }

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
            DañoFisico = CalcularDaño(dañoBase, nivel, rareza);
            DañoMagico = CalcularDaño(dañoBase, nivel, rareza);
            Perfeccion = 50;
        }

        public Arma() : base("", Rareza.Normal, "UnaMano") { }
        // Constructor extendido para permitir setear perfección
        public Arma(string nombre, int dañoBase, int nivel, Rareza rareza, string categoria, int perfeccion, int bonificadorAtributos)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            DañoFisico = CalcularDaño(dañoBase, nivel, rareza);
            DañoMagico = CalcularDaño(dañoBase, nivel, rareza);
            Perfeccion = perfeccion;
        }

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
            Console.WriteLine($"{personaje.Nombre} equipa el arma {Nombre} ({Rareza}, {Categoria}, DañoFisico: {DañoFisico}, Daño Mágico: {DañoMagico}, Nivel: {Nivel}).");
        }
        // Implementación de bonificador de estadística
        public double ObtenerBonificador(string estadistica)
        {
            if (estadistica == "Daño" || estadistica == "Ataque")
                return DañoFisico;
            return 0;
        }
    }
}

