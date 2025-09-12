using MiJuegoRPG.Habilidades;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Personaje;
using System;

namespace MiJuegoRPG.Habilidades
{
    // Clase que representa un hechizo mágico, hereda de Habilidad
    public class Hechizo : Habilidad
    {
        // Daño mágico que inflige el hechizo
        public int DanioMagico { get; set; }

        // Constructor: define el daño y el costo de maná (por defecto 10)
        public Hechizo(int danioMagico) : base("Hechizo", 10)
        {
            DanioMagico = danioMagico;
        }

        // Método para usar el hechizo, recibe el personaje que lo lanza y el objetivo
        public override void Usar(Personaje.Personaje usuario, ICombatiente objetivo)
        {
            // Verifica si el personaje tiene suficiente maná
            if (!usuario.GastarMana(Costo))
            {
                Console.WriteLine("¡No tienes suficiente maná para lanzar el hechizo!");
                return; // Si no hay maná, no se ejecuta el hechizo
            }
            // Si hay maná suficiente, se lanza el hechizo y se descuenta el maná
            Console.WriteLine($"{usuario.Nombre} lanza un hechizo sobre {objetivo.Nombre} y hace {DanioMagico} de daño mágico.");
            objetivo.RecibirDanioMagico(DanioMagico);
        }

        // Implementación del método abstracto heredado de Habilidad para el caso sin objetivo
        public override void Usar(Personaje.Personaje usuario)
        {
            Console.WriteLine($"{usuario.Nombre} lanza un hechizo, pero no hay objetivo específico.");
        }
    }
}