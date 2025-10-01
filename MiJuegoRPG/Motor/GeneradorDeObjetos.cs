using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor
{
    public static class GeneradorObjetos
    {
    // Repositorio de armas (lazy) migrando carga jerárquica/overlay
    private static readonly Lazy<MiJuegoRPG.Motor.Servicios.Repos.ArmaRepository> _armaRepoLazy
        = new(() => new MiJuegoRPG.Motor.Servicios.Repos.ArmaRepository());
    private static List<ArmaData>? armasDisponibles;
    private static List<ArmaduraData>? armadurasDisponibles;
    private static readonly Lazy<MiJuegoRPG.Motor.Servicios.Repos.ArmaduraRepository> _armaduraRepoLazy
        = new(() => new MiJuegoRPG.Motor.Servicios.Repos.ArmaduraRepository());
    private static List<AccesorioData>? accesoriosDisponibles;
    private static List<BotasData>? botasDisponibles;
    private static List<CascoData>? cascosDisponibles;
    private static List<CinturonData>? cinturonesDisponibles;
    private static List<CollarData>? collaresDisponibles;
    private static List<PantalonData>? pantalonesDisponibles;

        /// <summary>
        /// Cuando está activo, la selección aleatoria de ítems usa ponderación por Rareza
        /// (ítems comunes salen mucho más que raros/legendarios). Por defecto ON.
        /// </summary>
        public static bool UsaSeleccionPonderadaRareza { get; set; } = true;

        // Configuración dinámica de rarezas (fuente: JSON)
        private static MiJuegoRPG.Objetos.RarezaConfig? rarezaConfig;

        /// <summary>
        /// Carga todos los ítems desde carpetas por tipo bajo DatosJuego/Equipo, si existen.
        /// Fallback: si no hay carpetas/tipos, intenta cargar desde los JSON agregados existentes.
        /// Tipos soportados: armas, armaduras, accesorios, botas, cinturones, collares, pantalones, mochilas.
        /// </summary>
        public static void CargarEquipoAuto()
        {
            try
            {
                string baseDir = MiJuegoRPG.Motor.Servicios.PathProvider.CombineData("Equipo");
                string configDir = MiJuegoRPG.Motor.Servicios.PathProvider.ConfigPath("");
                // Inicializar configuración de rarezas
                rarezaConfig = new MiJuegoRPG.Objetos.RarezaConfig();
                rarezaConfig.Cargar(
                    Path.Combine(configDir, "rareza_pesos.json"),
                    Path.Combine(configDir, "rareza_perfeccion.json")
                );

                bool hayCarpetas = Directory.Exists(baseDir) && Directory.GetDirectories(baseDir).Length > 0;

                if (hayCarpetas)
                {
                    armasDisponibles = CargarListaDesdeCarpeta<ArmaData>(Path.Combine(baseDir, "armas"));
                    // Nuevo: cargar armaduras desde repositorio jerárquico (base + overlay)
                    try
                    {
                        var repoArm = _armaduraRepoLazy.Value; // asegura carga
                        var todasArm = repoArm.Todas();
                        if (todasArm != null && todasArm.Count > 0)
                        {
                            armadurasDisponibles = new List<ArmaduraData>(todasArm);
                        }
                    }
                    catch (Exception exArm)
                    {
                        Console.WriteLine($"[Equipo] Error cargando armaduras repo: {exArm.Message}");
                    }
                    // Nuevo: cargar botas desde repositorio jerárquico (base + overlay)
                    try
                    {
                        var repoBotas = new MiJuegoRPG.Motor.Servicios.Repos.BotasRepository();
                        var todasBotas = repoBotas.Todas();
                        if (todasBotas != null && todasBotas.Count > 0)
                        {
                            botasDisponibles = new List<BotasData>(todasBotas);
                        }
                    }
                    catch (Exception exBotas)
                    {
                        Console.WriteLine($"[Equipo] Error cargando botas repo: {exBotas.Message}");
                    }
                    // Nuevo: cargar cascos desde repositorio jerárquico (base + overlay)
                    try
                    {
                        var repoCascos = new MiJuegoRPG.Motor.Servicios.Repos.CascosRepository();
                        var todosCascos = repoCascos.Todas();
                        if (todosCascos != null && todosCascos.Count > 0)
                        {
                            cascosDisponibles = new List<CascoData>(todosCascos);
                        }
                    }
                    catch (Exception exCascos)
                    {
                        Console.WriteLine($"[Equipo] Error cargando cascos repo: {exCascos.Message}");
                    }
                    // Nuevo: cargar cinturones
                    try
                    {
                        var repoCint = new MiJuegoRPG.Motor.Servicios.Repos.CinturonesRepository();
                        var todosCint = repoCint.Todas();
                        if (todosCint != null && todosCint.Count > 0)
                            cinturonesDisponibles = new List<CinturonData>(todosCint);
                    }
                    catch (Exception exCint)
                    {
                        Console.WriteLine($"[Equipo] Error cargando cinturones repo: {exCint.Message}");
                    }
                    // Nuevo: cargar collares
                    try
                    {
                        var repoCollares = new MiJuegoRPG.Motor.Servicios.Repos.CollaresRepository();
                        var todosColl = repoCollares.Todas();
                        if (todosColl != null && todosColl.Count > 0)
                            collaresDisponibles = new List<CollarData>(todosColl);
                    }
                    catch (Exception exColl)
                    {
                        Console.WriteLine($"[Equipo] Error cargando collares repo: {exColl.Message}");
                    }
                    // Nuevo: cargar pantalones
                    try
                    {
                        var repoPants = new MiJuegoRPG.Motor.Servicios.Repos.PantalonesRepository();
                        var todosPant = repoPants.Todas();
                        if (todosPant != null && todosPant.Count > 0)
                            pantalonesDisponibles = new List<PantalonData>(todosPant);
                    }
                    catch (Exception exPant)
                    {
                        Console.WriteLine($"[Equipo] Error cargando pantalones repo: {exPant.Message}");
                    }
                    // ...existing code...
                }
                else
                {
                    var baseEquipo = MiJuegoRPG.Motor.Servicios.PathProvider.CombineData("Equipo");
                    CargarArmas(Path.Combine(baseEquipo, "armas.json"));
                    // ...existing code...
                }

                // Validar rarezas de armas
                if (armasDisponibles != null && rarezaConfig != null)
                {
                    foreach (var arma in armasDisponibles)
                    {
                        if (!rarezaConfig.RarezaValida(arma.Rareza))
                        {
                            Console.WriteLine($"[Equipo][ADVERTENCIA] Rareza no reconocida en arma '{arma.Nombre}': '{arma.Rareza}'");
                        }
                    }
                }

                Console.WriteLine($"[Equipo] Armas:{armasDisponibles?.Count ?? 0} Armaduras:{armadurasDisponibles?.Count ?? 0} Accesorios:{accesoriosDisponibles?.Count ?? 0} Botas:{botasDisponibles?.Count ?? 0} Cascos:{cascosDisponibles?.Count ?? 0} Cinturones:{cinturonesDisponibles?.Count ?? 0} Collares:{collaresDisponibles?.Count ?? 0} Pantalones:{pantalonesDisponibles?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Equipo] Error en carga automática: {ex.Message}");
            }
        }

        /// <summary>
        /// Explora una carpeta (recursivo) y carga una lista de T. Cada archivo puede ser
        /// una lista JSON o un objeto único JSON. Archivos inválidos se ignoran con log.
        /// </summary>
        private static List<T> CargarListaDesdeCarpeta<T>(string dir)
        {
            var lista = new List<T>();
            try
            {
                if (!Directory.Exists(dir)) return lista;
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(file));
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in doc.RootElement.EnumerateArray())
                            {
                                try
                                {
                                    var item = JsonSerializer.Deserialize<T>(el.GetRawText(), opts);
                                    if (item != null) lista.Add(item);
                                }
                                catch (Exception exItem)
                                {
                                    Console.WriteLine($"[Equipo] Ignorando elemento en '{file}': {exItem.Message}");
                                }
                            }
                        }
                        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            try
                            {
                                var uno = JsonSerializer.Deserialize<T>(doc.RootElement.GetRawText(), opts);
                                if (uno != null) lista.Add(uno);
                            }
                            catch (Exception exObj)
                            {
                                Console.WriteLine($"[Equipo] Ignorando '{file}': {exObj.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Equipo] Ignorando '{file}': raíz JSON no es objeto ni lista");
                        }
                    }
                    catch (Exception exFile)
                    {
                        Console.WriteLine($"[Equipo] Ignorando '{file}': {exFile.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Equipo] Error leyendo carpeta '{dir}': {ex.Message}");
            }
            return lista;
        }

        private static List<AccesorioData> CargarAccesoriosDesdeCarpetaTolerante(string dir)
        {
            var lista = new List<AccesorioData>();
            try
            {
                if (!Directory.Exists(dir)) return lista;
                var porNombre = new Dictionary<string, AccesorioData>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(file));
                        if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var el in doc.RootElement.EnumerateArray())
                            {
                                var acc = ParseAccesorioDesdeJson(el);
                                if (acc == null) continue;
                                if (!porNombre.ContainsKey(acc.Nombre)) porNombre[acc.Nombre] = acc; // evitar duplicados
                            }
                        }
                        else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            var acc = ParseAccesorioDesdeJson(doc.RootElement);
                            if (acc != null && !porNombre.ContainsKey(acc.Nombre)) porNombre[acc.Nombre] = acc;
                        }
                    }
                    catch (Exception exFile)
                    {
                        Console.WriteLine($"[Equipo] Ignorando '{file}': {exFile.Message}");
                    }
                }
                lista.AddRange(porNombre.Values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Equipo] Error leyendo carpeta '{dir}': {ex.Message}");
            }
            return lista;
        }

        private static AccesorioData? ParseAccesorioDesdeJson(JsonElement el)
        {
            try
            {
                string nombre = GetString(el, "Nombre") ?? "sin_nombre";
                var acc = new AccesorioData
                {
                    Nombre = nombre,
                    BonificacionAtaque = GetInt(el, "BonificacionAtaque", 0),
                    BonificacionDefensa = GetInt(el, "BonificacionDefensa", 0),
                    Nivel = GetInt(el, "Nivel", 1),
                    TipoObjeto = GetString(el, "TipoObjeto") ?? "Accesorio",
                    Rareza = GetString(el, "Rareza") ?? "Comun",
                    Perfeccion = GetInt(el, "Perfeccion", 50),
                    Descripcion = GetString(el, "Descripcion")
                };

                // Nivel rango "X - Y"
                var nivelStr = GetString(el, "Nivel");
                if (!string.IsNullOrWhiteSpace(nivelStr) && TryParseRango(nivelStr!, out var nmin, out var nmax))
                {
                    acc.NivelMin = Math.Max(1, nmin);
                    acc.NivelMax = Math.Max(acc.NivelMin.Value, nmax);
                }

                // Perfeccion rango "X - Y"
                var perfStr = GetString(el, "Perfeccion");
                if (!string.IsNullOrWhiteSpace(perfStr) && TryParseRango(perfStr!, out var pmin, out var pmax))
                {
                    acc.PerfeccionMin = Math.Clamp(pmin, 0, 100);
                    acc.PerfeccionMax = Math.Clamp(pmax, 0, 100);
                    acc.Perfeccion = Math.Clamp(acc.Perfeccion, 0, 100);
                }

                // Rareza CSV en Rareza
                var rareza = GetString(el, "Rareza");
                if (!string.IsNullOrWhiteSpace(rareza) && rareza!.Contains(','))
                {
                    acc.RarezasPermitidasCsv = rareza;
                    // Mantener Rareza con una opción razonable (por ejemplo Normal)
                    acc.Rareza = "Normal";
                }
                return acc;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryParseRango(string s, out int min, out int max)
        {
            min = max = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var parts = s.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            if (int.TryParse(parts[0], out var a) && int.TryParse(parts[1], out var b))
            {
                min = Math.Min(a, b);
                max = Math.Max(a, b);
                return true;
            }
            return false;
        }

        private static string? GetString(JsonElement el, string name)
        {
            if (el.ValueKind != JsonValueKind.Object) return null;
            if (!el.TryGetProperty(name, out var prop)) return null;
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.TryGetInt32(out var i) ? i.ToString() : prop.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        private static int GetInt(JsonElement el, string name, int def)
        {
            if (el.ValueKind != JsonValueKind.Object) return def;
            if (!el.TryGetProperty(name, out var prop)) return def;
            if (prop.ValueKind == JsonValueKind.Number)
            {
                if (prop.TryGetInt32(out var i)) return i;
                try { return (int)Math.Round(prop.GetDouble()); } catch { return def; }
            }
            if (prop.ValueKind == JsonValueKind.String)
            {
                var s = prop.GetString();
                if (int.TryParse(s, out var i)) return i;
                if (double.TryParse(s, out var d)) return (int)Math.Round(d);
            }
            return def;
        }

        /// <summary>
        /// Selección aleatoria (uniforme o ponderada por rareza según flag, usando rarezaConfig).
        /// </summary>
        private static T ElegirAleatorio<T>(IReadOnlyList<T> lista, Func<T, string> rarezaSelector)
        {
            var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            if (!UsaSeleccionPonderadaRareza || rarezaConfig == null)
            {
                return lista[rand.Next(lista.Count)];
            }

            // Construir pesos desde rarezaConfig
            double totalPeso = 0;
            var acumulados = new double[lista.Count];
            for (int i = 0; i < lista.Count; i++)
            {
                var rz = rarezaSelector(lista[i]);
                rarezaConfig.Pesos.TryGetValue(rz, out var peso);
                if (peso <= 0) peso = 1; // seguridad
                totalPeso += peso;
                acumulados[i] = totalPeso;
            }
            double tiro = rand.NextDouble() * totalPeso;
            for (int i = 0; i < acumulados.Length; i++)
            {
                if (tiro < acumulados[i]) return lista[i];
            }
            return lista[^1];
        }
        public static void CargarBotas(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                botasDisponibles = JsonSerializer.Deserialize<List<BotasData>>(jsonString);
                Console.WriteLine($"Se cargaron {botasDisponibles?.Count ?? 0} botas del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar botas: {ex.Message}");
                botasDisponibles = new List<BotasData>();
            }
        }

        public static void CargarCinturones(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                cinturonesDisponibles = JsonSerializer.Deserialize<List<CinturonData>>(jsonString);
                Console.WriteLine($"Se cargaron {cinturonesDisponibles?.Count ?? 0} cinturones del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar cinturones: {ex.Message}");
                cinturonesDisponibles = new List<CinturonData>();
            }
        }

        public static void CargarCascos(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                cascosDisponibles = JsonSerializer.Deserialize<List<CascoData>>(jsonString);
                Console.WriteLine($"Se cargaron {cascosDisponibles?.Count ?? 0} cascos del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar cascos: {ex.Message}");
                cascosDisponibles = new List<CascoData>();
            }
        }

        public static void CargarCollares(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                collaresDisponibles = JsonSerializer.Deserialize<List<CollarData>>(jsonString);
                Console.WriteLine($"Se cargaron {collaresDisponibles?.Count ?? 0} collares del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar collares: {ex.Message}");
                collaresDisponibles = new List<CollarData>();
            }
        }

        public static void CargarPantalones(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                pantalonesDisponibles = JsonSerializer.Deserialize<List<PantalonData>>(jsonString);
                Console.WriteLine($"Se cargaron {pantalonesDisponibles?.Count ?? 0} pantalones del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar pantalones: {ex.Message}");
                pantalonesDisponibles = new List<PantalonData>();
            }
        }
        public static Botas GenerarBotasAleatorias(int nivelJugador)
        {
            if (botasDisponibles != null && botasDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = botasDisponibles[rand.Next(botasDisponibles.Count)];

                // Rarezas permitidas (string, soporte dinámico)
                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                // Perfección
                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            // Sin config de rarezas: respetar rango declarado en el ítem
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                // Nivel
                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                // Defensa
                int defensaBase = baseData.Defensa;
                if (baseData.DefensaMin.HasValue && baseData.DefensaMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.DefensaMin.Value);
                    int dmax = Math.Max(dmin, baseData.DefensaMax.Value);
                    defensaBase = rand.Next(dmin, dmax + 1);
                }
                int defensaFinal = (int)Math.Round(defensaBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var botas = new Botas(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                botas.SetId = baseData.SetId;
                // Habilidades otorgadas (opcional)
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    botas.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            botas.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return botas;
            }
            else
            {
                throw new InvalidOperationException("No hay botas disponibles para generar.");
            }
        }

        public static Cinturon GenerarCinturonAleatorio(int nivelJugador)
        {
            if (cinturonesDisponibles != null && cinturonesDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = cinturonesDisponibles[rand.Next(cinturonesDisponibles.Count)];

                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                int cargaBase = baseData.BonificacionCarga;
                if (baseData.BonificacionCargaMin.HasValue && baseData.BonificacionCargaMax.HasValue)
                {
                    int cmin = Math.Max(0, baseData.BonificacionCargaMin.Value);
                    int cmax = Math.Max(cmin, baseData.BonificacionCargaMax.Value);
                    cargaBase = rand.Next(cmin, cmax + 1);
                }
                int bonifCarga = (int)Math.Round(cargaBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var cinturon = new Cinturon(baseData.Nombre, bonifCarga, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                cinturon.SetId = baseData.SetId;
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    cinturon.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            cinturon.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return cinturon;
            }
            else
            {
                throw new InvalidOperationException("No hay cinturones disponibles para generar.");
            }
        }

        public static Collar GenerarCollarAleatorio(int nivelJugador)
        {
            if (collaresDisponibles != null && collaresDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = collaresDisponibles[rand.Next(collaresDisponibles.Count)];

                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                int defBase = baseData.BonificacionDefensa;
                if (baseData.BonificacionDefensaMin.HasValue && baseData.BonificacionDefensaMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.BonificacionDefensaMin.Value);
                    int dmax = Math.Max(dmin, baseData.BonificacionDefensaMax.Value);
                    defBase = rand.Next(dmin, dmax + 1);
                }
                int eneBase = baseData.BonificacionEnergia;
                if (baseData.BonificacionEnergiaMin.HasValue && baseData.BonificacionEnergiaMax.HasValue)
                {
                    int emin = Math.Max(0, baseData.BonificacionEnergiaMin.Value);
                    int emax = Math.Max(emin, baseData.BonificacionEnergiaMax.Value);
                    eneBase = rand.Next(emin, emax + 1);
                }
                int bonifDefensa = (int)Math.Round(defBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                int bonifEnergia = (int)Math.Round(eneBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var collar = new Collar(baseData.Nombre, bonifDefensa, bonifEnergia, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                collar.SetId = baseData.SetId;
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    collar.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            collar.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return collar;
            }
            else
            {
                throw new InvalidOperationException("No hay collares disponibles para generar.");
            }
        }

        public static Pantalon GenerarPantalonAleatorio(int nivelJugador)
        {
            if (pantalonesDisponibles != null && pantalonesDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = pantalonesDisponibles[rand.Next(pantalonesDisponibles.Count)];

                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                int defensaBase = baseData.Defensa;
                if (baseData.DefensaMin.HasValue && baseData.DefensaMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.DefensaMin.Value);
                    int dmax = Math.Max(dmin, baseData.DefensaMax.Value);
                    defensaBase = rand.Next(dmin, dmax + 1);
                }
                int defensaFinal = (int)Math.Round(defensaBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var pantalon = new Pantalon(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                pantalon.SetId = baseData.SetId;
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    pantalon.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            pantalon.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return pantalon;
            }
            else
            {
                throw new InvalidOperationException("No hay pantalones disponibles para generar.");
            }
        }

        public static Casco GenerarCascoAleatorio(int nivelJugador)
        {
            if (cascosDisponibles != null && cascosDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = cascosDisponibles[rand.Next(cascosDisponibles.Count)];

                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                int defensaBase = baseData.Defensa;
                if (baseData.DefensaMin.HasValue && baseData.DefensaMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.DefensaMin.Value);
                    int dmax = Math.Max(dmin, baseData.DefensaMax.Value);
                    defensaBase = rand.Next(dmin, dmax + 1);
                }
                int defensaFinal = (int)Math.Round(defensaBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var casco = new MiJuegoRPG.Objetos.Casco(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                casco.SetId = baseData.SetId;
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    casco.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            casco.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return casco;
            }
            else
            {
                throw new InvalidOperationException("No hay cascos disponibles para generar.");
            }
        }
    public static void CargarArmaduras(string rutaArchivo)
        {
            // Legacy: mantener por compatibilidad puntual / herramientas QA.
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                armadurasDisponibles = JsonSerializer.Deserialize<List<ArmaduraData>>(jsonString) ?? new();
                Console.WriteLine($"[Legacy] Cargadas {armadurasDisponibles.Count} armaduras desde '{rutaArchivo}'. Considerar migración a ArmaduraRepository.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Legacy] Error al cargar armaduras: {ex.Message}");
                armadurasDisponibles = new List<ArmaduraData>();
            }
        }

        public static void CargarAccesorios(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                accesoriosDisponibles = JsonSerializer.Deserialize<List<AccesorioData>>(jsonString);
                Console.WriteLine($"Se cargaron {accesoriosDisponibles?.Count ?? 0} accesorios del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar accesorios: {ex.Message}");
                accesoriosDisponibles = new List<AccesorioData>();
            }
        }
        public static Armadura GenerarArmaduraAleatoria(int nivelJugador)
        {
            // Intentar usar ArmaduraRepository primero
            List<ArmaduraData>? fuente = null;
            try
            {
                var repo = _armaduraRepoLazy.Value;
                var todas = repo.Todas();
                if (todas != null && todas.Count > 0)
                    fuente = new List<ArmaduraData>(todas);
            }
            catch { /* ignorar y fallback */ }

            if (fuente == null || fuente.Count == 0)
            {
                if (armadurasDisponibles == null || armadurasDisponibles.Count == 0)
                    throw new InvalidOperationException("No hay armaduras disponibles para generar (repositorio y legacy vacíos).");
                fuente = armadurasDisponibles;
            }

            if (fuente.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                var baseData = fuente[rand.Next(fuente.Count)];

                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) permitidas.Add(s); else permitidas.Add("Normal");
                }
                var rzElegida = ElegirRarezaPonderada(permitidas);

                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida);
                        }
                    }
                }
                int perfeccion = (baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                    ? rand.Next(pMin, pMax + 1)
                    : baseData.Perfeccion;

                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                int defensaBase = baseData.Defensa;
                if (baseData.DefensaMin.HasValue && baseData.DefensaMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.DefensaMin.Value);
                    int dmax = Math.Max(dmin, baseData.DefensaMax.Value);
                    defensaBase = rand.Next(dmin, dmax + 1);
                }
                int defensaFinal = (int)Math.Round(defensaBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var armadura = new Armadura(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                armadura.SetId = baseData.SetId;
                if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
                {
                    armadura.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                    foreach (var h in baseData.HabilidadesOtorgadas)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Id))
                            armadura.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
                return armadura;
            }
            else
            {
                throw new InvalidOperationException("No hay armaduras disponibles para generar.");
            }
        }

        public static Accesorio GenerarAccesorioAleatorio(int nivelJugador)
        {
            if (accesoriosDisponibles != null && accesoriosDisponibles.Count > 0)
            {
                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                // Elegir base de datos de forma uniforme primero (la rareza real se decidirá luego)
                var baseData = accesoriosDisponibles[rand.Next(accesoriosDisponibles.Count)];

                // 1) Determinar rarezas permitidas por este ítem (si hay lista CSV), si no, permitir todas
                List<string> permitidas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = r.Trim();
                        s = NormalizarRarezaTexto(s);
                        if (!string.IsNullOrWhiteSpace(s))
                            permitidas.Add(s);
                    }
                }
                if (permitidas.Count == 0)
                {
                    // Si no hay CSV, permitir la del propio item y, si esa es una cadena con coma, parsearla también
                    var s = baseData.Rareza ?? "Comun";
                    if (s.Contains(','))
                    {
                        foreach (var t in s.Split(','))
                        {
                            var txt = NormalizarRarezaTexto(t.Trim());
                            if (!string.IsNullOrWhiteSpace(txt))
                                permitidas.Add(txt);
                        }
                    }
                    else
                    {
                        var unico = NormalizarRarezaTexto(s);
                        if (!string.IsNullOrWhiteSpace(unico)) permitidas.Add(unico); else permitidas.Add("Normal");
                    }
                }

                // 2) Elegir rareza con ponderación global, filtrada a las permitidas
                var rzElegida = ElegirRarezaPonderada(permitidas);

                // 3) Determinar rango de perfección según rareza (intersectar con rango opcional del ítem)
                (int pMin, int pMax) = RangoPerfeccionPorRareza(rzElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, baseData.PerfeccionMin.Value);
                    pMax = Math.Min(pMax, baseData.PerfeccionMax.Value);
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rzElegida); // fallback si la intersección es vacía
                        }
                    }
                }
                int perfeccion = rand.Next(pMin, pMax + 1);

                // 4) Determinar nivel por rango si existe
                int nivel = baseData.Nivel;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                // 5) Aplicar perfección: interpretamos los valores base como máximos a 100%
                // Normal (50%) se considera base -> factor perfeccion/50.0
                int bonifAtaque = (int)Math.Round(baseData.BonificacionAtaque * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                int bonifDefensa = (int)Math.Round(baseData.BonificacionDefensa * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                var accesorio = new Accesorio(baseData.Nombre, bonifAtaque, bonifDefensa, nivel, rzElegida, baseData.TipoObjeto, perfeccion);
                accesorio.SetId = baseData.SetId;
                return accesorio;
            }
            else
            {
                throw new InvalidOperationException("No hay accesorios disponibles para generar.");
            }
        }

        private static string NormalizarRarezaTexto(string s)
        {
            s = s.Replace("ó", "o").Replace("Ó", "O").Replace("ú", "u").Replace("Ú", "U").Replace("á", "a").Replace("Á", "A").Replace("é", "e").Replace("É", "E").Replace("í", "i").Replace("Í", "I");
            if (string.Equals(s, "Comun", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "Común", StringComparison.OrdinalIgnoreCase)) return "Normal"; // mapeo a enum existente
            if (string.Equals(s, "Raro", StringComparison.OrdinalIgnoreCase)) return "Rara";
            return s;
        }

        private static (int min, int max) RangoPerfeccionPorRareza(string rz)
        {
            if (rarezaConfig != null && rarezaConfig.RangosPerfeccion.TryGetValue(rz, out var r)) return r;
            return (50, 50);
        }

        private static string ElegirRarezaPonderada(List<string> candidatas)
        {
            // Fallback robusto: si no hay candidatas devolver "Normal" (seguridad)
            if (candidatas == null || candidatas.Count == 0) return "Normal";

            // Si la configuración dinámica de rarezas aún no está cargada (p.ej. en tests unitarios),
            // no forzamos "Normal" porque distorsiona la intención de RarezasPermitidasCsv.
            // Devolvemos determinísticamente la única candidata, o elegimos uniforme entre varias.
            if (rarezaConfig == null)
            {
                if (candidatas.Count == 1) return candidatas[0];
                var randLocal = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
                return candidatas[randLocal.Next(candidatas.Count)];
            }

            var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            double total = 0;
            var acumulados = new List<(string rz, double acum)>(candidatas.Count);
            for (int i = 0; i < candidatas.Count; i++)
            {
                var r = candidatas[i];
                rarezaConfig.Pesos.TryGetValue(r, out var peso);
                if (peso <= 0) peso = 1;
                total += peso;
                acumulados.Add((r, total));
            }
            double tiro = rand.NextDouble() * total;
            for (int i = 0; i < acumulados.Count; i++)
            {
                if (tiro < acumulados[i].acum) return acumulados[i].rz;
            }
            return candidatas[^1];
        }

        // Obsoleto: la carga de pesos ahora la realiza RarezaConfig (rarezaConfig.Pesos)
        private static void TryCargarPesosRareza(string baseDir) { }

        private class PesoRarezaEntry { public string? Nombre { get; set; } public int Peso { get; set; } }

        private class RangoRarezaEntry { public string? Nombre { get; set; } public int Min { get; set; } public int Max { get; set; } }

        // Obsoleto: rangos gestionados por RarezaConfig
        private static void TryCargarRangosPerfeccionPorRareza(string baseDir) { }

        public static void CargarArmas(string rutaArchivo)
        {
            // Mantener compatibilidad: carga manual legacy sólo si se fuerza.
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                armasDisponibles = JsonSerializer.Deserialize<List<ArmaData>>(jsonString) ?? new();
                Console.WriteLine($"[Legacy] Cargadas {armasDisponibles.Count} armas desde '{rutaArchivo}'. Considerar migración a ArmaRepository.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Legacy] Error al cargar armas: {ex.Message}");
                armasDisponibles = new List<ArmaData>();
            }
        }

        public static Arma GenerarArmaAleatoria(int nivelJugador)
        {
            // Intentar usar ArmaRepository primero
            List<ArmaData>? fuente = null;
            try
            {
                var repo = _armaRepoLazy.Value; // lazy init
                var todas = repo.Todas();
                if (todas != null && todas.Count > 0)
                {
                    fuente = new List<ArmaData>(todas);
                }
            }
            catch
            {
                // ignorar y caer a legacy
            }

            if (fuente == null || fuente.Count == 0)
            {
                // fallback legacy
                if (armasDisponibles == null || armasDisponibles.Count == 0)
                    throw new InvalidOperationException("No hay armas disponibles para generar (repositorio y legacy vacíos).");
                fuente = armasDisponibles;
            }

            if (fuente.Count > 0)
            {
                var baseData = ElegirAleatorio<ArmaData>(fuente, ad => NormalizarRarezaTexto(ad.Rareza ?? "Normal"));

                var rand = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;

                // 1) Determinar rarezas permitidas (CSV). Si no hay, usar la del ítem o Normal
                List<string> candidatas = new();
                if (!string.IsNullOrWhiteSpace(baseData.RarezasPermitidasCsv))
                {
                    foreach (var r in baseData.RarezasPermitidasCsv.Split(','))
                    {
                        var s = NormalizarRarezaTexto(r.Trim());
                        if (!string.IsNullOrWhiteSpace(s)) candidatas.Add(s);
                    }
                }
                if (candidatas.Count == 0)
                {
                    var s = NormalizarRarezaTexto(baseData.Rareza ?? "Normal");
                    if (!string.IsNullOrWhiteSpace(s)) candidatas.Add(s); else candidatas.Add("Normal");
                }
                var rarezaElegida = ElegirRarezaPonderada(candidatas);

                // 2) Rango de perfección por rareza, intersectado con el rango del ítem si existe
                (int pMin, int pMax) = RangoPerfeccionPorRareza(rarezaElegida);
                if (baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                {
                    pMin = Math.Max(pMin, Math.Clamp(baseData.PerfeccionMin.Value, 0, 100));
                    pMax = Math.Min(pMax, Math.Clamp(baseData.PerfeccionMax.Value, 0, 100));
                    if (pMin > pMax)
                    {
                        if (rarezaConfig == null && baseData.PerfeccionMin.HasValue && baseData.PerfeccionMax.HasValue)
                        {
                            pMin = Math.Clamp(baseData.PerfeccionMin.Value, 0, 100);
                            pMax = Math.Clamp(baseData.PerfeccionMax.Value, 0, 100);
                            if (pMin > pMax) { pMin = 50; pMax = 50; }
                        }
                        else
                        {
                            (pMin, pMax) = RangoPerfeccionPorRareza(rarezaElegida);
                        }
                    }
                }
                int perfeccion = baseData.Perfeccion;
                if (perfeccion <= 0 || perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
                {
                    perfeccion = rand.Next(pMin, pMax + 1);
                }

                // 3) Nivel: usar rango si se definió
                int nivel = baseData.NivelRequerido;
                if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
                {
                    int nmin = Math.Max(1, baseData.NivelMin.Value);
                    int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                    nivel = rand.Next(nmin, nmax + 1);
                }

                // 4) Daño: elegir base según rango o valor fijo. Opcionalmente usar canales si existen.
                int danioBase = baseData.Daño;
                if (baseData.DañoMin.HasValue && baseData.DañoMax.HasValue)
                {
                    int dmin = Math.Max(0, baseData.DañoMin.Value);
                    int dmax = Math.Max(dmin, baseData.DañoMax.Value);
                    danioBase = rand.Next(dmin, dmax + 1);
                }
                else if (baseData.DañoFisico.HasValue || baseData.DañoMagico.HasValue)
                {
                    // Si hay canales definidos, tomar un representativo para el constructor (el Arma recalcula internamente ambos)
                    danioBase = Math.Max(baseData.DañoFisico ?? 0, baseData.DañoMagico ?? 0);
                    if (danioBase == 0) danioBase = baseData.Daño;
                }

                // 5) Aplicar perfección como factor base previo a los cálculos del Arma
                int danioFinal = (int)Math.Round(danioBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);

                // 6) Instanciar arma (manteniendo compatibilidad con la lógica interna de Arma)
                var arma = new Arma(baseData.Nombre, danioFinal, nivel, rarezaElegida, baseData.Tipo, perfeccion, 0);
                return arma;
            }
            throw new InvalidOperationException("No hay armas disponibles para generar.");
        }
    }
}
