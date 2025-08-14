using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class MotorMisiones
    {
        private Juego juego;
        public MotorMisiones(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuMisionesNPC()
        {
            Console.Clear();
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
                    break;
                default:
                    Console.WriteLine("Opción inválida");
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
            }
        }
        public void MostrarMisiones()
        {
            Console.Clear();
            Console.WriteLine("--- Misiones Disponibles ---");
            try
            {
                var rutaMisiones = Path.Combine(Environment.CurrentDirectory, "DatosJuego", "misiones", "misiones.json");
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
        public void MostrarNPCs()
        {
            Console.Clear();
            Console.WriteLine("--- NPCs ---");
            try
            {
                var rutaNPCs = Path.Combine(Environment.CurrentDirectory, "DatosJuego", "npcs", "npc.json");
                var rutaMisiones = Path.Combine(Environment.CurrentDirectory, "DatosJuego", "misiones", "misiones.json");
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
        public void RevisarMisiones()
        {
            Console.Clear();
            Console.WriteLine("=== Misiones activas ===");
            if (juego.jugador == null || juego.jugador.Inventario == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                Console.ReadKey();
                return;
            }
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
