using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Dominio;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    /// Servicio que gestiona el flujo de UI y selección de nodos de recolección.
    /// Vista híbrida: agrupación por tipo + filtros (m/r/t/*) + búsqueda + cooldown + fallo.
    /// Mantiene cache por bioma para que el cooldown persista en sesión.
    public class RecoleccionService
    {
        private readonly Juego juego;
        private readonly ProgressionService progressionService;
    // Cache de nodos generados por bioma para mantener referencias (cooldowns estables)
    private readonly Dictionary<string, List<NodoRecoleccion>> cacheBiomaPorSector = new();
    private readonly RandomService rngSvc = RandomService.Instancia;
    // Estado persistible multisector: sectorId -> (nombreNodo -> epochUltimoUso)
    private readonly Dictionary<string, Dictionary<string,long>> cooldownsMultiSector = new();
        public RecoleccionService(Juego juego)
        {
            this.juego = juego;
            this.progressionService = new ProgressionService();
        }

        public Dictionary<string, Dictionary<string,long>> ExportarCooldownsMultiSector()
        {
            // Asegurar que el sector actual está sincronizado antes de exportar
            SincronizarSectorActual();
            // Clonar profundo para evitar mutaciones externas
            var copia = new Dictionary<string, Dictionary<string,long>>();
            foreach (var kv in cooldownsMultiSector)
                copia[kv.Key] = new Dictionary<string,long>(kv.Value);
            return copia;
        }
        public void ImportarCooldownsMultiSector(Dictionary<string, Dictionary<string,long>> data)
        {
            cooldownsMultiSector.Clear();
            if (data == null) return;
            foreach (var kv in data)
                cooldownsMultiSector[kv.Key] = new Dictionary<string,long>(kv.Value);
            // Aplicar al sector actual (si hay)
            if (juego.mapa?.UbicacionActual != null)
                AlEntrarSector(juego.mapa.UbicacionActual.Id);
        }
        private void SincronizarSectorActual()
        {
            var sector = juego.mapa.UbicacionActual;
            if (sector == null) return;
            if (!cooldownsMultiSector.TryGetValue(sector.Id, out var dic))
            {
                dic = new Dictionary<string,long>();
                cooldownsMultiSector[sector.Id] = dic;
            }
            foreach (var n in ObtenerTodosNodos())
            {
                if (n.UltimoUso.HasValue)
                    dic[n.Nombre ?? "?"] = new DateTimeOffset(n.UltimoUso.Value).ToUnixTimeSeconds();
            }
        }
        public void RegistrarUsoNodo(NodoRecoleccion n)
        {
            var sector = juego.mapa.UbicacionActual;
            if (sector == null) return;
            if (!cooldownsMultiSector.TryGetValue(sector.Id, out var dic))
            {
                dic = new Dictionary<string,long>();
                cooldownsMultiSector[sector.Id] = dic;
            }
            if (n.UltimoUso.HasValue)
                dic[n.Nombre ?? "?"] = new DateTimeOffset(n.UltimoUso.Value).ToUnixTimeSeconds();
        }
        public void AlEntrarSector(string sectorId)
        {
            // Generar nodos (o recuperar cache) y aplicar cooldowns guardados válidos
            var nodos = ObtenerTodosNodos();
            if (!cooldownsMultiSector.TryGetValue(sectorId, out var dic)) return; // nada que aplicar
            var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var n in nodos)
            {
                if (dic.TryGetValue(n.Nombre ?? "?", out var epoch))
                {
                    // Limpiar expirados para no crecer indefinidamente
                    var cd = n.CooldownEfectivo();
                    if (cd > 0 && epoch + cd > ahora)
                        n.UltimoUso = DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
                    else
                        dic.Remove(n.Nombre ?? "?");
                }
            }
        }

    public void MostrarMenu() // Bucle principal del menú híbrido de recolección
        {
            // Modo híbrido: vista agrupada + filtros rápidos
            while (true)
            {
                var todos = ObtenerTodosNodos();
                if (todos.Count == 0)
                {
                    juego.Ui.WriteLine("No hay nodos de recolección en este sector ni por bioma.");
                    return;
                }

                // Estado de filtro
                string filtroTipo = "*"; // * = todos
                string terminoBusqueda = string.Empty;

                while (true)
                {
                    var vista = FiltrarVista(todos, filtroTipo, terminoBusqueda, out var mapping);
                    MostrarVistaAgrupada(filtroTipo, terminoBusqueda, mapping);
                    juego.Ui.Write("Comando (m/r/t/*, b=buscar, c=limpiar, 0=volver, número=acción): ");
                    var input = InputService.LeerOpcion();
                    if (input == "0") return; // salir del menú híbrido
                    switch (input.ToLower())
                    {
                        case "m": filtroTipo = TipoRecoleccion.Minar.ToString(); continue;
                        case "r": filtroTipo = TipoRecoleccion.Recolectar.ToString(); continue;
                        case "t": filtroTipo = TipoRecoleccion.Talar.ToString(); continue;
                        case "*": filtroTipo = "*"; continue;
                        case "c": terminoBusqueda = string.Empty; continue;
                        case "b":
                            juego.Ui.Write("Buscar nombre contiene: ");
                            terminoBusqueda = (juego.Ui.ReadLine() ?? string.Empty).Trim();
                            continue;
                    }
                    if (int.TryParse(input, out int idx))
                    {
                        if (mapping.TryGetValue(idx, out var nodoSel))
                        {
                            // Determinar tipo de ejecución
                            TipoRecoleccion tipoExec;
                            if (!string.IsNullOrWhiteSpace(nodoSel.Tipo) && Enum.TryParse<TipoRecoleccion>(nodoSel.Tipo, true, out var parsed))
                                tipoExec = parsed;
                            else
                            {
                                // Nodo genérico: preguntar
                                juego.Ui.WriteLine("Tipo para ejecutar: 1=Recolectar 2=Minar 3=Talar 0=Cancelar");
                                var opt = InputService.LeerOpcion();
                                tipoExec = opt switch
                                {
                                    "1" => TipoRecoleccion.Recolectar,
                                    "2" => TipoRecoleccion.Minar,
                                    "3" => TipoRecoleccion.Talar,
                                    _ => TipoRecoleccion.Recolectar
                                };
                                if (opt == "0") continue;
                            }
                            EjecutarAccion(tipoExec, nodoSel);
                            continue; // permanece en vista
                        }
                        juego.Ui.WriteLine("Índice fuera de rango.");
                        continue;
                    }
                    juego.Ui.WriteLine("Comando no reconocido.");
                }
            }
        }

        private List<NodoRecoleccion> ObtenerNodos(TipoRecoleccion tipo)
        {
            var lista = new List<NodoRecoleccion>();
            var sector = juego.mapa.UbicacionActual;
            if (sector != null && sector.NodosRecoleccion != null && sector.NodosRecoleccion.Count > 0)
            {
                lista.AddRange(sector.NodosRecoleccion);
            }
            if (lista.Count == 0 && sector != null && !string.IsNullOrWhiteSpace(sector.Region))
            {
                lista.AddRange(TablaBiomas.GenerarNodosParaBioma(sector.Region));
            }
            if (lista.Count == 0) return lista;

            // 1. Intentar obtener solo nodos del tipo exacto
            var exactos = lista.Where(n => !string.IsNullOrWhiteSpace(n.Tipo) && string.Equals(n.Tipo, tipo.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (exactos.Count > 0) return exactos;

            // 2. Fallback: nodos genéricos (sin tipo) SOLO si no hay específicos
            var genericos = lista.Where(n => string.IsNullOrWhiteSpace(n.Tipo)).ToList();
            return genericos;
        }

    private List<NodoRecoleccion> ObtenerTodosNodos() // Obtiene nodos del sector o genera por bioma (con cache)
        {
            var sector = juego.mapa.UbicacionActual;
            if (sector == null) return new List<NodoRecoleccion>();
            // Si el sector define nodos propios se usan directamente (persisten en objeto SectorData)
            if (sector.NodosRecoleccion != null && sector.NodosRecoleccion.Count > 0)
            {
                // Hidratación: si el nodo del sector no tiene materiales (autocompletado),
                // intentar completar desde la plantilla del bioma por nombre
                foreach (var n in sector.NodosRecoleccion)
                {
                    if (n.Materiales == null || n.Materiales.Count == 0)
                        HidratarNodoDesdeBioma(n);
                }
                return sector.NodosRecoleccion;
            }
            // Caso bioma: cachear para mantener referencias (cooldown)
            var clave = $"bioma:{sector.Id}:{sector.Region}";
            if (cacheBiomaPorSector.TryGetValue(clave, out var cached)) return cached;
            var generados = (!string.IsNullOrWhiteSpace(sector.Region)) ? TablaBiomas.GenerarNodosParaBioma(sector.Region) : new List<NodoRecoleccion>();
            cacheBiomaPorSector[clave] = generados;
            return generados;
        }

        private bool HidratarNodoDesdeBioma(NodoRecoleccion nodo)
        {
            try
            {
                var nombre = (nodo.Nombre ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(nombre)) return false;
                var region = juego.mapa?.UbicacionActual?.Region;

                // 1) Intentar en el bioma del sector (si existe)
                if (!string.IsNullOrWhiteSpace(region) && TablaBiomas.Biomas.TryGetValue(region!, out var bioma))
                {
                    var plantilla = BuscarNodoEnBiomaPorNombre(bioma, nombre);
                    if (plantilla != null)
                    {
                        CopiarPropsNodo(plantilla, nodo);
                        return true;
                    }
                }
                // 2) Búsqueda global (fallback) en todos los biomas
                foreach (var kv in TablaBiomas.Biomas)
                {
                    var plantilla = BuscarNodoEnBiomaPorNombre(kv.Value, nombre);
                    if (plantilla != null)
                    {
                        CopiarPropsNodo(plantilla, nodo);
                        return true;
                    }
                }
            }
            catch { /* no-op: hidratación best-effort */ }
            return false;
        }

        private static NodoRecoleccion? BuscarNodoEnBiomaPorNombre(BiomaRecoleccion bioma, string nombre)
        {
            static string Norm(string s)
            {
                s = s.Trim().ToLowerInvariant();
                try
                {
                    var form = s.Normalize(System.Text.NormalizationForm.FormD);
                    var sb = new System.Text.StringBuilder(form.Length);
                    foreach (var ch in form)
                    {
                        var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                        if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                            sb.Append(ch);
                    }
                    return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
                }
                catch { return s; }
            }

            var target = Norm(nombre);
            foreach (var lista in new[] { bioma.NodosComunes, bioma.NodosRaros })
            {
                if (lista == null) continue;
                foreach (var n in lista)
                {
                    if (Norm(n.Nombre ?? string.Empty) == target)
                        return n;
                }
            }
            return null;
        }

        private static void CopiarPropsNodo(NodoRecoleccion origen, NodoRecoleccion destino)
        {
            // Copiar solo si faltan datos en el destino
            if (string.IsNullOrWhiteSpace(destino.Tipo) && !string.IsNullOrWhiteSpace(origen.Tipo))
                destino.Tipo = origen.Tipo;
            if (string.IsNullOrWhiteSpace(destino.Requiere) && !string.IsNullOrWhiteSpace(origen.Requiere))
                destino.Requiere = origen.Requiere;
            if ((destino.Materiales == null || destino.Materiales.Count == 0) && (origen.Materiales != null && origen.Materiales.Count > 0))
            {
                destino.Materiales = new List<MaterialCantidad>();
                foreach (var m in origen.Materiales)
                    destino.Materiales.Add(new MaterialCantidad { Nombre = m.Nombre, Cantidad = m.Cantidad });
            }
            if (!destino.ProduccionMin.HasValue && origen.ProduccionMin.HasValue)
                destino.ProduccionMin = origen.ProduccionMin;
            if (!destino.ProduccionMax.HasValue && origen.ProduccionMax.HasValue)
                destino.ProduccionMax = origen.ProduccionMax;
            if (string.IsNullOrWhiteSpace(destino.Rareza) && !string.IsNullOrWhiteSpace(origen.Rareza))
                destino.Rareza = origen.Rareza;
            // Cooldown: si el destino no define uno, usar base del origen
            if (destino.Cooldown <= 0)
            {
                if (origen.Cooldown > 0) destino.CooldownBase = origen.Cooldown;
                else if (origen.CooldownBase.HasValue) destino.CooldownBase = origen.CooldownBase;
            }
        }

    private List<NodoRecoleccion> FiltrarVista(List<NodoRecoleccion> baseList, string filtroTipo, string busqueda, out Dictionary<int, NodoRecoleccion> mapping) // Aplica búsqueda y filtro de tipo, genera índice estable
        {
            mapping = new();
            IEnumerable<NodoRecoleccion> fuente = baseList;
            if (!string.IsNullOrWhiteSpace(busqueda))
                fuente = fuente.Where(n => (n.Nombre ?? string.Empty).IndexOf(busqueda, StringComparison.OrdinalIgnoreCase) >= 0);
            if (filtroTipo != "*")
                fuente = fuente.Where(n => !string.IsNullOrWhiteSpace(n.Tipo) && string.Equals(n.Tipo, filtroTipo, StringComparison.OrdinalIgnoreCase));

            var ordenTipos = new List<string> { TipoRecoleccion.Recolectar.ToString(), TipoRecoleccion.Minar.ToString(), TipoRecoleccion.Talar.ToString(), "_GEN" };
            var grupos = fuente.GroupBy(n => string.IsNullOrWhiteSpace(n.Tipo) ? "_GEN" : n.Tipo!, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            int idx = 1;
            foreach (var t in ordenTipos)
            {
                if (!grupos.TryGetValue(t, out var lista)) continue;
                foreach (var n in lista)
                    mapping[idx++] = n;
            }
            return mapping.Values.ToList();
        }

    private void MostrarVistaAgrupada(string filtroTipo, string busqueda, Dictionary<int, NodoRecoleccion> mapping) // Render agrupado con secciones y estado de cooldown
        {
            juego.Ui.WriteLine("\n=== Recolección (vista híbrida) ===");
            juego.Ui.WriteLine($"Filtro Tipo: {(filtroTipo == "*" ? "Todos" : filtroTipo)} | Búsqueda: {(string.IsNullOrWhiteSpace(busqueda) ? "(ninguna)" : busqueda)}");
            if (juego.jugador != null)
            {
                juego.Ui.WriteLine($"Energía: {juego.jugador.EnergiaActual}/{juego.jugador.EnergiaMaxima}");
            }
            if (mapping.Count == 0)
            {
                juego.Ui.WriteLine("(Sin nodos coincidentes)");
                return;
            }
            // Volver a agrupar para mostrar con encabezados
            var gruposOrden = new (string key, string titulo)[] {
                (TipoRecoleccion.Recolectar.ToString(), "Recolectar"),
                (TipoRecoleccion.Minar.ToString(), "Minar"),
                (TipoRecoleccion.Talar.ToString(), "Talar"),
                ("_GEN", "Genéricos")
            };
            var lookupIdx = mapping.ToDictionary(k => k.Value, v => v.Key);
            var grupos = mapping.Values.GroupBy(n => string.IsNullOrWhiteSpace(n.Tipo) ? "_GEN" : n.Tipo!, StringComparer.OrdinalIgnoreCase)
                                       .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            foreach (var g in gruposOrden)
            {
                if (!grupos.TryGetValue(g.key, out var lista) || lista.Count == 0) continue;
                juego.Ui.WriteLine($"-- {g.titulo} --");
                foreach (var n in lista)
                {
                    int idx = lookupIdx[n];
                    var req = string.IsNullOrWhiteSpace(n.Requiere) ? "" : $" [Req: {n.Requiere}]";
                    var gen = string.IsNullOrWhiteSpace(n.Tipo) ? " [Gen]" : "";
                    var cd = n.EstaEnCooldown() ? $" [CD {n.SegundosRestantesCooldown()}s]" : (n.CooldownEfectivo() > 0 ? $" [Listo {n.CooldownEfectivo()}s]" : "");
                    var rare = !string.IsNullOrWhiteSpace(n.Rareza) ? FormatearRarezaTag(n.Rareza!) : string.Empty;
                    var prod = "";
                    if (n.ProduccionMin.HasValue && n.ProduccionMax.HasValue && n.ProduccionMin.Value > 0)
                    {
                        prod = n.ProduccionMin.Value == n.ProduccionMax.Value
                            ? $" [Prod {n.ProduccionMin.Value}]"
                            : $" [Prod {n.ProduccionMin.Value}-{n.ProduccionMax.Value}]";
                    }
                    var mats = (n.Materiales != null && n.Materiales.Count > 0) ? $" => {string.Join(", ", n.Materiales.Select(m => m.Cantidad + "x " + m.Nombre))}" : string.Empty;
                    juego.Ui.WriteLine($"{idx}. {n.Nombre}{gen}{rare}{prod}{req}{cd}{mats}");
                }
            }
        }

        // Nueva lógica centralizada antes en Juego.RealizarAccionRecoleccion
    public void EjecutarAccion(TipoRecoleccion tipo, NodoRecoleccion nodo) // Valida requisitos, energía, cooldown y aplica resultado
        {
            // Validar requisito herramienta
            if (!string.IsNullOrEmpty(nodo.Requiere))
            {
                // Soporta partidas antiguas donde NuevosObjetos podría ser null
                var lista = juego.jugador?.Inventario?.NuevosObjetos;
                bool tieneHerramienta = TieneHerramientaRequerida(lista, nodo.Requiere);
                if (!tieneHerramienta)
                {
                    juego.Ui.WriteLine($"Necesitas un {nodo.Requiere} para realizar esta acción.");
                    InputService.Pausa();
                    return;
                }
            }
            // Cooldown
            if (nodo.EstaEnCooldown())
            {
                juego.Ui.WriteLine($"El nodo aún está en cooldown ({nodo.SegundosRestantesCooldown()}s restantes).");
                InputService.Pausa();
                return;
            }
            // Energía
            if (juego.jugador != null)
            {
                juego.energiaService.MostrarEnergia(juego.jugador);
                var bioma = juego.mapa?.UbicacionActual?.Region;
                // Heurística simple para inferir herramienta: si requiere contiene Pico/Hacha
                string? herramienta = null;
                if (!string.IsNullOrWhiteSpace(nodo.Requiere)) herramienta = nodo.Requiere;
                else if (tipo == MiJuegoRPG.Dominio.TipoRecoleccion.Minar) herramienta = "Pico";
                else if (tipo == MiJuegoRPG.Dominio.TipoRecoleccion.Talar) herramienta = "Hacha";
                if (!juego.energiaService.GastarEnergiaRecoleccion(juego.jugador, tipo.ToString(), bioma, herramienta))
                {
                    InputService.Pausa();
                    return;
                }
            }
            // Resultado (probabilidad de fallo)
            bool fallo = rngSvc.NextDouble() < 0.15; // 15% fallo base
            nodo.UltimoUso = DateTime.UtcNow;
            RegistrarUsoNodo(nodo); // registrar para persistencia multisector
            if (fallo)
            {
                nodo.UsosFallidosRecientes++;
                juego.Ui.WriteLine($"Fallaste al recolectar en '{nodo.Nombre}'. No obtuviste recursos.");
                try
                {
                    var biomaLog = juego.mapa?.UbicacionActual?.Region ?? "?";
                    var rarezaLog = string.IsNullOrEmpty(nodo.Rareza) ? "" : nodo.Rareza;
                    MiJuegoRPG.Motor.Servicios.Logger.Info($"[Recolectar][FALLO] Nodo='{nodo.Nombre}' Bioma='{biomaLog}' Rareza='{rarezaLog}' CD={nodo.CooldownEfectivo()}s");
                }
                catch { }
                // Pequeña compensación opcional: XP mínima? por ahora no
            }
            else
            {
                nodo.UsosFallidosRecientes = 0;
                juego.Ui.WriteLine($"Recolectaste en el nodo: {nodo.Nombre}");
                int totalObtenido = 0;
                if (nodo.Materiales != null && nodo.Materiales.Count > 0)
                {
                    foreach (var mat in nodo.Materiales)
                    {
                        int cantidad = mat.Cantidad;
                        // Nuevo: si ProduccionMin/Max definen rango válido, usar random dentro
                        if (nodo.ProduccionMin.HasValue && nodo.ProduccionMax.HasValue && nodo.ProduccionMin.Value > 0 && nodo.ProduccionMax.Value >= nodo.ProduccionMin.Value)
                        {
                            var min = nodo.ProduccionMin.Value;
                            var max = nodo.ProduccionMax.Value;
                            cantidad = (min == max) ? min : rngSvc.Next(min, max + 1);
                        }
                        juego.Ui.WriteLine($"  - {cantidad}x {mat.Nombre}");
                        totalObtenido += cantidad;
                        if (juego.jugador != null && juego.jugador.Inventario != null)
                        {
                            // Rareza futura: mapear Rareza nodo a rareza material (por ahora Normal)
                            juego.jugador.Inventario.AgregarObjeto(new Objetos.Material(mat.Nombre, Objetos.Rareza.Normal), cantidad);
                        }
                    }
                }
                else
                {
                    juego.Ui.WriteLine("No encontraste materiales en este nodo.");
                }
                try
                {
                    var biomaLog = juego.mapa?.UbicacionActual?.Region ?? "?";
                    var rarezaLog = string.IsNullOrEmpty(nodo.Rareza) ? "" : nodo.Rareza;
                    MiJuegoRPG.Motor.Servicios.Logger.Info($"[Recolectar][OK] Nodo='{nodo.Nombre}' Bioma='{biomaLog}' Rareza='{rarezaLog}' Total={totalObtenido} CD={nodo.CooldownEfectivo()}s");
                }
                catch { }
                if (juego.jugador != null)
                    progressionService.AplicarExpRecoleccion(juego.jugador, tipo);
                // Hook de acciones: registrar recolección exitosa
                try { if (juego.jugador != null) MiJuegoRPG.Motor.Servicios.AccionRegistry.Instancia.RegistrarAccion("RecolectarMaterial", juego.jugador); } catch { }
                if (juego.jugador != null)
                {
                    // Registrar actividad y evaluar clases dinámicas
                    juego.jugador.RegistrarActividad($"Recoleccion.{tipo}");
                    try { juego.claseService?.Evaluar(juego.jugador); } catch { }
                }
            }
            InputService.Pausa();
        }

        /// <summary>
        /// Verifica si el inventario del jugador contiene una herramienta que satisface el requisito del nodo.
        /// - Coincidencia exacta case-insensitive prioritaria.
        /// - Fallback: contiene (case-insensitive) para tolerar variantes como "Pico de Hierro" vs "Pico".
        /// - Null-safe para partidas antiguas sin lista de objetos inicializada.
        /// </summary>
        private static bool TieneHerramientaRequerida(List<ObjetoConCantidad>? lista, string requiere)
        {
            if (string.IsNullOrWhiteSpace(requiere) || lista == null || lista.Count == 0)
                return false;

            string ReqNorm(string s) => s.Trim();

            string req = ReqNorm(requiere);

            foreach (var entry in lista)
            {
                var nombre = entry?.Objeto?.Nombre;
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;
                var nom = ReqNorm(nombre);

                // 1) Coincidencia exacta (ignorando mayúsculas/minúsculas)
                if (string.Equals(nom, req, StringComparison.OrdinalIgnoreCase))
                    return true;

                // 2) Fallback: nombre contiene el requisito (p. ej., "Pico de Hierro" satisface "Pico")
                if (nom.IndexOf(req, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static string FormatearRarezaTag(string rareza)
        {
            return rareza.ToLower() switch
            {
                "raro" => " [Raro]",
                "epico" => " [Épico]",
                _ => "" // Comun u otros no se muestran para no saturar
            };
        }
    }
}
