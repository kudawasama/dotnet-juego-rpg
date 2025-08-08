using System;

namespace MiJuegoRPG.Interfaces
{
    public interface ICombatiente
    {
        string Nombre { get; set; }
        int Vida { get; set; }
        int VidaMaxima { get; set; }
        int Defensa { get; }
        bool EstaVivo { get; }
        
        void RecibirDanio(int danio);
        int Atacar(ICombatiente objetivo);
    }
}