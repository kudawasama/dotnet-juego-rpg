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

        /// 
        /// <returns></returns><summary>
        /// Devuelve la carpeta raíz de datos del juego (DatosJuego).
        /// Orden de búsqueda:
        /// 1). <raiz>/MiJuegoRPG/DatosJuego
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
                if (Directory.Exists(c))
                    return c;
            }
            // Último recurso: devolver ruta canónica aunque no exista aún
            return Path.Combine(RaizProyecto, "MiJuegoRPG", "DatosJuego");
        }

        /// <summary>
        /// Devuelve la carpeta PjDatos bajo MiJuegoRPG (para guardados/config del jugador).
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public static string MapaTxtPath()
            => Path.Combine(MiJuegoRPG.Motor.Juego.ObtenerRutaRaizProyecto(), "MiJuegoRPG", "mapa.txt");

        /// <summary>
        /// Carpeta base de Sectores del mapa (DatosJuego/mapa/SectoresMapa).
        /// </summary>
        /// <returns></returns>
        public static string SectoresDir()
            => CombineData("mapa", "SectoresMapa");

        public static string PjDatosPath(params string[] parts)
            => Path.Combine(PjDatosDir(), Path.Combine(parts));

        /// <summary>
        /// Carpeta de definición de enemigos individuales (DatosJuego/enemigos).
        /// Permite colocar múltiples archivos JSON, uno por enemigo o por lote.
        /// </summary>
        /// <returns></returns>
        public static string EnemigosDir()
            => CombineData("enemigos");

        /// <summary>
        /// Carpeta base de materiales individuales (DatosJuego/Materiales).
        /// </summary>
        /// <returns></returns>
        public static string MaterialesDir()
            => CombineData("Materiales");

        /// <summary>
        /// Carpeta base de armas individuales (DatosJuego/Equipo/armas).
        /// </summary>
        /// <returns></returns>
        public static string ArmasDir()
            => CombineData("Equipo", "armas");

        /// <summary>
        /// Carpeta base de armaduras individuales (DatosJuego/Equipo/armaduras).
        /// </summary>
        /// <returns></returns>
        public static string ArmadurasDir()
            => CombineData("Equipo", "armaduras");

        /// <summary>
        /// Carpeta base de botas individuales (DatosJuego/Equipo/botas).
        /// </summary>
        /// <returns></returns>
        public static string BotasDir()
            => CombineData("Equipo", "botas");

        /// <summary>
        /// Carpeta base de cascos individuales (DatosJuego/Equipo/cascos).
        /// </summary>
        /// <returns></returns>
        public static string CascosDir()
            => CombineData("Equipo", "cascos");

        /// <summary>
        /// Carpeta base de cinturones individuales (DatosJuego/Equipo/cinturones).
        /// </summary>
        /// <returns></returns>
        public static string CinturonesDir()
            => CombineData("Equipo", "cinturones");

        /// <summary>
        /// Carpeta base de collares individuales (DatosJuego/Equipo/collares).
        /// </summary>
        /// <returns></returns>
        public static string CollaresDir()
            => CombineData("Equipo", "collares");

        /// <summary>
        /// Carpeta base de pantalones individuales (DatosJuego/Equipo/pantalones).
        /// </summary>
        /// <returns></returns>
        public static string PantalonesDir()
            => CombineData("Equipo", "pantalones");
    }
}
