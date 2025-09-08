using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AccionRecoleccion
    {
        private Juego juego;
        public AccionRecoleccion(Juego juego)
        {
            this.juego = juego;
        }
        public void RealizarAccionRecoleccion(string tipo)
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                return;
            }
            
            // Simple material generation for collection action
            var random = new Random();
            var materiales = new[] { "Madera", "Piedra", "Hierro", "Hierba" };
            var rarezas = new[] { MiJuegoRPG.Objetos.Rareza.Normal, MiJuegoRPG.Objetos.Rareza.Normal, MiJuegoRPG.Objetos.Rareza.Rara };
            
            if (random.Next(0, 100) < 60) // 60% chance de encontrar algo
            {
                var materialNombre = materiales[random.Next(materiales.Length)];
                var rareza = rarezas[random.Next(rarezas.Length)];
                var material = new MiJuegoRPG.Objetos.Material(materialNombre, rareza);
                
                juego.jugador.Inventario.Agregar(material);
                Console.WriteLine($"Has recolectado: {material.Nombre}");
            }
            else
            {
                Console.WriteLine("No encontraste materiales esta vez.");
            }
        }
    }
}
