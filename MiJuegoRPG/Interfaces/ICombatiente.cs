using System;

namespace MiJuegoRPG.Interfaces
{
    public interface ICombatiente
    {
    string Nombre { get; set; }
    int Vida { get; set; }
    int VidaMaxima { get; set; }
    int Defensa { get; }
    int DefensaMagica { get;}
    bool EstaVivo { get; }

    int AtacarFisico(ICombatiente objetivo);
    int AtacarMagico(ICombatiente objetivo);
    void RecibirDanioFisico(int danioFisico);
    void RecibirDanioMagico(int danioMagico);
    }
}