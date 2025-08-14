
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
                Console.Write("Selecciona una opción: ");
                string opcion = Console.ReadLine() ?? "";
                switch (opcion)
                {
                    case "1":
                        if (jugador != null) MostrarEstadoPersonaje(jugador);
                        else Console.WriteLine("No hay personaje cargado.");
                        break;
                    case "2":
                        // Volver al sector anterior: si está en ciudad, mostrar menú de ciudad; si está fuera, mostrar menú fuera de ciudad
                        if (ubicacionActual != null && ubicacionActual.Tipo != null && ubicacionActual.Tipo.Equals("Ciudad", StringComparison.OrdinalIgnoreCase))
                        {
                            MostrarMenuCiudad(ref salir);
                        }
                        else
                        {
                            MostrarMenuFueraCiudad(ref salir);
                        }
                        break;
                    case "3":
                        GestionarInventario();
                        break;
                    case "4":
                        GuardarPersonaje();
                        Console.WriteLine("¡Personaje guardado exitosamente!");
                        break;
                    case "0":
                        salir = true;
                        Console.WriteLine("¡Gracias por jugar!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    // ...existing code...
        public void CrearPersonaje()
        {
            Console.WriteLine("=== Creación de Personaje ===");
            Console.Write("Ingresa el nombre de tu personaje: ");
            string nombre = Console.ReadLine() ?? "Héroe";
            jugador = new MiJuegoRPG.Personaje.Personaje(nombre);
            // Ubicación inicial: Ciudad de Bairan
            var bairan = estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Bairan");
            if (bairan != null)
                jugador.UbicacionActual = bairan;
            else
                jugador.UbicacionActual = ubicacionActual;
            ubicacionActual = jugador.UbicacionActual;
            Console.WriteLine($"Personaje creado: {jugador.Nombre} en {jugador.UbicacionActual.Nombre}");
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
            ubicacionActual = estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Bairan")
                ?? estadoMundo.Ubicaciones[0];
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
            string opcion = "";
            while (!salir)
            {
                Console.WriteLine(FormatoRelojMundo);
                Console.WriteLine($"Ubicación actual: {ubicacionActual.Nombre}");
                Console.WriteLine("=== Menú de Ciudad ===");
                Console.WriteLine("1. Tienda");
                Console.WriteLine("2. Escuela de Entrenamiento");
                Console.WriteLine("3. Explorar sector");
                Console.WriteLine("4. Descansar en posada");
                Console.WriteLine("5. Salir de la ciudad");
                Console.WriteLine("9. Menú fijo");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("Selecciona una opción: ");
                var key = Console.ReadKey(true);
                opcion = key.KeyChar.ToString();
                switch (opcion)
                {
                    case "1": MostrarTienda(); break;
                    case "2": Entrenar(); break;
                    case "3": menuPrincipal.MostrarMenuMisionesNPC(); break;
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
                    // Al volver, mostrar el menú adecuado según la ubicación actual
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
                        salir = true;
                    }
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
            int expActual = pj.Experiencia;
            int expSiguiente = pj.ExperienciaSiguienteNivel;
            int expFaltante = expSiguiente - expActual;
            double porcentaje = expSiguiente > 0 ? (double)expActual / expSiguiente * 100.0 : 0.0;
            Console.WriteLine($"Experiencia: {expActual} / {expSiguiente} (Faltan {expFaltante})");
            Console.WriteLine($"Progreso al siguiente nivel: {porcentaje:F2}%");
            Console.WriteLine("\n--- Atributos Base ---");
            Console.WriteLine("===================================");
            var ab = pj.AtributosBase;
            var atributos = new Dictionary<string, (string abrev, double valor, double exp, double req)> {
                {"Fuerza", ("Fza", ab.Fuerza, pj.ExpFuerza, pj.FuerzaExpRequerida)},
                {"Destreza", ("Dxt", ab.Destreza, pj.ExpDestreza, pj.DestrezaExpRequerida)},
                {"Vitalidad", ("Vit", ab.Vitalidad, pj.ExpVitalidad, pj.VitalidadExpRequerida)},
                {"Agilidad", ("Agi", ab.Agilidad, pj.ExpAgilidad, pj.AgilidadExpRequerida)},
                {"Suerte", ("Srt", ab.Suerte, pj.ExpSuerte, pj.SuerteExpRequerida)},
                {"Defensa", ("Def", ab.Defensa, pj.ExpDefensa, pj.DefensaExpRequerida)},
                {"Resistencia", ("Res", ab.Resistencia, pj.ExpResistencia, pj.ResistenciaExpRequerida)},
                {"Sabiduría", ("Sab", ab.Sabiduría, 0, 1)},
                {"Inteligencia", ("Int", ab.Inteligencia, pj.ExpInteligencia, pj.InteligenciaExpRequerida)},
                {"Percepción", ("Per", ab.Percepcion, pj.ExpPercepcion, pj.PercepcionExpRequerida)},
                {"Persuasión", ("Prs", ab.Persuasion, 0, 1)},
                {"Liderazgo", ("Lid", ab.Liderazgo, 0, 1)},
                {"Carisma", ("Car", ab.Carisma, 0, 1)},
                {"Voluntad", ("Vol", ab.Voluntad, 0, 1)}
            };
            foreach (var atributo in atributos)
            {
                string abrev = atributo.Value.abrev;
                double valor = atributo.Value.valor;
                double exp = atributo.Value.exp;
                double req = atributo.Value.req;
                double bonificador = pj.ObtenerBonificadorAtributo(atributo.Key);
                double total = valor + bonificador;
                double prog = req > 0 ? exp / req * 100.0 : 0.0;
                double faltante = req - exp;
                string textoProg = req > 1 ? $" ({prog:F2}% de {req}, faltan {faltante:F2})" : "";
                Console.WriteLine($"{abrev}: {total} (Base: {valor}, Bonif: {bonificador}){textoProg}");
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
            // Ganar experiencia por explorar
            if (jugador != null)
            {
                jugador.ExpDestreza += 0.2;
                jugador.ExpAgilidad += 0.1;
                jugador.ExpPercepcion += 0.1;
                RevisarAtributosPorExperiencia(jugador);
            }
            // 1. Probabilidad de Mazmorra
            if (random.Next(100) < ProbMazmorra)
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
            if (jugador != null)
            {
                double expBase = 0.01; // Base de experiencia
                double indiceNivel = Math.Pow(1.05, jugador.Nivel - 1); // Progresión por nivel de jugador
                int minutos = 1; // Cada acción equivale a 1 minuto
                switch (tipo)
                {
                    case "Recolectar":
                        double indicePercepcion = jugador.IndiceAtributo.ContainsKey("percepcion") ? jugador.IndiceAtributo["percepcion"] : 1.0;
                        double expPercepcion = expBase / (indiceNivel * indicePercepcion);
                        if (expPercepcion < 0.0001) expPercepcion = 0.0001;
                        jugador.ExpPercepcion += expPercepcion * minutos;
                        double indiceInteligencia = jugador.IndiceAtributo.ContainsKey("inteligencia") ? jugador.IndiceAtributo["inteligencia"] : 1.0;
                        double expInteligencia = expBase / (indiceNivel * indiceInteligencia);
                        if (expInteligencia < 0.0001) expInteligencia = 0.0001;
                        jugador.ExpInteligencia += expInteligencia * minutos;
                        Console.WriteLine($"Has ganado {expPercepcion * minutos:F4} exp de Percepción y {expInteligencia * minutos:F4} exp de Inteligencia.");
                        break;
                    case "Minar":
                        double indiceFuerza = jugador.IndiceAtributo.ContainsKey("fuerza") ? jugador.IndiceAtributo["fuerza"] : 1.0;
                        double expFuerza = expBase / (indiceNivel * indiceFuerza);
                        if (expFuerza < 0.0001) expFuerza = 0.0001;
                        jugador.ExpFuerza += expFuerza * minutos;
                        double indiceResistencia = jugador.IndiceAtributo.ContainsKey("resistencia") ? jugador.IndiceAtributo["resistencia"] : 1.0; // Progresión por nivel de atributo, mientras mas alto sea el nivel, más experiencia se gana
                        double expResistencia = expBase / (indiceNivel * indiceResistencia);
                        if (expResistencia < 0.0001) expResistencia = 0.0001;
                        jugador.ExpResistencia += expResistencia * minutos;
                        Console.WriteLine($"Has ganado {expFuerza * minutos:F4} exp de Fuerza y {expResistencia * minutos:F4} exp de Resistencia.");
                        break;
                    case "Talar":
                        double indiceFuerzaT = jugador.IndiceAtributo.ContainsKey("fuerza") ? jugador.IndiceAtributo["fuerza"] : 1.0;
                        double expFuerzaT = expBase / (indiceNivel * indiceFuerzaT);
                        if (expFuerzaT < 0.0001) expFuerzaT = 0.0001;
                        jugador.ExpFuerza += expFuerzaT * minutos;
                        double indiceDestreza = jugador.IndiceAtributo.ContainsKey("destreza") ? jugador.IndiceAtributo["destreza"] : 1.0;
                        double expDestreza = expBase / (indiceNivel * indiceDestreza);
                        if (expDestreza < 0.0001) expDestreza = 0.0001;
                        jugador.ExpDestreza += expDestreza * minutos;
                        Console.WriteLine($"Has ganado {expFuerzaT * minutos:F4} exp de Fuerza y {expDestreza * minutos:F4} exp de Destreza.");
                        break;
                }
                RevisarAtributosPorExperiencia(jugador);
            }
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
            MostrarMenuRecoleccion();
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
    // ...existing code...

        // Método para cargar el personaje usando GuardaPersonaje
        public void CargarPersonaje()
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
            // Mostrar sectores disponibles fuera de la ciudad
            var sectores = mapa.ObtenerSectores();
            // Buscar el sector actual por nombre
            var sectorActual = sectores.Find(s => s.Nombre == ubicacionActual.Nombre);
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
                // Console.Clear();
                Console.WriteLine("=== Menú de Rutas ===");
                Console.WriteLine("Sectores conectados:");
                for (int i = 0; i < sectoresConectados.Count; i++)
                {
                    var s = sectoresConectados[i];
                    Console.WriteLine($"{i + 1}. {s.Nombre} - {s.Descripcion}");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Selecciona el sector al que deseas viajar: ");
                string opcion = Console.ReadLine() ?? "0";
                if (opcion == "0")
                {
                    volver = true;
                    break;
                }
                if (int.TryParse(opcion, out int idx) && idx > 0 && idx <= sectoresConectados.Count)
                {
                    var destino = sectoresConectados[idx - 1];
                    ubicacionActual = new Ubicacion
                    {
                        Nombre = destino.Nombre,
                        Tipo = destino.Tipo,
                        Descripcion = destino.Descripcion
                    };
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
                // Al terminar, simplemente regresar al flujo del menú principal
                MostrarMenuPorUbicacion(ref volver);
            }
        }
    }
}
        // Menú de recolección fuera de ciudad
