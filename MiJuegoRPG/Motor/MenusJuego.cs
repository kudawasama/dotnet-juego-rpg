using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios; // UIStyle

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
            // Recuperación pasiva de energía antes de mostrar menú
            if (juego.jugador != null)
            {
                juego.energiaService.RecuperacionPasiva(juego.jugador);
            }

            //Console.Clear();
            juego.Ui.WriteLine(juego.FormatoRelojMundo);
            juego.Ui.WriteLine("--- Menú de Viaje ---");
            if (juego.estadoMundo?.Ubicaciones == null || juego.estadoMundo.Ubicaciones.Count == 0)
            {
                juego.Ui.WriteLine("No hay ubicaciones disponibles.");
                InputService.Pausa("Presiona cualquier tecla para volver...");
                return;
            }
            int i = 1;
            foreach (var ubicacion in juego.estadoMundo.Ubicaciones)
            {
                if (ubicacion.Desbloqueada)
                    juego.Ui.WriteLine($"{i}. {ubicacion.Nombre} - {ubicacion.Descripcion}");
                i++;
            }
            juego.Ui.WriteLine("0. Volver");
            var opcion = InputService.LeerOpcion("Elige tu destino: ");
            if (int.TryParse(opcion, out int seleccion) && seleccion > 0 && seleccion <= juego.estadoMundo.Ubicaciones.Count)
            {
                var destinoUbic = juego.estadoMundo.Ubicaciones[seleccion - 1];
                if (destinoUbic.Desbloqueada)
                {
                    // Buscar el SectorData correspondiente al destino
                    var destinoSector = juego.mapa.ObtenerSectores().Find(s => s.Id == destinoUbic.Id);
                    if (destinoSector != null)
                    {
                        juego.mapa.UbicacionActual = destinoSector;
                        juego.Ui.WriteLine($"Viajaste a {destinoSector.Nombre}.");
                        juego.Ui.WriteLine(destinoSector.Descripcion);
                    }
                    else
                    {
                        juego.Ui.WriteLine("No se encontró el sector correspondiente a la ubicación seleccionada.");
                    }
                }
                else
                {
                    juego.Ui.WriteLine("No tienes acceso a esa ubicación.");
                }
            }
            else if (seleccion == 0)
            {
                // Volver al menú anterior
                return;
            }
            else
            {
                juego.Ui.WriteLine("Opción no válida.");
            }
            InputService.Pausa("Presiona cualquier tecla para continuar...");
        }

        public void MostrarMenuGuardado()
        {
            //Console.Clear();
            juego.Ui.WriteLine(juego.FormatoRelojMundo);
            juego.Ui.WriteLine("=== Menú de Guardar/Cargar ===");
            juego.Ui.WriteLine("1. Guardar partida");
            juego.Ui.WriteLine("2. Cargar partida");
            juego.Ui.WriteLine("3. Volver al menú principal");
            var opcion = InputService.LeerOpcion();
            switch (opcion)
            {
                case "1":
                    juego.GuardarPersonaje();
                    break;
                case "2":
                    juego.CargarPersonaje();
                    break;
                case "3":
                    // Volver al menú de la ciudad
                    break;
                default:
                    juego.Ui.WriteLine("Opción no válida.");
                    break;
            }
            InputService.Pausa("\nPresiona cualquier tecla para continuar...");
        }

        public void MostrarMenuMisionesNPC()
        {
            //Console.Clear();
            juego.Ui.WriteLine(juego.FormatoRelojMundo);
            juego.Ui.WriteLine("--- Menú de Misiones y NPC ---");
            juego.Ui.WriteLine("1. Ver misiones activas");
            juego.Ui.WriteLine("2. Ver NPCs de la ciudad actual");
            juego.Ui.WriteLine("3. Volver");
            var opcion = InputService.LeerOpcion("Seleccione una opción: ");
            switch (opcion)
            {
                case "1":
                    MostrarMisionesActivas();
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
                case "2":
                    MostrarNPCsCiudadActual();
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
                case "3":
                    // Volver al menú anterior
                    break;
                default:
                    juego.Ui.WriteLine("Opción inválida");
                    InputService.Pausa();
                    MostrarMenuMisionesNPC();
                    break;
            }
        }

        private void MostrarMisionesActivas()
        {
            juego.Ui.WriteLine("--- Misiones Activas ---");
            if (juego.jugador == null || juego.jugador.MisionesActivas == null || juego.jugador.MisionesActivas.Count == 0)
            {
                juego.Ui.WriteLine("No tienes misiones activas.");
                return;
            }
            foreach (var m in juego.jugador.MisionesActivas)
            {
                juego.Ui.WriteLine($"- {m.Nombre}: {m.Descripcion}");
                var ubiId = CanonicalIdUbicacion(m.UbicacionNPC ?? string.Empty);
                juego.Ui.Write($"  NPC: {m.UbicacionNPC} ");
                EscribirEtiquetaRepCortaColor(ubiId);
                juego.Ui.WriteLine();
                juego.Ui.WriteLine($"  Requisitos: {string.Join(", ", m.Requisitos)}");
                juego.Ui.WriteLine($"  Recompensas: {string.Join(", ", m.Recompensas)}");
                juego.Ui.WriteLine($"  Exp. Nivel: {m.ExpNivel}");
                if (m.ExpAtributos != null)
                    juego.Ui.WriteLine($"  Exp. Atributos: {string.Join(", ", m.ExpAtributos.Select(a => a.Key + ": " + a.Value))}");
                juego.Ui.WriteLine($"  Estado: {m.Estado}");
                if (m.Condiciones != null && m.Condiciones.Count > 0)
                    juego.Ui.WriteLine($"  Condiciones: {string.Join(", ", m.Condiciones)}");
                if (!string.IsNullOrEmpty(m.SiguienteMisionId))
                    juego.Ui.WriteLine($"  Siguiente misión: {m.SiguienteMisionId}");
                juego.Ui.WriteLine();
            }
        }

        private void MostrarNPCsCiudadActual()
        {
            juego.Ui.WriteLine("--- NPCs de la ciudad actual ---");
            var ciudadActual = juego.mapa.UbicacionActual?.Id ?? juego.mapa.UbicacionActual?.Nombre;
            var ciudadActualId = CanonicalIdUbicacion(ciudadActual ?? string.Empty);
            // Intentar primera ruta dentro de MiJuegoRPG/DatosJuego y luego fallback a raíz/DatosJuego
            string rutaNPCs = MiJuegoRPG.Motor.Servicios.PathProvider.NpcsPath("NPC.json");
            string rutaMisiones = MiJuegoRPG.Motor.Servicios.PathProvider.MisionesPath("Misiones.json");
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
        var npcsCiudad = npcs?.FindAll(n => CanonicalIdUbicacion(n.Ubicacion) == ciudadActualId);
                if (npcsCiudad != null && npcsCiudad.Count > 0)
                {
                    for (int i = 0; i < npcsCiudad.Count; i++)
                    {
                        var npc = npcsCiudad[i];
                        juego.Ui.Write($"{i + 1}. {npc.Nombre} ");
            EscribirEtiquetaRepCortaColor(ciudadActualId);
                        juego.Ui.WriteLine();
                    }
                    if (int.TryParse(InputService.LeerOpcion("Selecciona un NPC para ver sus misiones: "), out int seleccion) && seleccion > 0 && seleccion <= npcsCiudad.Count)
                    {
                        var npcSeleccionado = npcsCiudad[seleccion - 1];
                        // Gating reputación negativa para interacción con NPC
                        if (juego.jugador != null && !PuedeInteractuarConNPC(ciudadActualId, out var motivoNPC))
                        {
                            juego.Ui.WriteLine(motivoNPC);
                            return;
                        }
                        juego.Ui.Write($"--- Misiones disponibles con {npcSeleccionado.Nombre} ");
                        EscribirEtiquetaRepCortaColor(ciudadActualId);
                        juego.Ui.WriteLine(" ---");
                        // Mostrar solo misiones accesibles y permitir aceptar una
                        var misionesAccesibles = npcSeleccionado.Misiones
                            .Select(id => misiones.Find(m => m.Id == id))
                            .Where(m => m != null && juego.jugador != null && juego.jugador.PuedeAccederMision(m))
                            .ToList();
                        // Buscar si el jugador tiene una misión activa asociada a este NPC para entregar
                        var misionActivaEntregable = juego.jugador?.MisionesActivas.FirstOrDefault(ma => npcSeleccionado.Misiones.Contains(ma.Id));
                        if (misionActivaEntregable != null)
                        {
                            juego.Ui.WriteLine($"\nTienes una misión activa para entregar: {misionActivaEntregable.Nombre}");
                            var entregar = InputService.LeerOpcion("¿Deseas entregar la misión? (s/n): ");
                            if (entregar != null && entregar.Trim().ToLower() == "s")
                            {
                                if (juego.jugador != null)
                                {
                                    // Gating también al entregar
                                    if (!PuedeInteractuarConNPC(ciudadActualId, out var motivoEnt))
                                    {
                                        juego.Ui.WriteLine(motivoEnt);
                                    }
                                    else
                                    {
                                        juego.jugador.CompletarMision(misionActivaEntregable.Id);
                                    }
                                }
                                else
                                {
                                    juego.Ui.WriteLine("No hay personaje cargado.");
                                }
                                return;
                            }
                        }
                        if (misionesAccesibles.Count == 0)
                        {
                            juego.Ui.WriteLine("No hay misiones disponibles para aceptar con este NPC.");
                        }
                        else
                        {
                            for (int j = 0; j < misionesAccesibles.Count; j++)
                            {
                                var mision = misionesAccesibles[j];
                                if (mision != null)
                                {
                                    juego.Ui.WriteLine($"\n{j + 1}. {mision.Nombre}");
                                    juego.Ui.WriteLine($"   Descripción: {mision.Descripcion}");
                                    juego.Ui.WriteLine($"   Estado: {mision.Estado}");
                                    if (mision.Requisitos != null && mision.Requisitos.Count > 0)
                                        juego.Ui.WriteLine($"   Requisitos: {string.Join(", ", mision.Requisitos)}");
                                    if (mision.Recompensas != null && mision.Recompensas.Count > 0)
                                        juego.Ui.WriteLine($"   Recompensas: {string.Join(", ", mision.Recompensas)}");
                                    if (mision.ExpNivel > 0)
                                        juego.Ui.WriteLine($"   Exp. Nivel: {mision.ExpNivel}");
                                    if (mision.ExpAtributos != null && mision.ExpAtributos.Count > 0)
                                        juego.Ui.WriteLine($"   Exp. Atributos: {string.Join(", ", mision.ExpAtributos.Select(a => a.Key + ": " + a.Value))}");
                                }
                            }
                            var input = InputService.LeerOpcion("\n¿Deseas aceptar alguna misión? Ingresa el número o presiona Enter para volver: ");
                            if (int.TryParse(input, out int idx) && idx > 0 && idx <= misionesAccesibles.Count)
                            {
                                var misionSeleccionada = misionesAccesibles[idx - 1]!;
                                // Agregar la misión a las activas del jugador
                                if (juego.jugador != null)
                                {
                                    // Gating también al aceptar misión
                                    if (!PuedeInteractuarConNPC(ciudadActualId, out var motivoAcep))
                                    {
                                        juego.Ui.WriteLine(motivoAcep);
                                        return;
                                    }
                                    juego.jugador.MisionesActivas.Add(new MiJuegoRPG.Personaje.Personaje.MisionConId
                                    {
                                        Id = misionSeleccionada.Id,
                                        Nombre = misionSeleccionada.Nombre,
                                        Descripcion = misionSeleccionada.Descripcion,
                                        UbicacionNPC = misionSeleccionada.UbicacionNPC,
                                        Requisitos = misionSeleccionada.Requisitos,
                                        Recompensas = misionSeleccionada.Recompensas,
                                        ExpNivel = (int)misionSeleccionada.ExpNivel,
                                        ExpAtributos = misionSeleccionada.ExpAtributos?.ToDictionary(e => e.Key, e => (int)e.Value) ?? new Dictionary<string, int>(),
                                        Estado = "Activa",
                                        SiguienteMisionId = misionSeleccionada.SiguienteMisionId ?? string.Empty,
                                        Condiciones = misionSeleccionada.Condiciones ?? new List<string>()
                                    });
                                    juego.Ui.WriteLine($"\n¡Has aceptado la misión: {misionSeleccionada.Nombre}!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    juego.Ui.WriteLine("No hay NPCs en esta ciudad.");
                }
            }
            else
            {
                juego.Ui.WriteLine("Archivo de NPCs no encontrado.");
            }
        }

        public void MostrarMenuPersonaje()
        {
            while (true)
            {
                juego.Ui.WriteLine("\n=== Menú de Personaje ===");
                juego.Ui.WriteLine("1. Ver inventario");
                juego.Ui.WriteLine("2. Ver equipo equipado");
                juego.Ui.WriteLine("3. Ver información y atributos");
                juego.Ui.WriteLine("4. Volver");
                var opcion = InputService.LeerOpcion("Seleccione una opción: ");
                switch (opcion)
                {
                    case "1":
                        MostrarInventario();
                        break;
                    case "2":
                        MostrarEquipo();
                        break;
                    case "3":
                        MostrarInformacionPersonaje();
                        break;
                    case "4":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        public void MostrarInventario()
        {
            if (juego.jugador == null)
            {
                juego.Ui.WriteLine("No hay personaje cargado.");
                return;
            }
            var inventario = juego.jugador.Inventario;
            UIStyle.Header(juego.Ui, "Inventario");
            inventario.MostrarInventario();
            while (true)
            {
                UIStyle.SubHeader(juego.Ui, "Opciones de Inventario");
                juego.Ui.WriteLine("1. Usar objeto");
                juego.Ui.WriteLine("2. Equipar objeto");
                juego.Ui.WriteLine("3. Desequipar objeto");
                juego.Ui.WriteLine("4. Volver");
                var opcion = InputService.LeerOpcion("Selecciona una opción: ");
                switch (opcion)
                {
                    case "1":
                        // Usar objeto por índice
                        var inputIdx = InputService.LeerOpcion("Ingresa el número del objeto a usar: ");
                        if (int.TryParse(inputIdx, out int seleccion) && seleccion > 0 && seleccion <= inventario.NuevosObjetos.Count)
                        {
                            var objCant = inventario.NuevosObjetos[seleccion - 1];
                            // Usar según tipo
                            objCant.Objeto.Usar(juego.jugador);
                            // Consumir si corresponde (p. ej. pociones)
                            if (objCant.Objeto is MiJuegoRPG.Objetos.Pocion)
                            {
                                objCant.Cantidad--;
                                if (objCant.Cantidad <= 0)
                                    inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                            }
                        }
                        else
                        {
                            juego.Ui.WriteLine("Selección inválida.");
                        }
                        break;
                    case "2":
                        // Equipar
                        var inputEq = InputService.LeerOpcion("Ingresa el número del objeto a equipar: ");
                        if (int.TryParse(inputEq, out int selEq) && selEq > 0 && selEq <= inventario.NuevosObjetos.Count)
                        {
                            var objCant = inventario.NuevosObjetos[selEq - 1];
                            inventario.EquiparObjeto(objCant.Objeto, juego.jugador);
                        }
                        else
                        {
                            juego.Ui.WriteLine("Selección inválida.");
                        }
                        break;
                    case "3":
                        // Desequipar simple
                        juego.Ui.WriteLine("¿Qué deseas desequipar? 1.Arma 2.Casco 3.Armadura 4.Pantalón 5.Botas 6.Collar 7.Cinturón 8.Accesorio1 9.Accesorio2 0.Cancelar");
                        var deq = InputService.LeerOpcion("> ");
                        var eq = inventario.Equipo;
                        switch (deq)
                        {
                            case "1": eq.Arma = null; break;
                            case "2": eq.Casco = null; break;
                            case "3": eq.Armadura = null; break;
                            case "4": eq.Pantalon = null; break;
                            case "5": eq.Zapatos = null; break;
                            case "6": eq.Collar = null; break;
                            case "7": eq.Cinturon = null; break;
                            case "8": eq.Accesorio1 = null; break;
                            case "9": eq.Accesorio2 = null; break;
                            case "0": break;
                            default: juego.Ui.WriteLine("Opción no válida."); break;
                        }
                        break;
                    case "4":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        // Etiqueta compacta de reputación para la ubicación/facción actual
        private string EtiquetaRepCorta(string ubicacion)
        {
            var info = BandaInfo(ubicacion ?? string.Empty);
            var signo = info.valor >= 0 ? "+" : string.Empty;
            return $"[{info.nombre} {signo}{info.valor}]";
        }

        // Escribe la etiqueta con color según banda
        private void EscribirEtiquetaRepCortaColor(string ubicacion)
        {
            var info = BandaInfo(ubicacion ?? string.Empty);
            var signo = info.valor >= 0 ? "+" : string.Empty;
            var etiqueta = $"[{info.nombre} {signo}{info.valor}]";
            juego.Ui.SetColor(foreground: info.color);
            juego.Ui.Write(etiqueta);
            juego.Ui.ResetColor();
        }

        // Configuración de bandas desde JSON (DatosJuego/config/reputacion_bandas.json) con fallback por defecto
        private class BandaRepConfig
        {
            public string Nombre { get; set; } = string.Empty;
            public int Min { get; set; }
            public int Max { get; set; }
            public string Color { get; set; } = "Gray";
        }

        private static List<BandaRepConfig>? _bandasConfig;
        private static bool _bandasIntentadoCargar = false;

        private static void CargarBandasConfig()
        {
            if (_bandasIntentadoCargar) return;
            _bandasIntentadoCargar = true;
            try
            {
                // Intentar cargar desde MiJuegoRPG/DatosJuego/config, luego fallback a DatosJuego/config
                string ruta = MiJuegoRPG.Motor.Servicios.PathProvider.ConfigPath("reputacion_bandas.json");
                if (File.Exists(ruta))
                {
                    var json = File.ReadAllText(ruta);
                    var bandas = JsonSerializer.Deserialize<List<BandaRepConfig>>(json);
                    if (bandas != null && bandas.Count > 0)
                    {
                        _bandasConfig = bandas;
                    }
                }
            }
            catch
            {
                // Ignorar y usar fallback
            }
        }

        private static ConsoleColor ParseColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color)) return ConsoleColor.Gray;
            return Enum.TryParse<ConsoleColor>(color, true, out var c) ? c : ConsoleColor.Gray;
        }

        private (string nombre, int valor, ConsoleColor color) BandaInfo(string ubicacion)
        {
            if (juego.jugador == null) return (string.Empty, 0, ConsoleColor.Gray);
            var pj = juego.jugador;
            string fac = MiJuegoRPG.Comercio.ShopService.FaccionPorUbicacion(ubicacion ?? string.Empty);
            int repFac = 0; if (!string.IsNullOrWhiteSpace(fac)) pj.ReputacionesFaccion.TryGetValue(fac, out repFac);
            int repG = pj.Reputacion;
            int refRep = string.IsNullOrWhiteSpace(fac) ? repG : repFac;

            // Intentar con configuración
            CargarBandasConfig();
            if (_bandasConfig != null && _bandasConfig.Count > 0)
            {
                foreach (var b in _bandasConfig)
                {
                    if (refRep >= b.Min && refRep <= b.Max)
                    {
                        return (b.Nombre, refRep, ParseColor(b.Color));
                    }
                }
            }

            // Fallback por defecto
            string nombre = refRep <= -150 ? "Perseguido"
                           : refRep <= -50  ? "Hostil"
                           : refRep < 0     ? "Tenso"
                           : refRep < 50    ? "Neutral"
                           : refRep < 100   ? "Amistoso"
                           : "Aliado";
            ConsoleColor col = nombre switch
            {
                "Perseguido" => ConsoleColor.DarkRed,
                "Hostil" => ConsoleColor.Red,
                "Tenso" => ConsoleColor.DarkYellow,
                "Neutral" => ConsoleColor.Gray,
                "Amistoso" => ConsoleColor.Green,
                "Aliado" => ConsoleColor.Cyan,
                _ => ConsoleColor.Gray
            };
            return (nombre, refRep, col);
        }

        // Devuelve banda por valor directo (para evaluar global)
        private (string nombre, ConsoleColor color) BandaPorValor(int valor)
        {
            return MiJuegoRPG.Motor.Servicios.ReputacionPoliticas.BandaPorValor(valor);
        }

        // Chequeo de gating para NPCs basado en reputación por facción/ubicación
        private bool PuedeInteractuarConNPC(string ubicacion, out string motivo)
        {
            motivo = string.Empty;
            if (juego.jugador == null) { motivo = "No hay personaje."; return false; }
            var pj = juego.jugador;
            string fac = MiJuegoRPG.Comercio.ShopService.FaccionPorUbicacion(ubicacion ?? string.Empty);
            int repFac = 0; if (!string.IsNullOrWhiteSpace(fac)) pj.ReputacionesFaccion.TryGetValue(fac, out repFac);
            int repGlobal = pj.Reputacion;
            if (MiJuegoRPG.Motor.Servicios.ReputacionPoliticas.DebeBloquearNPC(repFac, repGlobal))
            {
                motivo = string.IsNullOrWhiteSpace(fac)
                    ? "Tu reputación global es nefasta. El NPC te ignora."
                    : $"La {fac} te considera indeseable. Este NPC no quiere hablar contigo.";
                return false;
            }
            return true;
        }

        public void GuardarPartida()
        {
            juego.Ui.WriteLine("[Stub] GuardarPartida: Implementar lógica de guardado aquí.");
        }

        public void CargarPartida()
        {
            juego.Ui.WriteLine("[Stub] CargarPartida: Implementar lógica de carga aquí.");
        }

        private void MostrarEquipo()   // Método para mostrar el equipo del jugador
        {
            if (juego.jugador == null)
            {
                juego.Ui.WriteLine("No hay personaje cargado.");
                return;
            }
            juego.Ui.WriteLine("--- Equipo Equipado ---");
            var eq = juego.jugador.Inventario.Equipo;
            if (eq.Arma != null)
            {
                juego.Ui.WriteLine($"Arma: {eq.Arma.Nombre}");
                if (eq.Arma is MiJuegoRPG.Objetos.Arma arma)
                {
                    juego.Ui.WriteLine($"  DañoFisico: {arma.DañoFisico}");
                    juego.Ui.WriteLine($"  DañoMagico: {arma.DañoMagico}");
                    juego.Ui.WriteLine($"  Rareza: {arma.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {arma.Perfeccion}");
                }
            }
            else
            {
                juego.Ui.WriteLine("Arma: Sin equipar");
            }
            // Casco
            if (eq.Casco != null)
            {
                juego.Ui.WriteLine($"Casco: {eq.Casco.Nombre}");
                if (eq.Casco is MiJuegoRPG.Objetos.Casco casco)
                {
                    juego.Ui.WriteLine($"  Rareza: {casco.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {casco.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Casco: Sin equipar");

            // Armadura
            if (eq.Armadura != null)
            {
                juego.Ui.WriteLine($"Armadura: {eq.Armadura.Nombre}");
                if (eq.Armadura is MiJuegoRPG.Objetos.Armadura armadura)
                {
                    juego.Ui.WriteLine($"  Rareza: {armadura.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {armadura.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Armadura: Sin equipar");

            // Pantalón
            if (eq.Pantalon != null)
            {
                juego.Ui.WriteLine($"Pantalón: {eq.Pantalon.Nombre}");
                if (eq.Pantalon is MiJuegoRPG.Objetos.Pantalon pantalon)
                {
                    juego.Ui.WriteLine($"  Rareza: {pantalon.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {pantalon.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Pantalón: Sin equipar");

            // Zapatos
            if (eq.Zapatos != null)
            {
                juego.Ui.WriteLine($"Zapatos: {eq.Zapatos.Nombre}");
                if (eq.Zapatos is MiJuegoRPG.Objetos.Botas botas)
                {
                    juego.Ui.WriteLine($"  Rareza: {botas.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {botas.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Zapatos: Sin equipar");

            // Collar
            if (eq.Collar != null)
            {
                juego.Ui.WriteLine($"Collar: {eq.Collar.Nombre}");
                if (eq.Collar is MiJuegoRPG.Objetos.Collar collar)
                {
                    juego.Ui.WriteLine($"  Rareza: {collar.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {collar.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Collar: Sin equipar");

            // Cinturón
            if (eq.Cinturon != null)
            {
                juego.Ui.WriteLine($"Cinturón: {eq.Cinturon.Nombre}");
                if (eq.Cinturon is MiJuegoRPG.Objetos.Cinturon cinturon)
                {
                    juego.Ui.WriteLine($"  Rareza: {cinturon.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {cinturon.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Cinturón: Sin equipar");

            // Accesorio 1
            if (eq.Accesorio1 != null)
            {
                juego.Ui.WriteLine($"Accesorio 1: {eq.Accesorio1.Nombre}");
                if (eq.Accesorio1 is MiJuegoRPG.Objetos.Accesorio acc1)
                {
                    juego.Ui.WriteLine($"  Rareza: {acc1.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {acc1.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Accesorio 1: Sin equipar");

            // Accesorio 2
            if (eq.Accesorio2 != null)
            {
                juego.Ui.WriteLine($"Accesorio 2: {eq.Accesorio2.Nombre}");
                if (eq.Accesorio2 is MiJuegoRPG.Objetos.Accesorio acc2)
                {
                    juego.Ui.WriteLine($"  Rareza: {acc2.Rareza}");
                    juego.Ui.WriteLine($"  Perfección: {acc2.Perfeccion}");
                }
            }
            else juego.Ui.WriteLine("Accesorio 2: Sin equipar");
        }

        private void MostrarInformacionPersonaje()
        {
            if (juego.jugador == null)
            {
                juego.Ui.WriteLine("No hay personaje cargado.");
                return;
            }
            var pj = juego.jugador;
            juego.Ui.WriteLine($"--- Información de {pj.Nombre} ---");
            juego.Ui.WriteLine($"Nivel: {pj.Nivel}  Exp: {pj.Experiencia}/{pj.ExperienciaSiguienteNivel}");
            juego.Ui.WriteLine($"Vida: {pj.Vida}/{pj.VidaMaxima}  Oro: {pj.Oro}");
            juego.Ui.WriteLine($"Clase: {pj.Clase?.Nombre ?? "Sin clase"}");
            juego.Ui.WriteLine($"Título: {pj.Titulo}");
            juego.Ui.WriteLine("--- Atributos ---");
            foreach (var atributo in pj.AtributosBase.GetType().GetProperties())
            {
                var nombre = atributo.Name;
                var valorBase = Convert.ToDouble(atributo.GetValue(pj.AtributosBase));
                var bonif = pj.ObtenerBonificadorAtributo(nombre);
                var total = valorBase + bonif;
                if (bonif != 0)
                    juego.Ui.WriteLine($"{nombre}: {total} ({valorBase} base + {bonif} equipo)");
                else
                    juego.Ui.WriteLine($"{nombre}: {total} ({valorBase} base)");
            }
            juego.Ui.WriteLine("--- Estadísticas ---");
            if (pj.Estadisticas != null)
            {
                foreach (var stat in pj.Estadisticas.GetType().GetProperties())
                {
                    var nombre = stat.Name;
                    var valorBase = Convert.ToDouble(stat.GetValue(pj.Estadisticas));
                    var bonif = pj.ObtenerBonificadorEstadistica(nombre);
                    var total = valorBase + bonif;
                    if (bonif != 0)
                        juego.Ui.WriteLine($"{nombre}: {total} ({valorBase} base + {bonif} equipo)");
                    else
                        juego.Ui.WriteLine($"{nombre}: {total} ({valorBase} base)");
                }
            }
        }

        public void MostrarMenuPrincipalFijo()
        {
            while (true)
            {
                UIStyle.Header(juego.Ui, "Menú Principal");
                juego.Ui.WriteLine("1. Ver personaje");
                juego.Ui.WriteLine("2. Inventario");
                juego.Ui.WriteLine("3. Equipo equipado");
                juego.Ui.WriteLine("4. Guardar partida");
                juego.Ui.WriteLine("5. Opciones");
                juego.Ui.WriteLine("6. Salir del juego");
                juego.Ui.WriteLine("7. Volver");
                var opcion = InputService.LeerOpcion("Seleccione una opción: ");
                switch (opcion)
                {
                    case "1":
                        MostrarInformacionPersonaje();
                        break;
                    case "2":
                        MostrarInventario();
                        break;
                    case "3":
                        MostrarEquipo();
                        break;
                    case "4":
                        juego.GuardarPersonaje();
                        juego.Ui.WriteLine("Partida guardada.");
                        break;
                    case "5":
                        var mo = new MiJuegoRPG.Motor.Menus.MenuOpciones(juego);
                        mo.Mostrar();
                        break;
                    case "6":
                        Environment.Exit(0);
                        break;
                    case "7":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        // Ejemplo de integración en menús contextuales:
        private void MostrarMenuCiudad(string ciudad)
        {
            while (true)
            {
        var ciudadId = CanonicalIdUbicacion(ciudad);
        juego.Ui.Write($"\n=== Menú de Ciudad: {ciudad} ");
        EscribirEtiquetaRepCortaColor(ciudadId);
                juego.Ui.WriteLine(" ===");
                juego.Ui.WriteLine("1. Explorar sector (NPC y misiones)");
                juego.Ui.WriteLine("2. Ver misiones activas");
                juego.Ui.WriteLine("3. Ir a tienda");
                juego.Ui.WriteLine("4. Menú fijo");
                juego.Ui.WriteLine("5. Salir de la ciudad");
                var opcion = InputService.LeerOpcion("Seleccione una opción: ");
                switch (opcion)
                {
                    case "1":
                        MostrarMenuMisionesNPC();
                        break;
                    case "2":
                        MostrarMisionesActivas();
                        break;
                    case "3":
            MostrarMenuTienda(ciudadId);
                        break;
                    case "4":
                        MostrarMenuPrincipalFijo();
                        break;
                    case "5":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        /// <summary>
        /// Muestra el menú de combate principal, recibiendo el enemigo actual como parámetro.
        /// </summary>
        /// <param name="enemigoActual">Instancia del enemigo actual en combate.</param>
        public void MostrarMenuCombate(Enemigo enemigoActual)
        {
            while (true)
            {
                juego.Ui.WriteLine("\n=== MENÚ DE COMBATE ===");
                juego.Ui.WriteLine("1. Atacar");
                juego.Ui.WriteLine("2. Hechizo/Habilidades");
                juego.Ui.WriteLine("3. Defenderse");
                juego.Ui.WriteLine("4. Observar");
                juego.Ui.WriteLine("5. Usar objeto especial");
                juego.Ui.WriteLine("6. Cambiar de posición");
                juego.Ui.WriteLine("7. Huir");
                juego.Ui.WriteLine("8. Acciones");
                juego.Ui.WriteLine("9. Menú principal (fijo)");
                var opcion = InputService.LeerOpcion("Elige una acción: ");
                switch (opcion)
                {
                    case "1":
                        // Lógica de ataque
                        break;
                    case "2":
                        // Hechizo/Habilidades
                        if (juego.jugador == null || juego.jugador.Habilidades == null || juego.jugador.Habilidades.Count == 0)
                        {
                            juego.Ui.WriteLine("No tienes habilidades disponibles.");
                            break;
                        }
                        juego.Ui.WriteLine("Elige una habilidad:");
                        int idx = 1;
                        var listaHab = new List<string>(juego.jugador.Habilidades.Keys);
                        foreach (var habId in listaHab)
                        {
                            var hab = juego.jugador.Habilidades[habId];
                            juego.Ui.WriteLine($"{idx}. {hab.Nombre} (Nv {hab.Nivel}, Exp {hab.Exp})");
                            idx++;
                        }
                        var input = InputService.LeerOpcion("Selecciona el número de la habilidad: ");
                        if (int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= listaHab.Count)
                        {
                            var habId = listaHab[seleccion - 1];
                            juego.jugador.UsarHabilidad(habId);
                        }
                        else
                        {
                            juego.Ui.WriteLine("Selección inválida.");
                        }
                        break;
                    case "3":
                        juego.Ui.WriteLine("Te pones en guardia y aumentas tu defensa temporalmente.");
                        // Aquí se integrará la lógica real de defenderse
                        break;
                    case "4":
                        juego.Ui.WriteLine("Observas cuidadosamente al enemigo...");
                        // Aquí se integrará la lógica real de observar
                        break;
                    case "5":
                        juego.Ui.WriteLine("Usas un objeto especial...");
                        // Aquí se integrará la lógica real de usar objeto especial
                        break;
                    case "6":
                        juego.Ui.WriteLine("Cambias de posición en el campo de batalla.");
                        // Aquí se integrará la lógica real de cambiar de posición
                        break;
                    case "7":
                        juego.Ui.WriteLine("Intentas huir del combate...");
                        // Aquí se integrará la lógica real de huida
                        return;
                    case "8":
                        if (juego.jugador is MiJuegoRPG.Personaje.Personaje pjConcreto)
                            MiJuegoRPG.Motor.Menus.MenuCombateAvanzado.Mostrar(pjConcreto, enemigoActual, string.Empty);

                        else
                            juego.Ui.WriteLine("Error: El jugador no es del tipo esperado para el menú avanzado.");
                        break;
                    case "9":
                        MostrarMenuPrincipalFijo();
                        break;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        // Servicio de tienda integrado
        private readonly MiJuegoRPG.Comercio.ShopService _shop = new(new MiJuegoRPG.Comercio.PriceService());

        public void MostrarMenuTienda(string ubicacionClave)
        {
            // Ahora se pasa el ID del sector; ShopService es tolerante a ID/Nombre
            var vendor = _shop.GetVendorPorUbicacion(CanonicalIdUbicacion(ubicacionClave));
            if (vendor == null) { juego.Ui.WriteLine("No hay mercader en esta zona."); return; }
            if (juego.jugador != null && !_shop.PuedeAtender(juego.jugador, vendor, out var motivo))
            {
                juego.Ui.WriteLine(motivo);
                return;
            }

            while (true)
            {
                juego.Ui.Write($"\n=== Tienda: {vendor.Nombre} ({vendor.Ubicacion}) ");
                EscribirEtiquetaRepCortaColor(vendor.Ubicacion ?? CanonicalIdUbicacion(ubicacionClave));
                juego.Ui.WriteLine($" === Oro: {juego.jugador?.Oro}");
                for (int i = 0; i < vendor.Stock.Count; i++)
                {
                    var si = vendor.Stock[i];
                    var pBase = new MiJuegoRPG.Comercio.PriceService().PrecioDe(si.Item);
                    var p = _shop.GetPrecioCompra(juego.jugador!, vendor, si.Item);
                    juego.Ui.WriteLine($"{i + 1}. {si.Item.Nombre} x{si.Cantidad}  [{p} oro] (base {pBase})");
                }
                juego.Ui.WriteLine("C. Comprar | V. Vender | S. Salir");
                var op = InputService.LeerOpcion("> ")?.Trim().ToUpperInvariant();
                if (op == "S") break;
                if (juego.jugador == null) { juego.Ui.WriteLine("No hay personaje."); continue; }

                if (op == "C")
                {
                    int.TryParse(InputService.LeerOpcion("Índice a comprar: "), out var idx);
                    int.TryParse(InputService.LeerOpcion("Cantidad: "), out var cant);
                    if (_shop.Comprar(juego.jugador, vendor, idx - 1, cant, out var msg)) juego.Ui.WriteLine(msg);
                    else juego.Ui.WriteLine($"No se pudo comprar: {msg}");
                }
                else if (op == "V")
                {
                    var inv = juego.jugador.Inventario.NuevosObjetos;
                    for (int i = 0; i < inv.Count; i++)
                    {
                        var si = inv[i];
                        var prBase = new MiJuegoRPG.Comercio.PriceService().PrecioReventa(si.Objeto);
                        var pr = _shop.GetPrecioVenta(juego.jugador!, vendor, si.Objeto);
                        juego.Ui.WriteLine($"{i + 1}. {si.Objeto.Nombre} x{si.Cantidad}  [vende: {pr}] (base {prBase})");
                    }
                    int.TryParse(InputService.LeerOpcion("Índice a vender: "), out var idx);
                    int.TryParse(InputService.LeerOpcion("Cantidad: "), out var cant);
                    if (_shop.Vender(juego.jugador, vendor, idx - 1, cant, out var msg)) juego.Ui.WriteLine(msg);
                    else juego.Ui.WriteLine($"No se pudo vender: {msg}");
                }
            }
        }

    // Descuento movido a ShopService para unificar la lógica de precios
        private void MostrarMenuMisiones()
        {
            while (true)
            {
                juego.Ui.WriteLine("\n=== Misiones Activas ===");
                juego.Ui.WriteLine("1. Ver detalles de misión");
                juego.Ui.WriteLine("2. Entregar misión");
                juego.Ui.WriteLine("3. Menú principal (fijo)");
                juego.Ui.WriteLine("4. Volver");
                var opcion = InputService.LeerOpcion("Seleccione una opción: ");
                switch (opcion)
                {
                    case "1":
                        // Lógica de detalles
                        break;
                    case "2":
                        // Lógica de entrega
                        break;
                    case "3":
                        MostrarMenuPrincipalFijo();
                        break;
                    case "4":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        private void MostrarMenuNPC(string nombreNPC)
        {
            while (true)
            {
                // Gating por reputación para diálogos generales del NPC (ubicación actual)
                var ubic = CanonicalIdUbicacion(juego.mapa.UbicacionActual?.Id ?? juego.mapa.UbicacionActual?.Nombre ?? string.Empty);
                if (juego.jugador != null && !PuedeInteractuarConNPC(ubic, out var motivo))
                {
                    juego.Ui.WriteLine(motivo);
                    return;
                }
                juego.Ui.Write($"\n=== NPC: {nombreNPC} ");
                EscribirEtiquetaRepCortaColor(ubic);
                juego.Ui.WriteLine(" ===");
                juego.Ui.WriteLine("1. Hablar");
                juego.Ui.WriteLine("2. Ver misiones disponibles");
                juego.Ui.WriteLine("3. Menú principal (fijo)");
                juego.Ui.WriteLine("4. Volver");
                var opcion = InputService.LeerOpcion("Seleccione una opción: ");
                switch (opcion)
                {
                    case "1":
                        // Lógica de diálogo
                        break;
                    case "2":
                        // Lógica de misiones
                        break;
                    case "3":
                        MostrarMenuPrincipalFijo();
                        break;
                    case "4":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
        public void MostrarMenuRecoleccion()
        {
            juego.recoleccionService.MostrarMenu();
        }

        // Normaliza una ubicación (Id o Nombre) al Id de sector si existe
        private string CanonicalIdUbicacion(string ubicacion)
        {
            try
            {
                var sectores = juego?.mapa?.ObtenerSectores();
                if (sectores != null)
                {
                    // Buscar por Id exacto primero
                    var s = sectores.FirstOrDefault(s => string.Equals(s.Id, ubicacion, StringComparison.OrdinalIgnoreCase));
                    if (s != null) return s.Id;
                    // Luego por Nombre
                    s = sectores.FirstOrDefault(s => string.Equals(s.Nombre, ubicacion, StringComparison.OrdinalIgnoreCase));
                    if (s != null) return s.Id;
                }
            }
            catch { }
            return ubicacion;
        }
    }
}
