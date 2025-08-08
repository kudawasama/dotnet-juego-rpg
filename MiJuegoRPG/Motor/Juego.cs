using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
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
                    ClaseNombre = jugador.Clase.Nombre,
                    Vida = jugador.Vida,
                    VidaMaxima = jugador.VidaMaxima
                };

                string directorio = "Guardados";
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                string rutaCompleta = Path.Combine(directorio, $"{nombreArchivo}.json");
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
                Console.WriteLine($"Error al cargar personaje: {ex.Message}");
            }
        }

        private MiJuegoRPG.Personaje.Personaje CrearPersonajeDesdeDatos(PersonajeData datos)
        {
            // Crear clase basada en el nombre guardado
            Clase clase = datos.ClaseNombre switch
            {
                "Mago" => new Clase("Mago", new AtributosBase(2, 10, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15), new Personaje.Estadisticas(new AtributosBase(2, 10, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15))),
                "Ladrón" => new Clase("Ladrón", new AtributosBase(4, 3, 8, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15), new Personaje.Estadisticas(new AtributosBase(4, 3, 8, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15))),
                _ => new Clase("Guerrero", new AtributosBase(10, 2, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15), new Personaje.Estadisticas(new AtributosBase(10, 2, 3, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)))
            };

            var personaje = new MiJuegoRPG.Personaje.Personaje(datos.Nombre, clase)
            {
                Vida = datos.Vida,
                VidaMaxima = datos.VidaMaxima
            };

            return personaje;
        }
    }
}