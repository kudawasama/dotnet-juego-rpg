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
            Console.WriteLine("1. Ver misiones activas");
            Console.WriteLine("2. Ver NPCs de la ciudad actual");
            Console.WriteLine("3. Volver");
            Console.Write("Seleccione una opción: ");
            var opcion = Console.ReadLine();
            switch (opcion)
            {
                case "1":
                    MostrarMisionesActivas();
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
                case "2":
                    MostrarNPCsCiudadActual();
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
                case "3":
                    // Volver al menú anterior
                    break;
                default:
                    Console.WriteLine("Opción inválida");
                    Console.ReadKey();
                    MostrarMenuMisionesNPC();
                    break;
            }
        }

        private void MostrarMisionesActivas()
        {
            Console.WriteLine("--- Misiones Activas ---");
            if (juego.jugador == null || juego.jugador.MisionesActivas == null || juego.jugador.MisionesActivas.Count == 0)
            {
                Console.WriteLine("No tienes misiones activas.");
                return;
            }
            foreach (var m in juego.jugador.MisionesActivas)
            {
                Console.WriteLine($"- {m.Nombre}: {m.Descripcion}");
                Console.WriteLine($"  NPC: {m.UbicacionNPC}");
                Console.WriteLine($"  Requisitos: {string.Join(", ", m.Requisitos)}");
                Console.WriteLine($"  Recompensas: {string.Join(", ", m.Recompensas)}");
                Console.WriteLine($"  Exp. Nivel: {m.ExpNivel}");
                if (m.ExpAtributos != null)
                    Console.WriteLine($"  Exp. Atributos: {string.Join(", ", m.ExpAtributos.Select(a => a.Key + ": " + a.Value))}");
                Console.WriteLine($"  Estado: {m.Estado}");
                if (m.Condiciones != null && m.Condiciones.Count > 0)
                    Console.WriteLine($"  Condiciones: {string.Join(", ", m.Condiciones)}");
                if (!string.IsNullOrEmpty(m.SiguienteMisionId))
                    Console.WriteLine($"  Siguiente misión: {m.SiguienteMisionId}");
                Console.WriteLine();
            }
        }

        private void MostrarNPCsCiudadActual()
        {
            Console.WriteLine("--- NPCs de la ciudad actual ---");
            var ciudadActual = juego.ubicacionActual?.Nombre;
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
                var npcsCiudad = npcs?.FindAll(n => n.Ubicacion == ciudadActual);
                if (npcsCiudad != null && npcsCiudad.Count > 0)
                {
                    for (int i = 0; i < npcsCiudad.Count; i++)
                    {
                        var npc = npcsCiudad[i];
                        Console.WriteLine($"{i + 1}. {npc.Nombre}");
                    }
                    Console.Write("Selecciona un NPC para ver sus misiones: ");
                    if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= npcsCiudad.Count)
                    {
                        var npcSeleccionado = npcsCiudad[seleccion - 1];
                        Console.WriteLine($"--- Misiones disponibles con {npcSeleccionado.Nombre} ---");
                        // Mostrar solo misiones accesibles y permitir aceptar una
                        var misionesAccesibles = npcSeleccionado.Misiones
                            .Select(id => misiones.Find(m => m.Id == id))
                            .Where(m => m != null && juego.jugador != null && juego.jugador.PuedeAccederMision(m))
                            .ToList();
                        // Buscar si el jugador tiene una misión activa asociada a este NPC para entregar
                        var misionActivaEntregable = juego.jugador?.MisionesActivas.FirstOrDefault(ma => npcSeleccionado.Misiones.Contains(ma.Id));
                        if (misionActivaEntregable != null)
                        {
                            Console.WriteLine($"\nTienes una misión activa para entregar: {misionActivaEntregable.Nombre}");
                            Console.Write("¿Deseas entregar la misión? (s/n): ");
                            var entregar = Console.ReadLine();
                            if (entregar != null && entregar.Trim().ToLower() == "s")
                            {
                                if (juego.jugador != null)
                                {
                                    juego.jugador.CompletarMision(misionActivaEntregable.Id);
                                }
                                else
                                {
                                    Console.WriteLine("No hay personaje cargado.");
                                }
                                return;
                            }
                        }
                        if (misionesAccesibles.Count == 0)
                        {
                            Console.WriteLine("No hay misiones disponibles para aceptar con este NPC.");
                        }
                        else
                        {
                            for (int j = 0; j < misionesAccesibles.Count; j++)
                            {
                                var mision = misionesAccesibles[j];
                                if (mision != null)
                                {
                                    Console.WriteLine($"\n{j + 1}. {mision.Nombre}");
                                    Console.WriteLine($"   Descripción: {mision.Descripcion}");
                                    Console.WriteLine($"   Estado: {mision.Estado}");
                                    if (mision.Requisitos != null && mision.Requisitos.Count > 0)
                                        Console.WriteLine($"   Requisitos: {string.Join(", ", mision.Requisitos)}");
                                    if (mision.Recompensas != null && mision.Recompensas.Count > 0)
                                        Console.WriteLine($"   Recompensas: {string.Join(", ", mision.Recompensas)}");
                                    if (mision.ExpNivel > 0)
                                        Console.WriteLine($"   Exp. Nivel: {mision.ExpNivel}");
                                    if (mision.ExpAtributos != null && mision.ExpAtributos.Count > 0)
                                        Console.WriteLine($"   Exp. Atributos: {string.Join(", ", mision.ExpAtributos.Select(a => a.Key + ": " + a.Value))}");
                                }
                            }
                            Console.Write("\n¿Deseas aceptar alguna misión? Ingresa el número o presiona Enter para volver: ");
                            var input = Console.ReadLine();
                            if (int.TryParse(input, out int idx) && idx > 0 && idx <= misionesAccesibles.Count)
                            {
                                var misionSeleccionada = misionesAccesibles[idx - 1]!;
                                // Agregar la misión a las activas del jugador
                                if (juego.jugador != null)
                                {
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
                                    Console.WriteLine($"\n¡Has aceptado la misión: {misionSeleccionada.Nombre}!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No hay NPCs en esta ciudad.");
                }
            }
            else
            {
                Console.WriteLine("Archivo de NPCs no encontrado.");
            }
        }

        public void MostrarMenuPersonaje()
        {
            while (true)
            {
                Console.WriteLine("\n=== Menú de Personaje ===");
                Console.WriteLine("1. Ver inventario");
                Console.WriteLine("2. Ver equipo equipado");
                Console.WriteLine("3. Ver información y atributos");
                Console.WriteLine("4. Volver");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
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
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

    public void MostrarInventario()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                return;
            }
            Console.WriteLine("--- Inventario ---");
            foreach (var obj in juego.jugador.Inventario.Objetos)
            {
                Console.WriteLine($"- {obj.Nombre} ({obj.Categoria})");
            }
        }

    public void GuardarPartida()
        {
            Console.WriteLine("[Stub] GuardarPartida: Implementar lógica de guardado aquí.");
        }

    public void CargarPartida()
        {
            Console.WriteLine("[Stub] CargarPartida: Implementar lógica de carga aquí.");
        }

        private void MostrarEquipo()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                return;
            }
            Console.WriteLine("--- Equipo Equipado ---");
            var eq = juego.jugador.Inventario.Equipo;
            if (eq.Arma != null)
            {
                Console.WriteLine($"Arma: {eq.Arma.Nombre}");
                if (eq.Arma is MiJuegoRPG.Objetos.Arma arma)
                {
                    Console.WriteLine($"  Daño: {arma.Daño}");
                    Console.WriteLine($"  Rareza: {arma.Rareza}");
                    Console.WriteLine($"  Perfección: {arma.Perfeccion}");
                }
            }
            else
            {
                Console.WriteLine("Arma: Sin equipar");
            }
            // Casco
            if (eq.Casco != null)
            {
                Console.WriteLine($"Casco: {eq.Casco.Nombre}");
                if (eq.Casco is MiJuegoRPG.Objetos.Casco casco)
                {
                    Console.WriteLine($"  Rareza: {casco.Rareza}");
                    Console.WriteLine($"  Perfección: {casco.Perfeccion}");
                }
            }
            else Console.WriteLine("Casco: Sin equipar");

            // Armadura
            if (eq.Armadura != null)
            {
                Console.WriteLine($"Armadura: {eq.Armadura.Nombre}");
                if (eq.Armadura is MiJuegoRPG.Objetos.Armadura armadura)
                {
                    Console.WriteLine($"  Rareza: {armadura.Rareza}");
                    Console.WriteLine($"  Perfección: {armadura.Perfeccion}");
                }
            }
            else Console.WriteLine("Armadura: Sin equipar");

            // Pantalón
            if (eq.Pantalon != null)
            {
                Console.WriteLine($"Pantalón: {eq.Pantalon.Nombre}");
                if (eq.Pantalon is MiJuegoRPG.Objetos.Pantalon pantalon)
                {
                    Console.WriteLine($"  Rareza: {pantalon.Rareza}");
                    Console.WriteLine($"  Perfección: {pantalon.Perfeccion}");
                }
            }
            else Console.WriteLine("Pantalón: Sin equipar");

            // Zapatos
            if (eq.Zapatos != null)
            {
                Console.WriteLine($"Zapatos: {eq.Zapatos.Nombre}");
                if (eq.Zapatos is MiJuegoRPG.Objetos.Botas botas)
                {
                    Console.WriteLine($"  Rareza: {botas.Rareza}");
                    Console.WriteLine($"  Perfección: {botas.Perfeccion}");
                }
            }
            else Console.WriteLine("Zapatos: Sin equipar");

            // Collar
            if (eq.Collar != null)
            {
                Console.WriteLine($"Collar: {eq.Collar.Nombre}");
                if (eq.Collar is MiJuegoRPG.Objetos.Collar collar)
                {
                    Console.WriteLine($"  Rareza: {collar.Rareza}");
                    Console.WriteLine($"  Perfección: {collar.Perfeccion}");
                }
            }
            else Console.WriteLine("Collar: Sin equipar");

            // Cinturón
            if (eq.Cinturon != null)
            {
                Console.WriteLine($"Cinturón: {eq.Cinturon.Nombre}");
                if (eq.Cinturon is MiJuegoRPG.Objetos.Cinturon cinturon)
                {
                    Console.WriteLine($"  Rareza: {cinturon.Rareza}");
                    Console.WriteLine($"  Perfección: {cinturon.Perfeccion}");
                }
            }
            else Console.WriteLine("Cinturón: Sin equipar");

            // Accesorio 1
            if (eq.Accesorio1 != null)
            {
                Console.WriteLine($"Accesorio 1: {eq.Accesorio1.Nombre}");
                if (eq.Accesorio1 is MiJuegoRPG.Objetos.Accesorio acc1)
                {
                    Console.WriteLine($"  Rareza: {acc1.Rareza}");
                    Console.WriteLine($"  Perfección: {acc1.Perfeccion}");
                }
            }
            else Console.WriteLine("Accesorio 1: Sin equipar");

            // Accesorio 2
            if (eq.Accesorio2 != null)
            {
                Console.WriteLine($"Accesorio 2: {eq.Accesorio2.Nombre}");
                if (eq.Accesorio2 is MiJuegoRPG.Objetos.Accesorio acc2)
                {
                    Console.WriteLine($"  Rareza: {acc2.Rareza}");
                    Console.WriteLine($"  Perfección: {acc2.Perfeccion}");
                }
            }
            else Console.WriteLine("Accesorio 2: Sin equipar");
        }

        private void MostrarInformacionPersonaje()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                return;
            }
            var pj = juego.jugador;
            Console.WriteLine($"--- Información de {pj.Nombre} ---");
            Console.WriteLine($"Nivel: {pj.Nivel}  Exp: {pj.Experiencia}/{pj.ExperienciaSiguienteNivel}");
            Console.WriteLine($"Vida: {pj.Vida}/{pj.VidaMaxima}  Oro: {pj.Oro}");
            Console.WriteLine($"Clase: {pj.Clase?.Nombre ?? "Sin clase"}");
            Console.WriteLine($"Título: {pj.Titulo}");
            Console.WriteLine("--- Atributos ---");
            foreach (var atributo in pj.AtributosBase.GetType().GetProperties())
            {
                var nombre = atributo.Name;
                var valorBase = Convert.ToDouble(atributo.GetValue(pj.AtributosBase));
                var bonif = pj.ObtenerBonificadorAtributo(nombre);
                var total = valorBase + bonif;
                if (bonif != 0)
                    Console.WriteLine($"{nombre}: {total} ({valorBase} base + {bonif} equipo)");
                else
                    Console.WriteLine($"{nombre}: {total} ({valorBase} base)");
            }
            Console.WriteLine("--- Estadísticas ---");
            if (pj.Estadisticas != null)
            {
                foreach (var stat in pj.Estadisticas.GetType().GetProperties())
                {
                    var nombre = stat.Name;
                    var valorBase = Convert.ToDouble(stat.GetValue(pj.Estadisticas));
                    var bonif = pj.ObtenerBonificadorEstadistica(nombre);
                    var total = valorBase + bonif;
                    if (bonif != 0)
                        Console.WriteLine($"{nombre}: {total} ({valorBase} base + {bonif} equipo)");
                    else
                        Console.WriteLine($"{nombre}: {total} ({valorBase} base)");
                }
            }
        }

        public void MostrarMenuPrincipalFijo()
        {
            while (true)
            {
                Console.WriteLine("\n=== Menú Principal ===");
                Console.WriteLine("1. Ver personaje");
                Console.WriteLine("2. Inventario");
                Console.WriteLine("3. Equipo equipado");
                Console.WriteLine("4. Guardar partida");
                Console.WriteLine("5. Salir del juego");
                Console.WriteLine("6. Volver");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
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
                        Console.WriteLine("Partida guardada.");
                        break;
                    case "5":
                        Environment.Exit(0);
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        // Ejemplo de integración en menús contextuales:
        private void MostrarMenuCiudad(string ciudad)
        {
            while (true)
            {
                Console.WriteLine($"\n=== Menú de Ciudad: {ciudad} ===");
                Console.WriteLine("1. Explorar sector (NPC y misiones)");
                Console.WriteLine("2. Ver misiones activas");
                Console.WriteLine("3. Ir a tienda");
                Console.WriteLine("4. Menú principal (fijo)");
                Console.WriteLine("5. Salir de la ciudad");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        MostrarMenuMisionesNPC();
                        break;
                    case "2":
                        MostrarMisionesActivas();
                        break;
                    case "3":
                        MostrarMenuTienda(ciudad);
                        break;
                    case "4":
                        MostrarMenuPrincipalFijo();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        private void MostrarMenuCombate()
        {
            while (true)
            {
                Console.WriteLine("\n=== Combate ===");
                Console.WriteLine("1. Atacar");
                Console.WriteLine("2. Usar habilidad");
                Console.WriteLine("3. Usar objeto");
                Console.WriteLine("4. Huir");
                Console.WriteLine("5. Menú principal (fijo)");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        // Lógica de ataque
                        break;
                    case "2":
                        // Lógica de habilidad
                        break;
                    case "3":
                        // Lógica de objeto
                        break;
                    case "4":
                        // Lógica de huida
                        return;
                    case "5":
                        MostrarMenuPrincipalFijo();
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        // Servicio de tienda integrado
        private readonly MiJuegoRPG.Comercio.ShopService _shop = new(new MiJuegoRPG.Comercio.PriceService());

        private void MostrarMenuTienda(string tienda)
        {
            // Se asume que la ubicación actual es el nombre de la ciudad (puedes ajustar esto)
            var ubicacionActual = tienda;
            var vendor = _shop.GetVendorPorUbicacion(ubicacionActual);
            if (vendor == null) { Console.WriteLine("No hay mercader en esta zona."); return; }

            while (true)
            {
                Console.WriteLine($"\n=== Tienda: {vendor.Nombre} ({vendor.Ubicacion}) === Oro: {juego.jugador?.Oro}");
                for (int i=0;i<vendor.Stock.Count;i++)
                {
                    var si = vendor.Stock[i];
                    var p = new MiJuegoRPG.Comercio.PriceService().PrecioDe(si.Item);
                    Console.WriteLine($"{i+1}. {si.Item.Nombre} x{si.Cantidad}  [{p} oro]");
                }
                Console.WriteLine("C. Comprar | V. Vender | S. Salir");
                Console.Write("> ");
                var op = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (op=="S") break;
                if (juego.jugador==null) { Console.WriteLine("No hay personaje."); continue; }

                if (op=="C")
                {
                    Console.Write("Índice a comprar: "); int.TryParse(Console.ReadLine(), out var idx);
                    Console.Write("Cantidad: "); int.TryParse(Console.ReadLine(), out var cant);
                    if (_shop.Comprar(juego.jugador, vendor, idx-1, cant, out var msg)) Console.WriteLine(msg);
                    else Console.WriteLine($"No se pudo comprar: {msg}");
                }
                else if (op=="V")
                {
                    var inv = juego.jugador.Inventario.NuevosObjetos;
                    for (int i=0;i<inv.Count;i++)
                    {
                        var si = inv[i];
                        var pr = new MiJuegoRPG.Comercio.PriceService().PrecioReventa(si.Objeto);
                        Console.WriteLine($"{i+1}. {si.Objeto.Nombre} x{si.Cantidad}  [vende: {pr}]");
                    }
                    Console.Write("Índice a vender: "); int.TryParse(Console.ReadLine(), out var idx);
                    Console.Write("Cantidad: "); int.TryParse(Console.ReadLine(), out var cant);
                    if (_shop.Vender(juego.jugador, vendor, idx-1, cant, out var msg)) Console.WriteLine(msg);
                    else Console.WriteLine($"No se pudo vender: {msg}");
                }
            }
        }

        private void MostrarMenuMisiones()
        {
            while (true)
            {
                Console.WriteLine("\n=== Misiones Activas ===");
                Console.WriteLine("1. Ver detalles de misión");
                Console.WriteLine("2. Entregar misión");
                Console.WriteLine("3. Menú principal (fijo)");
                Console.WriteLine("4. Volver");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
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
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        private void MostrarMenuNPC(string nombreNPC)
        {
            while (true)
            {
                Console.WriteLine($"\n=== NPC: {nombreNPC} ===");
                Console.WriteLine("1. Hablar");
                Console.WriteLine("2. Ver misiones disponibles");
                Console.WriteLine("3. Menú principal (fijo)");
                Console.WriteLine("4. Volver");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
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
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    }
}
