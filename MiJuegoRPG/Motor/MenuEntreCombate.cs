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

        public MenuEntreCombate(Juego juego)
        {
            this.juego = juego;
        }

        public void MostrarMenu()
        {
            Console.WriteLine("Menú entre combates:");
            Console.WriteLine("1. Guardar personaje");
            Console.WriteLine("2. Cargar personaje");
            Console.WriteLine("3. Continuar combatiendo");
            Console.WriteLine("4. Salir del juego");

            var opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    juego.GuardarPersonaje();
                    break;
                case "2":
                    juego.CargarPersonaje();
                    break;
                case "3":
                    juego.ComenzarCombateAleatorio();
                    break;
                case "4":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
    }
}