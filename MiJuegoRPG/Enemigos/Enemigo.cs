using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Enemigos
{
    //Enemigo Normal Base, Standar de creacion de monstruos
    //Clase base para todos los enemigos
    //Asignacion de atributos basicos con agregacion de nuevos atributos en porcentanjes segun Nivel y Atributos del Pj
    // Los atributos basicos se asignan en el constructor.
    // Se pueden agregar nuevos atributos calculados en porcentaje segun el nivel y atributos del personaje.
    // Ejemplo: PoderEspecial, Velocidad, etc.
    // Se recomienda extender esta clase para enemigos con habilidades especiales.
    public abstract class Enemigo : ICombatiente
    {
        
        public string Nombre { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public int Ataque { get; set; }
        public int Defensa { get; set; }
        public int Nivel { get; set; }
        public int ExperienciaRecompensa { get; set; }
        public int OroRecompensa { get; set; }

        public bool EstaVivo => Vida > 0;

        protected Enemigo(string nombre, int vida, int ataque, int defensa, int nivel = 1, int experiencia = 0, int oro = 0)
        {
            Nombre = nombre;
            Vida = vida;
            VidaMaxima = vida; // Inicialmente la vida máxima es igual a la vida actual
            Ataque = ataque;
            Defensa = defensa;
            Nivel = nivel;
            ExperienciaRecompensa = experiencia;
            OroRecompensa = oro;
        }

        public virtual int Atacar(ICombatiente objetivo)
        {
            objetivo.RecibirDanio(Ataque);
            return Ataque;
        }

        public void RecibirDanio(int danio)
        {
            int danioReal = Math.Max(1, danio - Defensa);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }
        public void DarRecompensas(MiJuegoRPG.Personaje.Personaje jugador)
        {
            if (!EstaVivo)
            {
                Console.WriteLine($"{Nombre} ha sido derrotado.");
                jugador.GanarExperiencia(ExperienciaRecompensa);
                jugador.GanarOro(OroRecompensa);

            }
        }
    }
}