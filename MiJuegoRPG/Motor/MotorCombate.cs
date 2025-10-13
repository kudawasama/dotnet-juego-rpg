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
            if (juego.Jugador == null)
            {
                Console.WriteLine("No hay personaje para combatir. Creando nuevo personaje...");
                juego.Jugador = CreadorPersonaje.Crear();
            }
            var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(juego.Jugador);
            GeneradorEnemigos.IniciarCombate(juego.Jugador, enemigo);
        }

        // Aquí puedes agregar métodos para combate múltiple, jefe, etc.
    }
}
