using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Dominio; // Enums dominio
using MiJuegoRPG.Motor.Servicios; // GuardadoService, ProgressionService, etc.
using Serv = MiJuegoRPG.Motor.Servicios;
using TipoEncuentro = MiJuegoRPG.Motor.Servicios.TipoEncuentro;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
    public static Juego? Instancia { get; private set; }
        // Acción de recolección sobre un nodo específico
    // Método legado de recolección eliminado: ahora gestionado por RecoleccionService
    
   
    

        // Método para generar un material aleatorio (stub temporal)
        public Material GenerarMaterialAleatorio()
        {
            // Stub temporal: retorna un material de prueba
            return new Material("Madera", MiJuegoRPG.Objetos.Rareza.Normal);
        }
        public void Iniciar()
        {
            // Menú principal del juego
            bool salir = false;
            while (!salir)
            {
                Ui.WriteLine("\n=== Menú Principal ===");
                Ui.WriteLine("1. Estado del personaje");
                Ui.WriteLine("2. Ir a ubicación actual");
                Ui.WriteLine("3. Inventario");
                Ui.WriteLine("4. Guardar personaje");
                Ui.WriteLine("5. Menú administrador");
                Ui.WriteLine("6. Opciones");
                Ui.WriteLine("0. Salir del juego");
                string opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1":
                        if (jugador != null) MostrarEstadoPersonaje(jugador);
                        break;
                    // Puedes agregar más casos aquí para otras opciones del menú
                    case "2":
                        MostrarMenuPorUbicacion();
                        break;
                    case "3":
                        GestionarInventario();
                        break;
                    case "4":
                        GuardarPersonaje();
                        break;
                    case "5":
                        var menuAdmin = new MiJuegoRPG.Motor.Menus.MenuAdmin(this);
                        menuAdmin.MostrarMenuAdmin();
                        break;
                    case "6":
                        var menuOpc = new MiJuegoRPG.Motor.Menus.MenuOpciones(this);
                        menuOpc.Mostrar();
                        break;
                    case "0":
                        salir = true;
                        break;
                    default:
                        Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
        // Comando de administración para teletransportar al jugador a cualquier sector por Id
        public void TeletransportarASector(string idSector)
        {
            var destino = estadoMundo.Ubicaciones.Find(u => u.Id == idSector);
            if (destino != null)
            {
                var sectorData = mapa.ObtenerSectores().Find(s => s.Id == destino.Id);
                if (sectorData != null)
                {
                    mapa.UbicacionActual = sectorData;
                    try { recoleccionService.AlEntrarSector(sectorData.Id); } catch { }
                    Ui.WriteLine($"[ADMIN] Teletransportado a: {destino.Nombre} (ID: {destino.Id})");
                }
                else
                {
                    Ui.WriteLine($"[ADMIN] No se encontró el SectorData correspondiente al destino con Id: {destino.Id}");
                }
                MostrarMenuPorUbicacion();
            }
            else
            {
                Ui.WriteLine($"[ADMIN] No se encontró el sector con Id: {idSector}");
            }
        }

    // ProcesarComando eliminado: ahora los comandos admin están aislados en MenuAdmin.
            
        
        public void CrearPersonaje()
        {
            Ui.WriteLine("=== Creación de Personaje ===");
            jugador = MiJuegoRPG.Motor.CreadorPersonaje.CrearSinClase();
            // Ubicación inicial: buscar la ciudad principal por propiedad CiudadPrincipal
            var ciudadPrincipal = estadoMundo.Ubicaciones.Find(u => u != null && u.CiudadPrincipal);
            if (ciudadPrincipal != null)
            {
                var sectorData = mapa.ObtenerSectores().Find(s => s.Id == ciudadPrincipal.Id);
                if (sectorData != null)
                {
                    mapa.UbicacionActual = sectorData;
                    jugador.UbicacionActualId = sectorData.Id;
                }
            }
            else if (estadoMundo.Ubicaciones.Count > 0)
            {
                var sectorData = mapa.ObtenerSectores().Find(s => s.Id == estadoMundo.Ubicaciones[0].Id);
                if (sectorData != null)
                {
                    mapa.UbicacionActual = sectorData;
                    jugador.UbicacionActualId = sectorData.Id;
                }
            }
            else
                throw new Exception("No hay ubicaciones disponibles para asignar al personaje.");
            var nombreUbic = mapa.UbicacionActual != null ? mapa.UbicacionActual.Nombre : "[sin ubicación]";
            Ui.WriteLine($"Personaje creado: {jugador.Nombre} en {nombreUbic}");
            // ...después de crear el personaje y asignar atributos base...
            jugador.Estadisticas = new Estadisticas(jugador.AtributosBase);
            jugador.ManaActual = jugador.ManaMaxima;
            // Aplicar preferencias iniciales del logger del PJ (si existen)
            try { AplicarPreferenciasLoggerDeJugador(); } catch { }
        }
        // Menú de recolección fuera de ciudad
        public void MostrarMenuRecoleccion()
        {
            Ui.WriteLine("=== Menú de Recolección ===");
            Ui.WriteLine("1. Recolectar");
            Ui.WriteLine("2. Minar");
            Ui.WriteLine("3. Talar");
            Ui.WriteLine("0. Volver");
            string opcion = InputService.LeerOpcion("Selecciona una acción: ");
            TipoRecoleccion[] tipos = { TipoRecoleccion.Recolectar, TipoRecoleccion.Minar, TipoRecoleccion.Talar };
            int tipoIdx = -1;
            if (opcion == "1" || opcion == "2" || opcion == "3") tipoIdx = int.Parse(opcion) - 1;
            if (tipoIdx >= 0 && tipoIdx < tipos.Length)
            {
                // Obtener nodos disponibles (por bioma o personalizados)
                var sector = mapa.UbicacionActual;
                var tipoAccion = tipos[tipoIdx];
                List<MiJuegoRPG.Motor.NodoRecoleccion> nodos = new List<MiJuegoRPG.Motor.NodoRecoleccion>();
                // Si el sector tiene nodos personalizados, usarlos
                if (sector != null && sector.NodosRecoleccion != null && sector.NodosRecoleccion.Count > 0)
                {
                    nodos.AddRange(sector.NodosRecoleccion);
                }
                // Si no hay nodos personalizados, usar los del bioma
                if (nodos.Count == 0 && sector != null && !string.IsNullOrWhiteSpace(sector.Region))
                {
                    nodos.AddRange(MiJuegoRPG.Motor.TablaBiomas.GenerarNodosParaBioma(sector.Region));
                }
                if (nodos.Count == 0)
                {
                    Ui.WriteLine("No hay nodos de recolección disponibles en este sector.");
                    InputService.Pausa("Presiona cualquier tecla para volver...");
                    MostrarMenuPorUbicacion();
                    return;
                }
                // Mostrar submenú de nodos
                Ui.WriteLine("--- Selecciona un nodo de recolección ---");
                for (int i = 0; i < nodos.Count; i++)
                {
                    Ui.WriteLine($"{i + 1}. {nodos[i].Nombre}");
                }
                Ui.WriteLine("0. Volver");
                var nodoOpcion = InputService.LeerOpcion("Nodo: ");
                if (nodoOpcion == "0")
                {
                    MostrarMenuRecoleccion();
                    return;
                }
                if (int.TryParse(nodoOpcion, out int nodoIdx) && nodoIdx > 0 && nodoIdx <= nodos.Count)
                {
                    // Llamar al servicio (menú legacy interno todavía usa este bloque)
                    var tipoRecoleccion = tipoAccion switch {
                        MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar => MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar,
                        MiJuegoRPG.Dominio.TipoRecoleccion.Minar => MiJuegoRPG.Dominio.TipoRecoleccion.Minar,
                        MiJuegoRPG.Dominio.TipoRecoleccion.Talar => MiJuegoRPG.Dominio.TipoRecoleccion.Talar,
                        _ => MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar
                    };
                    recoleccionService.EjecutarAccion(tipoRecoleccion, nodos[nodoIdx - 1]);
                }
                else
                {
                    Ui.WriteLine("Opción de nodo no válida.");
                    MostrarMenuRecoleccion();
                }
            }
            else if (opcion == "0")
            {
                MostrarMenuPorUbicacion();
            }
            else
            {
                Ui.WriteLine("Opción no válida en el menú de recolección.");
            }

        }

        // Método para cargar personaje desde el menú (extraído del default)
        public void MostrarMenuCargarPersonaje()
        {
            var pj = guardadoService.CargarInteractivo();
            if (pj == null)
            {
                Ui.WriteLine("No se pudo cargar el personaje.");
                return;
            }
            jugador = pj;
            Ui.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
            if (!string.IsNullOrEmpty(jugador.UbicacionActualId))
            {
                var sectorData = mapa.ObtenerSectores().Find(s => s.Id == jugador.UbicacionActualId);
                if (sectorData != null)
                    mapa.UbicacionActual = sectorData;
            }
        }


        /// <summary>
        /// Sincroniza el tiempo del juego con el tiempo real del sistema.
        /// </summary>
        public void SincronizarTiempoConRelojReal()
        {
            // Calcula la diferencia en días entre la fecha de inicio y la fecha actual del sistema
            var ahora = DateTime.Now;
            var diasTranscurridos = (ahora.Date - FechaInicio.Date).Days;
            if (diasTranscurridos < 0) diasTranscurridos = 0;
            // Calcula los minutos transcurridos desde la FechaInicio hasta ahora
            MinutosMundo = (int)(ahora - FechaInicio).TotalMinutes;
        }

        public void IrATienda()
        {
            // Implementación pendiente
        }

        // --- Control de tiempo para QA/admin ---
        /// <summary>
        /// Ajusta los minutos transcurridos en el mundo. Acepta valores positivos o negativos.
        /// </summary>
        public void AjustarMinutosMundo(int delta)
        {
            try
            {
                checked { MinutosMundo += delta; }
            }
            catch (OverflowException)
            {
                MinutosMundo = delta > 0 ? int.MaxValue : int.MinValue;
            }
            if (MinutosMundo < 0) MinutosMundo = 0; // no permitir tiempo negativo desde el inicio
            MiJuegoRPG.Motor.Servicios.Logger.Info($"[Tiempo] Nuevo tiempo del mundo: {FormatoRelojMundo}");
        }

        /// <summary>
        /// Establece la hora del día (0-23) conservando la fecha y los minutos/segundos actuales.
        /// </summary>
        public void EstablecerHoraDelDia(int hora)
        {
            hora = Math.Clamp(hora, 0, 23);
            var actual = FechaActual;
            var objetivo = new DateTime(actual.Year, actual.Month, actual.Day, hora, actual.Minute, actual.Second);
            var deltaMin = (int)(objetivo - actual).TotalMinutes;
            AjustarMinutosMundo(deltaMin);
        }
        public void ProgresionPorActividad(string actividad)
        {
            // Aquí puedes definir la lógica de progresión según la actividad
            // Por ejemplo, aumentar experiencia, estadísticas, etc.
            MiJuegoRPG.Motor.Servicios.Logger.Info($"Progresión registrada por actividad: {actividad}");
        }
        public static Juego? ObtenerInstanciaActual()
        {
            return InstanciaActual;
        }
        // Campos y propiedades
        public Mapa mapa;
        public int MinutosMundo { get; set; } = 0;
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaActual => FechaInicio.AddMinutes(MinutosMundo);
        public string FormatoRelojMundo => $"[{FechaActual:dd-MM-yyyy} // {FechaActual:HH:mm:ss} hrs]";
    public static Juego? InstanciaActual { get; private set; }
    // Campo Random local eliminado: ahora todo usa RandomService.Instancia
    public EnergiaService energiaService { get; }
    public MiJuegoRPG.Interfaces.IUserInterface Ui { get; }
    private readonly MiJuegoRPG.Motor.Servicios.ProgressionService? progressionService;
    private GuardadoService guardadoService; // Nuevo servicio de guardado
    // Exposición controlada para servicios internos (evitar reflection)
    public MiJuegoRPG.Motor.Servicios.ProgressionService ProgressionService => progressionService!;
    public MiJuegoRPG.Motor.Servicios.RecoleccionService recoleccionService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.EncuentrosService? encuentrosService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.ClaseDinamicaService claseService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.ReputacionService reputacionService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.SupervivenciaService supervivenciaService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.SupervivenciaRuntimeService supervivenciaRuntimeService { get; private set; }
        public MiJuegoRPG.Personaje.Personaje? jugador;
        public MenusJuego menuPrincipal;
        public EstadoMundo estadoMundo;
        
        public MotorEventos motorEventos;
        public MotorCombate motorCombate;
        public MotorMisiones motorMisiones;
        public MotorEntrenamiento motorEntrenamiento;
        public MotorInventario motorInventario;
        public MotorRutas motorRutas;
        private MiJuegoRPG.Motor.Menus.MenuCiudad menuCiudad;

        // Constructor
        // Permite inyectar una IU alternativa (p.ej., SilentUserInterface) desde tests
        public static Func<MiJuegoRPG.Interfaces.IUserInterface>? UiFactory { get; set; }

        public Juego()
        {
            energiaService = new EnergiaService();
            Ui = UiFactory != null ? UiFactory() : new MiJuegoRPG.Motor.Servicios.ConsoleUserInterface();
            // Redirigir Logger a la UI seleccionada (no forzar nivel para respetar flags CLI)
            MiJuegoRPG.Motor.Servicios.Logger.SetSink(Ui);
            progressionService = new MiJuegoRPG.Motor.Servicios.ProgressionService();
            progressionService.Verbose = true; // se puede ajustar dinámicamente
            guardadoService = new GuardadoService(); // inicializa servicio de guardado
            recoleccionService = new MiJuegoRPG.Motor.Servicios.RecoleccionService(this);
            // Inicializamos EncuentrosService una vez por juego y cargamos tablas
            encuentrosService = new Serv.EncuentrosService();
            if (!encuentrosService.CargarDesdeJsonPorDefecto()) encuentrosService.RegistrarTablasPorDefecto();
            claseService = new MiJuegoRPG.Motor.Servicios.ClaseDinamicaService(this);
            reputacionService = new MiJuegoRPG.Motor.Servicios.ReputacionService(this);
            // Supervivencia: cargar configuración y preparar runtime (feature flag desactivada por defecto)
            supervivenciaService = new MiJuegoRPG.Motor.Servicios.SupervivenciaService();
            try { supervivenciaService.CargarConfig(); } catch { }
            supervivenciaRuntimeService = new MiJuegoRPG.Motor.Servicios.SupervivenciaRuntimeService(supervivenciaService)
            {
                FeatureEnabled = false
            };
            Instancia = this;
            string carpetaMapas = MiJuegoRPG.Motor.Servicios.PathProvider.MapasDir();
            mapa = MapaLoader.CargarMapaCompleto(carpetaMapas);
            InstanciaActual = this;
            menuPrincipal = new MenusJuego(this);
            estadoMundo = new EstadoMundo();
            // Cargar enemigos desde carpeta DatosJuego/enemigos (si existe) o caer a enemigos.json
            GeneradorEnemigos.CargarEnemigosPorDefecto();

            // Llenar estadoMundo.Ubicaciones con todos los sectores del mapa
            foreach (var sector in mapa.ObtenerSectores())
            {
                // Convertir SectorData a Ubicacion
                var ubic = new Ubicacion
                {
                    Id = sector.Id,
                    Nombre = sector.Nombre,
                    Tipo = sector.Tipo,
                    Descripcion = sector.Descripcion,
                    Desbloqueada = sector.CiudadInicial || sector.Id == "8_23", // Solo la ciudad inicial desbloqueada por defecto
                    // Puedes mapear más campos si lo necesitas
                };
                estadoMundo.Ubicaciones.Add(ubic);
            }
            // Desbloquear sectores adyacentes a la ciudad inicial
            var bairan = estadoMundo.Ubicaciones.Find(u => u.Nombre == "Bairan" || u.Nombre == "Ciudad de Bairan");
            if (bairan != null)
            {
                var sectorBairan = mapa.ObtenerSectores().Find(s => s.Nombre == "Bairan" || s.Nombre == "Ciudad de Bairan");
                if (sectorBairan != null)
                {
                    foreach (var idConexion in sectorBairan.Conexiones)
                    {
                        var ubicAdj = estadoMundo.Ubicaciones.Find(u => u.Id == idConexion);
                        if (ubicAdj != null)
                            ubicAdj.Desbloqueada = true;
                    }
                }
                // Asignar el sector de Bairan como ubicación actual si existe
                if (sectorBairan != null)
                {
                    mapa.UbicacionActual = sectorBairan;
                    try { recoleccionService.AlEntrarSector(sectorBairan.Id); } catch { }
                }
                // DEBUG opcional: comentar para reducir ruido en pruebas
                // Console.WriteLine($"[DEBUG] Ubicación actual: {mapa.UbicacionActual.Nombre} (ID: {mapa.UbicacionActual.Id})");
                // if (sectorBairan != null)
                // {
                //     Console.WriteLine($"[DEBUG] Conexiones de {sectorBairan.Nombre}:");
                //     foreach (var idConexion in sectorBairan.Conexiones)
                //     {
                //         var ubicAdj = estadoMundo.Ubicaciones.Find(u => u.Id == idConexion);
                //         if (ubicAdj != null)
                //         {
                //             Console.WriteLine($"  - {ubicAdj.Nombre} (ID: {ubicAdj.Id}) | Desbloqueada: {ubicAdj.Desbloqueada}");
                //         }
                //         else
                //         {
                //             Console.WriteLine($"  - [NO ENCONTRADO] ID: {idConexion}");
                //         }
                //     }
                // }
            }
            else
            {
                var primerSector = mapa.ObtenerSectores().FirstOrDefault();
                if (primerSector != null)
                {
                    mapa.UbicacionActual = primerSector;
                    try { recoleccionService.AlEntrarSector(primerSector.Id); } catch { }
                }
            }
            CargarProbabilidades();
            motorEventos = new MotorEventos(this);
            motorCombate = new MotorCombate(this);
            motorMisiones = new MotorMisiones(this);
            motorEntrenamiento = new MotorEntrenamiento(this);
            motorInventario = new MotorInventario(this);
            motorRutas = new MotorRutas(this);
            menuCiudad = new MiJuegoRPG.Motor.Menus.MenuCiudad(this);
            // Listeners básicos de eventos
            var bus = MiJuegoRPG.Motor.Servicios.BusEventos.Instancia;
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoAtributoSubido>(e => {
                MiJuegoRPG.Motor.Servicios.Logger.Info($"[EVENTO] Atributo subido: {e.Atributo} = {e.NuevoValor:F2}");
            });
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoNivelSubido>(e => {
                MiJuegoRPG.Motor.Servicios.Logger.Info($"[EVENTO] Nivel del jugador ahora: {e.Nivel}");
            });
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoMisionCompletada>(e => {
                MiJuegoRPG.Motor.Servicios.Logger.Info($"[EVENTO] Misión completada ({e.Id}): {e.Nombre}");
            });
            // Reputación: umbrales (12.3)
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoReputacionUmbralGlobal>(e => {
                var dir = e.Subida ? "sube" : "baja";
                var extra = string.IsNullOrWhiteSpace(e.Mensaje) ? string.Empty : $" | {e.Mensaje}";
                MiJuegoRPG.Motor.Servicios.Logger.Info($"[EVENTO] Reputación global {dir} de {e.ValorAnterior} a {e.ValorNuevo} (banda {e.BandaId}){extra}");
            });
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoReputacionUmbralFaccion>(e => {
                var dir = e.Subida ? "sube" : "baja";
                var extra = string.IsNullOrWhiteSpace(e.Mensaje) ? string.Empty : $" | {e.Mensaje}";
                MiJuegoRPG.Motor.Servicios.Logger.Info($"[EVENTO] Reputación facción '{e.Faccion}' {dir} de {e.ValorAnterior} a {e.ValorNuevo} (banda {e.BandaId}){extra}");
            });
            // Supervivencia: aviso al cruzar umbrales (27.9)
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoSupervivenciaUmbralCruzado>(e => {
                var txt = $"[{e.Tipo}] {e.EstadoAnterior} → {e.EstadoNuevo} ({e.Valor:P0})";
                if (e.EstadoNuevo == "CRÍTICO")
                    MiJuegoRPG.Motor.Servicios.Logger.Warn(txt);
                else
                    MiJuegoRPG.Motor.Servicios.Logger.Info(txt);
                try { Ui?.WriteLine($"[Supervivencia] {txt}"); } catch { }
            });
        }
        // Sincroniza y muestra el menú correcto según la ubicación actual
        public void MostrarMenuPorUbicacion()
        {
            bool salir = false;
            MostrarMenuPorUbicacion(ref salir);
        }
        public void MostrarMenuPorUbicacion(ref bool salir)
        {
            if (mapa.UbicacionActual != null && !string.IsNullOrWhiteSpace(mapa.UbicacionActual.Tipo))
            {
                var s = mapa.UbicacionActual;
                bool esCiudad = s.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase);
                // Solo mostrar menú de ciudad si es el centro de ciudad o está marcada como ciudad principal
                bool esCentro = s.EsCentroCiudad || s.CiudadPrincipal;
                if (esCiudad && esCentro)
                {
                    MostrarMenuCiudad(ref salir);
                }
                else
                {
                    MostrarMenuFueraCiudad(ref salir);
                }
            }
            else
            {
                Ui.WriteLine("No estás en ninguna ubicación válida. Volviendo al menú principal...");
                salir = true;
            }
        }

        // Menú de ciudad
        public void MostrarMenuCiudad(ref bool salir)
        {
            menuCiudad.MostrarMenuCiudad(ref salir);
        }

        // Menú fuera de ciudad
        private MiJuegoRPG.Motor.Menus.MenuFueraCiudad? _menuFueraCiudad;
        public void MostrarMenuFueraCiudad(ref bool salir)
        {
            if (_menuFueraCiudad == null) _menuFueraCiudad = new MiJuegoRPG.Motor.Menus.MenuFueraCiudad(this);
            _menuFueraCiudad.MostrarMenuFueraCiudad(ref salir);
        }

        // Menú fijo disponible en todos los menús
        public void MostrarMenuFijo(ref bool salir)
        {
            // Delegar la lógica al menú fijo modularizado
            var menuFijo = new MiJuegoRPG.Motor.Menus.MenuFijo(this);
            menuFijo.MostrarMenuFijo(ref salir);
        }
        


    // Muestra el estado completo del personaje con explicación
    public void MostrarEstadoPersonaje(MiJuegoRPG.Personaje.Personaje pj)
    {
        EstadoPersonajePrinter.MostrarEstadoPersonaje(pj);
    }
    // Overload: permite solicitar modo detallado
    public void MostrarEstadoPersonaje(MiJuegoRPG.Personaje.Personaje pj, bool detallado)
    {
        EstadoPersonajePrinter.MostrarEstadoPersonaje(pj, detallado);
    }
        public void MostrarMenuViajar()
        {
            //Console.Clear();
            Ui.WriteLine(FormatoRelojMundo);
            Ui.WriteLine("--- Menú de Viaje ---");
            if (estadoMundo?.Ubicaciones == null || estadoMundo.Ubicaciones.Count == 0)
            {
                Ui.WriteLine("No hay ubicaciones disponibles.");
                InputService.Pausa("Presiona cualquier tecla para volver...");
                return;
            }
            int i = 1;
            foreach (var ubicacion in estadoMundo.Ubicaciones)
            {
                if (ubicacion.Desbloqueada)
                    Ui.WriteLine($"{i}. {ubicacion.Nombre} - {ubicacion.Descripcion}");
                i++;
            }
            Ui.WriteLine("0. Volver");
            var opcion = InputService.LeerOpcion("Elige tu destino: ");
            if (int.TryParse(opcion, out int seleccion) && seleccion > 0 && seleccion <= estadoMundo.Ubicaciones.Count)
            {
                var destino = estadoMundo.Ubicaciones[seleccion - 1];
                if (destino.Desbloqueada)
                {
                    var sectorData = mapa.ObtenerSectores().Find(s => s.Id == destino.Id);
                    if (sectorData != null)
                    {
                        mapa.UbicacionActual = sectorData;
                        if (jugador != null)
                            MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(jugador);
                        mapa.MoverseA(destino.Id);
                        Ui.WriteLine($"Viajaste a {destino.Nombre}.");
                        Ui.WriteLine(destino.Descripcion);
                        MostrarMenuPorUbicacion();
                        return;
                    }
                    else
                    {
                        Ui.WriteLine($"No se encontró el SectorData correspondiente a {destino.Nombre}.");
                    }
                }
                else
                {
                    Ui.WriteLine("No tienes acceso a esa ubicación.");
                }
            }
            else if (seleccion == 0)
            {
                // Volver al menú anterior
                MostrarMenuPorUbicacion();
                return;
            }
            else
            {
                Ui.WriteLine("Opción no válida.");
            }
            InputService.Pausa("Presiona cualquier tecla para continuar...");
            MostrarMenuPorUbicacion();
        }

        // Probabilidades configurables

        private void InicializarUbicaciones()
        {
            // Inicialización básica: Ciudad de Bairan
            var ciudad = new MiJuegoRPG.Motor.Ubicacion
            {
                Id = "bairan",
                Nombre = "Ciudad de Bairan",
                Tipo = "Ciudad",
                Descripcion = "Ciudad fortificada en las montañas del caos, famosa por sus minas y guerreros.",
                Desbloqueada = true,
                EventosPosibles = new List<string> { "Tienda", "Escuela de Entrenamiento", "Explorar sector", "Descansar en posada" }
            };
            var bosque = new MiJuegoRPG.Motor.Ubicacion
            {
                Id = "bosque_oscuro",
                Nombre = "Bosque Oscuro",
                Tipo = "Ruta",
                Descripcion = "Un bosque peligroso, ideal para aventureros.",
                Desbloqueada = false,
                EventosPosibles = new List<string> { "Explorar" }
            };
            var rio = new MiJuegoRPG.Motor.Ubicacion
            {
                Id = "rio_plateado",
                Nombre = "Río Plateado",
                Tipo = "Ruta",
                Descripcion = "Un río que requiere barco para cruzar.",
                Desbloqueada = false,
                Requisitos = new Dictionary<string, object> { { "Barco", true } },
                EventosPosibles = new List<string> { "Evento especial", "Encuentro enemigo" }
            };
            var ciudadBruma = new MiJuegoRPG.Motor.Ubicacion
            {
                Id = "ciudad_bruma",
                Nombre = "Ciudad Bruma",
                Tipo = "Ciudad",
                Descripcion = "Ciudad misteriosa, famosa por su magia.",
                Desbloqueada = false,
                EventosPosibles = new List<string> { "Tienda mágica", "Escuela de magia", "Explorar sector" }
            };
            // Rutas entre ubicaciones
            ciudad.Rutas.Add(new MiJuegoRPG.Motor.Ruta { Nombre = "Camino al Bosque Oscuro", Destino = "Bosque Oscuro", Desbloqueada = true });
            ciudad.Rutas.Add(new MiJuegoRPG.Motor.Ruta { Nombre = "Camino al Río Plateado", Destino = "Río Plateado", Desbloqueada = false, Requisitos = new Dictionary<string, object> { { "Barco", true } } });
            ciudad.Rutas.Add(new MiJuegoRPG.Motor.Ruta { Nombre = "Camino a Ciudad Bruma", Destino = "Ciudad Bruma", Desbloqueada = false });
            estadoMundo.Ubicaciones.Add(ciudad);
            estadoMundo.Ubicaciones.Add(bosque);
            estadoMundo.Ubicaciones.Add(rio);
            estadoMundo.Ubicaciones.Add(ciudadBruma);
        }

        public int ProbMonstruo = 40;
        public int ProbObjeto = 30;
        public int ProbMazmorra = 10;
        public int ProbEvento = 20;

        private void CargarProbabilidades()
        {
            string rutaConfig = MiJuegoRPG.Motor.Servicios.PathProvider.PjDatosPath("probabilidades.txt");
            if (File.Exists(rutaConfig))
            {
                var lineas = File.ReadAllLines(rutaConfig);
                foreach (var linea in lineas)
                {
                    var partes = linea.Split('=');
                    if (partes.Length == 2)
                    {
                        var clave = partes[0].Trim().ToLower();
                        var valor = partes[1].Trim();
                        if (int.TryParse(valor, out int prob))
                        {
                            switch (clave)
                            {
                                case "monstruo": ProbMonstruo = prob; break;
                                case "objeto": ProbObjeto = prob; break;
                                case "mazmorra": ProbMazmorra = prob; break;
                                case "evento": ProbEvento = prob; break;
                            }
                        }
                    }
                }
            }
        }

        public static string ObtenerRutaRaizProyecto()
        {
            var dir = new System.IO.DirectoryInfo(System.Environment.CurrentDirectory);
            while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, "MiJuegoRPG.sln")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName ?? System.Environment.CurrentDirectory;
        }

        // Método para mostrar la tienda (implementación básica)
        public void MostrarTienda()
        {
            // Usar ID de sector como clave canónica (evita ambigüedad de nombres)
            var ubic = mapa.UbicacionActual;
            if (ubic == null)
            {
                Ui.WriteLine("[Aviso] No hay ubicación actual válida para abrir la tienda.");
                return;
            }
            menuPrincipal.MostrarMenuTienda(ubic.Id);
        }

    

        public void MostrarMenuGuardado()
        {
            //Console.Clear();
            Ui.WriteLine(FormatoRelojMundo);
            Ui.WriteLine("=== Menú de Guardar/Cargar ===");
            Ui.WriteLine("1. Guardar partida");
            Ui.WriteLine("2. Cargar partida");
            Ui.WriteLine("3. Volver al menú principal");

            var opcion = InputService.LeerOpcion();

            switch (opcion)
            {
                case "1":
                    GuardarPersonaje();
                    break;
                case "2":
                    CargarPersonaje();
                    break;
                case "3":
                    // Volver al menú de la ciudad
                    break;
                default:
                    Ui.WriteLine("Opción no válida.");
                    break;
            }

            InputService.Pausa("\nPresiona cualquier tecla para continuar...");
        }


        // Nuevo método para explorar, que ahora delega al GeneradorEnemigos
        public void ExplorarSector()
        {
            // Ganar experiencia por explorar
            if (jugador != null)
            {
                // Antes se sumaba a campos legacy ExpDestreza/ExpAgilidad/ExpPercepcion y luego se revisaba.
                // Ahora aplicamos pequeñas fracciones directamente a los atributos base para simular micro progreso explorando.
                // Podría unificarse vía ProgressionService si se define una fuente 'exploracion'; por ahora incrementos directos controlados.
                jugador.AtributosBase.Destreza += 0.0002;
                jugador.AtributosBase.Agilidad += 0.0001;
                jugador.AtributosBase.Percepcion += 0.0002;
            }
            // Hook de acciones: registrar exploración de sector
            try { if (jugador != null) MiJuegoRPG.Motor.Servicios.AccionRegistry.Instancia.RegistrarAccion("ExplorarSector", jugador); } catch { }
            // Sistema de encuentros por bioma (instancia única por juego)
            if (encuentrosService == null)
            {
                encuentrosService = new Serv.EncuentrosService();
                if (!encuentrosService.CargarDesdeJsonPorDefecto()) encuentrosService.RegistrarTablasPorDefecto();
            }
            var bioma = mapa.UbicacionActual?.Region ?? "";
            // Aplicar tick de supervivencia al explorar (si la bandera está activada)
            try { if (jugador != null) supervivenciaRuntimeService.ApplyTick(jugador, "Explorar", bioma, 60); } catch { }
            var nivel = jugador?.Nivel ?? 1;
            int GetKills(string clave)
            {
                if (jugador == null) return 0;
                return jugador.GetKills(clave, bioma);
            }
            var res = (jugador != null)
                ? encuentrosService.Resolver(bioma, jugador, clave => GetKills(clave))
                : encuentrosService.Resolver(bioma, nivel, clave => GetKills(clave));
            switch (res.Tipo)
            {
                case TipoEncuentro.Nada:
                    Ui.WriteLine("No ha ocurrido nada especial en la exploración.");
                    InputService.Pausa("Presiona cualquier tecla para continuar...");
                    break;
                case TipoEncuentro.BotinComun:
                    {
                        int oro = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(3, 12);
                        if (jugador != null) jugador.Oro += oro;
                        Ui.WriteLine($"Encuentras un pequeño botín: {oro} de oro.");
                        InputService.Pausa();
                    }
                    break;
                case TipoEncuentro.Materiales:
                    {
                        var ubicActual = mapa.UbicacionActual;
                        var sectorActual = ubicActual != null
                            ? mapa.ObtenerSectores().Find(s => s.Id == ubicActual.Id)
                            : null;
                        var nodosMaterial = new List<NodoRecoleccion>();
                        if (sectorActual != null && sectorActual.NodosRecoleccion != null && sectorActual.NodosRecoleccion.Count > 0)
                            nodosMaterial.AddRange(sectorActual.NodosRecoleccion);
                        else if (sectorActual != null && !string.IsNullOrWhiteSpace(sectorActual.Region))
                            nodosMaterial.AddRange(MiJuegoRPG.Motor.TablaBiomas.GenerarNodosParaBioma(sectorActual.Region!));
                        if (nodosMaterial.Count > 0)
                            recoleccionService.EjecutarAccion(MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar, nodosMaterial[0]);
                        else
                            Ui.WriteLine("No hay nodos de recolección disponibles.");
                    }
                    break;
                case TipoEncuentro.NPC:
                    Ui.WriteLine("Te cruzas con un viajero. (Ganchos de misión por implementar)");
                    InputService.Pausa();
                    break;
                case TipoEncuentro.CombateComunes:
                case TipoEncuentro.CombateBioma:
                case TipoEncuentro.MiniJefe:
                    {
                        Ui.WriteLine("¡Un enemigo aparece!");
                        if (jugador != null)
                        {
                            try
                            {
                                // Si el encuentro trae un filtro de tipos (Param), úsalo para generar el enemigo acorde
                                string? filtro = res.Param;
                                // Soportar formato "lider_manada:lobo" -> usar parte derecha como filtro
                                if (!string.IsNullOrWhiteSpace(filtro) && filtro.Contains(':'))
                                    filtro = filtro.Split(':').Last();
                                var enemigo = string.IsNullOrWhiteSpace(filtro)
                                    ? GeneradorEnemigos.GenerarEnemigoAleatorio(jugador)
                                    : GeneradorEnemigos.GenerarEnemigoAleatorio(jugador, filtro);
                                GeneradorEnemigos.IniciarCombate(jugador, enemigo);
                            }
                            catch (Exception ex)
                            {
                                Ui.WriteLine($"Error al iniciar combate: {ex.Message}");
                                InputService.Pausa();
                            }
                        }
                        else
                        {
                            Ui.WriteLine("No hay personaje cargado.");
                            InputService.Pausa();
                        }
                    }
                    break;
                case TipoEncuentro.MazmorraRara:
                    Ui.WriteLine("¡Has encontrado una entrada a una mazmorra!");
                    MostrarMenuMazmorra();
                    break;
                case TipoEncuentro.EventoRaro:
                    Ui.WriteLine("¡Ocurre un evento extraño en la zona! (placeholder)");
                    InputService.Pausa();
                    break;
            }
            MostrarMenuPorUbicacion();
        }

        // Acciones de recolección, minería y tala
        // Revisa si algún atributo sube por experiencia acumulada
    // RevisarAtributosPorExperiencia eliminado: el nuevo sistema maneja experiencia y subida inmediatamente dentro de ProgressionService.

    // Método Entrenar() removido: usar motorEntrenamiento.Entrenar() directamente

        public void GestionarInventario()
        {
            motorInventario.GestionarInventario();
        }

        // Día actual del juego basado en la fecha real del sistema
        public static int DiaActual
        {
            get
            {
                // Puedes cambiar la fecha base si quieres que el día 1 sea otro
                DateTime fechaBase = new DateTime(2025, 8, 16); // Día 1 del juego
                var hoy = DateTime.Now.Date;
                int dias = (hoy - fechaBase).Days + 1;
                return dias > 0 ? dias : 1;
            }
        }

        // Método para cargar el personaje usando GuardaPersonaje
        public void CargarPersonaje()
        {
            var pj = guardadoService.CargarInteractivo();
            if (pj == null) return;
            jugador = pj;
            jugador.Estadisticas = new Estadisticas(jugador.AtributosBase);
            jugador.ManaActual = jugador.ManaMaxima;
            try { AplicarPreferenciasLoggerDeJugador(); } catch { }
            Ui.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
            // Auto-activar una clase por defecto si hay clases desbloqueadas pero no hay clase activa
            try
            {
                if (jugador.Clase == null && jugador.ClasesDesbloqueadas != null && jugador.ClasesDesbloqueadas.Count > 0)
                {
                    var seleccion = jugador.ClasesDesbloqueadas.OrderBy(n => n).First();
                    jugador.Clase = new MiJuegoRPG.Personaje.Clase { Nombre = seleccion };
                    // Recalcular estadísticas preservando ratio de maná
                    var ratio = jugador.ManaMaxima > 0 ? (double)jugador.ManaActual / jugador.ManaMaxima : 1.0;
                    jugador.Estadisticas = new Estadisticas(jugador.AtributosBase);
                    jugador.ManaActual = (int)(jugador.ManaMaxima * ratio);
                    Ui.WriteLine($"[CLASES] Clase activa auto-seleccionada: '{seleccion}'. Puedes cambiarla desde Menú Admin → opción 21.");
                }
            }
            catch { /* tolerante a guardados antiguos */ }
            if (!string.IsNullOrEmpty(jugador.UbicacionActualId))
            {
                var sectorData = mapa.ObtenerSectores().Find(s => s.Id == jugador.UbicacionActualId);
                if (sectorData != null)
                    mapa.UbicacionActual = sectorData;
            }
            else
            {
                var sectorBairan = mapa.ObtenerSectores().Find(s => s.Nombre == "Bairan" || s.Nombre == "Ciudad de Bairan");
                if (sectorBairan != null)
                {
                    foreach (var idConexion in sectorBairan.Conexiones)
                    {
                        var ubicAdj = estadoMundo.Ubicaciones.Find(u => u.Id == idConexion);
                        if (ubicAdj != null) ubicAdj.Desbloqueada = true;
                    }
                    mapa.UbicacionActual = sectorBairan;
                }
                else
                {
                    var primerSector = mapa.ObtenerSectores().FirstOrDefault();
                    if (primerSector != null) mapa.UbicacionActual = primerSector;
                }
            }
        }

        private void AplicarPreferenciasLoggerDeJugador()
        {
            if (jugador == null) return;
            // Respetar apagado total si viene de CLI (--log-off) ya aplicado en Program
            if (MiJuegoRPG.Motor.Servicios.Logger.Enabled)
                MiJuegoRPG.Motor.Servicios.Logger.Enabled = jugador.PreferenciaLoggerEnabled;
            var nivel = jugador.PreferenciaLoggerLevel ?? "Info";
            if (Enum.TryParse<MiJuegoRPG.Motor.Servicios.LogLevel>(nivel, true, out var lvl))
                MiJuegoRPG.Motor.Servicios.Logger.Level = lvl;
        }
        // Sincronizar el ID de ubicación antes de guardar
        public void GuardarPersonaje()
        {
            if (jugador == null)
            {
                Ui.WriteLine("No hay personaje para guardar.");
                return;
            }
            if (mapa.UbicacionActual != null)
                jugador.UbicacionActualId = mapa.UbicacionActual.Id;
            guardadoService.Guardar(jugador);
        }

            // Menú básico de mazmorra
        public void MostrarMenuMazmorra()
        {
            Ui.WriteLine("=== Mazmorra encontrada ===");
            Ui.WriteLine("1. Entrar en la mazmorra");
            Ui.WriteLine("2. Volver");
            var opcion = InputService.LeerOpcion();
            switch (opcion)
            {
                case "1":
                    Ui.WriteLine("¡Has entrado en la mazmorra! (Lógica por implementar)");
                    break;
                case "2":
                    Ui.WriteLine("Regresando...");
                    break;
                default:
                    Ui.WriteLine("Opción no válida.");
                    break;
            }
            InputService.Pausa("Presiona cualquier tecla para continuar...");
            MostrarMenuPorUbicacion();
        }

        // Menú básico de rutas
        public void MostrarMenuRutas()
        {
            // Mostrar sectores disponibles fuera de la ciudad
            var sectores = mapa.ObtenerSectores();
            // Buscar el sector actual por ID
            var sectorActual = mapa.UbicacionActual != null
                ? sectores.Find(s => s.Id == mapa.UbicacionActual.Id)
                : null;
            // DEBUG: Mostrar ID de ubicación actual y conexiones
            if (mapa.UbicacionActual != null)
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[DEBUG] Ubicación actual: {mapa.UbicacionActual.Nombre} (ID: {mapa.UbicacionActual.Id})");
            if (sectorActual != null)
            {
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[DEBUG] Conexiones de {sectorActual.Nombre}:");
                foreach (var idConexion in sectorActual.Conexiones)
                {
                    var conectado = sectores.Find(s => s.Id == idConexion);
                    if (conectado != null)
                        MiJuegoRPG.Motor.Servicios.Logger.Debug($"  - {conectado.Nombre} (ID: {conectado.Id})");
                    else
                        MiJuegoRPG.Motor.Servicios.Logger.Debug($"  - [NO ENCONTRADO] ID: {idConexion}");
                }
            }
            List<PjDatos.SectorData> sectoresConectados = new List<PjDatos.SectorData>();
            if (sectorActual != null && sectorActual.Conexiones != null)
            {
                foreach (var id in sectorActual.Conexiones)
                {
                    var conectado = sectores.Find(s => s.Id == id);
                    if (conectado != null)
                        sectoresConectados.Add(conectado);
                }
            }
            if (sectoresConectados.Count == 0)
            {
                Ui.WriteLine("No hay sectores conectados disponibles.");
                InputService.Pausa("Pulsa cualquier tecla para volver.");
                return;
            }
            bool volver = false;
            while (!volver)
            {
                Ui.WriteLine("=== Menú de Rutas ===");
                Ui.WriteLine("Sectores conectados:");
                for (int i = 0; i < sectoresConectados.Count; i++)
                {
                    var s = sectoresConectados[i];
                    Ui.WriteLine($"{i + 1}. {s.Nombre} - {s.Descripcion}");
                }
                Ui.WriteLine("0. Volver");
                string opcion = InputService.LeerOpcion("Selecciona el sector al que deseas viajar: ") ?? "0";
                if (opcion == "0")
                {
                    volver = true;
                    break;
                }
                if (int.TryParse(opcion, out int idx) && idx > 0 && idx <= sectoresConectados.Count)
                {
                    var destino = sectoresConectados[idx - 1];
                    // Determinar si es primera visita antes de movernos
                    bool yaDescubierto = mapa.SectoresDescubiertos.ContainsKey(destino.Id) && mapa.SectoresDescubiertos[destino.Id];
                    // Centralizar la validación y el movimiento en Mapa
                    if (mapa.MoverseA(destino.Id))
                    {
                        // Actualizar la ubicación actual del juego
                        var ubicacionOriginal = estadoMundo.Ubicaciones.FirstOrDefault(u => u.Id == destino.Id);
                        var sectorData = mapa.ObtenerSectores().Find(s => s.Id == destino.Id);
                        if (sectorData != null)
                            mapa.UbicacionActual = sectorData;
                        if (jugador != null)
                        {
                            // Aplicar micro experiencia de exploración (Percepción + bonus Agilidad si primera visita)
                            progressionService?.AplicarExpExploracion(jugador, primeraVisita: !yaDescubierto);
                            MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(jugador);
                        }
                        Ui.WriteLine($"Te has movido a: {destino.Nombre}");
                        Ui.WriteLine(destino.Descripcion);
                        InputService.Pausa("Pulsa cualquier tecla para continuar...");
                        volver = true;
                    }
                    else
                    {
                        // El mensaje de error ya fue mostrado por Mapa.MoverseA
                        InputService.Pausa("Pulsa cualquier tecla para volver.");
                    }
                }
                else
                {
                    Ui.WriteLine("Opción no válida.");
                    InputService.Pausa("Pulsa cualquier tecla para volver.");
                }
            }
            // Al terminar, mostrar el menú correspondiente a la nueva ubicación
            MostrarMenuPorUbicacion(ref volver);
		}
        }
}
