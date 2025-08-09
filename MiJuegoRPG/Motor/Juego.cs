using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
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

            // Cargar enemigos desde el archivo JSON al inicio del juego.
            string rutaBase;
            var dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            if (dir?.Parent?.Parent != null)
                rutaBase = dir.Parent.Parent.FullName;
            else
                rutaBase = AppDomain.CurrentDomain.BaseDirectory;
            string rutaArmas = Path.Combine(rutaBase, "MiJuegoRPG", "PjDatos", "armas.json");
            Objetos.GestorArmas.CargarArmas(rutaArmas);

            string rutaEnemigos = Path.Combine(rutaBase, "MiJuegoRPG", "PjDatos", "enemigos.json");
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
                Console.WriteLine("1. Explorar");
                Console.WriteLine("2. Entrenar");
                Console.WriteLine("3. Ir a la tienda");
                Console.WriteLine("4. Gestionar inventario");
                Console.WriteLine("5. Guardar/Cargar");
                Console.WriteLine("6. Salir");

                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        ExplorarSector();
                        break;
                    case "2":
                        Entrenar();
                        break;
                    case "3":
                        IrATienda();
                        break;
                    case "4":
                        GestionarInventario();
                        break;
                    case "5":
                        MostrarMenuGuardado();
                        break;
                    case "6":
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
                    // Aquí podrías llamar a GeneradorEnemigos y sumar XP de fuerza, resistencia, etc.
                    Console.WriteLine("¡Te enfrentas a un enemigo!");
                    // jugador.Entrenar("fuerza");
                    break;
                case "Descubrir objeto":
                    Console.WriteLine("Has encontrado un objeto útil.");
                    // jugador.Entrenar("destreza");
                    break;
                case "Encontrar mazmorra":
                    Console.WriteLine("Has descubierto una mazmorra misteriosa.");
                    // jugador.Entrenar("resistencia");
                    break;
                case "Encontrar NPC":
                    Console.WriteLine("Un NPC te ofrece una misión.");
                    // jugador.Entrenar("carisma");
                    break;
                case "Evento especial":
                    Console.WriteLine("Ocurre un evento inesperado en el área.");
                    // jugador.Entrenar("suerte");
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
            Console.WriteLine("Tienda aún no implementada.");
            Console.ReadKey();
        }

        public void GestionarInventario()
        {
            Console.WriteLine("Inventario aún no implementado.");
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
            string rutaCarpeta = @"C:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\MiJuegoRPG\PjDatos\PjGuardados";
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
                string rutaPj = @"C:\Users\ASUS\OneDrive\Documentos\GitHub\dotnet-juego-rpg\MiJuegoRPG\PjDatos\PjGuardados";
                var archivos = Directory.GetFiles(rutaPj, "*.json");
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
                    jugador = JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json);
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
            // Aquí iría la lógica para cada opción, incluyendo explorar, tienda, entrenamiento, etc.
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
            var opcion = Console.ReadLine();
            // Aquí iría la lógica para viajar y verificar requisitos
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
    }
}
