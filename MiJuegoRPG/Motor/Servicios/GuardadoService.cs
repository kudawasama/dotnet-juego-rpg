using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Servicio de guardado/carga que abstrae la persistencia (actualmente SQLite vía PersonajeSqliteService).
    /// Futuro: permitir estrategia JSON/local, cifrado, slots, backups.
    /// </summary>
    public class GuardadoService
    {
        private readonly PersonajeSqliteService _sqlite;
        public bool Verbose { get; set; } = true;
    private readonly string rutaCooldowns;
    private readonly string rutaEncuentrosCooldowns;

        public GuardadoService(string? rutaDb = null)
        {
            _sqlite = new PersonajeSqliteService(rutaDb);
            // Archivo adicional para cooldowns (persistencia ligera fuera de SQLite por simplicidad temporal)
            rutaCooldowns = PathProvider.PjDatosPath("cooldowns_nodos.json");
            rutaEncuentrosCooldowns = PathProvider.PjDatosPath("cooldowns_encuentros.json");
        }

        public void Guardar(Personaje.Personaje pj)
        {
            if (pj == null) return;
            try
            {
                // Sincronizar ubicación actual si existe mapa
                var juego = Juego.ObtenerInstanciaActual();
                if (juego?.mapa?.UbicacionActual != null)
                    pj.UbicacionActualId = juego.mapa.UbicacionActual.Id;
                _sqlite.Guardar(pj);
                // Guardar cooldowns de nodos si existe servicio de recolección
                if (juego?.recoleccionService != null)
                {
                    var data = juego.recoleccionService.ExportarCooldownsMultiSector();
                    var jsonCd = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(rutaCooldowns, jsonCd);
                }
                // Guardar cooldowns de encuentros si existe servicio de encuentros
                if (juego?.encuentrosService != null)
                {
                    var dataE = juego.encuentrosService.ExportarCooldowns();
                    var jsonE = System.Text.Json.JsonSerializer.Serialize(dataE, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(rutaEncuentrosCooldowns, jsonE);
                }
                if (Verbose) Console.WriteLine($"[Guardado] Personaje '{pj.Nombre}' guardado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Guardado] Error al guardar: {ex.Message}");
            }
        }

        public Personaje.Personaje? CargarInteractivo()
        {
            try
            {
                var nombres = _sqlite.ListarNombres();
                if (nombres.Count == 0)
                {
                    Console.WriteLine("No hay personajes guardados.");
                    return null;
                }
                Console.WriteLine("=== Personajes Guardados ===");
                for (int i = 0; i < nombres.Count; i++) Console.WriteLine($"{i + 1}. {nombres[i]}");
                Console.Write("Selecciona: ");
                var op = Console.ReadLine();
                if (int.TryParse(op, out int idx) && idx > 0 && idx <= nombres.Count)
                {
                    var pj = _sqlite.Cargar(nombres[idx - 1]);
                    if (pj != null && Verbose) Console.WriteLine($"[Guardado] Personaje '{pj.Nombre}' cargado.");
                    // Cargar cooldowns si archivo existe y servicios ya inicializados
                    try
                    {
                        var juego = Juego.ObtenerInstanciaActual();
                        if (pj != null && juego?.recoleccionService != null && System.IO.File.Exists(rutaCooldowns))
                        {
                            var jsonCd = System.IO.File.ReadAllText(rutaCooldowns);
                            var dic = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string,long>>>(jsonCd) ?? new();
                            juego.recoleccionService.ImportarCooldownsMultiSector(dic);
                        }
                        if (pj != null && juego?.encuentrosService != null && System.IO.File.Exists(rutaEncuentrosCooldowns))
                        {
                            var jsonE = System.IO.File.ReadAllText(rutaEncuentrosCooldowns);
                            var dicE = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, DateTime>>(jsonE) ?? new();
                            juego.encuentrosService.ImportarCooldowns(dicE);
                        }
                    }
                    catch (Exception exCd)
                    {
                        Console.WriteLine($"[Guardado] No se pudieron restaurar cooldowns: {exCd.Message}");
                    }
                    return pj;
                }
                Console.WriteLine("Selección inválida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Guardado] Error al cargar: {ex.Message}");
            }
            return null;
        }
    }
}
