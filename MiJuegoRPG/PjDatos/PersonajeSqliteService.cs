#if !TEST_MODE
using Microsoft.Data.Sqlite;
using System.Text.Json;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Servicio para guardar, cargar y listar personajes usando SQLite como backend.
    /// Serializa el objeto Personaje a JSON y lo almacena en la base de datos.
    /// </summary>
    public class PersonajeSqliteService
    {
        // Ruta al archivo de base de datos SQLite (por defecto juego.db en la carpeta de salida)
        private readonly string _dbPath;

        /// <summary>
        /// Inicializa el servicio y crea la tabla si no existe.
        /// </summary>
        /// <param name="dbPath">Ruta personalizada al archivo .db (opcional)</param>
        public PersonajeSqliteService(string? dbPath = null)
        {
            _dbPath = dbPath ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DatosCompartidos", "juego.db");
            Inicializar();
        }

        /// <summary>
        /// Crea la tabla de personajes si no existe.
        /// </summary>
        private void Inicializar()
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS personajes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nombre TEXT UNIQUE,
                datos TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Guarda o actualiza un personaje en la base de datos.
        /// </summary>
        /// <param name="pj">El personaje a guardar</param>
        public void Guardar(Personaje.Personaje pj)
        {
            // Serializa el personaje a JSON
            var options = new JsonSerializerOptions { WriteIndented = false };
            // Registrar el converter polimórfico para Objeto
            options.Converters.Add(new MiJuegoRPG.Objetos.ObjetoJsonConverter());
            var json = JsonSerializer.Serialize(pj, options);
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            var cmd = con.CreateCommand();
            // Inserta o actualiza por nombre
            cmd.CommandText = @"INSERT INTO personajes (nombre, datos) VALUES ($nombre, $datos)
                ON CONFLICT(nombre) DO UPDATE SET datos = $datos;";
            cmd.Parameters.AddWithValue("$nombre", pj.Nombre);
            cmd.Parameters.AddWithValue("$datos", json);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Carga un personaje por nombre desde la base de datos.
        /// </summary>
        /// <param name="nombre">Nombre del personaje</param>
        /// <returns>El personaje o null si no existe</returns>
        public Personaje.Personaje? Cargar(string nombre)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT datos FROM personajes WHERE nombre = $nombre";
            cmd.Parameters.AddWithValue("$nombre", nombre);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var json = reader.GetString(0);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new MiJuegoRPG.Objetos.ObjetoJsonConverter());
                return JsonSerializer.Deserialize<Personaje.Personaje>(json, options);
            }
            return null;
        }

        /// <summary>
        /// Lista los nombres de todos los personajes guardados.
        /// </summary>
        /// <returns>Lista de nombres</returns>
        public List<string> ListarNombres()
        {
            var lista = new List<string>();
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT nombre FROM personajes ORDER BY nombre";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(reader.GetString(0));
            return lista;
        }
    }
}
#endif
