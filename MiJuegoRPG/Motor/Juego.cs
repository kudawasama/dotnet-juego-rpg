using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
        // Atributos base para cada clase
        private static readonly AtributosBase MagoAtributos = new AtributosBase(2, 10, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        private static readonly AtributosBase LadronAtributos = new AtributosBase(4, 3, 8, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        private static readonly AtributosBase GuerreroAtributos = new AtributosBase(10, 2, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        private MiJuegoRPG.Personaje.Personaje? jugador;
        private MenuEntreCombate menuEntreCombate;
        
        public Juego()
        {
            menuEntreCombate = new MenuEntreCombate(this);
        }

        public void Iniciar()
        {
            Console.WriteLine("Bienvenido a Mi Primer Juego.");
            jugador = CreadorPersonaje.Crear();
            
            // Iniciar el bucle principal del juego
            BuclePrincipal();
        }

        private void BuclePrincipal()
        {
            while (true)
            {
                if (jugador != null)
                {
                    jugador.MostrarEstado();
                    menuEntreCombate.MostrarMenu();
                }
                else
                {
                    Console.WriteLine("No hay personaje activo. Creando nuevo personaje...");
                    jugador = CreadorPersonaje.Crear();
                }
            }
        }


                // Dentro de la clase 'Juego'
        public void ComenzarCombate()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para combatir. Creando nuevo personaje...");
                jugador = CreadorPersonaje.Crear();
            }

            // Aquí puedes añadir la lógica para elegir un enemigo aleatorio
            // Por ahora, crearemos un Goblin por defecto
            var enemigo = new MiJuegoRPG.Enemigos.Goblin();

            Console.Clear();
            Console.WriteLine($"¡Un {enemigo.Nombre} salvaje ha aparecido!");

            // Iniciar el combate por turnos
            var combate = new CombatePorTurnos(jugador, enemigo);
            combate.IniciarCombate();

            // Una vez que el combate termina, puedes volver a mostrar el menú
            Console.WriteLine("\nEl combate ha terminado. Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        public void GuardarPersonaje()
        {
            if (jugador == null)
            {
                Console.WriteLine("No hay personaje para guardar.");
                return;
            }

            try
            {
                Console.Write("Nombre del archivo (sin extensión): ");
                string nombreArchivo = Console.ReadLine() ?? jugador.Nombre;
                
                var datosPersonaje = new PersonajeData
                {
                    Nombre = jugador.Nombre,
                    Nivel = jugador.Nivel,
                    Experiencia = jugador.Experiencia,
                    ExperienciaSiguienteNivel = jugador.ExperienciaSiguienteNivel,
                    Oro = jugador.Oro,
                    VidaActual = jugador.Vida,
                    VidaMaxima = jugador.VidaMaxima,
                    Atributos = jugador.Atributos,
                    Clase = jugador.Clase,
                    Inventario = jugador.Inventario,
                    Defensa = jugador.Defensa,
                    EstaVivo = jugador.EstaVivo,
                    ClaseNombre = jugador.Clase.Nombre
                };
                // Crear directorio si no existe
                string directorio = "Guardados";
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }
                directorio = Path.Combine( "MiJuegoRPG", "PjDatos", "Saves.json");
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }
                string rutaCompleta = Path.Combine(AppContext.BaseDirectory, "Guardados", $"{nombreArchivo}.json"); // Ruta completa del archivo
                string json = JsonSerializer.Serialize(datosPersonaje, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(rutaCompleta, json);
                Console.WriteLine($"Personaje guardado exitosamente en: {rutaCompleta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        public void CargarPersonaje()
        {
            try
            {
                string directorio = "Guardados";
                if (!Directory.Exists(directorio))
                {
                    Console.WriteLine("No hay personajes guardados.");
                    return;
                }

                var archivos = Directory.GetFiles(directorio, "*.json");
                if (archivos.Length == 0)
                {
                    Console.WriteLine("No se encontraron personajes guardados.");
                    return;
                }

                Console.WriteLine("\n=== Personajes Disponibles ===");
                for (int i = 0; i < archivos.Length; i++)
                {
                    string nombreArchivo = Path.GetFileNameWithoutExtension(archivos[i]);
                    Console.WriteLine($"{i + 1}. {nombreArchivo}");
                }

                Console.Write("\nSelecciona un personaje (número): ");
                if (int.TryParse(Console.ReadLine(), out int seleccion) &&
                    seleccion > 0 && seleccion <= archivos.Length)
                {
                    string archivoSeleccionado = archivos[seleccion - 1];
                    string json = File.ReadAllText(archivoSeleccionado);
                    var datosPersonaje = JsonSerializer.Deserialize<PersonajeData>(json);

                    if (datosPersonaje != null)
                    {
                        jugador = CrearPersonajeDesdeDatos(datosPersonaje);
                        Console.WriteLine($"¡Personaje {datosPersonaje.Nombre} cargado exitosamente!");
                    }
                    else
                    {
                        Console.WriteLine("Error: No se pudieron leer los datos del personaje.");
                    }
                }
                else
                {
                    Console.WriteLine("Selección inválida.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar: {ex.Message}");
            }
        }

        private MiJuegoRPG.Personaje.Personaje CrearPersonajeDesdeDatos(PersonajeData datos)
        {
            Clase clase = datos.ClaseNombre switch
            {
                "Mago" => new Clase("Mago", MagoAtributos, new Personaje.Estadisticas(MagoAtributos)),
                "Ladrón" => new Clase("Ladrón", LadronAtributos, new Personaje.Estadisticas(LadronAtributos)),
                _ => new Clase("Guerrero", GuerreroAtributos, new Personaje.Estadisticas(GuerreroAtributos))
            };

            var personaje = new MiJuegoRPG.Personaje.Personaje(datos.Nombre, clase)
            {
                Nivel = datos.Nivel,
                Experiencia = datos.Experiencia,
                ExperienciaSiguienteNivel = datos.ExperienciaSiguienteNivel,
                Oro = datos.Oro,
                Vida = datos.VidaActual,
                VidaMaxima = datos.VidaMaxima,
                Inventario = datos.Inventario
            };

            return personaje;
        }
    }
}