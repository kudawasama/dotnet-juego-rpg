using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AccionRecoleccion
    {
        private Juego juego;
        public AccionRecoleccion(Juego juego)
        {
            this.juego = juego;
        }
        // Clase legacy: la lógica de recolección fue movida a RecoleccionService.
        // Se mantiene un stub para no romper menús antiguos mientras se migra completamente.
        public void RealizarAccionRecoleccion(string tipo)
        {
            // Delegar al menú nuevo centralizado
            juego.MostrarMenuRecoleccion();
        }
    }
}
