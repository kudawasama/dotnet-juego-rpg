using System;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Enemigos
{
    // Esta clase es un tipo de Enemigo concreto.
    // La usamos para todos los enemigos que no tienen un comportamiento especial.
    public class EnemigoEstandar : Enemigo
    {
        // El constructor simplemente llama al constructor de la clase base (Enemigo).
        public EnemigoEstandar(string nombre, int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase, int nivel, int experienciaRecompensa, int oroRecompensa)
            : base(nombre, vidaBase, ataqueBase, defensaBase, defensaMagicaBase, nivel, experienciaRecompensa, oroRecompensa)
        {
            // No se necesita lógica adicional aquí, ya que el comportamiento es estándar.
        }

    public MiJuegoRPG.Objetos.Arma? ArmaEquipada { get; set; }
    }
}