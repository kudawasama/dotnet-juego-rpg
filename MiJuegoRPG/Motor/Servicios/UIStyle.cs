using System;

namespace MiJuegoRPG.Motor.Servicios
{
    // Utilidad ligera para unificar estilo de títulos y secciones en la UI de consola
    public static class UIStyle
    {
        // Encabezado principal con línea superior e inferior
        public static void Header(MiJuegoRPG.Interfaces.IUserInterface ui, string titulo)
        {
            if (ui == null) return;
            var line = new string('=', Math.Max(10, Math.Min(80, titulo.Length + 8)));
            ui.SetColor(foreground: ConsoleColor.Cyan);
            ui.WriteLine(line);
            ui.WriteLine($"  {titulo}");
            ui.WriteLine(line);
            ui.ResetColor();
        }

        // Subtítulo con guiones
        public static void SubHeader(MiJuegoRPG.Interfaces.IUserInterface ui, string titulo)
        {
            if (ui == null) return;
            ui.SetColor(foreground: ConsoleColor.Yellow);
            ui.WriteLine($"-- {titulo} --");
            ui.ResetColor();
        }

        // Línea silenciada o de ayuda
        public static void Hint(MiJuegoRPG.Interfaces.IUserInterface ui, string texto)
        {
            if (ui == null) return;
            ui.SetColor(foreground: ConsoleColor.DarkGray);
            ui.WriteLine(texto);
            ui.ResetColor();
        }

        // Indicador compacto de supervivencia: H/S/F y temperatura en una línea o dos, con etiquetas por umbral
        public static void SurvivalCompact(MiJuegoRPG.Interfaces.IUserInterface ui, MiJuegoRPG.Motor.Juego juego)
        {
            if (ui == null || juego == null || juego.jugador == null || juego.supervivenciaService == null) return;
            try
            {
                var sup = juego.supervivenciaService;
                var pj = juego.jugador;
                var (wH, wS, wF) = sup.ObtenerUmbralesAdvertencia();
                var (cH, cS, cF) = sup.ObtenerUmbralesCriticos();
                string Etiqueta(double v, double warn, double crit) => sup.EtiquetaDesdeUmbrales(v, warn, crit).Replace("ADVERTENCIA","ADVERT.");
                ConsoleColor ColorDeEtiqueta(string et)
                {
                    return et == "CRÍTICO" ? ConsoleColor.Red : et.StartsWith("ADVERT") ? ConsoleColor.Yellow : ConsoleColor.Green;
                }
                double pct(double v) => Math.Clamp(v, 0.0, 1.0) * 100.0;

                // H/S/F
                var etH = Etiqueta(pj.Hambre, wH, cH);
                var etS = Etiqueta(pj.Sed, wS, cS);
                var etF = Etiqueta(pj.Fatiga, wF, cF);

                ui.SetColor(foreground: ConsoleColor.Gray);
                ui.Write("Hambre: ");
                ui.SetColor(foreground: ColorDeEtiqueta(etH));
                ui.Write($"{pct(pj.Hambre):F0}% ({etH})  ");
                ui.ResetColor();

                ui.SetColor(foreground: ConsoleColor.Gray);
                ui.Write("Sed: ");
                ui.SetColor(foreground: ColorDeEtiqueta(etS));
                ui.Write($"{pct(pj.Sed):F0}% ({etS})  ");
                ui.ResetColor();

                ui.SetColor(foreground: ConsoleColor.Gray);
                ui.Write("Fatiga: ");
                ui.SetColor(foreground: ColorDeEtiqueta(etF));
                ui.Write($"{pct(pj.Fatiga):F0}% ({etF})  ");
                ui.ResetColor();

                // Temperatura
                double t = pj.TempActual;
                string estadoTemp = sup.EstadoTemperatura(t).Replace("GOLPE DE CALOR","GOLPE CALOR");
                ConsoleColor colT = estadoTemp switch
                {
                    "HIPOTERMIA" => ConsoleColor.Blue,
                    "FRÍO" => ConsoleColor.Cyan,
                    "GOLPE CALOR" => ConsoleColor.Red,
                    "CALOR" => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                ui.SetColor(foreground: ConsoleColor.Gray);
                ui.Write("Temp: ");
                ui.SetColor(foreground: colT);
                ui.Write($"{t:F1}°C ({estadoTemp})");
                ui.ResetColor();
                ui.WriteLine("");
            }
            catch
            {
                // Silencioso: no romper el menú si falta config o hay algún NRE
            }
        }
    }
}
