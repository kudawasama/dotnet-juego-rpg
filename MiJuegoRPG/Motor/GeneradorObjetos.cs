namespace MiJuegoRPG.Motor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using MiJuegoRPG.Motor.Servicios;
    using MiJuegoRPG.Objetos;
    using MiJuegoRPG.PjDatos;

    public static class GeneradorObjetos
    {
        // Repositorios (carga perezosa)
        private static readonly Lazy<Servicios.Repos.ArmaRepository> ArmaRepo = new(() => new Servicios.Repos.ArmaRepository());
        private static readonly Lazy<Servicios.Repos.ArmaduraRepository> ArmaduraRepo = new(() => new Servicios.Repos.ArmaduraRepository());

        // CatÃ¡logos cargados en memoria (fallback o cache de repos)
        private static List<ArmaData>? armasDisponibles;
        private static List<ArmaduraData>? armadurasDisponibles;
        private static List<AccesorioData>? accesoriosDisponibles;
        private static List<BotasData>? botasDisponibles;
        private static List<CascoData>? cascosDisponibles;
        private static List<CinturonData>? cinturonesDisponibles;
        private static List<CollarData>? collaresDisponibles;
        private static List<PantalonData>? pantalonesDisponibles;

        // ConfiguraciÃ³n de selecciÃ³n de rareza
        public static bool UsaSeleccionPonderadaRareza { get; set; } = true;

        // -------------------- CARGA DE CATÃLOGOS --------------------
        public static void CargarEquipoAuto()
        {
            try
            {
                // Cargar RarezaConfig desde archivos si existen
                try
                {
                    var rutaPesos = PathProvider.EquipoPath("rareza_pesos.json");
                    var rutaRangos = PathProvider.EquipoPath("rareza_perfeccion.json");
                    if (File.Exists(rutaPesos) && File.Exists(rutaRangos))
                    {
                        var cfg = new RarezaConfig();
                        cfg.Cargar(rutaPesos, rutaRangos);
                        RarezaConfig.SetInstancia(cfg);
                    }
                }
                catch (Exception exCfg)
                {
                    Logger.Warn($"[Equipo] No se pudo cargar RarezaConfig: {exCfg.Message}");
                }

                // Intentar cargar desde repositorios
                try
                {
                    var todas = ArmaRepo.Value.Todas();
                    if (todas != null && todas.Count > 0)
                    {
                        armasDisponibles = new List<ArmaData>(todas);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[Equipo] Error cargando armas repo: {ex.Message}");
                }

                try
                {
                    var todas = ArmaduraRepo.Value.Todas();
                    if (todas != null && todas.Count > 0)
                    {
                        armadurasDisponibles = new List<ArmaduraData>(todas);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[Equipo] Error cargando armaduras repo: {ex.Message}");
                }

                // Fallback: cargar desde JSON en Datos/Equipo
                var baseEquipo = PathProvider.CombineData("Equipo");
                var rutaAccesorios = Path.Combine(baseEquipo, "Accesorios.json");
                var rutaBotas = Path.Combine(baseEquipo, "Botas.json");
                var rutaCascos = Path.Combine(baseEquipo, "Cascos.json");
                var rutaCinturones = Path.Combine(baseEquipo, "Cinturones.json");
                var rutaCollares = Path.Combine(baseEquipo, "Collares.json");
                var rutaPantalones = Path.Combine(baseEquipo, "Pantalones.json");
                var rutaArmas = Path.Combine(baseEquipo, "armas.json");
                var rutaArmaduras = Path.Combine(baseEquipo, "Armaduras.json");

                if ((accesoriosDisponibles == null || accesoriosDisponibles.Count == 0) && File.Exists(rutaAccesorios))
                {
                    CargarAccesorios(rutaAccesorios);
                }

                if ((botasDisponibles == null || botasDisponibles.Count == 0) && File.Exists(rutaBotas))
                {
                    CargarBotas(rutaBotas);
                }

                if ((cascosDisponibles == null || cascosDisponibles.Count == 0) && File.Exists(rutaCascos))
                {
                    CargarCascos(rutaCascos);
                }

                if ((cinturonesDisponibles == null || cinturonesDisponibles.Count == 0) && File.Exists(rutaCinturones))
                {
                    CargarCinturones(rutaCinturones);
                }

                if ((collaresDisponibles == null || collaresDisponibles.Count == 0) && File.Exists(rutaCollares))
                {
                    CargarCollares(rutaCollares);
                }

                if ((pantalonesDisponibles == null || pantalonesDisponibles.Count == 0) && File.Exists(rutaPantalones))
                {
                    CargarPantalones(rutaPantalones);
                }

                if ((armasDisponibles == null || armasDisponibles.Count == 0) && File.Exists(rutaArmas))
                {
                    CargarArmas(rutaArmas);
                }

                if ((armadurasDisponibles == null || armadurasDisponibles.Count == 0) && File.Exists(rutaArmaduras))
                {
                    CargarArmaduras(rutaArmaduras);
                }

                Logger.Info($"[Equipo] Armas:{armasDisponibles?.Count ?? 0} Armaduras:{armadurasDisponibles?.Count ?? 0} Accesorios:{accesoriosDisponibles?.Count ?? 0} Botas:{botasDisponibles?.Count ?? 0} Cascos:{cascosDisponibles?.Count ?? 0} Cinturones:{cinturonesDisponibles?.Count ?? 0} Collares:{collaresDisponibles?.Count ?? 0} Pantalones:{pantalonesDisponibles?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Equipo] Error en carga automÃ¡tica: {ex.Message}");
            }
        }

        public static void CargarAccesorios(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                accesoriosDisponibles = JsonSerializer.Deserialize<List<AccesorioData>>(jsonString) ?? new List<AccesorioData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar accesorios: {ex.Message}");
                accesoriosDisponibles = new List<AccesorioData>();
            }
        }

        public static void CargarArmas(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                armasDisponibles = JsonSerializer.Deserialize<List<ArmaData>>(jsonString) ?? new List<ArmaData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Legacy] Error al cargar armas: {ex.Message}");
                armasDisponibles = new List<ArmaData>();
            }
        }

        public static void CargarArmaduras(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                armadurasDisponibles = JsonSerializer.Deserialize<List<ArmaduraData>>(jsonString) ?? new List<ArmaduraData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Legacy] Error al cargar armaduras: {ex.Message}");
                armadurasDisponibles = new List<ArmaduraData>();
            }
        }

        public static void CargarBotas(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                botasDisponibles = JsonSerializer.Deserialize<List<BotasData>>(jsonString) ?? new List<BotasData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar botas: {ex.Message}");
                botasDisponibles = new List<BotasData>();
            }
        }

        public static void CargarCinturones(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                cinturonesDisponibles = JsonSerializer.Deserialize<List<CinturonData>>(jsonString) ?? new List<CinturonData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar cinturones: {ex.Message}");
                cinturonesDisponibles = new List<CinturonData>();
            }
        }

        public static void CargarCascos(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                cascosDisponibles = JsonSerializer.Deserialize<List<CascoData>>(jsonString) ?? new List<CascoData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar cascos: {ex.Message}");
                cascosDisponibles = new List<CascoData>();
            }
        }

        public static void CargarCollares(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                collaresDisponibles = JsonSerializer.Deserialize<List<CollarData>>(jsonString) ?? new List<CollarData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar collares: {ex.Message}");
                collaresDisponibles = new List<CollarData>();
            }
        }

        public static void CargarPantalones(string rutaArchivo)
        {
            try
            {
                var jsonString = File.ReadAllText(rutaArchivo);
                pantalonesDisponibles = JsonSerializer.Deserialize<List<PantalonData>>(jsonString) ?? new List<PantalonData>();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al cargar pantalones: {ex.Message}");
                pantalonesDisponibles = new List<PantalonData>();
            }
        }

        // -------------------- GENERACIÃ“N --------------------
        public static Arma GenerarArmaAleatoria(int nivelJugador)
        {
            List<ArmaData>? fuente = null;
            try
            {
                var todas = ArmaRepo.Value.Todas();
                if (todas != null && todas.Count > 0)
                {
                    fuente = new List<ArmaData>(todas);
                }
            }
            catch
            {
                // ignorar
            }

            if (fuente == null || fuente.Count == 0)
            {
                if (armasDisponibles == null || armasDisponibles.Count == 0)
                {
                    throw new InvalidOperationException("No hay armas disponibles para generar.");
                }

                fuente = armasDisponibles;
            }

            var baseData = ElegirAleatorio(fuente, ad => NormalizarRarezaTexto(ad.Rareza ?? "Normal"));
            var rand = RandomService.Instancia;

            var candidatas = CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv);
            var rarezaElegida = ElegirRarezaPonderada(candidatas);
            (int pMin, int pMax) = IntersectarPerfeccion(rarezaElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);

            int perfeccion = baseData.Perfeccion;
            if (perfeccion <= 0 || perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue)
            {
                perfeccion = rand.Next(pMin, pMax + 1);
            }

            int nivel = baseData.NivelRequerido;
            if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
            {
                int nmin = Math.Max(1, baseData.NivelMin.Value);
                int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                nivel = rand.Next(nmin, nmax + 1);
            }

            int danoBase = baseData.Daño;
            if (baseData.DañoMin.HasValue && baseData.DañoMax.HasValue)
            {
                int dmin = Math.Max(0, baseData.DañoMin.Value);
                int dmax = Math.Max(dmin, baseData.DañoMax.Value);
                danoBase = rand.Next(dmin, dmax + 1);
            }
            else if (baseData.DañoFisico.HasValue || baseData.DañoMagico.HasValue)
            {
                danoBase = Math.Max(baseData.DañoFisico ?? 0, baseData.DañoMagico ?? 0);
                if (danoBase == 0)
                {
                    danoBase = baseData.Daño;
                }
            }

            int danoFinal = (int)Math.Round(danoBase * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
            return new Arma(baseData.Nombre, danoFinal, nivel, rarezaElegida, baseData.Tipo, perfeccion, 0);
        }

        public static Armadura GenerarArmaduraAleatoria(int nivelJugador)
        {
            List<ArmaduraData>? fuente = null;
            try
            {
                var todas = ArmaduraRepo.Value.Todas();
                if (todas != null && todas.Count > 0)
                {
                    fuente = new List<ArmaduraData>(todas);
                }
            }
            catch
            {
                // ignorar
            }

            if (fuente == null || fuente.Count == 0)
            {
                if (armadurasDisponibles == null || armadurasDisponibles.Count == 0)
                {
                    throw new InvalidOperationException("No hay armaduras disponibles para generar.");
                }

                fuente = armadurasDisponibles;
            }

            var rand = RandomService.Instancia;
            var baseData = fuente[rand.Next(fuente.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);

            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var armadura = new Armadura(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                armadura.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        armadura.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return armadura;
        }

        public static Accesorio GenerarAccesorioAleatorio(int nivelJugador)
        {
            if (accesoriosDisponibles == null || accesoriosDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay accesorios disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = accesoriosDisponibles[rand.Next(accesoriosDisponibles.Count)];
            var permitidas = CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv);
            var rzElegida = ElegirRarezaPonderada(permitidas);
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
                ? rand.Next(pMin, pMax + 1)
                : baseData.Perfeccion;

            int nivel = baseData.Nivel;
            if (baseData.NivelMin.HasValue && baseData.NivelMax.HasValue)
            {
                int nmin = Math.Max(1, baseData.NivelMin.Value);
                int nmax = Math.Max(nmin, baseData.NivelMax.Value);
                nivel = rand.Next(nmin, nmax + 1);
            }

            int bonifAtaque = (int)Math.Round(baseData.BonificacionAtaque * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
            int bonifDefensa = (int)Math.Round(baseData.BonificacionDefensa * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
            var accesorio = new Accesorio(baseData.Nombre, bonifAtaque, bonifDefensa, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            return accesorio;
        }

        public static Botas GenerarBotasAleatorias(int nivelJugador)
        {
            if (botasDisponibles == null || botasDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay botas disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = botasDisponibles[rand.Next(botasDisponibles.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var botas = new Botas(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                botas.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        botas.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return botas;
        }

        public static Cinturon GenerarCinturonAleatorio(int nivelJugador)
        {
            if (cinturonesDisponibles == null || cinturonesDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay cinturones disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = cinturonesDisponibles[rand.Next(cinturonesDisponibles.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var cinturon = new Cinturon(baseData.Nombre, bonifCarga, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                cinturon.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        cinturon.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return cinturon;
        }

        public static Collar GenerarCollarAleatorio(int nivelJugador)
        {
            if (collaresDisponibles == null || collaresDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay collares disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = collaresDisponibles[rand.Next(collaresDisponibles.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var collar = new Collar(baseData.Nombre, bonifDefensa, bonifEnergia, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                collar.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        collar.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return collar;
        }

        public static Pantalon GenerarPantalonAleatorio(int nivelJugador)
        {
            if (pantalonesDisponibles == null || pantalonesDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay pantalones disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = pantalonesDisponibles[rand.Next(pantalonesDisponibles.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var pantalon = new Pantalon(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                pantalon.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        pantalon.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return pantalon;
        }

        public static Casco GenerarCascoAleatorio(int nivelJugador)
        {
            if (cascosDisponibles == null || cascosDisponibles.Count == 0)
            {
                throw new InvalidOperationException("No hay cascos disponibles para generar.");
            }

            var rand = RandomService.Instancia;
            var baseData = cascosDisponibles[rand.Next(cascosDisponibles.Count)];
            var rzElegida = ElegirRarezaPonderada(CrearListaRarezasPermitidas(baseData.Rareza, baseData.RarezasPermitidasCsv));
            (int pMin, int pMax) = IntersectarPerfeccion(rzElegida, baseData.PerfeccionMin, baseData.PerfeccionMax);
            int perfeccion = baseData.Perfeccion <= 0 || baseData.Perfeccion > 100 || baseData.PerfeccionMin.HasValue || baseData.PerfeccionMax.HasValue
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
            var casco = new Casco(baseData.Nombre, defensaFinal, nivel, rzElegida, baseData.TipoObjeto, perfeccion)
            {
                SetId = baseData.SetId,
            };

            if (baseData.HabilidadesOtorgadas != null && baseData.HabilidadesOtorgadas.Count > 0)
            {
                casco.HabilidadesOtorgadas = new List<HabilidadOtorgadaRef>();
                foreach (var h in baseData.HabilidadesOtorgadas)
                {
                    if (!string.IsNullOrWhiteSpace(h.Id))
                    {
                        casco.HabilidadesOtorgadas.Add(new HabilidadOtorgadaRef { Id = h.Id!, NivelMinimo = h.NivelMinimo ?? 1 });
                    }
                }
            }

            return casco;
        }

        // -------------------- HELPERS --------------------
        private static List<string> CrearListaRarezasPermitidas(string? rarezaBase, string? csv)
        {
            var lista = new List<string>();
            if (!string.IsNullOrWhiteSpace(csv))
            {
                foreach (var r in csv.Split(','))
                {
                    var s = NormalizarRarezaTexto(r.Trim());
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        lista.Add(s);
                    }
                }
            }

            if (lista.Count == 0)
            {
                var s = NormalizarRarezaTexto(rarezaBase ?? "Normal");
                if (!string.IsNullOrWhiteSpace(s))
                {
                    lista.Add(s);
                }
                else
                {
                    lista.Add("Normal");
                }
            }

            return lista;
        }

        private static (int Min, int Max) IntersectarPerfeccion(string rzElegida, int? minItem, int? maxItem)
        {
            (int min, int max) = RangoPerfeccionPorRareza(rzElegida);
            if (minItem.HasValue && maxItem.HasValue)
            {
                int pMin = Math.Max(min, Math.Clamp(minItem.Value, 0, 100));
                int pMax = Math.Min(max, Math.Clamp(maxItem.Value, 0, 100));
                if (pMin <= pMax)
                {
                    return (pMin, pMax);
                }
            }

            return (min, max);
        }

        private static T ElegirAleatorio<T>(IReadOnlyList<T> lista, Func<T, string> rarezaSelector)
        {
            var rand = RandomService.Instancia;
            var cfg = RarezaConfig.Instancia;
            if (!UsaSeleccionPonderadaRareza || cfg == null)
            {
                return lista[rand.Next(lista.Count)];
            }

            double totalPeso = 0;
            var acumulados = new double[lista.Count];
            for (int i = 0; i < lista.Count; i++)
            {
                var rz = rarezaSelector(lista[i]);
                cfg.Pesos.TryGetValue(rz, out var peso);
                if (peso <= 0)
                {
                    peso = 1;
                }

                totalPeso += peso;
                acumulados[i] = totalPeso;
            }

            double tiro = rand.NextDouble() * totalPeso;
            for (int i = 0; i < acumulados.Length; i++)
            {
                if (tiro < acumulados[i])
                {
                    return lista[i];
                }
            }

            return lista[^1];
        }

        private static string NormalizarRarezaTexto(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return "Normal";
            }

            s = s.Replace("Ã³", "o").Replace("Ã“", "O").Replace("Ãº", "u").Replace("Ãš", "U").Replace("Ã¡", "a").Replace("Ã", "A").Replace("Ã©", "e").Replace("Ã‰", "E").Replace("Ã­", "i").Replace("Ã", "I");
            if (string.Equals(s, "Comun", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "ComÃºn", StringComparison.OrdinalIgnoreCase))
            {
                return "Normal";
            }

            if (string.Equals(s, "Raro", StringComparison.OrdinalIgnoreCase))
            {
                return "Rara";
            }

            return s;
        }

        private static (int Min, int Max) RangoPerfeccionPorRareza(string rz)
        {
            var cfg = RarezaConfig.Instancia;
            if (cfg != null && cfg.RangosPerfeccion.TryGetValue(rz, out var r))
            {
                return r;
            }

            return rz switch
            {
                "Normal" => (50, 50),
                "Superior" => (51, 60),
                "Rara" => (61, 75),
                "Epica" => (76, 90),
                "Ã‰pica" => (76, 90),
                "Legendaria" => (91, 100),
                _ => (50, 50),
            };
        }

        private static string ElegirRarezaPonderada(List<string> candidatas)
        {
            if (candidatas == null || candidatas.Count == 0)
            {
                return "Normal";
            }

            var cfg = RarezaConfig.Instancia;
            var rand = RandomService.Instancia;
            if (cfg == null || !UsaSeleccionPonderadaRareza)
            {
                return candidatas.Count == 1 ? candidatas[0] : candidatas[rand.Next(candidatas.Count)];
            }

            double total = 0;
            var acumulados = new List<(string Rz, double Acum)>(candidatas.Count);
            foreach (var r in candidatas)
            {
                cfg.Pesos.TryGetValue(r, out var peso);
                if (peso <= 0)
                {
                    peso = 1;
                }

                total += peso;
                acumulados.Add((r, total));
            }

            double tiro = rand.NextDouble() * total;
            for (int i = 0; i < acumulados.Count; i++)
            {
                if (tiro < acumulados[i].Acum)
                {
                    return acumulados[i].Rz;
                }
            }

            return candidatas[^1];
        }
    }
}
