using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Objetos;
using System.Text.Json.Serialization;
using System.Threading;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
        public Mapa mapa;
        // Reloj mundial (minutos acumulados en el juego)
        public int MinutosMundo { get; set; } = 0;
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaActual => FechaInicio.AddMinutes(MinutosMundo);
        public string FormatoRelojMundo => $"[{FechaActual:dd-MM-yyyy} // {FechaActual:HH:mm:ss} hrs]";
    private Thread hiloTiempo; // Hilo para el control del tiempo
    private bool tiempoActivo = false;
    private int velocidadTiempoMs = 1000; // 1 segundo real por cada minuto de juego
        // Método para ajustar la velocidad del tiempo automático
        public void AjustarVelocidadTiempo(int milisegundos)
        {
            velocidadTiempoMs = milisegundos;
        }

        public void Iniciar()
        {
            //Console.Clear();
            Console.WriteLine(FormatoRelojMundo);
            Console.WriteLine("¡Bienvenido a Mi Juego RPG!");
            Console.WriteLine("1. Crear personaje nuevo");
            Console.WriteLine("2. Cargar personaje guardado");
            Console.WriteLine("0. Salir");
            Console.Write("Selecciona una opción: ");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                        jugador = MiJuegoRPG.Motor.CreadorPersonaje.Crear();
                        // Asignar ubicación inicial solo a personajes nuevos
                        if (mapa.UbicacionActual != null)
                        {
                            ubicacionActual = new Ubicacion {
                                Nombre = mapa.UbicacionActual.Nombre,
                                Tipo = "Ciudad",
                                Descripcion = mapa.UbicacionActual.Descripcion,
                                Desbloqueada = true,
                                EventosPosibles = new List<string> { "Tienda", "Escuela de Entrenamiento", "Explorar sector", "Descansar en posada" }
                            };
                        }
                    break;
                case "2":
                    CargarPersonaje();
                    if (jugador == null)
                    {
                        Console.WriteLine("No se pudo cargar el personaje. Se creará uno nuevo.");
                        jugador = MiJuegoRPG.Motor.CreadorPersonaje.Crear();
                            // Asignar ubicación inicial solo si es nuevo
                            if (mapa.UbicacionActual != null)
                            {
                                ubicacionActual = new Ubicacion {
                                    Nombre = mapa.UbicacionActual.Nombre,
                                    Tipo = "Ciudad",
                                    Descripcion = mapa.UbicacionActual.Descripcion,
                                    Desbloqueada = true,
                                    EventosPosibles = new List<string> { "Tienda", "Escuela de Entrenamiento", "Explorar sector", "Descansar en posada" }
                                };
                            }
                    }
                    break;
                case "0":
                    Environment.Exit(0);
                    return;
                default:
                    Console.WriteLine("Opción no válida. Se creará personaje nuevo por defecto.");
                    jugador = MiJuegoRPG.Motor.CreadorPersonaje.Crear();
                    break;
            }
            IniciarTiempoAutomatico();
            // Mostrar menú según el tipo de sector
            MostrarMenuPorUbicacion();
        }

        // Inicia el hilo de avance automático de tiempo
        private void IniciarTiempoAutomatico()
        {
            tiempoActivo = true;
            hiloTiempo = new Thread(() =>
            {
                while (tiempoActivo)
                {
                    AvanzarTiempo(1); // Avanza 1 minuto cada ciclo
                    Console.WriteLine($"Reloj mundial: {FormatoRelojMundo}");
                    Thread.Sleep(velocidadTiempoMs); // Espera según velocidad configurada
                }
            });
            hiloTiempo.IsBackground = true;
            hiloTiempo.Start();
        }

        // Detiene el hilo de avance automático de tiempo
        private void DetenerTiempoAutomatico()
        {
            tiempoActivo = false;
            if (hiloTiempo != null && hiloTiempo.IsAlive)
            {
                hiloTiempo.Join(1000);
            }
        }

            // Método para avanzar el tiempo manualmente
            public void AvanzarTiempo(int minutos)
            {
                MinutosMundo += minutos;
            }
        
        // Sincroniza y muestra el menú correcto según la ubicación actual
        public void MostrarMenuPorUbicacion()
        {
            if (mapa.UbicacionActual != null)
            {
                if (mapa.UbicacionActual.Tipo != null && mapa.UbicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                {
                    var menuCiudad = new MenuCiudad(this);
                    menuCiudad.MostrarMenuPrincipal();
                }
                else
                {
                    var menuFueraCiudad = new MenuFueraCiudad(this);
                    menuFueraCiudad.MostrarMenuFueraCiudad();
                }
            }
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
        public static Juego? ObtenerInstanciaActual() => InstanciaActual;
        public static Juego? InstanciaActual { get; private set; }
        // Probabilidades configurables
    public int ProbMonstruo = 40;
    public int ProbObjeto = 30;
    public int ProbMazmorra = 10;
    public int ProbEvento = 20;
        // Instancia de Random compartida
        private readonly Random random = new Random();

    public MiJuegoRPG.Personaje.Personaje? jugador;
        private MenuCiudad menuPrincipal;
    public EstadoMundo estadoMundo;
    public Ubicacion ubicacionActual;
        public MotorEventos motorEventos;
public MotorCombate motorCombate;

        public MotorMisiones motorMisiones;
        public MotorEntrenamiento motorEntrenamiento;
        public MotorInventario motorInventario;
        public MotorRutas motorRutas;
        public Juego()
        {
            string carpetaMapas = System.IO.Path.Combine(ObtenerRutaRaizProyecto(), "MiJuegoRPG", "PjDatos", "mapa");
            mapa = MapaLoader.CargarMapaCompleto(carpetaMapas);
            InstanciaActual = this;
            menuPrincipal = new MenuCiudad(this);
            estadoMundo = new EstadoMundo();
            InicializarUbicaciones();
            ubicacionActual = estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Albor") ?? estadoMundo.Ubicaciones[0];
            CargarProbabilidades();
            motorEventos = new MotorEventos(this);
            motorCombate = new MotorCombate(this);
            motorMisiones = new MotorMisiones(this);
            motorEntrenamiento = new MotorEntrenamiento(this);
            motorInventario = new MotorInventario(this);
            motorRutas = new MotorRutas(this);
        }

        private void InicializarUbicaciones()
        {
            // Ejemplo de inicialización básica
            var ciudad = new MiJuegoRPG.Motor.Ubicacion {
                Id = "albor",
                Nombre = "Ciudad de Albor",
                Tipo = "Ciudad",
                Descripcion = "La ciudad inicial, llena de vida y oportunidades.",
                Desbloqueada = true,
                EventosPosibles = new List<string> { "Tienda", "Escuela de Entrenamiento", "Explorar sector", "Descansar en posada" }
            };
            var bosque = new MiJuegoRPG.Motor.Ubicacion {
                Id = "bosque_oscuro",
                Nombre = "Bosque Oscuro",
                Tipo = "Ruta",
                Descripcion = "Un bosque peligroso, ideal para aventureros.",
                Desbloqueada = false,
                EventosPosibles = new List<string> { "Explorar" }
            };
            var rio = new MiJuegoRPG.Motor.Ubicacion {
                Id = "rio_plateado",
                Nombre = "Río Plateado",
                Tipo = "Ruta",
                Descripcion = "Un río que requiere barco para cruzar.",
                Desbloqueada = false,
                Requisitos = new Dictionary<string, object> { { "Barco", true } },
                EventosPosibles = new List<string> { "Evento especial", "Encuentro enemigo" }
            };
            var ciudadBruma = new MiJuegoRPG.Motor.Ubicacion {
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
            //Console.Clear();
            Console.WriteLine("=== Tienda ===");
            Console.WriteLine("1. Comprar poción curativa (10 oro)");
            Console.WriteLine("2. Salir de la tienda");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    if (jugador != null && jugador.Oro >= 10)
                    {
                        jugador.Oro -= 10;
                        jugador.Inventario.AgregarObjeto(new Objetos.Pocion("Poción Curativa", 20));
                        Console.WriteLine("¡Has comprado una poción curativa!");
                    }
                    else if (jugador != null)
                    {
                        Console.WriteLine("No tienes suficiente oro.");
                    }
                    else
                    {
                        Console.WriteLine("No hay personaje cargado.");
                    }
                    break;
                case "2":
                    Console.WriteLine("Saliendo de la tienda...");
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú principal...");
            Console.ReadKey();
            MostrarMenuPorUbicacion();
        }

        public void MostrarMenuMisionesNPC()
        {
            motorMisiones.MostrarMenuMisionesNPC();
        }

        private void MostrarMisiones()
        {
            motorMisiones.MostrarMisiones();
        }

        private void MostrarNPCs()
        {
            motorMisiones.MostrarNPCs();
        }


        // Nuevo método para mostrar el submenú de Guardar/Cargar.
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
            motorEventos.ExplorarSector();
        }

        // Acciones de recolección, minería y tala
        public void RealizarAccionRecoleccion(string tipo)
        {
            var random = new Random();
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
            MostrarMenuPorUbicacion();
        }

        public void Entrenar()
        {
            motorEntrenamiento.Entrenar();
        }

        public void GestionarInventario()
        {
            motorInventario.GestionarInventario();
        }

        // Método para guardar el personaje en un archivo JSON
        public void GuardarPersonaje()
        {
            if (jugador != null)
            {
                GestorArchivos.GuardarPersonaje(jugador);
            }
            else
            {
                Console.WriteLine("No hay personaje cargado para guardar.");
            }
        }

        // Método para cargar el personaje desde un archivo JSON
        public void CargarPersonaje()
        {
            var pj = GestorArchivos.CargarPersonaje();
            if (pj != null)
                jugador = pj;
        }

        // Método que encapsula el combate, usando la clase GeneradorEnemigos
        public void ComenzarCombate()
        {
            motorCombate.ComenzarCombate();
        }

        // Combate con varios enemigos distintos
    // Eliminado combate múltiple, solo combate clásico

        public void MostrarMenuUbicacion()
        {
            //Console.Clear();
            Console.WriteLine(FormatoRelojMundo);
            Console.WriteLine($"=== {ubicacionActual.Nombre} ===");
            Console.WriteLine(ubicacionActual.Descripcion);
            int i = 1;
            var opcionesMenu = new List<string>(ubicacionActual.EventosPosibles);
            if (ubicacionActual.Tipo == "Ciudad")
            {
                opcionesMenu.Add("Salir de la ciudad");
            }
            if (ubicacionActual.Tipo != "Ciudad")
            {
                opcionesMenu.Add("Volver a la ciudad");
            }
            // Solo agregar 'Explorar' si no está ya en la lista
            if (!opcionesMenu.Any(o => o.Equals("Explorar", StringComparison.OrdinalIgnoreCase)))
            {
                opcionesMenu.Add("Explorar");
            }
            foreach (var opcionTxt in opcionesMenu)
            {
                Console.WriteLine($"{i}. {opcionTxt}");
                i++;
            }
            var opcion = Console.ReadLine();
            int seleccion;
            if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= opcionesMenu.Count)
            {
                string eventoElegido = opcionesMenu[seleccion - 1];
                if (eventoElegido == "Explorar" || eventoElegido == "Explorar sector")
                {
                    ExplorarSector();
                }
                else if (eventoElegido == "Encuentro enemigo")
                {
                    ComenzarCombate();
                }
                else if (eventoElegido == "Descubrir objeto")
                {
                    if (jugador != null)
                    {
                        int prob = random.Next(100);
                        if (prob < 50)
                        {
                            var tipoPocion = prob < 20 ? "Poción Curativa" : prob < 35 ? "Poción de Energía" : "Poción de Resistencia";
                            int curacion = tipoPocion == "Poción Curativa" ? 20 : tipoPocion == "Poción de Energía" ? 10 : 5;
                            jugador.Inventario.AgregarObjeto(new Objetos.Pocion(tipoPocion, curacion));
                            Console.WriteLine($"Has encontrado una {tipoPocion} y la agregas a tu inventario.");
                        }
                        else
                        {
                            Console.WriteLine("No has encontrado ningún objeto útil.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No hay personaje cargado.");
                    }
                }
                else if (eventoElegido == "Encontrar mazmorra")
                {
                    if (jugador != null)
                    {
                        int prob = random.Next(100);
                        if (prob < 30)
                        {
                            Console.WriteLine("¡Has descubierto la entrada a una mazmorra misteriosa!");
                            Console.WriteLine("¿Qué deseas hacer?");
                            Console.WriteLine("1. Entrar en la mazmorra");
                            Console.WriteLine("2. Ignorar y seguir explorando");
                            Console.WriteLine("3. Volver a la ciudad");
                            var opcionMazmorra = Console.ReadLine();
                            switch (opcionMazmorra)
                            {
                                case "1":
                                    Console.WriteLine("Te adentras en la mazmorra... ¡Prepárate para el peligro!");
                                    // Jefe dinámico según nivel y atributos del personaje
                                    if (jugador == null)
                                    {
                                        Console.WriteLine("No hay personaje cargado. Creando nuevo personaje...");
                                        jugador = CreadorPersonaje.Crear();
                                    }
                                    int nivelPj = jugador.Nivel;
                                    int fuerzaPj = jugador.AtributosBase.Fuerza;
                                    int inteligenciaPj = jugador.AtributosBase.Inteligencia;
                                    int agilidadPj = jugador.AtributosBase.Agilidad;
                                    int dificultad = nivelPj + fuerzaPj + inteligenciaPj + agilidadPj;
                                    Random rand = new Random();
                                    int tipoJefe = rand.Next(3); // 0: GranGoblin, 1: Goblin, 2: EnemigoEstandar
                                    MiJuegoRPG.Enemigos.Enemigo jefeMazmorra;
                                    if (tipoJefe == 0)
                                    {
                                        jefeMazmorra = new MiJuegoRPG.Enemigos.GranGoblin();
                                        jefeMazmorra.Vida += dificultad * 2;
                                        // No modificar Ataque porque no es settable
                                    }
                                    else if (tipoJefe == 1)
                                    {
                                        jefeMazmorra = new MiJuegoRPG.Enemigos.Goblin("Goblin Jefe", 60 + dificultad, 12 + dificultad / 2, 8 + dificultad / 3, nivelPj + 2, 40 + dificultad, 30 + dificultad);
                                    }
                                    else
                                    {
                                        jefeMazmorra = new MiJuegoRPG.Enemigos.EnemigoEstandar("Bestia de la Mazmorra", 100 + dificultad, 18 + dificultad / 2, 10 + dificultad / 3, nivelPj + 3, 60 + dificultad, 50 + dificultad);
                                    }
                                    Console.WriteLine($"¡Un {jefeMazmorra.Nombre} aparece para defender la mazmorra!");
                                    if (jugador != null)
                                    {
                                        MiJuegoRPG.Motor.GeneradorEnemigos.IniciarCombate(jugador, jefeMazmorra);
                                        if (jefeMazmorra.Vida <= 0)
                                        {
                                            Console.WriteLine("¡Has derrotado al jefe de la mazmorra!");
                                            jugador.Inventario.AgregarObjeto(new MiJuegoRPG.Objetos.Pocion("Tesoro de la Mazmorra", 50 + dificultad));
                                            jugador.Oro += 100 + dificultad;
                                            Console.WriteLine($"Recibes el Tesoro de la Mazmorra y {100 + dificultad} de oro.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("El jefe de la mazmorra te ha vencido. ¡Debes entrenar más!");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No hay personaje cargado para combatir.");
                                    }
                                    break;
                                case "2":
                                    Console.WriteLine("Ignoras la mazmorra y continúas explorando el área.");
                                    break;
                                case "3":
                                    var ciudadDesbloqueada = estadoMundo.Ubicaciones.Find(u => u.Tipo == "Ciudad" && u.Desbloqueada);
                                    if (ciudadDesbloqueada != null)
                                    {
                                        ubicacionActual = ciudadDesbloqueada;
                                        Console.WriteLine("Has regresado a la ciudad.");
                                        MostrarMenuUbicacion();
                                    }
                                    else
                                    {
                                        Console.WriteLine("No tienes acceso a ninguna ciudad desbloqueada en este momento.");
                                    }
                                    break;
                                default:
                                    Console.WriteLine("Opción no válida. Ignoras la mazmorra y continúas explorando.");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No encontraste ninguna mazmorra en esta exploración.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No hay personaje cargado.");
                    }
                }
                else if (eventoElegido == "Salir de la ciudad")
                {
                    MostrarMenuRutas();
                }
                else if (eventoElegido == "Volver a la ciudad")
                {
                    var ciudadDesbloqueada = estadoMundo.Ubicaciones.Find(u => u.Tipo == "Ciudad" && u.Desbloqueada);
                    if (ciudadDesbloqueada != null)
                    {
                        ubicacionActual = ciudadDesbloqueada;
                        Console.WriteLine("Has regresado a la ciudad.");
                        MostrarMenuUbicacion();
                    }
                    else
                    {
                        Console.WriteLine("No tienes acceso a ninguna ciudad desbloqueada en este momento.");
                    }
                }
                else if (eventoElegido == "Escuela de Entrenamiento")
                {
                    Entrenar();
                }
                else if (eventoElegido == "Tienda")
                {
                    MostrarTienda();
                }
                else if (eventoElegido == "Descansar en posada")
                {
                    if (jugador != null)
                    {
                        jugador.Vida = jugador.VidaMaxima;
                        Console.WriteLine("Has descansado en la posada y recuperado toda tu vida.");
                    }
                    else
                    {
                        Console.WriteLine("No hay personaje cargado.");
                    }
                }
                else
                {
                    Console.WriteLine($"Evento '{eventoElegido}' aún no implementado.");
                }
            }
            else
            {
                Console.WriteLine("Opción no válida.");
            }
        }

        public void MostrarMenuRutas()
        {
            motorRutas.MostrarMenuRutas();
        }

    public void ProgresionPorActividad(string actividad)
        {
            if (jugador == null) return;
            switch (actividad.ToLower())
            {
                case "combate":
                    jugador.Entrenar("fuerza");
                    jugador.Entrenar("resistencia");
                    break;
                case "estudio":
                    jugador.Entrenar("inteligencia");
                    break;
                case "exploracion":
                    jugador.Entrenar("destreza");
                    jugador.Entrenar("resistencia");
                    break;
                case "trabajo":
                    jugador.Entrenar("resistencia");
                    break;
                case "magia":
                    jugador.Entrenar("magia");
                    jugador.Entrenar("inteligencia");
                    break;
                case "suerte":
                    jugador.Entrenar("suerte");
                    break;
            }
            // Desbloqueo de clases/títulos por combinaciones
            if (jugador.AtributosBase.Fuerza >= 20 && jugador.AtributosBase.Resistencia >= 15 && jugador.ClaseDesbloqueada == "Sin clase")
            {
                jugador.ClaseDesbloqueada = "Guerrero";
                jugador.Titulo = "El Fuerte";
                Console.WriteLine("¡Has desbloqueado la clase Guerrero!");
            }
            if (jugador.AtributosBase.Inteligencia >= 20 && jugador.AtributosBase.Destreza >= 10 && jugador.ClaseDesbloqueada == "Sin clase")
            {
                jugador.ClaseDesbloqueada = "Mago de Batalla";
                jugador.Titulo = "El Sabio";
                Console.WriteLine("¡Has desbloqueado la clase Mago de Batalla!");
            }
            if (jugador.AtributosBase.Destreza >= 20 && jugador.AtributosBase.Resistencia >= 15 && jugador.ClaseDesbloqueada == "Sin clase")
            {
                jugador.ClaseDesbloqueada = "Explorador";
                jugador.Titulo = "El Veloz";
                Console.WriteLine("¡Has desbloqueado la clase Explorador!");
            }
        }

        // Opción para revisar misiones activas
        private void RevisarMisiones()
        {
            motorMisiones.RevisarMisiones();
        }

        // Muestra el estado completo del personaje con explicación
        private void MostrarEstadoPersonaje(MiJuegoRPG.Personaje.Personaje pj)
        {
            Console.WriteLine("\n=== ESTADO DEL PERSONAJE ===");
            Console.WriteLine($"Nombre: {pj.Nombre}");
            Console.WriteLine($"Nivel: {pj.Nivel}");
            Console.WriteLine($"Clase: {(pj.Clase != null ? pj.Clase.Nombre : "Sin clase")}");
            Console.WriteLine($"Título: {pj.Titulo}");
            Console.WriteLine($"Vida: {pj.Vida}/{pj.VidaMaxima}");
            Console.WriteLine("\n--- Atributos ---");
            var atr = pj.AtributosBase;
            Console.WriteLine($"Fuerza: {atr.Fuerza} (Determina el daño físico)");
            Console.WriteLine($"Defensa: {atr.Defensa} (Reduce el daño recibido)");
            Console.WriteLine($"Agilidad: {atr.Agilidad} (Aumenta la evasión y velocidad)");
            Console.WriteLine($"Inteligencia: {atr.Inteligencia} (Aumenta el daño mágico y experiencia)");
            Console.WriteLine($"Vitalidad: {atr.Vitalidad} (Aumenta la vida máxima)");
            Console.WriteLine($"Suerte: {atr.Suerte} (Aumenta probabilidad de críticos y drops)");
            Console.WriteLine("\n--- Estadísticas ---");
            Console.WriteLine($"Experiencia: {pj.Experiencia}");
            Console.WriteLine($"Oro: {pj.Oro}");
            Console.WriteLine($"Inventario: {pj.Inventario?.NuevosObjetos.Count ?? 0} objetos");
            Console.WriteLine("\nExplicación: Los atributos determinan el desempeño en combate y exploración. La clase y el título pueden otorgar bonificaciones especiales.\n");
        }

        internal void IrATienda()
        {
            MostrarTienda();
        }
        // Fin de la clase Juego
    }
}
