using System;
using System.Collections.Generic;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class MotorEventos
    {
        private Juego juego;
        private Random random = new Random();
        public MotorEventos(Juego juego)
        {
            this.juego = juego;
        }

        public void ExplorarSector()
        {
            var sectorActual = juego.mapa.UbicacionActual;
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
                    int resultado = random.Next(100);
                    if (resultado < 40)
                    {
                        Console.WriteLine("¡Te has encontrado con un enemigo!");
                        if (juego.jugador == null)
                        {
                            Console.WriteLine("No hay personaje cargado. Creando nuevo personaje...");
                            juego.jugador = CreadorPersonaje.Crear();
                        }
                        var enemigo = GeneradorEnemigos.GenerarEnemigoAleatorio(juego.jugador);
                        GeneradorEnemigos.IniciarCombate(juego.jugador, enemigo);
                    }
                    else if (resultado < 70)
                    {
                        Console.WriteLine("¡Has encontrado un objeto!");
                        if (juego.jugador != null)
                        {
                            juego.jugador.Inventario.AgregarObjeto(new Pocion("Objeto misterioso", 1));
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
                    RealizarAccionRecoleccion(eventoElegido);
                }
                else if (eventoElegido == "Escuela de Entrenamiento")
                {
                    juego.Entrenar();
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
                int probMonstruo = random.Next(100);
                int probObjeto = random.Next(100);
                int probMazmorra = random.Next(100);
                int probEvento = random.Next(100);
                bool monstruo = probMonstruo < juego.ProbMonstruo;
                bool objeto = probObjeto < juego.ProbObjeto;
                bool mazmorra = probMazmorra < juego.ProbMazmorra;
                bool evento = probEvento < juego.ProbEvento;

                if (monstruo)
                {
                    Console.WriteLine("¡Un monstruo aparece!");
                    juego.motorCombate.ComenzarCombate();
                    juego.ProgresionPorActividad("combate");
                }
                if (objeto)
                {
                    Console.WriteLine("Encuentras un objeto especial: Poción curativa.");
                    if (juego.jugador != null)
                    {
                        juego.jugador.Inventario.AgregarObjeto(new Pocion("Poción Curativa", 20));
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

        public void RealizarAccionRecoleccion(string tipo)
        {
            var random = new Random();
            switch (tipo)
            {
                case "Recolectar":
                    if (random.Next(100) < 60)
                    {
                        Console.WriteLine("Recolectaste hierbas curativas.");
                        if (juego.jugador != null)
                            juego.jugador.Inventario.AgregarObjeto(new Pocion("Hierba Curativa", 10));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste nada útil.");
                    }
                    break;
                case "Minar":
                    if (random.Next(100) < 40)
                    {
                        Console.WriteLine("Minaste mineral raro.");
                        if (juego.jugador != null)
                            juego.jugador.Inventario.AgregarObjeto(new Pocion("Mineral Raro", 0));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste minerales.");
                    }
                    break;
                case "Talar":
                    if (random.Next(100) < 50)
                    {
                        Console.WriteLine("Talarte madera resistente.");
                        if (juego.jugador != null)
                            juego.jugador.Inventario.AgregarObjeto(new Pocion("Madera Resistente", 0));
                    }
                    else
                    {
                        Console.WriteLine("No encontraste árboles útiles.");
                    }
                    break;
            }
        }
    }
}
