using System;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Enemigos
{
    // Clase base abstracta para todos los enemigos del juego.
    public abstract class Enemigo : ICombatiente
    {

        // Implementación básica de ICombatiente
        public string Nombre { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public int Defensa { get; private set; }
        public int DefensaMagica { get; set; }
        public bool EstaVivo => Vida > 0;
        public int Ataque { get; set; }

        // Implementación explícita de los métodos de ICombatiente
        public virtual int AtacarFisico(ICombatiente objetivo)
        {
            int danio = Ataque;
            objetivo.RecibirDanioFisico(danio);
            return danio;
        }

        public virtual int AtacarMagico(ICombatiente objetivo)
        {
            int danio = Ataque; // Puedes ajustar la lógica si tienes un atributo de ataque mágico
            objetivo.RecibirDanioMagico(danio);
            return danio;
        }

        public virtual void RecibirDanioFisico(int danioFisico)
        {
            int danioReal = Math.Max(1, danioFisico - Defensa);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }

        public virtual void RecibirDanioMagico(int danioMagico)
        {
            int danioReal = Math.Max(1, danioMagico - DefensaMagica);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }
        // Drop de objetos
        public List<MiJuegoRPG.Objetos.Objeto> ObjetosDrop { get; set; } = new List<MiJuegoRPG.Objetos.Objeto>();
        public Dictionary<string, double> ProbabilidadesDrop { get; set; } = new Dictionary<string, double>();

        // Constantes para ajustar la dificultad del drop
        public const double BASE_DROP_RATE = 0.05; // 5% base
        public const double RAREZA_MULTIPLIER_Rota = 0.1;
        public const double RAREZA_MULTIPLIER_Pobre = 0.3;
        public const double RAREZA_MULTIPLIER_Normal = 0.5;
        public const double RAREZA_MULTIPLIER_Superior = 0.7;
        public const double RAREZA_MULTIPLIER_Rara = 0.85;
        public const double RAREZA_MULTIPLIER_Legendaria = 0.95;
        public const double RAREZA_MULTIPLIER_Ornamentada = 1.0;

        public MiJuegoRPG.Objetos.Objeto? IntentarDrop()
        {
            var random = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            foreach (var obj in ObjetosDrop)
            {
                double rate = BASE_DROP_RATE;
                // Ajuste por rareza
                switch (obj.Rareza)
                {
                    case Rareza.Rota: rate *= RAREZA_MULTIPLIER_Rota; break;
                    case Rareza.Pobre: rate *= RAREZA_MULTIPLIER_Pobre; break;
                    case Rareza.Normal: rate *= RAREZA_MULTIPLIER_Normal; break;
                    case Rareza.Superior: rate *= RAREZA_MULTIPLIER_Superior; break;
                    case Rareza.Rara: rate *= RAREZA_MULTIPLIER_Rara; break;
                    case Rareza.Legendaria: rate *= RAREZA_MULTIPLIER_Legendaria; break;
                    case Rareza.Ornamentada: rate *= RAREZA_MULTIPLIER_Ornamentada; break;
                }
                // Ajuste por nivel del monstruo y del objeto
                // Ejemplo: si el objeto es de nivel mucho mayor que el monstruo, reduce el rate
                int nivelMonstruo = this.Nivel;
                int nivelObjeto = (obj is MiJuegoRPG.Objetos.Arma arma) ? arma.Nivel : nivelMonstruo;
                if (nivelObjeto > nivelMonstruo)
                {
                    rate *= 0.5;
                }
                // Drop aleatorio
                if (random.NextDouble() < rate)
                {
                    return obj;
                }
            }
            return null;
        }
    
        // Propiedades del enemigo. 'set' es privado para evitar cambios externos.
        // Defensa ya está arriba
        public int Nivel { get; private set; }
        public int ExperienciaRecompensa { get; private set; }
        public int OroRecompensa { get; private set; }
        
               

        // Constructor modificado. Ahora recibe atributos base y el nivel.
        // Las variables del constructor deben tener los mismos nombres que las propiedades.
        protected Enemigo(string nombre, int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase, int nivel, int experienciaRecompensa, int oroRecompensa)
        {
            Nombre = nombre;
            Nivel = nivel;
            ExperienciaRecompensa = experienciaRecompensa;
            OroRecompensa = oroRecompensa;

            // Llamamos a un método para calcular los atributos basados en el nivel.
            CalcularAtributos(vidaBase, ataqueBase, defensaBase, defensaMagicaBase);
        }

        // Nuevo método privado para calcular los atributos escalados.
        private void CalcularAtributos(int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase)
        {
            int factorEscalado = Nivel;

            VidaMaxima = vidaBase + (vidaBase * factorEscalado / 2);
            Vida = VidaMaxima;

            Ataque = ataqueBase + (ataqueBase * factorEscalado / 2);
            Defensa = defensaBase + (defensaBase * factorEscalado / 2);
            DefensaMagica = defensaMagicaBase + (defensaMagicaBase * factorEscalado / 2);
        }


        // Método para recibir daño.
        public void RecibirDanio(int danio)
        {
            int danioReal = Math.Max(1, danio - Defensa);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }
        
        // Método para dar recompensas.
        public void DarRecompensas(MiJuegoRPG.Personaje.Personaje jugador)
        {
            if (!EstaVivo)
            {
                Console.WriteLine($"El {Nombre} ha sido derrotado.");
                jugador.GanarExperiencia(ExperienciaRecompensa);
                jugador.GanarOro(OroRecompensa);
                var drop = IntentarDrop();
                if (drop != null)
                {
                    Console.WriteLine($"¡Has obtenido el objeto: {drop.Nombre} ({drop.Rareza})!");
                    // Aquí podrías agregar el objeto al inventario del jugador
                }
            }
        }
    }
}