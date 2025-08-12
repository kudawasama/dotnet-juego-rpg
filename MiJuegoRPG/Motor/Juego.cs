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
            // Método para crear un personaje nuevo
            public void CrearPersonaje()
            {
                jugador = CreadorPersonaje.Crear();
                Console.WriteLine("Personaje creado exitosamente.");
            }
        public void Iniciar()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje cargado o creado. No se puede iniciar el juego.");
                return;
            }

            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine(FormatoRelojMundo);
                Console.WriteLine($"Bienvenido, {jugador.Nombre}!");
                Console.WriteLine("=== Menú Principal ===");
                Console.WriteLine("1. Estado del personaje");
                Console.WriteLine("2. Ir a ubicación actual");
                Console.WriteLine("3. Inventario");
                Console.WriteLine("4. Guardar personaje");
                Console.WriteLine("0. Salir del juego");
                Console.Write("Selecciona una opción: ");
                string opcion = Console.ReadLine() ?? "";

                switch (opcion)
                {
                    case "1":
                        MostrarEstadoPersonaje(jugador);
                        MostrarMenuFijo(ref salir);
                        break;
                    case "2":
                        MostrarMenuPorUbicacion(ref salir);
                        break;
                    case "3":
                        GestionarInventario();
                        MostrarMenuFijo(ref salir);
                        break;
                    case "4":
                        GuardarPersonaje();
                        Console.WriteLine("¡Personaje guardado exitosamente!");
                        MostrarMenuFijo(ref salir);
                        break;
                    case "0":
                        salir = true;
                        Console.WriteLine("¡Gracias por jugar!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        MostrarMenuFijo(ref salir);
                        break;
                }
            }
        }

        public void MostrarMenuUbicacion()
        {
            // Este menú ya no se usa
            Console.WriteLine("Este menú ha sido reemplazado por el menú de ubicación principal.");
            Console.WriteLine("Presiona cualquier tecla para volver al menú principal...");
            Console.ReadKey();
            return;
        }

        public void AvanzarTiempo(int minutos)
        {
            // Implementación pendiente
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
        public int ProbMonstruo = 40;
        public int ProbObjeto = 30;
        public int ProbMazmorra = 10;
        public int ProbEvento = 20;
        private readonly Random random = new Random();
        public MiJuegoRPG.Personaje.Personaje? jugador;
        private MenusJuego menuPrincipal;
        public EstadoMundo estadoMundo;
        public Ubicacion ubicacionActual;
        public MotorEventos motorEventos;
        public MotorCombate motorCombate;
        public MotorMisiones motorMisiones;
        public MotorEntrenamiento motorEntrenamiento;
        public MotorInventario motorInventario;
        public MotorRutas motorRutas;

        // Constructor
        public Juego()
        {
            string carpetaMapas = System.IO.Path.Combine(ObtenerRutaRaizProyecto(), "MiJuegoRPG", "PjDatos", "mapa");
            mapa = MapaLoader.CargarMapaCompleto(carpetaMapas);
            InstanciaActual = this;
            menuPrincipal = new MenusJuego(this);
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
        // Sincroniza y muestra el menú correcto según la ubicación actual
        public void MostrarMenuPorUbicacion()
        {
            bool salir = false;
            MostrarMenuPorUbicacion(ref salir);
        }
        public void MostrarMenuPorUbicacion(ref bool salir)
        {
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine(FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {ubicacionActual.Nombre}");
                if (ubicacionActual.Tipo != null && ubicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("=== Menú de Ciudad ===");
                    Console.WriteLine("1. Tienda");
                    Console.WriteLine("2. Escuela de Entrenamiento");
                    Console.WriteLine("3. Explorar sector");
                    Console.WriteLine("4. Descansar en posada");
                    Console.WriteLine("5. Salir de la ciudad");
                }
                else
                {
                    Console.WriteLine("=== Menú Fuera de Ciudad ===");
                    Console.WriteLine("1. Explorar");
                    Console.WriteLine("2. Encontrar mazmorra");
                    Console.WriteLine("3. Volver a la ciudad");
                }
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("Selecciona una opción: ");
                string opcion = Console.ReadLine() ?? "";

                if (ubicacionActual.Tipo != null && ubicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                {
                    switch (opcion)
                    {
                        case "1": MostrarTienda(); break;
                        case "2": Entrenar(); break;
                        case "3": ExplorarSector(); break;
                        case "4":
                            if (jugador != null)
                            {
                                jugador.Vida = jugador.VidaMaxima;
                                Console.WriteLine("Has descansado y recuperado toda tu vida.");
                            }
                            else
                                Console.WriteLine("No hay personaje cargado.");
                            MostrarMenuFijo(ref salir);
                            break;
                        case "5": MostrarMenuRutas(); break;
                        case "9": MostrarMenuFijo(ref salir); break;
                        case "0": return;
                        default:
                            Console.WriteLine("Opción no válida.");
                            MostrarMenuFijo(ref salir);
                            break;
                    }
                }
                else
                {
                    switch (opcion)
                    {
                        case "1": ExplorarSector(); break;
                        case "2": MostrarMenuMazmorra(); break;
                        case "3":
                            var ciudadDesbloqueada = estadoMundo.Ubicaciones.Find(u => u.Tipo == "Ciudad" && u.Desbloqueada);
                            if (ciudadDesbloqueada != null)
                            {
                                ubicacionActual = ciudadDesbloqueada;
                                Console.WriteLine("Has regresado a la ciudad.");
                            }
                            else
                                Console.WriteLine("No tienes acceso a ninguna ciudad desbloqueada en este momento.");
                            MostrarMenuFijo(ref salir);
                            break;
                        case "9": MostrarMenuFijo(ref salir); break;
                        case "0": return;
                        default:
                            Console.WriteLine("Opción no válida.");
                            MostrarMenuFijo(ref salir);
                            break;
                    }
                }
            }
        }

        // Menú fijo disponible en todos los menús
        public void MostrarMenuFijo(ref bool salir)
        {
            Console.WriteLine("\n=== Menú Fijo ===");
            Console.WriteLine("1. Estado del personaje");
            Console.WriteLine("2. Guardar personaje");
            Console.WriteLine("3. Volver al menú principal");
            Console.WriteLine("0. Salir del juego");
            Console.Write("Selecciona una opción: ");
            string opcion = Console.ReadLine() ?? "";
            switch (opcion)
            {
                case "1":
                    if (jugador != null) MostrarEstadoPersonaje(jugador);
                    else Console.WriteLine("No hay personaje cargado.");
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
                case "2":
                    GuardarPersonaje();
                    Console.WriteLine("¡Personaje guardado exitosamente!");
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
                case "3":
                    return;
                case "0":
                    salir = true;
                    Console.WriteLine("¡Gracias por jugar!");
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
        

        // Muestra el estado completo del personaje con explicación
        private void MostrarEstadoPersonaje(MiJuegoRPG.Personaje.Personaje pj)
        {
            Console.WriteLine("\n=== ESTADO DEL PERSONAJE ===");
            Console.WriteLine($"Nombre: {pj.Nombre}");
            Console.WriteLine($"Clase: {(pj.Clase != null ? pj.Clase.Nombre : "Sin clase")}");
            Console.WriteLine($"Título: {pj.Titulo}");
            Console.WriteLine($"Nivel: {pj.Nivel}");
            Console.WriteLine($"Vida: {pj.Vida}/{pj.VidaMaxima}");
            Console.WriteLine($"Oro: {pj.Oro}");
            Console.WriteLine("\n--- Atributos Base ---");
            Console.WriteLine("===================================");
            var ab = pj.AtributosBase;
            var atributos = new Dictionary<string, double> {
                {"Fuerza", ab.Fuerza}, {"Destreza", ab.Destreza}, {"Vitalidad", ab.Vitalidad}, {"Agilidad", ab.Agilidad},
                {"Suerte", ab.Suerte}, {"Defensa", ab.Defensa}, {"Resistencia", ab.Resistencia}, {"Sabiduría", ab.Sabiduría},
                {"Inteligencia", ab.Inteligencia}, {"Percepción", ab.Percepcion}, {"Persuasión", ab.Persuasion},
                {"Liderazgo", ab.Liderazgo}, {"Carisma", ab.Carisma}, {"Voluntad", ab.Voluntad}
            };
            foreach (var atributo in atributos)
            {
                double bonificador = pj.ObtenerBonificadorAtributo(atributo.Key);
                double total = atributo.Value + bonificador;
                Console.WriteLine($"{atributo.Key}: {atributo.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorAtributo(atributo.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }
            Console.WriteLine("\n--- Estadísticas Físicas ---");
            var est = pj.Estadisticas;
            var estadisticasFisicas = new Dictionary<string, double> {
                {"Ataque", est.Ataque}, {"Defensa Física", est.DefensaFisica}, {"Daño", est.Daño}, {"Crítico", est.Critico},
                {"Evasión", est.Evasion}, {"Velocidad", est.Velocidad}, {"Regeneración", est.Regeneracion}, {"Salud", est.Salud},
                {"Energía", est.Energia}, {"Carga", est.Carga}, {"Poder Ofensivo Físico", est.PoderOfensivoFisico}, {"Poder Defensivo Físico", est.PoderDefensivoFisico}
            };
            foreach (var stat in estadisticasFisicas)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\n--- Estadísticas Mágicas ---");
            var estadisticasMagicas = new Dictionary<string, double> {
                {"Poder Mágico", est.PoderMagico}, {"Defensa Mágica", est.DefensaMagica}, {"Regeneración Mana", est.RegeneracionMana},
                {"Mana", est.Mana}, {"Poder Ofensivo Mágico", est.PoderOfensivoMagico}, {"Poder Defensivo Mágico", est.PoderDefensivoMagico},
                {"Afinidad Elemental", est.AfinidadElemental}, {"Poder Elemental", est.PoderElemental}, {"Resistencia Elemental", est.ResistenciaElemental},
                {"Resistencia Mágica", est.ResistenciaMagica}
            };
            foreach (var stat in estadisticasMagicas)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\n--- Estadísticas Espirituales y Especiales ---");
            var estadisticasEspeciales = new Dictionary<string, double> {
                {"Poder Espiritual", est.PoderEspiritual}, {"Poder Curativo", est.PoderCurativo}, {"Poder de Soporte", est.PoderDeSoporte},
                {"Poder de Control", est.PoderDeControl}, {"Poder de Invocación", est.PoderDeInvocacion}, {"Poder de Transmutación", est.PoderDeTransmutacion},
                {"Poder de Alteración", est.PoderDeAlteracion}, {"Poder de Ilusión", est.PoderDeIlusion}, {"Poder de Conjuración", est.PoderDeConjuracion},
                {"Poder de Destrucción", est.PoderDeDestruccion}, {"Poder de Restauración", est.PoderDeRestauracion}, {"Poder de Transporte", est.PoderDeTransporte},
                {"Poder de Manipulación", est.PoderDeManipulacion}
            };
            foreach (var stat in estadisticasEspeciales)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
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

        // Probabilidades configurables

        private void InicializarUbicaciones()
        {
            // Ejemplo de inicialización básica
            var ciudad = new MiJuegoRPG.Motor.Ubicacion
            {
                Id = "albor",
                Nombre = "Ciudad de Albor",
                Tipo = "Ciudad",
                Descripcion = "La ciudad inicial, llena de vida y oportunidades.",
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

        // Método para guardar el personaje usando GuardaPersonaje
        public void GuardarPersonaje()
        {
            if (jugador != null)
            {
                GuardaPersonaje.GuardarPersonaje(jugador);
            }
            else
            {
                Console.WriteLine("No hay personaje cargado para guardar.");
            }
        }

        // Método para cargar el personaje usando GuardaPersonaje
        public void CargarPersonaje()
        {
            var personajes = GuardaPersonaje.CargarTodosLosPersonajes();
            if (personajes.Count == 0)
            {
                Console.WriteLine("No hay personajes guardados.");
                return;
            }
            Console.WriteLine("Personajes guardados disponibles:");
            for (int i = 0; i < personajes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {personajes[i].Nombre}");
            }
            Console.Write("Elige el número del personaje a cargar: ");
            string entrada = Console.ReadLine() ?? string.Empty;
            if (int.TryParse(entrada, out int seleccion) && seleccion > 0 && seleccion <= personajes.Count)
            {
                jugador = personajes[seleccion - 1];
                Console.WriteLine($"Personaje '{jugador.Nombre}' cargado correctamente.");
            }
            else
            {
                Console.WriteLine("Selección inválida. No se pudo cargar el personaje.");
            }
        }

        // Método que encapsula el combate, usando la clase GeneradorEnemigos
        public void ComenzarCombate()
        {
            motorCombate.ComenzarCombate();
        }

        // Combate con varios enemigos distintos
        // Eliminado combate múltiple, solo combate clásico

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
            Console.WriteLine("=== Menú de Rutas ===");
            Console.WriteLine("1. Viajar a otra ubicación");
            Console.WriteLine("2. Volver");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    MostrarMenuViajar();
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
    }
}
