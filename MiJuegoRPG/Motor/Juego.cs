
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
                Console.WriteLine("\n=== Menú Principal ===");
                Console.WriteLine("1. Estado del personaje");
                Console.WriteLine("2. Ir a ubicación actual");
                Console.WriteLine("3. Inventario");
                Console.WriteLine("4. Guardar personaje");
                Console.WriteLine("5. Menú administrador");
                Console.WriteLine("0. Salir del juego");
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
                    case "0":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
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
                    Console.WriteLine($"[ADMIN] Teletransportado a: {destino.Nombre} (ID: {destino.Id})");
                }
                else
                {
                    Console.WriteLine($"[ADMIN] No se encontró el SectorData correspondiente al destino con Id: {destino.Id}");
                }
                MostrarMenuPorUbicacion();
            }
            else
            {
                Console.WriteLine($"[ADMIN] No se encontró el sector con Id: {idSector}");
            }
        }

    // ProcesarComando eliminado: ahora los comandos admin están aislados en MenuAdmin.
            
        
        public void CrearPersonaje()
        {
            Console.WriteLine("=== Creación de Personaje ===");
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
            Console.WriteLine($"Personaje creado: {jugador.Nombre} en {mapa.UbicacionActual.Nombre}");
            // ...después de crear el personaje y asignar atributos base...
            jugador.Estadisticas = new Estadisticas(jugador.AtributosBase);
            jugador.ManaActual = jugador.ManaMaxima;
        }
        // Menú de recolección fuera de ciudad
        public void MostrarMenuRecoleccion()
        {
            Console.WriteLine("=== Menú de Recolección ===");
            Console.WriteLine("1. Recolectar");
            Console.WriteLine("2. Minar");
            Console.WriteLine("3. Talar");
            Console.WriteLine("0. Volver");
            Console.Write("Selecciona una acción: ");
            var key = Console.ReadKey(true);
            string opcion = key.KeyChar.ToString();
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
                    Console.WriteLine("No hay nodos de recolección disponibles en este sector.");
                    Console.WriteLine("Presiona cualquier tecla para volver...");
                    Console.ReadKey();
                    MostrarMenuPorUbicacion();
                    return;
                }
                // Mostrar submenú de nodos
                Console.WriteLine("--- Selecciona un nodo de recolección ---");
                for (int i = 0; i < nodos.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {nodos[i].Nombre}");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Nodo: ");
                var nodoOpcion = Console.ReadLine();
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
                    Console.WriteLine("Opción de nodo no válida.");
                    MostrarMenuRecoleccion();
                }
            }
            else if (opcion == "0")
            {
                MostrarMenuPorUbicacion();
            }
            else
            {
                Console.WriteLine("Opción no válida en el menú de recolección.");
            }

        }

        // Método para cargar personaje desde el menú (extraído del default)
        public void MostrarMenuCargarPersonaje()
        {
            var pj = guardadoService.CargarInteractivo();
            if (pj == null)
            {
                Console.WriteLine("No se pudo cargar el personaje.");
                return;
            }
            jugador = pj;
            Console.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
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
        public void ProgresionPorActividad(string actividad)
        {
            // Aquí puedes definir la lógica de progresión según la actividad
            // Por ejemplo, aumentar experiencia, estadísticas, etc.
            Console.WriteLine($"Progresión registrada por actividad: {actividad}");
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
    private readonly MiJuegoRPG.Motor.Servicios.ProgressionService? progressionService;
    private GuardadoService guardadoService; // Nuevo servicio de guardado
    // Exposición controlada para servicios internos (evitar reflection)
    public MiJuegoRPG.Motor.Servicios.ProgressionService ProgressionService => progressionService!;
    public MiJuegoRPG.Motor.Servicios.RecoleccionService recoleccionService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.ClaseDinamicaService claseService { get; private set; }
    public MiJuegoRPG.Motor.Servicios.ReputacionService reputacionService { get; private set; }
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
        public Juego()
        {
            energiaService = new EnergiaService();
            progressionService = new MiJuegoRPG.Motor.Servicios.ProgressionService();
            progressionService.Verbose = true; // se puede ajustar dinámicamente
            guardadoService = new GuardadoService(); // inicializa servicio de guardado
            recoleccionService = new MiJuegoRPG.Motor.Servicios.RecoleccionService(this);
            claseService = new MiJuegoRPG.Motor.Servicios.ClaseDinamicaService(this);
            reputacionService = new MiJuegoRPG.Motor.Servicios.ReputacionService(this);
            Instancia = this;
            string carpetaMapas = System.IO.Path.Combine(ObtenerRutaRaizProyecto(), "MiJuegoRPG", "DatosJuego", "mapa");
            mapa = MapaLoader.CargarMapaCompleto(carpetaMapas);
            InstanciaActual = this;
            menuPrincipal = new MenusJuego(this);
            estadoMundo = new EstadoMundo();
            string rutaEnemigos = Path.Combine(ObtenerRutaRaizProyecto(), "MiJuegoRPG", "DatosJuego", "enemigos.json");
            GeneradorEnemigos.CargarEnemigos(rutaEnemigos);

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
                // DEBUG: Mostrar ID de ubicación actual y desbloqueo de sectores conectados
                Console.WriteLine($"[DEBUG] Ubicación actual: {mapa.UbicacionActual.Nombre} (ID: {mapa.UbicacionActual.Id})");
                if (sectorBairan != null)
                {
                    Console.WriteLine($"[DEBUG] Conexiones de {sectorBairan.Nombre}:");
                    foreach (var idConexion in sectorBairan.Conexiones)
                    {
                        var ubicAdj = estadoMundo.Ubicaciones.Find(u => u.Id == idConexion);
                        if (ubicAdj != null)
                        {
                            Console.WriteLine($"  - {ubicAdj.Nombre} (ID: {ubicAdj.Id}) | Desbloqueada: {ubicAdj.Desbloqueada}");
                        }
                        else
                        {
                            Console.WriteLine($"  - [NO ENCONTRADO] ID: {idConexion}");
                        }
                    }
                }
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
                Console.WriteLine($"[EVENTO] Atributo subido: {e.Atributo} = {e.NuevoValor:F2}");
            });
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoNivelSubido>(e => {
                Console.WriteLine($"[EVENTO] Nivel del jugador ahora: {e.Nivel}");
            });
            bus.Suscribir<MiJuegoRPG.Motor.Servicios.EventoMisionCompletada>(e => {
                Console.WriteLine($"[EVENTO] Misión completada ({e.Id}): {e.Nombre}");
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
                if (mapa.UbicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
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
                Console.WriteLine("No estás en ninguna ubicación válida. Volviendo al menú principal...");
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
        public void MostrarMenuViajar()
        {
            //Console.Clear();
            Console.WriteLine(FormatoRelojMundo);
            Console.WriteLine("--- Menú de Viaje ---");
            if (estadoMundo?.Ubicaciones == null || estadoMundo.Ubicaciones.Count == 0)
            {
                Console.WriteLine("No hay ubicaciones disponibles.");
                Console.WriteLine("Presiona cualquier tecla para volver...");
                Console.ReadKey();
                return;
            }
            int i = 1;
            foreach (var ubicacion in estadoMundo.Ubicaciones)
            {
                if (ubicacion.Desbloqueada)
                    Console.WriteLine($"{i}. {ubicacion.Nombre} - {ubicacion.Descripcion}");
                i++;
            }
            Console.WriteLine("0. Volver");
            Console.Write("Elige tu destino: ");
            var opcion = Console.ReadLine();
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
                        Console.WriteLine($"Viajaste a {destino.Nombre}.");
                        Console.WriteLine(destino.Descripcion);
                        MostrarMenuPorUbicacion();
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"No se encontró el SectorData correspondiente a {destino.Nombre}.");
                    }
                }
                else
                {
                    Console.WriteLine("No tienes acceso a esa ubicación.");
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
                Console.WriteLine("Opción no válida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
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
            string rutaProyecto = ObtenerRutaRaizProyecto();
            string rutaConfig = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "probabilidades.txt");
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
            menuPrincipal.MostrarMenuTienda(mapa.UbicacionActual.Nombre);
        }

    

        public void MostrarMenuGuardado()
        {
            //Console.Clear();
            Console.WriteLine(FormatoRelojMundo);
            Console.WriteLine("=== Menú de Guardar/Cargar ===");
            Console.WriteLine("1. Guardar partida");
            Console.WriteLine("2. Cargar partida");
            Console.WriteLine("3. Volver al menú principal");

            var opcion = Console.ReadLine();

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
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
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
            // 1. Probabilidad de Mazmorra
            if (MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100) < ProbMazmorra) //
            {
                Console.WriteLine("¡Has encontrado una mazmorra!");
                MostrarMenuMazmorra();
                return;
            }
            // 2. Probabilidad de encontrar objetos
            if (MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100) < ProbObjeto)
            {
                string[] objetos = { "Oro", "Material", "Equipo" };
                string tipoObjeto = objetos[MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(objetos.Length)];
                switch (tipoObjeto)
                {
                    case "Oro":
                        int oro = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(5, 21);
                        if (jugador != null)
                        {
                            jugador.Oro += oro;
                            Console.WriteLine($"¡Has encontrado {oro} monedas de oro!");
                        }
                        else
                        {
                            Console.WriteLine($"¡Has encontrado {oro} monedas de oro! (No hay personaje cargado)");
                        }
                        break;
                    case "Material":
                        // Ahora se requiere un nodo de recolección específico. Si hay nodos disponibles, usar el primero; si no, mostrar advertencia.
                        var sectorActual = mapa.ObtenerSectores().Find(s => s.Id == mapa.UbicacionActual.Id);
                        List<NodoRecoleccion> nodosMaterial = new List<NodoRecoleccion>();
                        if (sectorActual != null && sectorActual.NodosRecoleccion != null && sectorActual.NodosRecoleccion.Count > 0)
                        {
                            nodosMaterial.AddRange(sectorActual.NodosRecoleccion);
                        }
                        else if (sectorActual != null)
                        {
                            nodosMaterial.AddRange(MiJuegoRPG.Motor.TablaBiomas.GenerarNodosParaBioma(sectorActual.Region));
                        }
                        if (nodosMaterial.Count > 0)
                        {
                            recoleccionService.EjecutarAccion(MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar, nodosMaterial[0]);
                        }
                        else
                        {
                            Console.WriteLine("No hay nodos de recolección disponibles para obtener materiales.");
                        }
                        break;
                    case "Equipo":
                        if (jugador != null)
                        {
                            jugador.Inventario.AgregarObjeto(new Objetos.Arma("Espada Misteriosa", 10));
                            Console.WriteLine("¡Has encontrado un arma misteriosa!");
                        }
                        else
                        {
                            Console.WriteLine("¡Has encontrado un arma misteriosa! (No hay personaje cargado)");
                        }
                        break;
                }
            }
            // 3. Probabilidad de batalla
            if (MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100) < ProbMonstruo)
            {
                Console.WriteLine("¡Un monstruo aparece!");
                // ComenzarCombate(); // Método no implementado, comentar o implementar si es necesario
                Console.WriteLine("(Combate no implementado)");
                return;
            }
            Console.WriteLine("No ha ocurrido nada especial en la exploración.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
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
            Console.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
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
        // Sincronizar el ID de ubicación antes de guardar
        public void GuardarPersonaje()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para guardar.");
                return;
            }
            if (mapa.UbicacionActual != null)
                jugador.UbicacionActualId = mapa.UbicacionActual.Id;
            guardadoService.Guardar(jugador);
        }

            // Menú básico de mazmorra
        public void MostrarMenuMazmorra()
        {
            Console.WriteLine("=== Mazmorra encontrada ===");
            Console.WriteLine("1. Entrar en la mazmorra");
            Console.WriteLine("2. Volver");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    Console.WriteLine("¡Has entrado en la mazmorra! (Lógica por implementar)");
                    break;
                case "2":
                    Console.WriteLine("Regresando...");
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            MostrarMenuPorUbicacion();
        }

        // Menú básico de rutas
        public void MostrarMenuRutas()
        {
            // Mostrar sectores disponibles fuera de la ciudad
            var sectores = mapa.ObtenerSectores();
            // Buscar el sector actual por ID
            var sectorActual = sectores.Find(s => s.Id == mapa.UbicacionActual.Id);
            // DEBUG: Mostrar ID de ubicación actual y conexiones
            Console.WriteLine($"[DEBUG] Ubicación actual: {mapa.UbicacionActual.Nombre} (ID: {mapa.UbicacionActual.Id})");
            if (sectorActual != null)
            {
                Console.WriteLine($"[DEBUG] Conexiones de {sectorActual.Nombre}:");
                foreach (var idConexion in sectorActual.Conexiones)
                {
                    var conectado = sectores.Find(s => s.Id == idConexion);
                    if (conectado != null)
                        Console.WriteLine($"  - {conectado.Nombre} (ID: {conectado.Id})");
                    else
                        Console.WriteLine($"  - [NO ENCONTRADO] ID: {idConexion}");
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
                Console.WriteLine("No hay sectores conectados disponibles.");
                Console.WriteLine("Pulsa cualquier tecla para volver.");
                Console.ReadKey();
                return;
            }
            bool volver = false;
            while (!volver)
            {
                Console.WriteLine("=== Menú de Rutas ===");
                Console.WriteLine("Sectores conectados:");
                for (int i = 0; i < sectoresConectados.Count; i++)
                {
                    var s = sectoresConectados[i];
                    Console.WriteLine($"{i + 1}. {s.Nombre} - {s.Descripcion}");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Selecciona el sector al que deseas viajar: ");
                string opcion = InputService.LeerOpcion() ?? "0";
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
                        Console.WriteLine($"Te has movido a: {destino.Nombre}");
                        Console.WriteLine(destino.Descripcion);
                        Console.WriteLine("Pulsa cualquier tecla para continuar...");
                        Console.ReadKey();
                        volver = true;
                    }
                    else
                    {
                        // El mensaje de error ya fue mostrado por Mapa.MoverseA
                        Console.WriteLine("Pulsa cualquier tecla para volver.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Opción no válida.");
                    Console.WriteLine("Pulsa cualquier tecla para volver.");
                    Console.ReadKey();
                }
            }
            // Al terminar, mostrar el menú correspondiente a la nueva ubicación
            MostrarMenuPorUbicacion(ref volver);
		}
        }
}
