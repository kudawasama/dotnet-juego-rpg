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
    }
}
