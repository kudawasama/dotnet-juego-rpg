using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Herramientas;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Objetos;

internal class Program
{
    private static void Main(string[] args)
    {
        // Flags de logging: --log-off o --log-level=debug|info|warn|error|off
        bool salirTrasHerramientas = false; // permite salir tras ejecutar ciertas herramientas CLI

        // Paso 0: procesar flags de salida temprana (help, tests, benchmarks) antes de cargar nada pesado
        if (ProcesarFlagsTempranos(args))
        {
            return;
        }

        try
        {
            if (args != null && args.Length > 0)
            {
                // Mostrar ayuda y salir
                if (Array.Exists(args, a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase)))
                {
                    PrintHelp();
                    return;
                }

                foreach (var a in args)
                {
                    // Early flags ya procesados en ProcesarFlagsTempranos (shadow-benchmark / test-rareza-meta)
                    if (a.StartsWith("--shadow-benchmark", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--test-rareza-meta", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.Equals(a, "--precision-hit", StringComparison.OrdinalIgnoreCase))
                    {
                        GameplayToggles.PrecisionCheckEnabled = true;
                    }
                    else if (string.Equals(a, "--penetracion", StringComparison.OrdinalIgnoreCase))
                    {
                        GameplayToggles.PenetracionEnabled = true;
                    }
                    else if (string.Equals(a, "--combat-verbose", StringComparison.OrdinalIgnoreCase))
                    {
                        GameplayToggles.CombatVerbose = true;
                    }
                    else if (string.Equals(a, "--damage-shadow", StringComparison.OrdinalIgnoreCase))
                    {
                        var cfg = CombatConfig.LoadOrDefault();
                        cfg.UseNewDamagePipelineShadow = true;
                    }
                    else if (string.Equals(a, "--damage-live", StringComparison.OrdinalIgnoreCase))
                    {
                        var cfg = CombatConfig.LoadOrDefault();
                        cfg.UseNewDamagePipelineLive = true;

                        // Activar también shadow para telemetría comparativa residual hasta retirar legacy
                        cfg.UseNewDamagePipelineShadow = false; // evitar doble cálculo innecesario
                        Console.WriteLine("[Pipeline] MODO LIVE ACTIVADO (experimental).");
                    }
                    else if (string.Equals(a, "--log-off", StringComparison.OrdinalIgnoreCase))
                    {
                        MiJuegoRPG.Motor.Servicios.Logger.Enabled = false;
                    }
                    else if (a.StartsWith("--log-level=", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = a.Substring("--log-level=".Length).Trim().ToLowerInvariant();
                        switch (val)
                        {
                            case "debug":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Debug;
                                break;
                            case "info":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Info;
                                break;
                            case "warn":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Warn;
                                break;
                            case "error":
                                MiJuegoRPG.Motor.Servicios.Logger.Level = MiJuegoRPG.Motor.Servicios.LogLevel.Error;
                                break;
                            case "off":
                                MiJuegoRPG.Motor.Servicios.Logger.Enabled = false;
                                break;
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
                                if (partes.Length >= 1 && int.TryParse(partes[0], out int tmpOl))
                                {
                                    ol = Math.Max(0, tmpOl);
                                }

                                if (partes.Length >= 2 && int.TryParse(partes[1], out int tmpOc))
                                {
                                    oc = Math.Max(0, tmpOc);
                                }
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
                                if (int.TryParse(val, out int tmp))
                                {
                                    max = Math.Clamp(tmp, 1, 12);
                                }
                            }

                            // Asegurar biomas cargados antes de hidratar
                            var rutaBiomas = PathProvider.CombineData("biomas.json");
                            if (File.Exists(rutaBiomas))
                            {
                                try
                                {
                                    TablaBiomas.CargarDesdeJson(rutaBiomas);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[WARN] No se pudieron cargar biomas: {ex.Message}");
                                }
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
                            bool write = false;
                            string? ruta = null;
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
                            {
                                Console.WriteLine($"Reporte: {res.ReportePath}");
                            }

                            // Salir tras completar esta herramienta para evitar entrar al juego en modo interactivo
                            salirTrasHerramientas = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Reparación de materiales falló: {e.Message}");
                            salirTrasHerramientas = true;
                        }
                    }
                    else if (a.StartsWith("--migrar-equipo=", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var mode = a.Substring("--migrar-equipo=".Length).Trim();
                            bool write = string.Equals(mode, "write", StringComparison.OrdinalIgnoreCase);
                            MiJuegoRPG.Herramientas.MigradorEquipoPerItem.Migrar(write);
                            salirTrasHerramientas = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[ERROR] Migración de equipo falló: {e.Message}");
                            salirTrasHerramientas = true;
                        }

                        continue;
                    }
                }
            }
        }
        catch
        {
            // best-effort: flags no críticos
        }

        // Si se solicitó salir tras ejecutar herramientas, terminar aquí
        if (salirTrasHerramientas)
        {
            return;
        }

        // Mostrar estado de la bandera de precisión si está activa
        if (GameplayToggles.PrecisionCheckEnabled)
        {
            Console.WriteLine("[INFO] Chequeo de precisión ACTIVADO (--precision-hit)");
        }

        // Mostrar estado de penetración si está activo
        if (GameplayToggles.PenetracionEnabled)
        {
            Console.WriteLine("[INFO] Penetración ACTIVADA (--penetracion)");
        }

        // Mostrar estado de verbosidad de combate si está activo
        if (GameplayToggles.CombatVerbose)
        {
            Console.WriteLine("[INFO] Verbosidad de Combate ACTIVADA (--combat-verbose)");
        }

        // Genera todos los archivos de regiones del mapa automáticamente al inicio
        // GeneradorSectores.CrearMapaCompleto(@"C:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa");

        // Cambia la ruta según la ubicación real de tus sectores
        // string rutaSectores = @"c:\Users\jose.cespedes\Documents\GitHub\dotnet-juego-rpg\MiJuegoRPG\DatosJuego\mapa\SectoresMapa";
        // ValidadorSectores.ValidarSectores(rutaSectores);

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
                    if (juego.Jugador == null)
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
            if (juego.Jugador != null)
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

        // Al terminar la ejecución interactiva, si el pipeline shadow estuvo activo (y no live) mostrar resumen agregado.
        var cfgFinal = CombatConfig.LoadOrDefault();
        if (cfgFinal.UseNewDamagePipelineShadow && !cfgFinal.UseNewDamagePipelineLive)
        {
            Console.WriteLine(MiJuegoRPG.Motor.Servicios.DamageResolver.ObtenerResumenShadow(reset: true));
        }
    }

    // Hacer privada para cumplir el orden de miembros (públicos antes que privados)
    private static void InicializarRarezasSiNecesario()
    {
        if (RarezaConfig.Instancia != null)
        {
            return;
        }

        try
        {
            var cfg = new RarezaConfig();
            var rutaPesos = PathProvider.ConfigPath("rareza_pesos.json");
            var rutaPerf = PathProvider.ConfigPath("rareza_perfeccion.json");
            if (File.Exists(rutaPesos) && File.Exists(rutaPerf))
            {
                cfg.Cargar(rutaPesos, rutaPerf);
                RarezaConfig.SetInstancia(cfg);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] No se pudieron cargar rarezas dinámicas: {ex.Message}");
        }
    }

    private static bool ProcesarFlagsTempranos(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            return false;
        }

        foreach (var a in args)
        {
            if (string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase))
            {
                // Dejar que la lógica normal imprima help (no early return aquí para no duplicar)
                return false;
            }

            if (a.StartsWith("--shadow-sweep", StringComparison.OrdinalIgnoreCase))
            {
                int n = 300;
                if (a.StartsWith("--shadow-sweep=", StringComparison.OrdinalIgnoreCase))
                {
                    var val = a.Substring("--shadow-sweep=".Length).Trim();
                    if (int.TryParse(val, out var parsed))
                    {
                        n = Math.Clamp(parsed, 50, 5000);
                    }
                }

                Console.WriteLine("[Sweep] Iniciando sweep shadow (F in {0.60,0.65,0.70} x PenCrit in {0.75,0.80})...");
                MiJuegoRPG.Motor.TestShadowBenchmark.Sweep(
                    n,
                    100,
                    50,
                    0.10,
                    0.20,
                    0.40,
                    CombatConfig.LoadOrDefault().CritMultiplier,
                    new double[] { 0.60, 0.65, 0.70 },
                    new double[] { 0.75, 0.80 });
                Console.WriteLine("[Sweep] Fin.");
                return true;
            }

            if (a.StartsWith("--shadow-benchmark", StringComparison.OrdinalIgnoreCase))
            {
                int n = 100;
                if (a.StartsWith("--shadow-benchmark=", StringComparison.OrdinalIgnoreCase))
                {
                    var val = a.Substring("--shadow-benchmark=".Length).Trim();
                    if (int.TryParse(val, out var parsed))
                    {
                        n = Math.Clamp(parsed, 10, 10000);
                    }
                }

                Console.WriteLine("[Benchmark] Iniciando benchmark shadow...");
                MiJuegoRPG.Motor.TestShadowBenchmark.Run(n);
                Console.WriteLine("[Benchmark] Fin.");
                return true;
            }

            if (string.Equals(a, "--test-rareza-meta", StringComparison.OrdinalIgnoreCase))
            {
                InicializarRarezasSiNecesario();
                MiJuegoRPG.Motor.TestRarezaMeta.Probar();
                return true;
            }

            if (string.Equals(a, "--smoke-combate", StringComparison.OrdinalIgnoreCase))
            {
                // Ejecuta un recorrido mínimo de combate y sale sin entrar a UI
                var code = MiJuegoRPG.Motor.Servicios.SmokeRunner.RunCombateSmoke();

                // Devuelve true para cortar la ejecución normal
                return true;
            }
        }

        return false;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Opciones CLI disponibles:\n");
        Console.WriteLine("  --help | -h                Muestra esta ayuda.");
        Console.WriteLine("  --precision-hit            Activa chequeo de precisión en ataques físicos.");
        Console.WriteLine("  --penetracion              Activa penetración (reduce defensa efectiva).");
        Console.WriteLine("  --combat-verbose           Muestra desglose detallado del cálculo de daño.");
        Console.WriteLine("  --damage-shadow            Ejecuta DamagePipeline en modo sombra (comparativo).");
        Console.WriteLine("  --damage-live              (Experimental) Reemplaza cálculo legacy por nuevo pipeline.");
        Console.WriteLine("  --shadow-benchmark[=N]     Ejecuta benchmark sintético (N por defecto 100) y sale.");
        Console.WriteLine("  --shadow-sweep[=N]         Ejecuta barrido tuning (F x PenCrit) N muestras por combinación y sale.");
        Console.WriteLine("  --test-rareza-meta         Ejecuta pruebas manuales de RarezaMeta y sale.");
        Console.WriteLine("  --smoke-combate            Recorre crear PJ básico -> 1 encuentro simple -> 1 ataque y sale.");
        Console.WriteLine("  --log-off                  Desactiva logger al inicio.");
        Console.WriteLine("  --log-level=debug|info|warn|error|off  Ajusta nivel de log.");
        Console.WriteLine("  --validar-datos[=report|ruta]  Valida referencias básicas (opcional genera reporte).");
        Console.WriteLine("  --generar-conexiones       Genera conexiones a partir de mapa base.");
        Console.WriteLine("  --validar-sectores         Valida consistencia de sectores.");
        Console.WriteLine("  --normalizar-conexiones    Fuerza bidireccionalidad de conexiones.");
        Console.WriteLine("  --asignar-biomas[=OL,OC]   Asigna biomas por bandas (overlap lineal y columnas).");
        Console.WriteLine("  --hidratar-nodos[=N]       Pobla nodos de recolección (default 5, máx 12).");
        Console.WriteLine("  --reparar-materiales=report[;ruta]  Reporta materiales inválidos.");
        Console.WriteLine("  --reparar-materiales=write[;ruta]   Repara eliminando materiales inválidos.");
        Console.WriteLine("  --migrar-equipo=report|write       Divide JSON agregados de equipo en uno por ítem.");
        Console.WriteLine();
        Console.WriteLine("Notas:");
        Console.WriteLine("- Los flags de benchmark y test salen antes de iniciar UI.");
        Console.WriteLine("- El modo sombra permite comparar daño legacy vs pipeline sin alterar gameplay.");
    }
}
