using MiJuegoRPG.Motor;
using MiJuegoRPG.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Herramientas;
using MiJuegoRPG.Motor.Servicios;

// Bandera global para activar el chequeo de precisión (hit chance)
public static class GameplayToggles
{
    public static bool PrecisionCheckEnabled = false;
}

class Program
{
    static void Main(string[] args)
    {
        // Flags de logging: --log-off o --log-level=debug|info|warn|error|off
        bool salirTrasHerramientas = false; // permite salir tras ejecutar ciertas herramientas CLI
        try
        {
            if (args != null && args.Length > 0)
            {
                // Ayuda rápida
                foreach (var a in args)
                {
                    if (string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Uso: dotnet run -- [opciones]\n");
                        Console.WriteLine("Opciones:");
                        Console.WriteLine("  --help, -h                 Muestra esta ayuda y termina.");
                        Console.WriteLine("  --log-off                  Desactiva el logger al inicio.");
                        Console.WriteLine("  --log-level=<nivel>        Establece nivel de log: debug|info|warn|error|off.");
                        Console.WriteLine("  --reparar-sectores         Ejecuta el reparador de sectores al inicio (opcional).");
                        Console.WriteLine("  --generar-conexiones       Genera conexiones cardinales (N/E/S/O) a partir de mapa.txt y actualiza JSONs.");
                        Console.WriteLine("  --validar-sectores         Valida sectores: IDs, bidireccionalidad y conectividad (BFS desde ciudadPrincipal).");
                        Console.WriteLine("  --normalizar-conexiones    Asegura bidireccionalidad en todas las conexiones (si A->B, agrega B->A si falta).");
                        Console.WriteLine("  --asignar-biomas[=ol,oc]  Asigna biomas por bandas desde bordes: Oceano Lejano (ol), Oceano (oc). Ej: --asignar-biomas=2,2");
                        Console.WriteLine("  --validar-datos[=report|<ruta>] Ejecuta validador referencial de datos (10.6).\n     - =report: genera reporte en PjDatos/validacion/\n     - =<ruta>: genera reporte en la ruta indicada\n");
                        Console.WriteLine("  --hidratar-nodos[=max]     Escribe nodos de recolección en sectores vacíos a partir del bioma. Ej: --hidratar-nodos=5\n");
                        Console.WriteLine("  --reparar-materiales=report[;ruta]  Escanea nodos y reporta materiales inválidos (Nombre vacío/Cantidad<=0). No modifica archivos.\n");
                        Console.WriteLine("  --reparar-materiales=write[;ruta]   Aplica reparación eliminando materiales inválidos. Genera reporte.\n");
                        Console.WriteLine("  --precision-hit             Activa el chequeo de precisión (probabilidad de acierto) en ataques físicos y mágicos.");
                        Console.WriteLine("Notas:");
                        Console.WriteLine("- Puedes cambiar el logger en runtime desde Menú Principal → Opciones.");
                        Console.WriteLine("- Las preferencias de logger se guardan por partida; los flags CLI tienen precedencia al inicio.");
                        return;
                    }
                }
                foreach (var a in args)
                {
                    if (string.Equals(a, "--precision-hit", StringComparison.OrdinalIgnoreCase))
                    {
                        GameplayToggles.PrecisionCheckEnabled = true;
                    }
                    if (string.Equals(a, "--log-off", StringComparison.OrdinalIgnoreCase))
                    {
                        MiJuegoRPG.Motor.Servicios.Logger.Enabled = false;
                    }
                    else if (a.StartsWith("--log-level=", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = a.Substring("--log-level=".Length).Trim().ToLowerInvariant();
                        switch (val)
                        {
                            case "debug":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Debug; break;
                            case "info":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Info; break;
                            case "warn":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Warn; break;
                            case "error":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Error; break;
                            case "off":
                                MiJuegoRPG.Motor.Servicios.Logger.Enabled = false; break;
                        }
                    }
                    else if (a.StartsWith("--validar-datos", StringComparison.OrdinalIgnoreCase))
                    {
                        bool generarReporte = false;
                        string? ruta = null;
                        if (string.Equals(a, "--validar-datos", StringComparison.OrdinalIgnoreCase))
                        {
                            // solo validar, sin reporte
                        }
                        else if (a.StartsWith("--validar-datos=", StringComparison.OrdinalIgnoreCase))
                        {
                            var val = a.Substring("--validar-datos=".Length).Trim();
                            if (string.Equals(val, "report", StringComparison.OrdinalIgnoreCase))
                            {
                                generarReporte = true;
                            }
                            else if (!string.IsNullOrWhiteSpace(val))
                            {
                                generarReporte = true;
                                ruta = val;
                            }
                        }

                        // Ejecutar validador y continuar
                        var r = MiJuegoRPG.Motor.Servicios.Validacion.DataValidatorService.ValidarReferenciasBasicas(generarReporte, ruta);
                        if (!r.IsOk)
                        {
                            Console.WriteLine("[Validador] Se encontraron problemas (ver mensajes arriba). Se continuará igualmente.");
                        }
                    }
                    else if (string.Equals(a, "--generar-conexiones", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var mapaTxt = PathProvider.MapaTxtPath();
                            var sectoresDir = PathProvider.SectoresDir();
                            Console.WriteLine($"[INIT] Generando conexiones desde: {mapaTxt}\n       Sectores: {sectoresDir}");
                            GeneradorConexiones.Generar(mapaTxt, sectoresDir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Generación de conexiones falló: {e.Message}");
                        }
                    }
                    else if (string.Equals(a, "--validar-sectores", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var sectoresDir = PathProvider.SectoresDir();
                            Console.WriteLine($"[INIT] Validando sectores en: {sectoresDir}");
                            ValidadorSectores.ValidarSectores(sectoresDir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Validación de sectores falló: {e.Message}");
                        }
                    }
                    else if (string.Equals(a, "--normalizar-conexiones", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var sectoresDir = PathProvider.SectoresDir();
                            Console.WriteLine($"[INIT] Normalizando conexiones en: {sectoresDir}");
                            GeneradorConexiones.NormalizarBidireccionalidad(sectoresDir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Normalización de conexiones falló: {e.Message}");
                        }
                    }
                    else if (a.StartsWith("--asignar-biomas", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            int ol = 1, oc = 1; // anchos por defecto
                            if (a.StartsWith("--asignar-biomas=", StringComparison.OrdinalIgnoreCase))
                            {
                                var val = a.Substring("--asignar-biomas=".Length).Trim();
                                var partes = val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                if (partes.Length >= 1 && int.TryParse(partes[0], out int tmpOl)) ol = Math.Max(0, tmpOl);
                                if (partes.Length >= 2 && int.TryParse(partes[1], out int tmpOc)) oc = Math.Max(0, tmpOc);
                            }
                            var mapaTxt = PathProvider.MapaTxtPath();
                            var sectoresDir = PathProvider.SectoresDir();
                            Console.WriteLine($"[INIT] Asignando biomas por bandas (OL={ol}, O={oc})");
                            GeneradorBiomas.AsignarPorBandas(mapaTxt, sectoresDir, ol, oc);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Asignación de biomas falló: {e.Message}");
                        }
                    }
                    else if (a.StartsWith("--hidratar-nodos", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            int max = 5; // por defecto poblar hasta 5 nodos por sector
                            if (a.StartsWith("--hidratar-nodos=", StringComparison.OrdinalIgnoreCase))
                            {
                                var val = a.Substring("--hidratar-nodos=".Length).Trim();
                                if (int.TryParse(val, out int tmp)) max = Math.Clamp(tmp, 1, 12);
                            }
                            // Asegurar biomas cargados antes de hidratar
                            var rutaBiomas = PathProvider.CombineData("biomas.json");
                            if (File.Exists(rutaBiomas))
                            {
                                try { TablaBiomas.CargarDesdeJson(rutaBiomas); }
                                catch (Exception ex) { Console.WriteLine($"[WARN] No se pudieron cargar biomas: {ex.Message}"); }
                            }
                            var sectoresDir = PathProvider.SectoresDir();
                            Console.WriteLine($"[INIT] Hidratando nodos en: {sectoresDir} (max={max})");
                            MiJuegoRPG.Herramientas.HidratadorNodos.HidratarDesdeBiomas(sectoresDir, max);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Hidratación de nodos falló: {e.Message}");
                        }
                    }
                    else if (a.StartsWith("--reparar-materiales=", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var val = a.Substring("--reparar-materiales=".Length).Trim();
                            var partes = val.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            bool write = false; string? ruta = null;
                            if (partes.Length >= 1)
                            {
                                write = string.Equals(partes[0], "write", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (partes.Length >= 2)
                            {
                                ruta = partes[1];
                            }
                            var res = MiJuegoRPG.Herramientas.ReparadorMateriales.Reparar(write, ruta);
                            Console.WriteLine($"[ReparadorMateriales] {(write ? "WRITE" : "DRY-RUN")} done → sectores={res.SectoresEscaneados}, modificados={res.SectoresModificados}, nodos={res.NodosAfectados}, materialesEliminados={res.MaterialesEliminados}, listasNormalizadas={res.ListasNullNormalizadas}");
                            if (!string.IsNullOrWhiteSpace(res.ReportePath))
                                Console.WriteLine($"Reporte: {res.ReportePath}");
                            // Salir tras completar esta herramienta para evitar entrar al juego en modo interactivo
                            salirTrasHerramientas = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Reparación de materiales falló: {e.Message}");
                            salirTrasHerramientas = true;
                        }
                    }
                }
            }
        }
        catch { /* best-effort: flags no críticos */ }
        // Si se solicitó salir tras ejecutar herramientas, terminar aquí
        if (salirTrasHerramientas) return;
        // Mostrar estado de la bandera de precisión si está activa
        if (GameplayToggles.PrecisionCheckEnabled)
        {
            Console.WriteLine("[INFO] Chequeo de precisión ACTIVADO (--precision-hit)");
        }
        // Genera todos los archivos de regiones del mapa automáticamente al inicio
        //GeneradorSectores.CrearMapaCompleto(@"C:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa");

        // Cambia la ruta según la ubicación real de tus sectores
        //string rutaSectores = @"c:\Users\jose.cespedes\Documents\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa";
        //ValidadorSectores.ValidarSectores(rutaSectores);

        // Cargar biomas desde datos (best-effort, para recolección por bioma y herramientas)
        try
        {
            var rutaBiomas = PathProvider.CombineData("biomas.json");
            if (File.Exists(rutaBiomas))
            {
                TablaBiomas.CargarDesdeJson(rutaBiomas);
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[Biomas] Cargados tipos: {string.Join(", ", MiJuegoRPG.Motor.TablaBiomas.Biomas.Keys)}");
            }
            else
            {
                MiJuegoRPG.Motor.Servicios.Logger.Warn($"[Biomas] No se encontró biomas.json en {rutaBiomas}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Falló carga de biomas: {ex.Message}");
        }

        // Reparación opcional de sectores (solo si se pasa argumento --reparar-sectores)
        if (args != null && Array.Exists(args, a => a.Equals("--reparar-sectores", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var raiz = Juego.ObtenerRutaRaizProyecto();
                var rutaSectores = Path.Combine(raiz, "MiJuegoRPG", "DatosJuego", "mapa", "SectoresMapa");
                if (Directory.Exists(rutaSectores))
                {
                    Console.WriteLine($"[INIT] Reparando sectores en: {rutaSectores}");
                    ReparadorSectores.RepararSectores(rutaSectores);
                }
                else
                {
                    Console.WriteLine($"[WARN] Ruta de sectores no encontrada: {rutaSectores} (se omite reparación)");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Reparación de sectores falló: {e.Message}");
            }
        }

    // Aquí puedes agregar la lógica para iniciar el juego
        try
        {
            Juego juego = new Juego();
            var ui = juego.Ui;

            UIStyle.Header(ui, "Mi Juego RPG");
            UIStyle.SubHeader(ui, "Inicio");
            UIStyle.Hint(ui, "Usa --help para ver opciones de línea de comando");
            ui.WriteLine("1. Crear personaje nuevo");
            ui.WriteLine("2. Cargar personaje guardado");
            ui.WriteLine("0. Salir");
            string opcion = MiJuegoRPG.Motor.InputService.LeerOpcion("Selecciona una opción: ") ?? "1";

            switch (opcion)
            {
                case "2":
                    juego.CargarPersonaje();
                    if (juego.jugador == null)
                    {
                        ui.WriteLine("No se pudo cargar el personaje. Se creará uno nuevo.");
                        juego.CrearPersonaje();
                    }
                    break;
                case "1":
                    juego.CrearPersonaje();
                    break;
                case "0":
                    ui.WriteLine("¡Hasta pronto!");
                    return;
                default:
                    juego.CrearPersonaje();
                    break;
            }

            juego.Iniciar();

            // Preguntar si quiere guardar solo si el personaje fue creado o cargado correctamente
            if (juego.jugador != null)
            {
                var respuesta = MiJuegoRPG.Motor.InputService.LeerOpcion("\n¿Deseas guardar tu personaje? (s/n): ") ?? string.Empty;
                if (respuesta.Equals("s", StringComparison.OrdinalIgnoreCase) || respuesta.Equals("si", StringComparison.OrdinalIgnoreCase))
                {
                    juego.GuardarPersonaje();
                    ui.WriteLine("¡Personaje guardado exitosamente!");
                }
            }
            else
            {
                ui.WriteLine("No hay personaje para guardar.");
            }
            ui.WriteLine("Presiona cualquier tecla para salir...");
            MiJuegoRPG.Motor.InputService.Pausa();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el juego: {ex.Message}");
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
