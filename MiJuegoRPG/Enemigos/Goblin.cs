//crear enemigo nombre goblin desde la clase padre enemigo

using System;
namespace MiJuegoRPG.Enemigos
{
    public class Goblin : Enemigo
    {
        public Goblin() : base("Goblin", 50, 10, 5, 1, 5, 5)
        {
            // Constructor del Goblin con atributos específicos
            // Vida: 50, Ataque: 10, Defensa: 5, Nivel: 1, Experiencia: 5, Oro: 5
            {
            }
        }
    }
}