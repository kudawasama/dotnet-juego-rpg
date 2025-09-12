using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuFueraCiudad
    {
        
        private Juego juego;
        public MenuFueraCiudad(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuFueraCiudad(ref bool salir)
        {
            // Recuperación pasiva de energía antes de mostrar menú
            if (juego.jugador != null)
            {
                juego.energiaService.RecuperacionPasiva(juego.jugador);
            }

            string opcion = "";
            while (!salir)
            {
                juego.Ui.WriteLine(juego.FormatoRelojMundo);
                juego.Ui.WriteLine($"Ubicación actual: {juego.mapa.UbicacionActual.Nombre}");
                juego.Ui.WriteLine("=== Menú Fuera de Ciudad ===");
                juego.Ui.WriteLine("1. Explorar");
                juego.Ui.WriteLine("2. Recolectar");
                juego.Ui.WriteLine("5. Viajar");
                juego.Ui.WriteLine("9. Menú fijo");
                juego.Ui.WriteLine("0. Volver al menú principal");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1": juego.ExplorarSector(); break;
                    case "2":
                        // Nuevo menú híbrido de recolección
                        juego.recoleccionService.MostrarMenu();
                        break;
                    case "5":
                        juego.MostrarMenuRutas();
                        return;
                    case "9": juego.MostrarMenuFijo(ref salir); break;
                    case "0": return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
