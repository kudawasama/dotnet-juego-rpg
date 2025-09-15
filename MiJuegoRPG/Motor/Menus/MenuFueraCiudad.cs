using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;

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
                UIStyle.Header(juego.Ui, "Fuera de Ciudad");
                UIStyle.SubHeader(juego.Ui, $"Ubicación: {juego.mapa.UbicacionActual.Nombre}");
                UIStyle.Hint(juego.Ui, juego.FormatoRelojMundo);
                juego.Ui.WriteLine("1. Explorar");
                juego.Ui.WriteLine("2. Recolectar");
                juego.Ui.WriteLine("3. Combatir");
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
                    case "3":
                        if (juego.jugador == null)
                        {
                            juego.Ui.WriteLine("No hay personaje cargado.");
                            InputService.Pausa();
                            break;
                        }
                        try
                        {
                            var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(juego.jugador);
                            GeneradorEnemigos.IniciarCombate(juego.jugador, enemigo);
                        }
                        catch (Exception ex)
                        {
                            juego.Ui.WriteLine($"Error al iniciar combate: {ex.Message}");
                            InputService.Pausa();
                        }
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
