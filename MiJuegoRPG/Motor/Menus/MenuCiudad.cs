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
            // Recuperación pasiva de energía antes de mostrar menú (solo al entrar al menú, no después de descansar)
            if (juego.jugador != null)
                juego.energiaService.RecuperacionPasiva(juego.jugador!);

            string opcion = "";
            while (!salir)
            {
                Console.WriteLine(juego.FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {juego.mapa.UbicacionActual.Nombre}");
                Console.WriteLine("=== Menú de Ciudad ===");
                Console.WriteLine("1. Tienda");
                Console.WriteLine("2. Escuela de Entrenamiento");
                Console.WriteLine("3. Explorar sector");
                Console.WriteLine("4. Descansar en posada");
                Console.WriteLine("5. Viajar");
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1": juego.MostrarTienda(); break;
                    case "2": juego.motorEntrenamiento.Entrenar(); break;
                    case "3": juego.menuPrincipal.MostrarMenuMisionesNPC(); break;
                    case "4":
                        if (juego.jugador != null)
                        {
                            if (juego.jugador.UltimoDiaDescanso != Juego.DiaActual)
                            {
                                juego.jugador.DescansosHoy = 0;
                                juego.jugador.UltimoDiaDescanso = Juego.DiaActual;
                            }
                            juego.jugador.DescansosHoy++;

                            // Lógica de reducción: por ejemplo, cada descanso recupera menos energía
                            int maxEnergia = juego.jugador.EnergiaMaxima;
                            int reduccion = (juego.jugador.DescansosHoy - 1) * 10; // 10 menos por cada descanso extra
                            int energiaRecuperada = Math.Max(maxEnergia - reduccion, 0);
                            juego.jugador.EnergiaActual = Math.Min(juego.jugador.EnergiaActual + energiaRecuperada, maxEnergia);
                            juego.jugador.Vida = juego.jugador.VidaMaxima;

                            Console.WriteLine($"DEBUG: Energía tras descansar: {juego.jugador.EnergiaActual}/{juego.jugador.EnergiaMaxima}");
                            if (energiaRecuperada == 0)
                                Console.WriteLine("Ya no puedes recuperar más energía descansando hoy.");
                            else
                                Console.WriteLine("Has descansado y recuperado tu vida y parte de tu energía.");
                        }
                        else
                        {
                            Console.WriteLine("No hay personaje cargado.");
                        }
                        // NO volver a llamar a RecuperacionPasiva aquí
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
