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
            // Console.Clear();
            juego.Ui.WriteLine(juego.FormatoRelojMundo);
            juego.Ui.WriteLine("--- Menú de Misiones y NPC ---");
            juego.Ui.WriteLine("1. Ver misiones disponibles");
            juego.Ui.WriteLine("2. Ver NPCs");
            juego.Ui.WriteLine("3. Volver");
            var opcion = InputService.LeerOpcion("Seleccione una opción: ");
            switch (opcion)
            {
                case "1":
                    MostrarMisiones();
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
                case "2":
                    MostrarNPCs();
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
                case "3":
                    break;
                default:
                    juego.Ui.WriteLine("Opción inválida");
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
            }
        }
        public void MostrarMisiones()
        {
            // Console.Clear();
            juego.Ui.WriteLine("--- Misiones Disponibles ---");
            try
            {
                var rutaMisiones = MiJuegoRPG.Motor.Servicios.PathProvider.MisionesPath("misiones.json");
                if (File.Exists(rutaMisiones))
                {
                    var json = File.ReadAllText(rutaMisiones);
                    var misiones = JsonSerializer.Deserialize<List<Mision>>(json);
                    if (misiones != null && misiones.Count > 0)
                    {
                        foreach (var m in misiones)
                        {
                            juego.Ui.WriteLine($"- {m.Nombre}: {m.Descripcion}");
                        }
                    }
                    else
                    {
                        juego.Ui.WriteLine("No hay misiones disponibles.");
                    }
                }
                else
                {
                    juego.Ui.WriteLine("Archivo de misiones no encontrado.");
                }
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"Error al leer misiones: {ex.Message}");
            }
        }
        public void MostrarNPCs()
        {
            // Console.Clear();
            juego.Ui.WriteLine("--- NPCs ---");
            try
            {
                var rutaNPCs = MiJuegoRPG.Motor.Servicios.PathProvider.NpcsPath("npc.json");
                var rutaMisiones = MiJuegoRPG.Motor.Servicios.PathProvider.MisionesPath("misiones.json");
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
                            juego.Ui.WriteLine($"- {npc.Nombre}: {npc.Descripcion}");
                            if (npc.Misiones != null && npc.Misiones.Count > 0)
                            {
                                juego.Ui.WriteLine("  Misiones:");
                                foreach (var idMision in npc.Misiones)
                                {
                                    var mision = misiones.Find(m => m.Nombre == idMision);
                                    if (mision != null)
                                        juego.Ui.WriteLine($"    * {mision.Nombre}: {mision.Descripcion}");
                                    else
                                        juego.Ui.WriteLine($"    * (Misión no encontrada: {idMision})");
                                }
                            }
                            else
                            {
                                juego.Ui.WriteLine("  No tiene misiones asociadas.");
                            }
                        }
                    }
                    else
                    {
                        juego.Ui.WriteLine("No hay NPCs disponibles.");
                    }
                }
                else
                {
                    juego.Ui.WriteLine("Archivo de NPCs no encontrado.");
                }
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"Error al leer NPCs: {ex.Message}");
            }
        }
        public void RevisarMisiones()
        {
            // Console.Clear();
            juego.Ui.WriteLine("=== Misiones activas ===");
            if (juego.jugador == null || juego.jugador.Inventario == null)
            {
                juego.Ui.WriteLine("No hay personaje cargado.");
                InputService.Pausa();
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
                juego.Ui.WriteLine($"Misión: {mision.Nombre}");
                juego.Ui.WriteLine($"Solicitante: Herrero");
                juego.Ui.WriteLine($"Item solicitado: {string.Join(", ", mision.Requisitos)}");
                juego.Ui.WriteLine($"Ubicación del NPC: {mision.UbicacionNPC}");
                juego.Ui.WriteLine($"Estado: {mision.Estado}");
                juego.Ui.WriteLine("---");
            }
            InputService.Pausa("Presiona cualquier tecla para volver al menú...");
        }
    }
}
