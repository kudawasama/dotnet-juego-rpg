using System;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuOpciones
    {
        private readonly Juego juego;
        public MenuOpciones(Juego juego)
        {
            this.juego = juego;
        }

        public void Mostrar()
        {
            while (true)
            {
                juego.Ui.WriteLine("\n=== Opciones ===");
                juego.Ui.WriteLine($"Logger: {(Logger.Enabled ? "ON" : "OFF")} - Nivel: {Logger.Level}");
                juego.Ui.WriteLine($"Precisión (hit-check): {(GameplayToggles.PrecisionCheckEnabled ? "ON" : "OFF")}");
                juego.Ui.WriteLine($"Penetración: {(GameplayToggles.PenetracionEnabled ? "ON" : "OFF")}");
                juego.Ui.WriteLine($"Verbosidad Combate: {(GameplayToggles.CombatVerbose ? "ON" : "OFF")}");
                juego.Ui.WriteLine("1. Alternar Logger ON/OFF");
                juego.Ui.WriteLine("2. Cambiar nivel de log");
                juego.Ui.WriteLine("3. Alternar Precisión (hit-check)");
                juego.Ui.WriteLine("4. Alternar Penetración");
                juego.Ui.WriteLine("5. Alternar Verbosidad de Combate");
                juego.Ui.WriteLine("6. Volver");
                var op = InputService.LeerOpcion("> ");
                switch (op)
                {
                    case "1":
                        Logger.Enabled = !Logger.Enabled;
                        if (juego.jugador != null)
                        {
                            juego.jugador.PreferenciaLoggerEnabled = Logger.Enabled;
                        }
                        juego.Ui.WriteLine($"Logger ahora {(Logger.Enabled ? "ON" : "OFF")}.");
                        break;
                    case "2":
                        CambiarNivel();
                        break;
                    case "3":
                        GameplayToggles.PrecisionCheckEnabled = !GameplayToggles.PrecisionCheckEnabled;
                        juego.Ui.WriteLine($"Precisión (hit-check) {(GameplayToggles.PrecisionCheckEnabled ? "ON" : "OFF")}.");
                        break;
                    case "4":
                        GameplayToggles.PenetracionEnabled = !GameplayToggles.PenetracionEnabled;
                        juego.Ui.WriteLine($"Penetración {(GameplayToggles.PenetracionEnabled ? "ON" : "OFF")}.");
                        break;
                    case "5":
                        GameplayToggles.CombatVerbose = !GameplayToggles.CombatVerbose;
                        juego.Ui.WriteLine($"Verbosidad de Combate {(GameplayToggles.CombatVerbose ? "ON" : "OFF")}.");
                        break;
                    case "6":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        private void CambiarNivel()
        {
            juego.Ui.WriteLine("Niveles disponibles: 1) Error  2) Warn  3) Info  4) Debug");
            var op = InputService.LeerOpcion("Selecciona nivel: ");
            switch (op)
            {
                case "1": Logger.Level = LogLevel.Error; break;
                case "2": Logger.Level = LogLevel.Warn; break;
                case "3": Logger.Level = LogLevel.Info; break;
                case "4": Logger.Level = LogLevel.Debug; break;
                default:
                    juego.Ui.WriteLine("Selección inválida.");
                    return;
            }
            if (juego.jugador != null)
            {
                juego.jugador.PreferenciaLoggerLevel = Logger.Level.ToString();
            }
            juego.Ui.WriteLine($"Nivel de log establecido a {Logger.Level}.");
        }
    }
}
