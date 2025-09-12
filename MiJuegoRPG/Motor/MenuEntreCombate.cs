//Menu entre combates
// Este archivo contiene las opciones del menú entre combates, como guardar el personaje y mostrar sus datos.
// continuar con el combate o salir del juego.
using System;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;


namespace MiJuegoRPG.Motor
{
    public class MenuEntreCombate
    {
        private Juego juego;
        private MenusJuego menusJuego;

        public MenuEntreCombate(Juego juego, MenusJuego menusJuego)
        {
            this.juego = juego;
            this.menusJuego = menusJuego;
        }

        public void MostrarMenu()
        {
            juego.Ui.WriteLine("Menú entre combates:");
            juego.Ui.WriteLine("1. Guardar personaje");
            juego.Ui.WriteLine("2. Cargar personaje");
            juego.Ui.WriteLine("3. Continuar combatiendo");
            juego.Ui.WriteLine("4. Menú principal fijo");
            juego.Ui.WriteLine("5. Salir del juego");

            var opcion = InputService.LeerOpcion();

            switch (opcion)
            {
                case "1":
                    juego.GuardarPersonaje();
                    break;
                case "2":
                    juego.CargarPersonaje();
                    break;
                case "3":
                    juego.motorCombate.ComenzarCombate();
                    break;
                case "4":
                    menusJuego.MostrarMenuPrincipalFijo();
                    break;
                case "5":
                    Environment.Exit(0);
                    break;
                default:
                    juego.Ui.WriteLine("Opción no válida.");
                    break;
            }
        }
    }
}