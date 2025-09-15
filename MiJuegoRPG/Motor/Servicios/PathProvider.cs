using System;
using System.IO;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Proveedor central de rutas del juego.
    /// Prioriza la carpeta canónica MiJuegoRPG/DatosJuego con fallbacks tolerantes.
    /// </summary>
    public static class PathProvider
    {
        // Raíz del repo (detectada por Juego.ObtenerRutaRaizProyecto cuando está disponible)
        private static string RaizProyecto
            => MiJuegoRPG.Motor.Juego.ObtenerRutaRaizProyecto();

        /// <summary>
        /// Devuelve la carpeta raíz de datos del juego (DatosJuego).
        /// Orden de búsqueda:
        /// 1) <raiz>/MiJuegoRPG/DatosJuego
        /// 2) <exe>/MiJuegoRPG/DatosJuego
        /// 3) <exe>/DatosJuego
        /// 4) <cwd>/MiJuegoRPG/DatosJuego
        /// </summary>
        public static string DatosJuegoDir()
        {
            var candidatos = new[]
            {
                Path.Combine(RaizProyecto, "MiJuegoRPG", "DatosJuego"),
                Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego"),
                Path.Combine(AppContext.BaseDirectory, "DatosJuego"),
                Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego")
            };
            foreach (var c in candidatos)
            {
                if (Directory.Exists(c)) return c;
            }
            // Último recurso: devolver ruta canónica aunque no exista aún
            return Path.Combine(RaizProyecto, "MiJuegoRPG", "DatosJuego");
        }

        /// <summary>
        /// Devuelve la carpeta PjDatos bajo MiJuegoRPG (para guardados/config del jugador).
        /// </summary>
        public static string PjDatosDir()
        {
            var dir = Path.Combine(RaizProyecto, "MiJuegoRPG", "PjDatos");
            return dir;
        }

        public static string CombineData(params string[] parts)
            => Path.Combine(DatosJuegoDir(), Path.Combine(parts));

        public static string ConfigPath(string fileName)
            => CombineData("config", fileName);

        public static string MisionesPath(string fileName)
            => CombineData("misiones", fileName);

        public static string NpcsPath(string fileName)
            => CombineData("npcs", fileName);

        public static string EquipoPath(string fileName)
            => CombineData("Equipo", fileName);

        public static string PocionesPath(string fileName)
            => CombineData("pociones", fileName);

        public static string MapasDir()
            => CombineData("mapa");

        /// <summary>
        /// Ruta al archivo de grilla del mapa (mapa.txt) bajo MiJuegoRPG.
        /// </summary>
        public static string MapaTxtPath()
            => Path.Combine(MiJuegoRPG.Motor.Juego.ObtenerRutaRaizProyecto(), "MiJuegoRPG", "mapa.txt");

        /// <summary>
        /// Carpeta base de Sectores del mapa (DatosJuego/mapa/SectoresMapa).
        /// </summary>
        public static string SectoresDir()
            => CombineData("mapa", "SectoresMapa");

        public static string PjDatosPath(params string[] parts)
            => Path.Combine(PjDatosDir(), Path.Combine(parts));

        /// <summary>
        /// Carpeta de definición de enemigos individuales (DatosJuego/enemigos).
        /// Permite colocar múltiples archivos JSON, uno por enemigo o por lote.
        /// </summary>
        public static string EnemigosDir()
            => CombineData("enemigos");
    }
}
