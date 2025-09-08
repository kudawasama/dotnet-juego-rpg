using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Dominio;

namespace MiJuegoRPG.Motor.Servicios
{
    /// Servicio que gestiona el flujo de UI y selección de nodos de recolección.
    /// Centraliza la lógica que antes estaba en Juego y MotorEventos.
    public class RecoleccionService
    {
        private readonly Juego juego;
        private readonly ProgressionService progressionService;
        public RecoleccionService(Juego juego)
        {
            this.juego = juego;
            this.progressionService = new ProgressionService();
        }

        public void MostrarMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Menú de Recolección ===");
                Console.WriteLine("1. Recolectar");
                Console.WriteLine("2. Minar");
                Console.WriteLine("3. Talar");
                Console.WriteLine("4. Volver");
                Console.Write("Selecciona una opción: ");
                var opcion = InputService.LeerOpcion();
                if (opcion == "4") return;
                TipoRecoleccion? tipo = opcion switch
                {
                    "1" => TipoRecoleccion.Recolectar,
                    "2" => TipoRecoleccion.Minar,
                    "3" => TipoRecoleccion.Talar,
                    _ => null
                };
                if (tipo == null)
                {
                    Console.WriteLine("Opción no válida.");
                    continue;
                }
                var nodos = ObtenerNodos(tipo.Value);
                if (nodos.Count == 0)
                {
                    Console.WriteLine($"No hay nodos disponibles para {tipo} en este sector.");
                    continue;
                }
                Console.WriteLine($"--- Selecciona un nodo de {tipo.Value} ---");
                for (int i = 0; i < nodos.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {nodos[i].Nombre}");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Nodo: ");
                var sel = InputService.LeerOpcion();
                if (sel == "0") continue;
                if (int.TryParse(sel, out int idx) && idx > 0 && idx <= nodos.Count)
                {
                    // Ejecutar directamente la acción centralizada
                    EjecutarAccion(tipo.Value, nodos[idx - 1]);
                }
                else
                {
                    Console.WriteLine("Selección inválida.");
                }
            }
        }

        private List<NodoRecoleccion> ObtenerNodos(TipoRecoleccion tipo)
        {
            var lista = new List<NodoRecoleccion>();
            var sector = juego.mapa.UbicacionActual;
            if (sector != null && sector.NodosRecoleccion != null && sector.NodosRecoleccion.Count > 0)
            {
                lista.AddRange(sector.NodosRecoleccion);
            }
            if (lista.Count == 0 && sector != null && !string.IsNullOrWhiteSpace(sector.Region))
            {
                lista.AddRange(TablaBiomas.GenerarNodosParaBioma(sector.Region));
            }
            // Filtrar si el nodo expone propiedad Tipo (si no existe, se devuelven todos)
            var filtrados = lista.Where(n => string.IsNullOrWhiteSpace(n.Tipo) || string.Equals(n.Tipo, tipo.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            return filtrados;
        }

        // Nueva lógica centralizada antes en Juego.RealizarAccionRecoleccion
        public void EjecutarAccion(TipoRecoleccion tipo, NodoRecoleccion nodo)
        {
            // Validar requisito herramienta
            if (!string.IsNullOrEmpty(nodo.Requiere))
            {
                bool tieneHerramienta = juego.jugador != null &&
                    juego.jugador.Inventario != null &&
                    juego.jugador.Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.Contains(nodo.Requiere));
                if (!tieneHerramienta)
                {
                    Console.WriteLine($"Necesitas un {nodo.Requiere} para realizar esta acción.");
                    InputService.Pausa();
                    return;
                }
            }
            // Energía
            if (juego.jugador != null)
            {
                juego.energiaService.MostrarEnergia(juego.jugador);
                if (!juego.energiaService.GastarEnergiaRecoleccion(juego.jugador))
                {
                    Console.WriteLine("No puedes realizar la acción por falta de energía.");
                    InputService.Pausa();
                    return;
                }
                progressionService.AplicarExpRecoleccion(juego.jugador, tipo);
            }
            Console.WriteLine($"Recolectaste en el nodo: {nodo.Nombre}");
            if (nodo.Materiales != null && nodo.Materiales.Count > 0)
            {
                foreach (var mat in nodo.Materiales)
                {
                    Console.WriteLine($"  - {mat.Cantidad}x {mat.Nombre}");
                    if (juego.jugador != null && juego.jugador.Inventario != null)
                        juego.jugador.Inventario.AgregarObjeto(new Objetos.Material(mat.Nombre, Objetos.Rareza.Normal));
                }
            }
            else
            {
                Console.WriteLine("No encontraste materiales en este nodo.");
            }
            InputService.Pausa();
        }
    }
}
