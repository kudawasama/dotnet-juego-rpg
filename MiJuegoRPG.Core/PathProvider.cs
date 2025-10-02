using System;
using System.IO;

namespace MiJuegoRPG.Core
{
    /// <summary>
    /// Proveedor de rutas desacoplado: sólo lógica de composición.
    /// La resolución de raíz se delega mediante un Func inyectable para evitar depender de Juego.ObtenerRutaRaizProyecto.
    /// </summary>
    public static class PathProvider
    {
        public static Func<string>? ResolveRoot; // asignable desde el ejecutable
        private static string RaizProyecto => (ResolveRoot != null ? ResolveRoot() : Directory.GetCurrentDirectory());

        public static void OverrideRootForTests(string? newRoot)
        {
            if (string.IsNullOrWhiteSpace(newRoot)) ResolveRoot = null; else ResolveRoot = () => newRoot!;
        }

        public static string DatosJuegoDir()
        {
            var candidatos = new[]
            {
                Path.Combine(RaizProyecto, "MiJuegoRPG", "DatosJuego"),
                Path.Combine(AppContext.BaseDirectory, "MiJuegoRPG","DatosJuego"),
                Path.Combine(AppContext.BaseDirectory, "DatosJuego"),
                Path.Combine(Directory.GetCurrentDirectory(), "MiJuegoRPG","DatosJuego")
            };
            foreach (var c in candidatos) if (Directory.Exists(c)) return c;
            return Path.Combine(RaizProyecto, "MiJuegoRPG", "DatosJuego");
        }

        public static string CombineData(params string[] parts) => Path.Combine(DatosJuegoDir(), Path.Combine(parts));
        public static string EquipoPath(string fileName) => CombineData("Equipo", fileName);
        public static string PocionesPath(string fileName) => CombineData("pociones", fileName);
        public static string EnemigosDir() => CombineData("enemigos");
        public static string MapasDir() => CombineData("mapa");
        public static string SectoresDir() => CombineData("mapa", "SectoresMapa");
        public static string MaterialesDir() => CombineData("Materiales");
        public static string ArmasDir() => CombineData("Equipo", "armas");
    }
}