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
                        Console.WriteLine($"{i + 1}. {npc.Nombre}: {npc.Descripcion}");
                    }
                    Console.Write("Selecciona un NPC para ver sus misiones: ");
                    if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= npcsCiudad.Count)
                    {
                        var npcSeleccionado = npcsCiudad[seleccion - 1];
                        Console.WriteLine($"--- Misiones disponibles con {npcSeleccionado.Nombre} ---");
                        foreach (var idMision in npcSeleccionado.Misiones)
                        {
                            var mision = misiones.Find(m => m.Id == idMision);
                            if (mision != null && juego.jugador != null && juego.jugador.PuedeAccederMision(mision))
                            {
                                Console.WriteLine($"* {mision.Nombre}: {mision.Descripcion}");
                                // Aquí puedes agregar lógica para aceptar la misión si el jugador lo desea
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

        private void MostrarInventario()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                return;
            }
            Console.WriteLine("--- Inventario ---");
            foreach (var obj in juego.jugador.Inventario.Objetos)
            {
                Console.WriteLine($"- {obj.Nombre} ({obj.Tipo})");
            }
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
            Console.WriteLine($"Arma: {eq.Arma?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Casco: {eq.Casco?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Armadura: {eq.Armadura?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Pantalón: {eq.Pantalon?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Zapatos: {eq.Zapatos?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Collar: {eq.Collar?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Cinturón: {eq.Cinturon?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Accesorio 1: {eq.Accesorio1?.Nombre ?? "Sin equipar"}");
            Console.WriteLine($"Accesorio 2: {eq.Accesorio2?.Nombre ?? "Sin equipar"}");
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
                var valorBase = atributo.GetValue(pj.AtributosBase);
                var valorTotal = atributo.GetValue(pj.Atributos);
                Console.WriteLine($"{atributo.Name}: {valorTotal} ({valorBase} base)");
            }
            Console.WriteLine("--- Estadísticas ---");
            if (pj.Estadisticas != null)
            {
                foreach (var stat in pj.Estadisticas.GetType().GetProperties())
                {
                    var valor = stat.GetValue(pj.Estadisticas);
                    Console.WriteLine($"{stat.Name}: {valor}");
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
                        juego.GuardarPartida();
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
                Console.WriteLine("1. Hablar con NPC");
                Console.WriteLine("2. Ver misiones");
                Console.WriteLine("3. Ir a tienda");
                Console.WriteLine("4. Menú principal (fijo)");
                Console.WriteLine("5. Salir de la ciudad");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        MostrarNPCsCiudadActual(ciudad);
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

        private void MostrarMenuTienda(string tienda)
        {
            while (true)
            {
                Console.WriteLine($"\n=== Tienda: {tienda} ===");
                Console.WriteLine("1. Comprar objetos");
                Console.WriteLine("2. Vender objetos");
                Console.WriteLine("3. Hablar con el vendedor");
                Console.WriteLine("4. Menú principal (fijo)");
                Console.WriteLine("5. Salir de la tienda");
                Console.Write("Seleccione una opción: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        // Lógica de compra
                        break;
                    case "2":
                        // Lógica de venta
                        break;
                    case "3":
                        // Lógica de diálogo
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
