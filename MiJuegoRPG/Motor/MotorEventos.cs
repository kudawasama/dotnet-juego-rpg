using System;
using System.Collections.Generic;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class MotorEventos
    {
        private Juego juego;
        // Random local eliminado: usar RandomService centralizado
        public MotorEventos(Juego juego)
        {
            this.juego = juego;
        }

        public void ExplorarSector()
        {
            var sectorActual = juego.Mapa.UbicacionActual;
            Console.WriteLine($"Sectores disponibles en {sectorActual.Nombre}:");
            var opciones = new List<string>(sectorActual.Eventos);
            Console.WriteLine("Elige una opción:");
            var opcion = Console.ReadLine();
            int seleccion;
            bool accionExploracion = false;
            if (int.TryParse(opcion, out seleccion) && seleccion > 0 && seleccion <= opciones.Count)
            {
                string eventoElegido = opciones[seleccion - 1];
                if (eventoElegido == "Explorar" || eventoElegido == "Explorar sector")
                {
                    accionExploracion = true;
                    int resultado = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100);
                    if (resultado < 40)
                    {
                        Console.WriteLine("¡Te has encontrado con un enemigo!");
                        if (juego.Jugador == null)
                        {
                            Console.WriteLine("No hay personaje cargado. Creando nuevo personaje...");
                            juego.Jugador = CreadorPersonaje.Crear();
                        }
                        var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(juego.Jugador);
                        GeneradorEnemigos.IniciarCombate(juego.Jugador, enemigo);
                    }
                    else if (resultado < 70)
                    {
                        Console.WriteLine("¡Has encontrado un objeto!");
                        if (juego.Jugador != null)
                        {
                            juego.Jugador.Inventario.AgregarObjeto(new Pocion("Objeto misterioso", 1));
                        }
                    }
                    else
                    {
                        Console.WriteLine("¡Has encontrado la entrada a una mazmorra!");
                    }
                }
                else if (eventoElegido == "Recolectar" || eventoElegido == "Minar" || eventoElegido == "Talar")
                {
                    accionExploracion = true;
                    var tipo = eventoElegido switch
                    {
                        "Recolectar" => MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar,
                        "Minar" => MiJuegoRPG.Dominio.TipoRecoleccion.Minar,
                        "Talar" => MiJuegoRPG.Dominio.TipoRecoleccion.Talar,
                        _ => MiJuegoRPG.Dominio.TipoRecoleccion.Recolectar
                    };
                    juego.RecoleccionService.EjecutarAccion(tipo, new NodoRecoleccion { Nombre = eventoElegido, Tipo = eventoElegido });
                }
                else if (eventoElegido == "Escuela de Entrenamiento")
                {
                    juego.MotorEntrenamiento.Entrenar();
                }
                else if (eventoElegido == "Tienda")
                {
                    juego.MostrarTienda();
                }
                else if (eventoElegido == "Descansar en posada")
                {
                    Console.WriteLine("Has descansado y recuperado energía.");
                }
                else
                {
                    Console.WriteLine($"Evento '{eventoElegido}' aún no implementado.");
                }
            }
            else
            {
                Console.WriteLine("Volviendo al menú principal...");
            }
            // Solo ejecutar eventos aleatorios si realmente se exploró
            if (accionExploracion)
            {
                int probMonstruo = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100);
                int probObjeto = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100);
                int probMazmorra = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100);
                int probEvento = MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100);
                bool monstruo = probMonstruo < juego.ProbMonstruo;
                bool objeto = probObjeto < juego.ProbObjeto;
                bool mazmorra = probMazmorra < juego.ProbMazmorra;
                bool evento = probEvento < juego.ProbEvento;

                if (monstruo)
                {
                    Console.WriteLine("¡Un monstruo aparece!");
                    juego.MotorCombate.ComenzarCombate();
                    juego.ProgresionPorActividad("combate");
                }
                if (objeto)
                {
                    Console.WriteLine("Encuentras un objeto especial: Poción curativa.");
                    if (juego.Jugador != null)
                    {
                        juego.Jugador.Inventario.AgregarObjeto(new Pocion("Poción Curativa", 20));
                        juego.ProgresionPorActividad("exploracion");
                    }
                }
                if (mazmorra)
                {
                    Console.WriteLine("¡Descubres la entrada a una mazmorra misteriosa!");
                    juego.ProgresionPorActividad("exploracion");
                }
                if (evento)
                {
                    Console.WriteLine("Ocurre un evento especial en el área. ¡Tu suerte aumenta!");
                    juego.ProgresionPorActividad("suerte");
                }
                if (!monstruo && !objeto && !mazmorra && !evento)
                {
                    Console.WriteLine("No ocurre nada relevante en esta exploración.");
                }
            }
        }

        // Método RealizarAccionRecoleccion eliminado: ahora gestionado por RecoleccionService
    }
}
