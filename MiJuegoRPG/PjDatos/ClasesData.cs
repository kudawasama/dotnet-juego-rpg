using System.Text.Json;
using System.IO;



namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Representa los requisitos y dependencias para desbloquear una clase superior.
    /// </summary>
    public class ClaseProgresion
    {
    /// <summary>Nombre de la clase superior (ej: Nigromante)</summary>
    public required string Nombre { get; set; }
    /// <summary>Clases previas requeridas para evolucionar (ej: Mago, Hechicero)</summary>
    public List<string> ClasesPrevias { get; set; } = new List<string>();
    /// <summary>Nivel mínimo requerido para evolucionar</summary>
    public int NivelMinimo { get; set; }
    /// <summary>Atributos requeridos y sus valores mínimos</summary>
    public Dictionary<string, double> AtributosRequeridos { get; set; } = new Dictionary<string, double>();
    /// <summary>Reputación mínima requerida (puedes omitir si no usas reputación)</summary>
    public int ReputacionMinima { get; set; }
    /// <summary>Nombre de la misión única requerida para evolucionar (opcional)</summary>
    public string? MisionUnica { get; set; }
    /// <summary>Nombre del objeto único requerido para evolucionar (opcional)</summary>
    public string? ObjetoUnico { get; set; }
    /// <summary>Indica si la clase es oculta y no se muestra al jugador hasta cumplir requisitos</summary>
    public bool Oculta { get; set; } = false;
    // Puedes agregar más condiciones según tu sistema
    }

    public static class ProgresionClases
    {
        /// <summary>
        /// Lista de todas las progresiones de clases disponibles en el juego, cargadas desde JSON.
        /// </summary>
        public static List<ClaseProgresion> Todas = new List<ClaseProgresion>();

        /// <summary>
        /// Carga la progresión de clases desde el archivo JSON.
        /// </summary>
        public static void CargarDesdeJson(string? rutaJson = null)
        {
            rutaJson ??= Path.Combine(Environment.CurrentDirectory, "PjDatos", "clases.json");
            if (File.Exists(rutaJson))
            {
                string json = File.ReadAllText(rutaJson);
                var lista = JsonSerializer.Deserialize<List<ClaseProgresion>>(json);
                Todas = lista ?? new List<ClaseProgresion>();
            }
        }

        // Obtiene las clases iniciales para llegar a una clase superior
        public static List<string> ObtenerClasesIniciales(string claseSuperior)
        {
            // Busca la clase superior y retorna las clases previas necesarias para evolucionar.
            var clase = Todas.FirstOrDefault(c => c.Nombre == claseSuperior);
            return clase?.ClasesPrevias ?? new List<string>();
        }

        // Verifica si un personaje puede evolucionar a una clase
        public static bool PuedeEvolucionar(Personaje.Personaje pj, ClaseProgresion clase)
        {
            // Verifica si el personaje cumple con el nivel mínimo
            if (pj.Nivel < clase.NivelMinimo) return false;
            // Verifica si el personaje cumple con los atributos requeridos
            foreach (var atributo in clase.AtributosRequeridos)
            {
                var valor = ObtenerValorAtributo(pj.AtributosBase, atributo.Key);
                if (valor < atributo.Value) return false;
            }
            // Verifica reputación si tu sistema la utiliza
            // if (pj.Reputacion < clase.ReputacionMinima) return false;
            // Verifica si la clase previa está entre las requeridas
            if (pj.Clase == null || !clase.ClasesPrevias.Contains(pj.Clase.Nombre)) return false;
            // Verifica misión única si aplica
            if (!string.IsNullOrEmpty(clase.MisionUnica))
            {
                // Debes implementar la lógica para verificar si el personaje completó la misión
                // if (!pj.MisionesCompletadas.Contains(clase.MisionUnica)) return false;
            }
            // Verifica objeto único si aplica
            if (!string.IsNullOrEmpty(clase.ObjetoUnico))
            {
                // Debes implementar la lógica para verificar si el personaje posee el objeto
                // if (!pj.Inventario.TieneObjeto(clase.ObjetoUnico)) return false;
            }
            return true;
        }

        // Ayuda para obtener el valor de un atributo por nombre
    private static double ObtenerValorAtributo(Personaje.AtributosBase atributos, string nombre)
        {
            // Devuelve el valor del atributo solicitado por nombre. Si no existe, retorna 0.0.
            return nombre switch
            {
                "Fuerza" => atributos.Fuerza,
                "Destreza" => atributos.Destreza,
                "Vitalidad" => atributos.Vitalidad,
                "Agilidad" => atributos.Agilidad,
                "Suerte" => atributos.Suerte,
                "Defensa" => atributos.Defensa,
                "Resistencia" => atributos.Resistencia,
                "Sabiduría" => atributos.Sabiduría,
                "Inteligencia" => atributos.Inteligencia,
                //"Fe" => atributos.Fe,
                "Percepcion" => atributos.Percepcion,
                "Persuasion" => atributos.Persuasion,
                "Liderazgo" => atributos.Liderazgo,
                "Carisma" => atributos.Carisma,
                "Voluntad" => atributos.Voluntad,
                _ => 0.0
            };
        }
        }
    }
