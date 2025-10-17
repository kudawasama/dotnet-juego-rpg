namespace MiJuegoRPG.PjDatos
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// Servicio estático para gestionar y validar progresiones de clases desde JSON.
    /// </summary>
    public static class ProgresionClases
    {
        /// <summary>
        /// Lista de todas las progresiones de clases disponibles en el juego, cargadas desde JSON.
        /// </summary>
        private static List<ClaseProgresion> todas = new List<ClaseProgresion>();

        /// <summary>
        /// Gets la lista de todas las progresiones de clases disponibles.
        /// </summary>
        public static List<ClaseProgresion> Todas => todas;

        /// <summary>
        /// Carga la progresión de clases desde el archivo JSON.
        /// </summary>
        /// <param name="rutaJson">Ruta opcional al archivo JSON. Si es null, usa la ruta por defecto.</param>
        public static void CargarDesdeJson(string? rutaJson = null)
        {
            var ruta = rutaJson ?? "DatosJuego/clases_dinamicas.json";
            if (!File.Exists(ruta))
            {
                Console.WriteLine($"[ClasesData] No se encontró el archivo: {ruta}");
                return;
            }

            try
            {
                var contenido = File.ReadAllText(ruta);
                var progresiones = JsonSerializer.Deserialize<List<ClaseProgresion>>(contenido) ?? new List<ClaseProgresion>();
                todas.Clear();
                todas.AddRange(progresiones);
                Console.WriteLine($"[ClasesData] Cargadas {progresiones.Count} progresiones de clases desde {ruta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClasesData] Error cargando progresiones: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si un personaje puede evolucionar a una clase específica.
        /// </summary>
        /// <param name="nombreClase">Nombre de la clase a la que se quiere evolucionar.</param>
        /// <param name="personaje">Personaje que se quiere evaluar para la evolución.</param>
        /// <returns>True si el personaje puede evolucionar a la clase especificada, false en caso contrario.</returns>
        public static bool PuedeEvolucionar(string nombreClase, MiJuegoRPG.Personaje.Personaje personaje)
        {
            var progresion = Todas.FirstOrDefault(p => p.Nombre.Equals(nombreClase, StringComparison.OrdinalIgnoreCase));
            if (progresion == null)
            {
                return false;
            }

            // Verifica si tiene el nivel mínimo
            if (personaje.Nivel < progresion.NivelMinimo)
            {
                return false;
            }

            // Verifica si tiene las clases previas
            foreach (var clasePrev in progresion.ClasesPrevias)
            {
                bool tieneClase = personaje.ClasesDesbloqueadas?.Any(cd => cd.Equals(clasePrev, StringComparison.OrdinalIgnoreCase)) == true;
                if (!tieneClase)
                {
                    return false;
                }
            }

            // Verifica si el personaje cumple con los atributos requeridos
            foreach (var (atributo, valorRequerido) in progresion.AtributosRequeridos)
            {
                double valorPersonaje = ObtenerAtributoPersonaje(personaje, atributo);
                if (valorPersonaje < valorRequerido)
                {
                    return false;
                }
            }

            // Verifica reputación si tu sistema la utiliza
            if (progresion.ReputacionMinima > 0)
            {
                // Aquí agregarías la lógica de reputación si la implementas
            }

            // Verifica misión única si aplica
            if (!string.IsNullOrEmpty(progresion.MisionUnica))
            {
                // Verificar si completó la misión
            }

            // Verifica objeto único si aplica
            if (!string.IsNullOrEmpty(progresion.ObjetoUnico))
            {
                // Verificar si tiene el objeto
            }

            return true;
        }

        private static double ObtenerAtributoPersonaje(MiJuegoRPG.Personaje.Personaje personaje, string atributo)
        {
            var atributos = personaje.AtributosBase;
            return atributo.ToLower() switch
            {
                "fuerza" => atributos.Fuerza,
                "destreza" => atributos.Destreza,
                "inteligencia" => atributos.Inteligencia,
                "resistencia" => atributos.Resistencia,
                "voluntad" => atributos.Voluntad,
                "percepcion" => atributos.Percepcion,
                "agilidad" => atributos.Agilidad,
                "suerte" => atributos.Suerte,

                // "Fe" => atributos.Fe,
                _ => 0.0,
            };
        }
    }
}
