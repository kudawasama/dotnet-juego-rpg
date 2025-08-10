using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using System;

namespace MiJuegoRPG.Motor
{
    public class MotorCombate
    {
        private Juego juego;
        public MotorCombate(Juego juego)
        {
            this.juego = juego;
        }

        public void ComenzarCombate()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje para combatir. Creando nuevo personaje...");
                juego.jugador = CreadorPersonaje.Crear();
            }
            var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(juego.jugador);
            GeneradorEnemigos.IniciarCombate(juego.jugador, enemigo);
        }

        // Aquí puedes agregar métodos para combate múltiple, jefe, etc.
    }
}
