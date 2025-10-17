using System;

namespace MiJuegoRPG.Objetos
{
    public class Arma : MiJuegoRPG.Objetos.Objeto, MiJuegoRPG.Interfaces.IBonificadorEstadistica
    {
        public int Perfeccion
        {
            get; set;
        }
        public string TipoObjeto { get; set; } = "Arma";
        public int DañoFisico
        {
            get; set;
        }
        public int DañoMagico
        {
            get; set;
        }
        public int Nivel
        {
            get; set;
        }
        public Dictionary<string, double>? BonificadorAtributos
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets multiplicadores de rareza dinámicos (se recomienda obtenerlos desde RarezaConfig).
        /// </summary>
        public static Dictionary<string, double> MultiplicadoresRareza
        {
            get; set;
        } = new Dictionary<string, double>
        {
            { "Rota", 0.05 },
            { "Pobre", 0.20 },
            { "Normal", 0.50 },
            { "Superior", 0.70 },
            { "Rara", 0.85 },
            { "Legendaria", 0.95 },
            { "Ornamentada", 1.0 }
        };

        public Arma(string nombre, int dañoBase, int nivel = 1, string rareza = "Normal", string categoria = "UnaMano")
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            DañoFisico = CalcularDaño(dañoBase, nivel, rareza);
            DañoMagico = CalcularDaño(dañoBase, nivel, rareza);
            Perfeccion = 50;
        }

        public Arma()
            : base("", "Normal", "UnaMano") { }
        // Constructor extendido para permitir setear perfección
        public Arma(string nombre, int dañoBase, int nivel, string rareza, string categoria, int perfeccion, int bonificadorAtributos)
            : base(nombre, rareza, categoria)
        {
            Nivel = nivel;
            DañoFisico = CalcularDaño(dañoBase, nivel, rareza);
            DañoMagico = CalcularDaño(dañoBase, nivel, rareza);
            Perfeccion = perfeccion;
        }

        private int CalcularDaño(int dañoBase, int nivel, string rareza)
        {
            var random = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            int dañoEscalado = dañoBase + (int)(dañoBase * (nivel - 1) * 0.5);
            double mult = MiJuegoRPG.Objetos.RarezaHelper.MultiplicadorBase(rareza);
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

