using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;

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
            if (juego.Jugador != null)
                juego.EnergiaService.RecuperacionPasiva(juego.Jugador!);

            string opcion = "";
            while (!salir)
            {
                UIStyle.Header(juego.Ui, "Menú de Ciudad");
                UIStyle.SubHeader(juego.Ui, $"Ubicación: {juego.Mapa.UbicacionActual.Nombre}");
                UIStyle.Hint(juego.Ui, juego.FormatoRelojMundo);
                // Indicador compacto de supervivencia (hambre/sed/fatiga/temp)
                UIStyle.SurvivalCompact(juego.Ui, juego);
                juego.Ui.WriteLine("1. Tienda");
                juego.Ui.WriteLine("2. Escuela de Entrenamiento");
                juego.Ui.WriteLine("3. Explorar sector");
                juego.Ui.WriteLine("4. Descansar en posada");
                juego.Ui.WriteLine("5. Viajar");
                juego.Ui.WriteLine("9. Menú fijo");
                juego.Ui.WriteLine("0. Volver al menú principal");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1":
                        juego.MostrarTienda();
                        break;
                    case "2":
                        juego.MotorEntrenamiento.Entrenar();
                        break;
                    case "3":
                        juego.MenuPrincipal.MostrarMenuMisionesNPC();
                        break;
                    case "4":
                        if (juego.Jugador != null)
                        {
                            if (juego.Jugador.UltimoDiaDescanso != Juego.DiaActual)
                            {
                                juego.Jugador.DescansosHoy = 0;
                                juego.Jugador.UltimoDiaDescanso = Juego.DiaActual;
                            }
                            juego.Jugador.DescansosHoy++;

                            // Lógica de reducción: por ejemplo, cada descanso recupera menos energía
                            int maxEnergia = juego.Jugador.EnergiaMaxima;
                            int reduccion = (juego.Jugador.DescansosHoy - 1) * 10; // 10 menos por cada descanso extra
                            int energiaRecuperada = Math.Max(maxEnergia - reduccion, 0);
                            juego.Jugador.EnergiaActual = Math.Min(juego.Jugador.EnergiaActual + energiaRecuperada, maxEnergia);
                            juego.Jugador.Vida = juego.Jugador.VidaMaxima;
                            // Recuperación de maná fuera de combate (lenta, parametrizada)
                            var rules = new ActionRulesService();
                            var manaRec = rules.RegenerarManaFueraCombate(juego.Jugador);

                            juego.Ui.WriteLine($"DEBUG: Energía tras descansar: {juego.Jugador.EnergiaActual}/{juego.Jugador.EnergiaMaxima}");
                            if (energiaRecuperada == 0)
                                juego.Ui.WriteLine("Ya no puedes recuperar más energía descansando hoy.");
                            else
                                juego.Ui.WriteLine($"Has descansado, recuperado tu vida y parte de tu energía. Maná +{manaRec}.");
                        }
                        else
                        {
                            juego.Ui.WriteLine("No hay personaje cargado.");
                        }
                        // NO volver a llamar a RecuperacionPasiva aquí
                        juego.MostrarMenuFijo(ref salir);
                        break;
                    case "5":
                        juego.MostrarMenuRutas();
                        return;
                    case "9":
                        juego.MostrarMenuFijo(ref salir);
                        break;
                    case "0":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
