using System;

namespace MiJuegoRPG.Enemigos
{
    public class Goblin : Enemigo
    {
        // Constructor por defecto, para usar como Plan B.
        public Goblin() : base("Goblin", 50, 10, 5, 5, 1, 5, 5) { }

        // Constructor con parámetros, para usar con los datos del JSON.
        public Goblin(string nombre, int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase, int nivel, int experienciaRecompensa, int oroRecompensa)
            : base(nombre, vidaBase, ataqueBase, defensaBase, defensaMagicaBase, nivel, experienciaRecompensa, oroRecompensa)
        {
            // Puedes agregar lógica específica si es necesario.
        }
    }
}