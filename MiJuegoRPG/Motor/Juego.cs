
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

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
            // Busca la carpeta raíz del proyecto (donde está el .sln)
            public static string ObtenerRutaRaizProyecto()
            {
                var dir = new DirectoryInfo(Environment.CurrentDirectory);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MiJuegoRPG.sln")))
                {
                    dir = dir.Parent;
                }
                return dir?.FullName ?? Environment.CurrentDirectory;
            }
        // Atributos base para cada clase (mantengo esto por ahora, lo cambiaremos después)
        private static readonly AtributosBase MagoAtributos = new AtributosBase(2, 10, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        private static readonly AtributosBase LadronAtributos = new AtributosBase(4, 3, 8, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        private static readonly AtributosBase GuerreroAtributos = new AtributosBase(10, 2, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        private MiJuegoRPG.Personaje.Personaje? jugador;
        private MenuCiudad menuPrincipal;
        private EstadoMundo estadoMundo;
        private Ubicacion ubicacionActual;

        public Juego()
        {
            menuPrincipal = new MenuCiudad(this);
            estadoMundo = new EstadoMundo();
            InicializarUbicaciones();
            ubicacionActual = estadoMundo.Ubicaciones.Find(u => u.Nombre == "Ciudad de Albor") ?? estadoMundo.Ubicaciones[0];
        }

        private void InicializarUbicaciones()
        {
            // Ejemplo de inicialización básica
            var ciudad = new MiJuegoRPG.Motor.Ubicacion {
                Nombre = "Ciudad de Albor",
                Tipo = "Ciudad",
                Descripcion = "La ciudad inicial, llena de vida y oportunidades.",
                Desbloqueada = true,
                EventosPosibles = new List<string> { "Tienda", "Escuela de Entrenamiento", "Explorar sector", "Descansar en posada", "Salir de la ciudad" }
            };
            var bosque = new MiJuegoRPG.Motor.Ubicacion {
                Nombre = "Bosque Oscuro",
                Tipo = "Ruta",
                Descripcion = "Un bosque peligroso, ideal para aventureros.",
                Desbloqueada = false,
                EventosPosibles = new List<string> { "Encuentro enemigo", "Descubrir objeto", "Encontrar mazmorra" }
            };
            var rio = new MiJuegoRPG.Motor.Ubicacion {
                Nombre = "Río Plateado",
                Tipo = "Ruta",
                Descripcion = "Un río que requiere barco para cruzar.",
                Desbloqueada = false,
                Requisitos = new Dictionary<string, object> { { "Barco", true } },
                EventosPosibles = new List<string> { "Evento especial", "Encuentro enemigo" }
            };
            var ciudadBruma = new MiJuegoRPG.Motor.Ubicacion {
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

        public void Iniciar()
        {
            Console.WriteLine("Bienvenido a Mi Primer Juego.\n");

            // Cargar enemigos y armas desde la carpeta PjDatos del proyecto
            string rutaProyecto = ObtenerRutaRaizProyecto();
            string rutaArmas = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "armas.json");
            Objetos.GestorArmas.CargarArmas(rutaArmas);

            string rutaEnemigos = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "enemigos.json");
            GeneradorEnemigos.CargarEnemigos(rutaEnemigos);

            bool personajeCargado = false;
            while (!personajeCargado)
            {
                Console.WriteLine("¿Qué deseas hacer?");
                Console.WriteLine("1. Crear personaje nuevo");
                Console.WriteLine("2. Cargar personaje guardado");
                Console.Write("Elige una opción: ");
                var opcion = Console.ReadLine();

                if (opcion == "2")
                {
                    CargarPersonaje();
                    if (jugador == null)
                    {
                        Console.WriteLine("No se encontró personaje guardado. ¿Deseas crear uno nuevo? (s/n): ");
                        var crearNuevo = Console.ReadLine();
                        if (crearNuevo != null && crearNuevo.Trim().ToLower() == "s")
                        {
                            jugador = CreadorPersonaje.Crear();
                            personajeCargado = true;
                        }
                        else
                        {
                            Console.WriteLine("No se ha creado ningún personaje. El juego no puede continuar.");
                        }
                    }
                    else
                    {
                        personajeCargado = true;
                    }
                }
                else if (opcion == "1")
                {
                    jugador = CreadorPersonaje.Crear();
                    personajeCargado = true;
                }
                else
                {
                    Console.WriteLine("Opción no válida. Intenta de nuevo.\n");
                }
            }

            // Iniciar el bucle principal del juego
            BuclePrincipal();
        }

        private void BuclePrincipal()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Menú Principal ===");
                Console.WriteLine("1. Ver ubicación actual");
                Console.WriteLine("2. Gestionar inventario");
                Console.WriteLine("3. Guardar/Cargar");
                Console.WriteLine("4. Revisar misiones activas");
                Console.WriteLine("0. Salir");

                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        MostrarMenuUbicacion();
                        break;
                    case "2":
                        GestionarInventario();
                        break;
                    case "3":
                        MostrarMenuGuardado();
                        break;
                    case "4":
                        RevisarMisiones();
                        break;
                    case "0":
                        Console.WriteLine("Gracias por jugar. ¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("Opción no válida. Intenta de nuevo.");
                        break;
                }
            }
        }


        // Nuevo método para mostrar el submenú de Guardar/Cargar.
        public void MostrarMenuGuardado()
            {
                Console.Clear();
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
            Console.WriteLine($"Sectores disponibles en {ubicacionActual.Nombre}:");
            int i = 1;
            foreach (var evento in ubicacionActual.EventosPosibles)
            {
                Console.WriteLine($"{i}. {evento}");
                i++;
            }
            Console.WriteLine($"{i}. Volver");
            var opcion = Console.ReadLine();
            int seleccion;
            if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= ubicacionActual.EventosPosibles.Count)
            {
                string eventoElegido = ubicacionActual.EventosPosibles[seleccion - 1];
                // Ejemplo de lógica de eventos
                switch (eventoElegido)
                {
                    case "Explorar sector":
                        GenerarEventoExploracion();
                        break;
                    case "Escuela de Entrenamiento":
                        Entrenar();
                        break;
                    case "Tienda":
                        IrATienda();
                        break;
                    case "Descansar en posada":
                        Console.WriteLine("Has descansado y recuperado energía.");
                        // Aquí podrías restaurar vida, energía, etc.
                        break;
                    default:
                        Console.WriteLine($"Evento '{eventoElegido}' aún no implementado.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Volviendo al menú principal...");
            }
        }

        private void GenerarEventoExploracion()
        {
            // Ejemplo de eventos aleatorios
            var eventos = new List<string> { "Encuentro con enemigo", "Descubrir objeto", "Encontrar mazmorra", "Encontrar NPC", "Evento especial" };
            var random = new Random();
            string evento = eventos[random.Next(eventos.Count)];
            Console.WriteLine($"Evento generado: {evento}");
            // Progresión por actividad
            switch (evento)
            {
                case "Encuentro con enemigo":
                    Console.WriteLine("¡Te enfrentas a un enemigo!");
                    ComenzarCombate();
                    ProgresionPorActividad("combate");
                    break;
                case "Descubrir objeto":
                    Console.WriteLine("Has encontrado una poción curativa y la agregas a tu inventario.");
                    if (jugador != null)
                    {
                        jugador.Inventario.AgregarObjeto(new Objetos.Pocion("Poción Curativa", 20));
                        ProgresionPorActividad("exploracion");
                    }
                    break;
                case "Encontrar mazmorra":
                    Console.WriteLine("Has descubierto una mazmorra misteriosa. Puedes intentar entrar en el futuro.");
                    ProgresionPorActividad("exploracion");
                    break;
                case "Encontrar NPC":
                    Console.WriteLine("Un NPC te ofrece una misión: 'Encuentra el mineral raro en la cueva'.");
                    ProgresionPorActividad("trabajo");
                    break;
                case "Evento especial":
                    Console.WriteLine("Ocurre un evento inesperado en el área. ¡Tu suerte aumenta!");
                    ProgresionPorActividad("suerte");
                    break;
            }
        }

        public void Entrenar()
        {
            Console.WriteLine("¿Qué atributo deseas entrenar?");
            Console.WriteLine("1. Fuerza");
            Console.WriteLine("2. Magia");
            Console.WriteLine("3. Agilidad");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    EntrenarAtributo("Fuerza");
                    break;
                case "2":
                    EntrenarAtributo("Magia");
                    break;
                case "3":
                    EntrenarAtributo("Agilidad");
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        private void EntrenarAtributo(string atributo)
        {
            if (jugador == null) return;
            // Aquí puedes sumar experiencia y subir el atributo
            Console.WriteLine($"Entrenando {atributo}...");
            // Ejemplo: jugador.Atributos.Fuerza++;
            // Si el atributo supera cierto umbral, desbloquear clase/título
        }

        public void IrATienda()
        {
            Console.WriteLine("Bienvenido a la tienda. Puedes comprar una poción por 10 monedas de oro.");
            if (jugador != null && jugador.Oro >= 10)
            {
                Console.WriteLine("¿Comprar poción curativa por 10 oro? (s/n)");
                var opcion = Console.ReadLine();
                if (opcion != null && opcion.Trim().ToLower() == "s")
                {
                    jugador.Oro -= 10;
                    jugador.Inventario.AgregarObjeto(new Objetos.Pocion("Poción Curativa", 20));
                    Console.WriteLine("¡Has comprado una poción curativa!");
                }
                else
                {
                    Console.WriteLine("No compraste nada.");
                }
            }
            else if (jugador != null)
            {
                Console.WriteLine("No tienes suficiente oro.");
            }
            else
            {
                Console.WriteLine("No hay personaje cargado.");
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        public void GestionarInventario()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("=== Inventario ===");
            if (jugador.Inventario.NuevosObjetos.Count == 0)
            {
                Console.WriteLine("Tu inventario está vacío.");
            }
            else
            {
                for (int i = 0; i < jugador.Inventario.NuevosObjetos.Count; i++)
                {
                    var obj = jugador.Inventario.NuevosObjetos[i];
                    Console.WriteLine($"{i + 1}. {obj.Nombre}");
                }
                Console.WriteLine("¿Quieres usar una poción? Ingresa el número o presiona Enter para salir.");
                var opcion = Console.ReadLine();
                int seleccion;
                if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= jugador.Inventario.NuevosObjetos.Count)
                {
                    var obj = jugador.Inventario.NuevosObjetos[seleccion - 1];
                    if (obj is Objetos.Pocion pocion)
                    {
                        jugador.Vida = Math.Min(jugador.Vida + pocion.Curacion, jugador.VidaMaxima);
                        jugador.Inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                        Console.WriteLine($"Usaste {pocion.Nombre} y recuperaste {pocion.Curacion} puntos de vida.");
                    }
                    else
                    {
                        Console.WriteLine($"No puedes usar {obj.Nombre}.");
                    }
                }
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        // Método para guardar el personaje en un archivo JSON
        public void GuardarPersonaje()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para guardar.");
                return;
            }

            string nombreArchivo = jugador?.Nombre ?? "personaje";
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                nombreArchivo = "personaje";
            }
            var dirActual = Environment.CurrentDirectory;
            var dirPadre = Directory.GetParent(dirActual);
            var dirProyecto = dirPadre != null ? dirPadre.Parent : null;
            string rutaProyecto = dirProyecto != null ? dirProyecto.FullName : dirActual;
            string rutaCarpeta = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "PjGuardados");
            Directory.CreateDirectory(rutaCarpeta);
            string rutaGuardado = Path.Combine(rutaCarpeta, nombreArchivo + ".json");
            try
            {
                string json = JsonSerializer.Serialize(jugador, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaGuardado, json);
                Console.WriteLine($"Personaje guardado exitosamente como {nombreArchivo}.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el personaje: {ex.Message}");
            }
        }

        // Método para cargar el personaje desde un archivo JSON
        public void CargarPersonaje()
        {
            try
            {
                // Obtener la ruta raíz del proyecto (no bin)
                string rutaProyecto = ObtenerRutaRaizProyecto();
                string rutaPj = Path.Combine(rutaProyecto, "MiJuegoRPG", "PjDatos", "PjGuardados");
                Console.WriteLine($"[DEBUG] Buscando personajes en: {rutaPj}");
                var archivos = Directory.Exists(rutaPj) ? Directory.GetFiles(rutaPj, "*.json") : Array.Empty<string>();
                if (archivos.Length == 0)
                {
                    Console.WriteLine("No hay personajes guardados.");
                    return;
                }
                Console.WriteLine("Personajes disponibles:");
                for (int i = 0; i < archivos.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(archivos[i])}");
                }
                Console.Write("Elige el número del personaje a cargar: ");
                if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= archivos.Length)
                {
                    string json = File.ReadAllText(archivos[seleccion - 1]);
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    options.Converters.Add(new ObjetoJsonConverter());
                    options.Converters.Add(new JsonStringEnumConverter());
                    jugador = JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json, options);
                    Console.WriteLine($"Personaje '{Path.GetFileNameWithoutExtension(archivos[seleccion - 1])}' cargado exitosamente.");
                }
                else
                {
                    Console.WriteLine("Selección inválida. No se cargó ningún personaje.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el personaje: {ex.Message}");
            }
        }

        // Método que encapsula el combate, usando la clase GeneradorEnemigos
        public void ComenzarCombate()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para combatir. Creando nuevo personaje...");
                jugador = CreadorPersonaje.Crear();
            }

            // Generamos un enemigo y luego iniciamos el combate con la nueva clase
            var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(jugador);
            GeneradorEnemigos.IniciarCombate(jugador, enemigo);
        }

        public void MostrarMenuUbicacion()
        {
            Console.Clear();
            Console.WriteLine($"=== {ubicacionActual.Nombre} ===");
            Console.WriteLine(ubicacionActual.Descripcion);
            int i = 1;
            foreach (var evento in ubicacionActual.EventosPosibles)
            {
                Console.WriteLine($"{i}. {evento}");
                i++;
            }
            Console.WriteLine($"{i}. Salir de la ciudad");
            var opcion = Console.ReadLine();
                int seleccion;
                if (int.TryParse(opcion, out seleccion))
                {
                    if (seleccion == i)
                    {
                        // Opción Salir de la ciudad
                        MostrarMenuRutas();
                    }
                    else if (seleccion > 0 && seleccion <= ubicacionActual.EventosPosibles.Count)
                    {
                        string eventoElegido = ubicacionActual.EventosPosibles[seleccion - 1];
                        switch (eventoElegido)
                        {
                            case "Tienda":
                                IrATienda();
                                break;
                            case "Escuela de Entrenamiento":
                                Entrenar();
                                break;
                            case "Explorar sector":
                                ExplorarSector();
                                break;
                            case "Descansar en posada":
                                Console.WriteLine("Has descansado y recuperado energía.");
                                // Aquí podrías restaurar vida, energía, etc.
                                break;
                            default:
                                Console.WriteLine($"Evento '{eventoElegido}' aún no implementado.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Opción no válida.");
                    }
                }
                else
                {
                    Console.WriteLine("Opción no válida.");
                }
        }

        public void MostrarMenuRutas()
        {
            Console.Clear();
            Console.WriteLine($"Rutas disponibles desde {ubicacionActual.Nombre}:");
            int i = 1;
            foreach (var ruta in ubicacionActual.Rutas)
            {
                Console.WriteLine($"{i}. {ruta.Nombre} {(ruta.Desbloqueada ? "(Desbloqueada)" : "(Bloqueada)")}");
                i++;
            }
            Console.WriteLine($"{i}. Volver");
            var opcion = Console.ReadLine();
            int seleccion;
            if (int.TryParse(opcion, out seleccion))
            {
                if (seleccion > 0 && seleccion <= ubicacionActual.Rutas.Count)
                {
                    var rutaElegida = ubicacionActual.Rutas[seleccion - 1];
                    if (rutaElegida.Desbloqueada)
                    {
                        var nuevaUbicacion = estadoMundo.Ubicaciones.Find(u => u.Nombre == rutaElegida.Destino);
                        if (nuevaUbicacion != null)
                        {
                            ubicacionActual = nuevaUbicacion;
                            Console.WriteLine($"Viajaste a {ubicacionActual.Nombre}.");
                            Console.WriteLine(ubicacionActual.Descripcion);
                            Console.WriteLine("Presiona cualquier tecla para ver los sectores y eventos disponibles...");
                            Console.ReadKey();
                            MostrarMenuUbicacion();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("No se encontró la ubicación de destino.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("La ruta está bloqueada. No puedes viajar aún.");
                    }
                }
                else if (seleccion == i)
                {
                    // Volver
                    return;
                }
                else
                {
                    Console.WriteLine("Opción no válida.");
                }
            }
            else
            {
                Console.WriteLine("Opción no válida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        private void ProgresionPorActividad(string actividad)
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
            Console.Clear();
            Console.WriteLine("=== Misiones activas ===");
            if (jugador == null || jugador.Inventario == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                Console.ReadKey();
                return;
            }
            // Ejemplo: buscar misiones en el inventario del jugador
            // (En una implementación real, las misiones estarían en una lista propia)
            var misionesEjemplo = new List<Mision> {
                new Mision {
                    Nombre = "Mineral raro para el herrero",
                    Descripcion = "Encuentra el mineral en la cueva y llévalo al herrero.",
                    UbicacionNPC = "Ciudad de Albor",
                    Requisitos = new List<string> { "Mineral raro" },
                    Estado = "En progreso"
                }
            };
            foreach (var mision in misionesEjemplo)
            {
                Console.WriteLine($"Misión: {mision.Nombre}");
                Console.WriteLine($"Solicitante: Herrero");
                Console.WriteLine($"Item solicitado: {string.Join(", ", mision.Requisitos)}");
                Console.WriteLine($"Ubicación del NPC: {mision.UbicacionNPC}");
                Console.WriteLine($"Estado: {mision.Estado}");
                Console.WriteLine("---");
            }
            Console.WriteLine("Presiona cualquier tecla para volver al menú...");
            Console.ReadKey();
        }
    }
}
