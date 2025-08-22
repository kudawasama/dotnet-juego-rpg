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

namespace MiJuegoRPG.Motor
{
    public class Juego
   
    {

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
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
            } 
            
        }
        public void CrearPersonaje()
        {
            Console.WriteLine("=== Creación de Personaje ===");
            jugador = MiJuegoRPG.Motor.CreadorPersonaje.CrearSinClase();
            // Ubicación inicial: buscar la ciudad principal por propiedad CiudadPrincipal
            var ciudadPrincipal = estadoMundo.Ubicaciones.Find(u => u != null && u.CiudadPrincipal);
            if (ciudadPrincipal != null)
                ubicacionActual = ciudadPrincipal;
            else if (estadoMundo.Ubicaciones.Count > 0)
                ubicacionActual = estadoMundo.Ubicaciones[0];
            else
                throw new Exception("No hay ubicaciones disponibles para asignar al personaje.");
            Console.WriteLine($"Personaje creado: {jugador.Nombre} en {ubicacionActual.Nombre}");
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
            switch (opcion)
            {
                case "1": RealizarAccionRecoleccion("Recolectar"); break;
                case "2": RealizarAccionRecoleccion("Minar"); break;
                case "3": RealizarAccionRecoleccion("Talar"); break;
                case "0": MostrarMenuPorUbicacion(); break;
                default:
                    Console.WriteLine("Opción no válida en el menú de recolección.");
                    break;
            }

        }

        // Método para cargar personaje desde el menú (extraído del default)
        public void MostrarMenuCargarPersonaje()
        {
            var pj = MiJuegoRPG.Motor.GestorArchivos.CargarPersonaje();
            if (pj != null)
            {
                jugador = pj;
                Console.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
                if (jugador.UbicacionActual != null)
                    ubicacionActual = jugador.UbicacionActual;
            }
            else
            {
                Console.WriteLine("No se pudo cargar el personaje.");
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

        private readonly Random random = new Random();
        public EnergiaService energiaService { get; }
        public MiJuegoRPG.Personaje.Personaje? jugador;
        public MenusJuego menuPrincipal;
        public EstadoMundo estadoMundo;
        public Ubicacion ubicacionActual;
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
                    Desbloqueada = sector.CiudadInicial || sector.Id == "bairan", // Solo la ciudad inicial desbloqueada por defecto
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
                ubicacionActual = bairan;
                // DEBUG: Mostrar ID de ubicación actual y desbloqueo de sectores conectados
                Console.WriteLine($"[DEBUG] Ubicación actual: {ubicacionActual.Nombre} (ID: {ubicacionActual.Id})");
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
                ubicacionActual = estadoMundo.Ubicaciones[0];
            }
            CargarProbabilidades();
            motorEventos = new MotorEventos(this);
            motorCombate = new MotorCombate(this);
            motorMisiones = new MotorMisiones(this);
            motorEntrenamiento = new MotorEntrenamiento(this);
            motorInventario = new MotorInventario(this);
            motorRutas = new MotorRutas(this);
            menuCiudad = new MiJuegoRPG.Motor.Menus.MenuCiudad(this);
        }
        // Sincroniza y muestra el menú correcto según la ubicación actual
        public void MostrarMenuPorUbicacion()
        {
            bool salir = false;
            MostrarMenuPorUbicacion(ref salir);
        }
        public void MostrarMenuPorUbicacion(ref bool salir)
        {
            if (ubicacionActual != null && ubicacionActual.Tipo != null && ubicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
            {
                MostrarMenuCiudad(ref salir);
            }
            else if (ubicacionActual != null && !string.IsNullOrWhiteSpace(ubicacionActual.Tipo))
            {
                MostrarMenuFueraCiudad(ref salir);
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
                    ubicacionActual = destino;
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
            menuPrincipal.MostrarMenuTienda(ubicacionActual.Nombre);
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
                jugador.ExpDestreza += 0.0002; // Aumento de experiencia por explorar
                jugador.ExpAgilidad += 0.0001; // Aumento de experiencia por explorar
                jugador.ExpPercepcion += 0.0002; //        
                RevisarAtributosPorExperiencia(jugador);
            }
            // 1. Probabilidad de Mazmorra
            if (random.Next(100) < ProbMazmorra) //
            {
                Console.WriteLine("¡Has encontrado una mazmorra!");
                MostrarMenuMazmorra();
                return;
            }
            // 2. Probabilidad de encontrar objetos
            if (random.Next(100) < ProbObjeto)
            {
                string[] objetos = { "Oro", "Material", "Equipo" };
                string tipoObjeto = objetos[random.Next(objetos.Length)];
                switch (tipoObjeto)
                {
                    case "Oro":
                        int oro = random.Next(5, 21);
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
                        RealizarAccionRecoleccion("Recolectar");
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
            if (random.Next(100) < ProbMonstruo)
            {
                Console.WriteLine("¡Un monstruo aparece!");
                ComenzarCombate();
                return;
            }
            Console.WriteLine("No ha ocurrido nada especial en la exploración.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            MostrarMenuPorUbicacion();
        }

        // Acciones de recolección, minería y tala
        // Revisa si algún atributo sube por experiencia acumulada
        private void RevisarAtributosPorExperiencia(MiJuegoRPG.Personaje.Personaje pj)
        {
            // Ahora los atributos suben en fracciones, no hay experiencia separada
            // Por compatibilidad, suma la experiencia acumulada al atributo y reinicia la experiencia
            if (pj.ExpFuerza > 0)
            {
                pj.AtributosBase.Fuerza += pj.ExpFuerza;
                pj.ExpFuerza = 0;
            }
            if (pj.ExpDestreza > 0)
            {
                pj.AtributosBase.Destreza += pj.ExpDestreza;
                pj.ExpDestreza = 0;
            }
            if (pj.ExpAgilidad > 0)
            {
                pj.AtributosBase.Agilidad += pj.ExpAgilidad;
                pj.ExpAgilidad = 0;
            }
            if (pj.ExpResistencia > 0)
            {
                pj.AtributosBase.Resistencia += pj.ExpResistencia;
                pj.ExpResistencia = 0;
            }
            if (pj.ExpInteligencia > 0)
            {
                pj.AtributosBase.Inteligencia += pj.ExpInteligencia;
                pj.ExpInteligencia = 0;
            }
            if (pj.ExpPercepcion > 0)
            {
                pj.AtributosBase.Percepcion += pj.ExpPercepcion;
                pj.ExpPercepcion = 0;
            }
            // Si quieres que otros atributos suban igual, agrégalos aquí
        }
        public void RealizarAccionRecoleccion(string tipo)
        {
            // Ganar experiencia por acción de recolección, escalada por nivel y dificultad
            //
            // Para modificar la experiencia ganada y su progresión según el nivel del jugador y el nivel del atributo:
            // - Cambia el valor de 'expBase' para subir o bajar la ganancia general de experiencia.
            // - Cambia la fórmula de 'indiceNivel' para ajustar la progresión según el nivel del jugador.
            // - Cambia la obtención de 'indiceAtributo' para ajustar la progresión según el nivel del atributo.
            // - Puedes modificar la fórmula final (actualmente: expBase / (indiceNivel * indiceAtributo)) para hacer la progresión más suave o más exigente.
            // Ejemplo: multiplicar en vez de dividir, cambiar el exponente, etc.
            //
            // Si quieres que la experiencia base sea mayor, sube 'expBase'.
            // Si quieres que la progresión sea más dura, aumenta el exponente de 'indiceNivel' o 'indiceAtributo'.
            // Si quieres que sea más fácil, bájalo o usa una suma en vez de multiplicación.
            //
            // Modifica aquí según tus necesidades:
            var energiaService = new EnergiaService();
            if (jugador != null)
            {
                // Mostrar energía antes de la acción
                energiaService.MostrarEnergia(jugador);
                // Intentar gastar energía
                if (!energiaService.GastarEnergiaRecoleccion(jugador))
                {
                    Console.WriteLine("No puedes realizar la acción por falta de energía.");
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    menuPrincipal.MostrarMenuRecoleccion();
                    return;
                }
                double expBase = 0.01; // Base de experiencia
                double indiceNivel = Math.Pow(1.05, jugador.Nivel - 1); // Progresión por nivel de jugador
                int minutos = 1; // Cada acción equivale a 1 minuto
                switch (tipo)
                {
                    case "Recolectar":
                        double valorPercepcion = jugador.AtributosBase.Percepcion > 0 ? jugador.AtributosBase.Percepcion : 1.0;
                        double indicePercepcion = 1.0 + (valorPercepcion / 10.0); // A mayor percepción, más difícil subir
                        double expPercepcion = expBase / (indiceNivel * indicePercepcion);
                        if (expPercepcion < 0.0001) expPercepcion = 0.0001;
                        jugador.ExpPercepcion += expPercepcion * minutos;
                        double valorInteligencia = jugador.AtributosBase.Inteligencia > 0 ? jugador.AtributosBase.Inteligencia : 1.0;
                        double indiceInteligencia = 1.0 + (valorInteligencia / 10.0);
                        double expInteligencia = expBase / (indiceNivel * indiceInteligencia);
                        if (expInteligencia < 0.0001) expInteligencia = 0.0001;
                        jugador.ExpInteligencia += expInteligencia * minutos;
                        Console.WriteLine($"Has ganado {expPercepcion * minutos:F4} exp de Percepción y {expInteligencia * minutos:F4} exp de Inteligencia.");
                        break;
                    case "Minar":
                        double valorFuerza = jugador.AtributosBase.Fuerza > 0 ? jugador.AtributosBase.Fuerza : 1.0;
                        double indiceFuerza = 1.0 + (valorFuerza / 10.0);
                        double expFuerza = expBase / (indiceNivel * indiceFuerza);
                        if (expFuerza < 0.0001) expFuerza = 0.0001;
                        jugador.ExpFuerza += expFuerza * minutos;
                        double valorResistencia = jugador.AtributosBase.Resistencia > 0 ? jugador.AtributosBase.Resistencia : 1.0;
                        double indiceResistencia = 1.0 + (valorResistencia / 10.0);
                        double expResistencia = expBase / (indiceNivel * indiceResistencia);
                        if (expResistencia < 0.0001) expResistencia = 0.0001;
                        jugador.ExpResistencia += expResistencia * minutos;
                        Console.WriteLine($"Has ganado {expFuerza * minutos:F4} exp de Fuerza y {expResistencia * minutos:F4} exp de Resistencia.");
                        break;
                    case "Talar":
                        double valorFuerzaT = jugador.AtributosBase.Fuerza > 0 ? jugador.AtributosBase.Fuerza : 1.0;
                        double indiceFuerzaT = 1.0 + (valorFuerzaT / 10.0);
                        double expFuerzaT = expBase / (indiceNivel * indiceFuerzaT);
                        if (expFuerzaT < 0.0001) expFuerzaT = 0.0001;
                        jugador.ExpFuerza += expFuerzaT * minutos;
                        double valorDestreza = jugador.AtributosBase.Destreza > 0 ? jugador.AtributosBase.Destreza : 1.0;
                        double indiceDestreza = 1.0 + (valorDestreza / 10.0);
                        double expDestreza = expBase / (indiceNivel * indiceDestreza);
                        if (expDestreza < 0.0001) expDestreza = 0.0001;
                        jugador.ExpDestreza += expDestreza * minutos;
                        Console.WriteLine($"Has ganado {expFuerzaT * minutos:F4} exp de Fuerza y {expDestreza * minutos:F4} exp de Destreza.");
                        break;
                }
                RevisarAtributosPorExperiencia(jugador);
            }
            // Usar el campo random de la clase
            switch (tipo)
            {
                case "Recolectar":
                    if (random.Next(100) < 60)
                    {
                        string[] materiales = { "Hierba Curativa", "Flor Azul", "Seta Roja", "Raíz Mágica" };
                        string material = materiales[random.Next(materiales.Length)];
                        Console.WriteLine($"Recolectaste: {material}.");
                        if (jugador != null)
                            jugador.Inventario.AgregarObjeto(new Objetos.Material(material, Objetos.Rareza.Normal));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste nada útil.");
                    }
                    break;
                case "Minar":
                    if (random.Next(100) < 40)
                    {
                        string[] minerales = { "Mineral de Hierro", "Mineral de Cobre", "Mineral de Plata", "Cristal Místico" };
                        string mineral = minerales[random.Next(minerales.Length)];
                        Console.WriteLine($"Minaste: {mineral}.");
                        if (jugador != null)
                            jugador.Inventario.AgregarObjeto(new Objetos.Material(mineral, Objetos.Rareza.Rara));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste minerales.");
                    }
                    break;
                case "Talar":
                    if (random.Next(100) < 50)
                    {
                        string[] maderas = { "Madera Resistente", "Madera Roja", "Madera Sagrada", "Madera Flexible" };
                        string madera = maderas[random.Next(maderas.Length)];
                        Console.WriteLine($"Talarte: {madera}.");
                        if (jugador != null)
                            jugador.Inventario.AgregarObjeto(new Objetos.Material(madera, Objetos.Rareza.Normal));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste árboles útiles.");
                    }
                    break;
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            menuPrincipal.MostrarMenuRecoleccion();
    }

        public void Entrenar()
        {
            motorEntrenamiento.Entrenar();
        }

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
            var pj = MiJuegoRPG.Motor.GestorArchivos.CargarPersonaje();
            if (pj != null)
            {
                jugador = pj;
                // Recalcular estadísticas y maná después de cargar
                jugador.Estadisticas = new Estadisticas(jugador.AtributosBase);
                jugador.ManaActual = jugador.ManaMaxima;

                Console.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
                if (jugador.UbicacionActual != null)
                    ubicacionActual = jugador.UbicacionActual;
                else
                {
                    // Si no tiene ubicación, asignar ciudad principal
                    var ciudadPrincipal = estadoMundo.Ubicaciones.Find(u =>
                        (u is not null && u.Requisitos != null &&
                         u.Requisitos.ContainsKey("ciudadPrincipal") &&
                         u.Requisitos["ciudadPrincipal"] is bool b && b)
                    );
                    if (ciudadPrincipal != null)
                        ubicacionActual = ciudadPrincipal;
                }
            }
            else
            {
                Console.WriteLine("No se pudo cargar el personaje.");
            }
        }
        public void GuardarPersonaje()
        {
            if (jugador != null)
            {
                // Guardar la ubicación actual en el personaje antes de serializar
                jugador.UbicacionActual = ubicacionActual;
                // Guardar usando SQLite
                MiJuegoRPG.Motor.GestorArchivos.GuardarPersonaje(jugador);
            }
            else
            {
                Console.WriteLine("No hay personaje cargado para guardar.");
            }
        }
    // ...existing code...

        // Método que encapsula el combate, usando la clase GeneradorEnemigos
        public void ComenzarCombate()
        {
            motorCombate.ComenzarCombate();
        }

      // Comienza a las 8 AM

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
            var sectorActual = sectores.Find(s => s.Id == ubicacionActual.Id);
            // DEBUG: Mostrar ID de ubicación actual y conexiones
            Console.WriteLine($"[DEBUG] Ubicación actual: {ubicacionActual.Nombre} (ID: {ubicacionActual.Id})");
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
                    // Lógica de movimiento usando el sistema de mapa
                    mapa.MoverseA(destino.Id);
                    // Actualizar la ubicación actual del juego
                    var ubicacionOriginal = estadoMundo.Ubicaciones.FirstOrDefault(u => u.Id == destino.Id);
                    if (ubicacionOriginal != null)
                        ubicacionActual = ubicacionOriginal;
                    else
                        ubicacionActual = new Ubicacion { Id = destino.Id, Nombre = destino.Nombre, Tipo = destino.Tipo, Descripcion = destino.Descripcion };
                    if (jugador != null)
                        MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(jugador);
                    Console.WriteLine($"Te has movido a: {destino.Nombre}");
                    Console.WriteLine(destino.Descripcion);
                    Console.WriteLine("Pulsa cualquier tecla para continuar...");
                    Console.ReadKey();
                    volver = true;
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
        // Menú de recolección fuera de ciudad
