using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuCiudad
    {
        private Juego juego;
        public MenuCiudad(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuCiudad(ref bool salir)
        {
            string opcion = "";
            while (!salir)
            {
                Console.WriteLine(juego.FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {juego.ubicacionActual.Nombre}");
                Console.WriteLine("=== Menú de Ciudad ===");
                Console.WriteLine("1. Tienda");
                Console.WriteLine("2. Escuela de Entrenamiento");
                Console.WriteLine("3. Explorar sector");
                Console.WriteLine("4. Descansar en posada");
                Console.WriteLine("5. Salir de la ciudad");
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1": juego.MostrarTienda(); break;
                    case "2": juego.Entrenar(); break;
                    case "3": juego.ExplorarSector(); break;
                    case "4":
                        if (juego.jugador != null)
                        {
                            if (juego.jugador.UltimoDiaDescanso != Juego.DiaActual)
                            {
                                juego.jugador.DescansosHoy = 0;
                                juego.jugador.UltimoDiaDescanso = Juego.DiaActual;
                            }
                            juego.jugador.Vida = juego.jugador.VidaMaxima;
                            juego.jugador.EnergiaActual = juego.jugador.EnergiaMaxima;
                            Console.WriteLine($"DEBUG: Energía tras descansar: {juego.jugador.EnergiaActual}/{juego.jugador.EnergiaMaxima}");
                            juego.jugador.DescansosHoy++; // <--- Aquí se incrementa el contador
                            Console.WriteLine("Has descansado y recuperado toda tu vida y energía.");
                        }
                        else
                            Console.WriteLine("No hay personaje cargado.");
                        juego.MostrarMenuFijo(ref salir);
                        break;
                    case "5":
                        juego.MostrarMenuRutas();
                        return;
                    case "9": juego.MostrarMenuFijo(ref salir); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
