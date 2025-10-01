using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    public static class GeneradorEnemigos
    {
    // Random centralizado a través de RandomService
        private static List<EnemigoData>? enemigosDisponibles;
        // Flag para tests: si es true, no se persisten drops en JSONs de objetos
        public static bool DesactivarPersistenciaDrops { get; set; } = false;

        // Este es el método que falta en tu archivo.
        // Se encarga de leer el archivo JSON y cargar los datos de los enemigos.
        public static void CargarEnemigos(string rutaArchivo)
        {
            // Logs de diagnóstico
            Logger.Debug($"[GeneradorEnemigos] Ruta recibida: {rutaArchivo}");
            Logger.Debug($"[GeneradorEnemigos] Ruta final usada: {rutaArchivo}");
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                enemigosDisponibles = JsonSerializer.Deserialize<List<EnemigoData>>(jsonString, options);
                Logger.Info($"Se cargaron {enemigosDisponibles?.Count ?? 0} enemigos del archivo.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar enemigos: {ex.Message}");
                enemigosDisponibles = new List<EnemigoData>();
            }
        }

        /// <summary>
        /// Carga enemigos priorizando carpeta DatosJuego/enemigos (todos los *.json),
        /// soportando tanto listas como un único objeto por archivo. Si no hay archivos
        /// válidos, cae a DatosJuego/enemigos.json. Mantiene compatibilidad con CargarEnemigos(string).
        /// </summary>
        public static void CargarEnemigosPorDefecto()
        {
            var dir = PathProvider.EnemigosDir();
            var acumulados = new List<EnemigoData>();
            var options = new JsonSerializerOptions();
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            bool algunArchivo = false;
            try
            {
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            // Convención: ignorar JSONs ubicados directamente en la raíz de 'nivel_*' bajo 'enemigos/por_bioma'
                            // para evitar duplicados respecto a subcarpetas por categoría (normal/elite/jefe/campo/legendario/unico/mundial)
                            if (DebeIgnorarseArchivoEnemigoPorConvencion(file))
                            {
                                Logger.Debug($"[GeneradorEnemigos] Ignorando '{file}' por convención de carpetas (raíz de nivel_*)");
                                continue;
                            }
                            var json = File.ReadAllText(file);
                            if (string.IsNullOrWhiteSpace(json)) continue;
                            algunArchivo = true;

                            using var doc = JsonDocument.Parse(json);
                            var kind = doc.RootElement.ValueKind;
                            if (kind == JsonValueKind.Array)
                            {
                                var lista = JsonSerializer.Deserialize<List<EnemigoData>>(json, options);
                                if (lista != null && lista.Count > 0)
                                {
                                    acumulados.AddRange(lista);
                                }
                                else
                                {
                                    Logger.Warn($"[GeneradorEnemigos] Lista vacía en '{file}'");
                                }
                            }
                            else if (kind == JsonValueKind.Object)
                            {
                                var uno = JsonSerializer.Deserialize<EnemigoData>(json, options);
                                if (uno != null && !string.IsNullOrWhiteSpace(uno.Nombre))
                                {
                                    acumulados.Add(uno);
                                }
                                else
                                {
                                    Logger.Warn($"[GeneradorEnemigos] Objeto enemigo inválido en '{file}'");
                                }
                            }
                            else
                            {
                                Logger.Warn($"[GeneradorEnemigos] Raíz JSON no válida en '{file}' (se esperaba objeto o lista)");
                            }
                        }
                        catch (Exception exFile)
                        {
                            Logger.Warn($"[GeneradorEnemigos] Error leyendo '{file}': {exFile.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[GeneradorEnemigos] Error al enumerar carpeta de enemigos: {ex.Message}");
            }

            if (acumulados.Count > 0)
            {
                enemigosDisponibles = acumulados;
                Logger.Info($"[GeneradorEnemigos] Cargados {enemigosDisponibles.Count} enemigos desde carpeta 'enemigos/'.");
                return;
            }

            // Fallback al archivo tradicional enemigos.json
            var rutaArchivo = PathProvider.CombineData("enemigos.json");
            if (!algunArchivo && !File.Exists(rutaArchivo))
            {
                Logger.Warn("[GeneradorEnemigos] No existe carpeta/archivos en 'enemigos/' ni 'enemigos.json'. Lista vacía.");
                enemigosDisponibles = new List<EnemigoData>();
                return;
            }
            CargarEnemigos(rutaArchivo);
        }
        
        // Ignora archivos ubicados directamente en la carpeta 'nivel_*' dentro de 'enemigos/por_bioma/<bioma>/'
        // Acepta únicamente archivos dentro de subcarpetas por categoría.
        private static bool DebeIgnorarseArchivoEnemigoPorConvencion(string filePath)
        {
            try
            {
                var lower = filePath.ToLowerInvariant();
                // Solo aplica a rutas bajo 'enemigos/por_bioma'
                if (!lower.Contains(Path.Combine("enemigos", "por_bioma").ToLowerInvariant()))
                    return false;

                var fileDir = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(fileDir)) return false;
                var dirInfo = new DirectoryInfo(fileDir);
                // Si el directorio inmediato es 'nivel_*', entonces el archivo está en la raíz del nivel
                if (dirInfo.Name.StartsWith("nivel_", StringComparison.OrdinalIgnoreCase))
                {
                    // Permitimos solo si además está en una subcarpeta de categoría; como está justo en 'nivel_*', lo ignoramos
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
        
        // Método para generar un enemigo aleatorio basado en el JSON.
        public static Enemigo GenerarEnemigoAleatorio(MiJuegoRPG.Personaje.Personaje jugador)
        {
            return GenerarEnemigoAleatorio(jugador, null);
        }

        // Sobrecarga: permite filtrar por tipos (separados por '|'), se matchea por nombre/Tag
        public static Enemigo GenerarEnemigoAleatorio(MiJuegoRPG.Personaje.Personaje jugador, string? filtroTipos)
        {
            if (enemigosDisponibles == null || enemigosDisponibles.Count == 0)
            {
                Logger.Warn("No se encontraron enemigos. Generando Goblin por defecto.");
                // Creamos un enemigo por defecto si no hay JSON
                var def = new EnemigoEstandar("Goblin", 50, 10, 5, 5, 1, 5, 5);
                def.Tag = "goblin";
                return def;
            }

            var enemigosApropiados = enemigosDisponibles
                .Where(e => e.Nivel <= jugador.Nivel + 2) // Filtra enemigos para que no sean demasiado difíciles
                .ToList();

            if (!string.IsNullOrWhiteSpace(filtroTipos))
            {
                var tipos = filtroTipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                        .Select(t => t.ToLowerInvariant()).ToHashSet();
                // Coincidencia por nombre o por tags (si existen)
                var filtrados = enemigosApropiados.Where(e =>
                    tipos.Any(t => e.Nombre.ToLowerInvariant().Contains(t))
                    || (e.Tags != null && e.Tags.Any(tag => tipos.Contains(tag.ToLowerInvariant())))
                ).ToList();
                if (filtrados.Count > 0)
                {
                    enemigosApropiados = filtrados;
                }
                else
                {
                    Logger.Warn($"Filtro de tipos '{filtroTipos}' no coincidió, usando lista sin filtrar.");
                }
            }
            
            if (!enemigosApropiados.Any())
            {
                Logger.Warn("No se encontraron enemigos apropiados. Generando Goblin por defecto.");
                var def = new EnemigoEstandar("Goblin", 50, 10, 5, 5, 1, 5, 5);
                def.Tag = "goblin";
                return def;
            }

            // Aplicar SpawnChance (si está definido) como pre-filtro suave: se incluye si pasa el roll
            var rng = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            var candidatos = new List<EnemigoData>();
            bool habiaConChance = false;
            foreach (var e in enemigosApropiados)
            {
                if (e.SpawnChance.HasValue)
                {
                    habiaConChance = true;
                    var p = Math.Clamp(e.SpawnChance.Value, 0.0, 1.0);
                    if (rng.NextDouble() < p) candidatos.Add(e);
                }
                else
                {
                    candidatos.Add(e); // si no tiene chance, siempre es elegible en esta fase
                }
            }
            if (candidatos.Count == 0)
            {
                // Si todos fallaron el roll y había SpawnChance definidos, relajar: usar la lista original para no quedarse sin enemigo
                if (habiaConChance) candidatos = enemigosApropiados;
                // Si no había SpawnChance, ya estaría vacío por otra razón (pero no debería), caer a lista original
                if (candidatos.Count == 0) candidatos = enemigosApropiados;
            }

            // Selección ponderada por SpawnWeight (default 1). Si todos tienen peso <=0, hacer uniforme.
            int totalPeso = candidatos.Sum(e => Math.Max(0, e.SpawnWeight ?? 1));
            EnemigoData enemigoData;
            if (totalPeso <= 0)
            {
                int idx = rng.Next(0, candidatos.Count);
                enemigoData = candidatos[idx];
            }
            else
            {
                int r = rng.Next(1, totalPeso + 1);
                int acc = 0;
                enemigoData = candidatos[0];
                foreach (var e in candidatos)
                {
                    acc += Math.Max(0, e.SpawnWeight ?? 1);
                    if (r <= acc) { enemigoData = e; break; }
                }
            }

            // Buscar arma por nombre si existe en el JSON del enemigo
            Objetos.Arma? arma = null;
            if (!string.IsNullOrWhiteSpace(enemigoData.ArmaNombre))
            {
                arma = Objetos.GestorArmas.BuscarArmaPorNombre(enemigoData.ArmaNombre);
            }

            var enemigo = new EnemigoEstandar(
                enemigoData.Nombre,
                enemigoData.VidaBase,
                enemigoData.AtaqueBase,
                enemigoData.DefensaBase,
                enemigoData.DefensaMagicaBase,
                enemigoData.Nivel,
                enemigoData.ExperienciaRecompensa,
                enemigoData.OroRecompensa
            );
            enemigo.IdData = string.IsNullOrWhiteSpace(enemigoData.Id) ? enemigoData.Nombre : enemigoData.Id!;
            if (arma != null)
            {
                enemigo.ArmaEquipada = arma;
            }
            // Tag básico: si hay filtro usa el primer valor como tag, si no usa el nombre en minúsculas
            var tagBase = enemigoData.Nombre.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(filtroTipos))
            {
                var prim = filtroTipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(prim)) tagBase = prim.ToLowerInvariant();
            }
            // Aplicar tags adicionales desde data si existen
            if (enemigoData.Tags != null && enemigoData.Tags.Count > 0)
            {
                enemigo.Tag = enemigoData.Tags[0].ToLowerInvariant();
            }
            else
            {
                enemigo.Tag = tagBase;
            }

            // Aplicar configuración avanzada desde data
            if (enemigoData.Inmunidades != null)
            {
                foreach (var kv in enemigoData.Inmunidades)
                {
                    enemigo.Inmunidades[kv.Key] = kv.Value;
                }
            }
            // Defaults por familia (p. ej., NoMuerto inmune a veneno si no está especificado)
            if (enemigoData.Familia == PjDatos.Familia.NoMuerto && !enemigo.Inmunidades.ContainsKey("veneno"))
            {
                enemigo.Inmunidades["veneno"] = true;
            }
            if (enemigoData.MitigacionFisicaPorcentaje.HasValue)
            {
                enemigo.MitigacionFisicaPorcentaje = Math.Clamp(enemigoData.MitigacionFisicaPorcentaje.Value, 0.0, 0.9);
            }
            if (enemigoData.MitigacionMagicaPorcentaje.HasValue)
            {
                enemigo.MitigacionMagicaPorcentaje = Math.Clamp(enemigoData.MitigacionMagicaPorcentaje.Value, 0.0, 0.9);
            }

            // Evasión por data (clamp defensivo)
            if (enemigoData.EvasionFisica.HasValue)
            {
                enemigo.EvasionFisica = Math.Clamp(enemigoData.EvasionFisica.Value, 0.0, 0.95); // mientras mas alto, menos probabilidad de ser golpeado
            }
            if (enemigoData.EvasionMagica.HasValue)
            {
                enemigo.EvasionMagica = Math.Clamp(enemigoData.EvasionMagica.Value, 0.0, 0.95); // mientras mas alto, menos probabilidad de ser golpeado
            }

            // Resistencias elementales (mitigación adicional por tipo)
            if (enemigoData.ResistenciasElementales != null)
            {
                foreach (var kv in enemigoData.ResistenciasElementales)
                {
                    enemigo.EstablecerMitigacionElemental(kv.Key, kv.Value);
                }
            }

            // Vulnerabilidades elementales (multiplicador post-mitigación)
            if (enemigoData.VulnerabilidadesElementales != null)
            {
                foreach (var kv in enemigoData.VulnerabilidadesElementales)
                {
                    enemigo.EstablecerVulnerabilidadElemental(kv.Key, kv.Value);
                }
            }

            // Daño elemental base
            if (enemigoData.DanioElementalBase != null)
            {
                foreach (var kv in enemigoData.DanioElementalBase)
                {
                    enemigo.AgregarDanioElementalBase(kv.Key, kv.Value);
                }
            }

            // Equipo inicial (arma)
            if (enemigoData.EquipoInicial != null && !string.IsNullOrWhiteSpace(enemigoData.EquipoInicial.Arma))
            {
                try
                {
                    var armaIni = Objetos.GestorArmas.BuscarArmaPorNombre(enemigoData.EquipoInicial.Arma);
                    if (armaIni != null)
                    {
                        ((EnemigoEstandar)enemigo).ArmaEquipada = armaIni;
                    }
                    else
                    {
                        Logger.Warn($"[GeneradorEnemigos] Arma inicial no encontrada: '{enemigoData.EquipoInicial.Arma}' para {enemigo.Nombre}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[GeneradorEnemigos] Error equipando arma inicial '{enemigoData.EquipoInicial.Arma}': {ex.Message}");
                }
            }

            // Drops configurados por JSON (chance individual por objeto + cantidades y UniqueOnce)
            if (enemigoData.Drops != null && enemigoData.Drops.Count > 0)
            {
                foreach (var drop in enemigoData.Drops)
                {
                    if (string.IsNullOrWhiteSpace(drop?.Tipo) || string.IsNullOrWhiteSpace(drop?.Nombre))
                        continue;
                    var tipo = drop.Tipo.Trim().ToLowerInvariant();
                    var nombre = drop.Nombre.Trim();
                    double chance = Math.Clamp(drop.Chance, 0.0, 1.0);
                    // Registrar metadatos de cantidad y unique
                    int cmin = Math.Max(1, drop.CantidadMin);
                    int cmax = Math.Max(cmin, drop.CantidadMax);
                    // clamp duro acorde a progresión lenta
                    int hardCap = 5; // se recorta luego según rareza en Enemigo.DarRecompensas
                    cmax = Math.Min(cmax, hardCap);
                    enemigo.RangoCantidadDrop[nombre] = (cmin, cmax);
                    if (drop.UniqueOnce) enemigo.DropsUniqueOnce.Add(nombre);

                    switch (tipo)
                    {
                        case "material":
                        {
                            var mat = Objetos.GestorMateriales.BuscarMaterialPorNombre(nombre);
                            if (mat == null)
                            {
                                // Crear stub si no existe
                                var rzStr = string.IsNullOrWhiteSpace(drop.Rareza) ? "Normal" : drop.Rareza;
                                // Normalización simple; se mantiene string sin enum
                                var rzObjStr = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                                mat = new Objetos.Material(nombre, rzObjStr, "Material");
                                if (!DesactivarPersistenciaDrops)
                                    Objetos.GestorMateriales.GuardarMaterialSiNoExiste(mat);
                            }
                            enemigo.ProbabilidadesDrop[nombre] = chance;
                            enemigo.ObjetosDrop.Add(mat);
                            break;
                        }
                        case "arma":
                        {
                            var armaDrop = Objetos.GestorArmas.BuscarArmaPorNombre(nombre);
                            if (armaDrop != null)
                            {
                                enemigo.ProbabilidadesDrop[nombre] = chance;
                                enemigo.ObjetosDrop.Add(armaDrop);
                            }
                            else
                            {
                                Logger.Warn($"[GeneradorEnemigos] Drop arma no encontrada '{nombre}' para {enemigo.Nombre}");
                            }
                            break;
                        }
                        case "pocion":
                        {
                            var poc = Objetos.GestorPociones.BuscarPocionPorNombre(nombre);
                            if (poc != null)
                            {
                                enemigo.ProbabilidadesDrop[nombre] = chance;
                                enemigo.ObjetosDrop.Add(poc);
                            }
                            else
                            {
                                Logger.Warn($"[GeneradorEnemigos] Drop poción no encontrada '{nombre}' para {enemigo.Nombre}");
                            }
                            break;
                        }
                        default:
                            Logger.Warn($"[GeneradorEnemigos] Tipo de drop no soportado '{drop.Tipo}' en {enemigo.Nombre}");
                            break;
                    }

                    // Mensajería reducida; ya aplicamos metadatos arriba
                }
            }

            // Ajuste contextual de dificultad si el jugador es muy novato y sin equipo
            try
            {
                bool novato = jugador.Nivel <= 2;
                bool sinEquipo = (jugador.ObtenerObjetosEquipados()?.Count ?? 0) == 0;
                if (novato && sinEquipo)
                {
                    enemigo.VidaMaxima = (int)Math.Round(enemigo.VidaMaxima * 1.10);
                    enemigo.Vida = enemigo.VidaMaxima;
                    enemigo.Ataque = (int)Math.Round(enemigo.Ataque * 1.10);
                }
            }
            catch { }

            // Ejemplo de drops básicos según tipo de enemigo
            if (enemigo.Nombre.ToLower().Contains("goblin"))
            {
                var armaDrop = new MiJuegoRPG.Objetos.Arma("Espada Oxidada", 5, nivel: 1, rareza: "Normal", categoria: "UnaMano");
                enemigo.ObjetosDrop.Add(armaDrop);
                if (!DesactivarPersistenciaDrops)
                    MiJuegoRPG.Objetos.GestorArmas.GuardarArmaSiNoExiste(armaDrop);

                var pocionDrop = new MiJuegoRPG.Objetos.Pocion("Poción Pequeña", 10, "Pobre");
                enemigo.ObjetosDrop.Add(pocionDrop);
                if (!DesactivarPersistenciaDrops)
                    MiJuegoRPG.Objetos.GestorPociones.GuardarPocionSiNoExiste(pocionDrop);
            }
            else if (enemigo.Nombre.ToLower().Contains("slime"))
            {
                var materialDrop = new MiJuegoRPG.Objetos.Material("Gelatina", "Rota");
                enemigo.ObjetosDrop.Add(materialDrop);
                if (!DesactivarPersistenciaDrops)
                    MiJuegoRPG.Objetos.GestorMateriales.GuardarMaterialSiNoExiste(materialDrop);
            }
            else if (enemigo.Nombre.ToLower().Contains("golem"))
            {
                var armaDrop = new MiJuegoRPG.Objetos.Arma("Martillo Pesado", 20, nivel: enemigo.Nivel, rareza: "Rara", categoria: "DosManos");
                enemigo.ObjetosDrop.Add(armaDrop);
                if (!DesactivarPersistenciaDrops)
                    MiJuegoRPG.Objetos.GestorArmas.GuardarArmaSiNoExiste(armaDrop);
            }
            // Puedes agregar más lógica para otros tipos de enemigos

            return enemigo;
        }

        // Método para iniciar el combate
        public static void IniciarCombate(MiJuegoRPG.Personaje.Personaje jugador, Enemigo enemigo)
        {
            //Console.Clear();
            // Solo mostrar aparición aquí, no en CombatePorTurnos
            var combate = new CombatePorTurnos(jugador, enemigo);
            combate.IniciarCombate();

            while (true)
            {
                var ui = Juego.ObtenerInstanciaActual()?.Ui;
                ui?.WriteLine("\nEl combate ha terminado.");
                ui?.WriteLine("1. Continuar...");
                ui?.WriteLine("2. Volver al menú anterior");
                var opcion = MiJuegoRPG.Motor.InputService.LeerOpcion("Elige una opción: ");
                if (opcion == "1") break;
                if (opcion == "2") {
                    // Volver al menú de ubicación principal moderno
                    var juego = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual();
                    if (juego != null)
                        juego.MostrarMenuPorUbicacion();
                    break;
                }
            }
        }
        // Devuelve una lista de instancias de todos los enemigos disponibles (apropiados para el nivel del jugador)
        public static List<ICombatiente> ObtenerTodosLosEnemigos(MiJuegoRPG.Personaje.Personaje? jugador = null)
        {
            var lista = new List<ICombatiente>();
            if (enemigosDisponibles == null || enemigosDisponibles.Count == 0)
            {
                lista.Add(new EnemigoEstandar("Goblin", 50, 10, 5, 5, 1, 5, 5));
                return lista;
            }
            var enemigosApropiados = jugador == null
                ? enemigosDisponibles
                : enemigosDisponibles.Where(e => e.Nivel <= (jugador.Nivel + 2)).ToList();
            foreach (var enemigoData in enemigosApropiados)
            {
                Objetos.Arma? arma = null;
                if (!string.IsNullOrWhiteSpace(enemigoData.ArmaNombre))
                {
                    arma = Objetos.GestorArmas.BuscarArmaPorNombre(enemigoData.ArmaNombre);
                }
                var enemigo = new EnemigoEstandar(
                    enemigoData.Nombre,
                    enemigoData.VidaBase,
                    enemigoData.AtaqueBase,
                    enemigoData.DefensaBase,
                    enemigoData.DefensaMagicaBase,
                    enemigoData.Nivel,
                    enemigoData.ExperienciaRecompensa,
                    enemigoData.OroRecompensa
                );
                if (arma != null)
                {
                    enemigo.ArmaEquipada = arma;
                }
                enemigo.Tag = enemigoData.Nombre.ToLowerInvariant();
                lista.Add(enemigo);
            }
            return lista;
        }

        // Inicia combate contra varios enemigos
        public static void IniciarCombateMultiple(MiJuegoRPG.Personaje.Personaje jugador, List<ICombatiente> enemigos)
        {
            //Console.Clear();
            var ui = Juego.ObtenerInstanciaActual()?.Ui;
            ui?.WriteLine($"¡Han aparecido {enemigos.Count} enemigos!");
            foreach (var enemigo in enemigos)
            {
                ui?.WriteLine($"- {enemigo.Nombre}");
            }
            var combate = new CombatePorTurnos(jugador, enemigos);
            combate.IniciarCombate();
            MiJuegoRPG.Motor.InputService.Pausa("\nEl combate ha terminado. Presiona cualquier tecla para continuar...");
        }
    }
}