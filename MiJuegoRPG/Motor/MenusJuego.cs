using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MenusJuego
    {
        private Juego juego;
        public MenusJuego(Juego juego)
        {
            this.juego = juego;
        }

        public void MostrarMenuViajar()
        {
            //Console.Clear();
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine("--- Menú de Viaje ---");
            if (juego.estadoMundo?.Ubicaciones == null || juego.estadoMundo.Ubicaciones.Count == 0)
            {
                Console.WriteLine("No hay ubicaciones disponibles.");
                Console.WriteLine("Presiona cualquier tecla para volver...");
                Console.ReadKey();
                return;
            }
            int i = 1;
            foreach (var ubicacion in juego.estadoMundo.Ubicaciones)
            {
                if (ubicacion.Desbloqueada)
                    Console.WriteLine($"{i}. {ubicacion.Nombre} - {ubicacion.Descripcion}");
                i++;
            }
            Console.WriteLine("0. Volver");
            Console.Write("Elige tu destino: ");
            var opcion = Console.ReadLine();
            if (int.TryParse(opcion, out int seleccion) && seleccion > 0 && seleccion <= juego.estadoMundo.Ubicaciones.Count)
            {
                var destino = juego.estadoMundo.Ubicaciones[seleccion - 1];
                if (destino.Desbloqueada)
                {
                    juego.ubicacionActual = destino;
                    Console.WriteLine($"Viajaste a {destino.Nombre}.");
                    Console.WriteLine(destino.Descripcion);
                }
                else
                {
                    Console.WriteLine("No tienes acceso a esa ubicación.");
                }
            }
            else if (seleccion == 0)
            {
                // Volver al menú anterior
                return;
            }
            else
            {
                Console.WriteLine("Opción no válida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        public void MostrarMenuGuardado()
        {
            //Console.Clear();
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine("=== Menú de Guardar/Cargar ===");
            Console.WriteLine("1. Guardar partida");
            Console.WriteLine("2. Cargar partida");
            Console.WriteLine("3. Volver al menú principal");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    if (juego.jugador != null)
                        GestorArchivos.GuardarPersonaje(juego.jugador);
                    else
                        Console.WriteLine("No hay personaje para guardar.");
                    break;
                case "2":
                    var pj = GestorArchivos.CargarPersonaje();
                    if (pj != null)
                        juego.jugador = pj;
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

        public void MostrarMenuMisionesNPC()
        {
            //Console.Clear();
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine("--- Menú de Misiones y NPC ---");
            Console.WriteLine("1. Ver misiones disponibles");
            Console.WriteLine("2. Ver NPCs");
            Console.WriteLine("3. Volver");
            Console.Write("Seleccione una opción: ");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    MostrarMisiones();
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
                case "2":
                    MostrarNPCs();
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
                case "3":
                    // Iniciar(); // Método no implementado
                    break;
                default:
                    Console.WriteLine("Opción inválida");
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
            }
        }

        private void MostrarMisiones()
        {
            //Console.Clear();
            Console.WriteLine("--- Misiones Disponibles ---");
            try
            {
                var rutaMisiones = Path.Combine(Environment.CurrentDirectory, "PjDatos", "misiones.json");
                if (File.Exists(rutaMisiones))
                {
                    var json = File.ReadAllText(rutaMisiones);
                    var misiones = JsonSerializer.Deserialize<List<Mision>>(json);
                    if (misiones != null && misiones.Count > 0)
                    {
                        foreach (var m in misiones)
                        {
                            Console.WriteLine($"- {m.Nombre}: {m.Descripcion}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No hay misiones disponibles.");
                    }
                }
                else
                {
                    Console.WriteLine("Archivo de misiones no encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer misiones: {ex.Message}");
            }
        }

        private void MostrarNPCs()
        {
            //Console.Clear();
            Console.WriteLine("--- NPCs ---");
            try
            {
                var rutaNPCs = Path.Combine(Environment.CurrentDirectory, "PjDatos", "npc.json");
                var rutaMisiones = Path.Combine(Environment.CurrentDirectory, "PjDatos", "misiones.json");
                List<Mision> misiones = new List<Mision>();
                if (File.Exists(rutaMisiones))
                {
                    var jsonMisiones = File.ReadAllText(rutaMisiones);
                    var listaMisiones = JsonSerializer.Deserialize<List<Mision>>(jsonMisiones);
                    if (listaMisiones != null)
                        misiones = listaMisiones;
                }
                if (File.Exists(rutaNPCs))
                {
                    var json = File.ReadAllText(rutaNPCs);
                    var npcs = JsonSerializer.Deserialize<List<NPC>>(json);
                    if (npcs != null && npcs.Count > 0)
                    {
                        foreach (var npc in npcs)
                        {
                            Console.WriteLine($"- {npc.Nombre}: {npc.Descripcion}");
                            if (npc.Misiones != null && npc.Misiones.Count > 0)
                            {
                                Console.WriteLine("  Misiones:");
                                foreach (var idMision in npc.Misiones)
                                {
                                    var mision = misiones.Find(m => m.Nombre == idMision);
                                    if (mision != null)
                                        Console.WriteLine($"    * {mision.Nombre}: {mision.Descripcion}");
                                    else
                                        Console.WriteLine($"    * (Misión no encontrada: {idMision})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("  No tiene misiones asociadas.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No hay NPCs disponibles.");
                    }
                }
                else
                {
                    Console.WriteLine("Archivo de NPCs no encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer NPCs: {ex.Message}");
            }
        }
    }
}
