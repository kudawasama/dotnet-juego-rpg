using System;
using System.IO;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor
{
    public class Juego
    {
        private MiJuegoRPG.Personaje.Personaje? jugador;
        private MenuEntreCombate MenuEntreCombate;
        
        public Juego()
        {
            MenuEntreCombate = new MenuEntreCombate(this);
        }
        
        public void Iniciar()
        {
            Console.WriteLine("Bienvenido al juego de texto estilo Satisfy.");
            Console.WriteLine("¿Deseas crear un nuevo personaje o cargar uno existente?");
            Console.WriteLine("1. Crear nuevo personaje");
            Console.WriteLine("2. Cargar personaje guardado");
            Console.Write("Selecciona una opción (1 o 2): ");
            var opcion = Console.ReadLine();

            if (opcion == "2")
            {
                CargarPersonaje();
                if (jugador == null)
                {
                    Console.WriteLine("Creando nuevo personaje...");
                    jugador = CreadorPersonaje.Crear();
                }
            }
            else
            {
                jugador = CreadorPersonaje.Crear();
            }

            // Iniciar bucle de juego principal
            BucleJuegoPrincipal();
        }

        private void BucleJuegoPrincipal()
        {
            while (jugador != null && jugador.EstaVivo)
            {
                jugador.MostrarEstado();
                
                Console.WriteLine("\n¿Deseas continuar jugando? (s/n)");
                var respuesta = Console.ReadLine();
                if (respuesta?.ToLower() != "s")
                {
                    break;
                }

                // Generar enemigo aleatorio
                var enemigo = GenerarEnemigoAleatorio();
                Console.WriteLine($"\n¡Un {enemigo.Nombre} aparece!");

                // Iniciar combate
                var combate = new CombatePorTurnos(jugador, enemigo);
                combate.IniciarCombate();

                // Mostrar menú entre combates si el jugador sigue vivo
                if (jugador.EstaVivo)
                {
                    MenuEntreCombate.MostrarMenu();
                }
            }

            if (jugador != null && !jugador.EstaVivo)
            {
                Console.WriteLine("¡Game Over! Tu personaje ha sido derrotado.");
            }
            else
            {
                Console.WriteLine("¡Gracias por jugar!");
            }
        }

        private Enemigo GenerarEnemigoAleatorio()
        {
            Random random = new Random();
            int tipoEnemigo = random.Next(1, 3);

            return tipoEnemigo switch
            {
                1 => new Goblin(),
                2 => new GranGoblin(),
                _ => new Goblin()
            };
        }

        public void GuardarPersonaje()
        {
            if (jugador != null)
            {
                try
                {
                    string rutaDirectorio = @"C:\Users\jose.cespedes\Desktop\Programacion\MiJuegoRPG\PjDatos";
                    Directory.CreateDirectory(rutaDirectorio);

                    string rutaArchivo = Path.Combine(rutaDirectorio, "Saves.json");
                    CreadorPersonaje.GuardarPersonaje(jugador, rutaArchivo);
                    Console.WriteLine("Personaje guardado correctamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar personaje: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No hay personaje para guardar.");
            }
        }

        public void CargarPersonaje()
        {
            try
            {
                string rutaArchivo = @"C:\Users\jose.cespedes\Desktop\Programacion\MiJuegoRPG\PjDatos\Saves.json";

                if (!File.Exists(rutaArchivo))
                {
                    Console.WriteLine("No se encontró ningún personaje guardado.");
                    return;
                }

                jugador = CreadorPersonaje.CargarPersonaje(rutaArchivo);
                Console.WriteLine("Personaje cargado exitosamente:");
                CreadorPersonaje.MostrarPersonaje(jugador);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar personaje: {ex.Message}");
            }
        }

        public void MostrarEstadoJugador()
        {
            if (jugador != null)
            {
                jugador.MostrarEstado();
            }
            else
            {
                Console.WriteLine("No hay personaje activo.");
            }
        }
    }
}