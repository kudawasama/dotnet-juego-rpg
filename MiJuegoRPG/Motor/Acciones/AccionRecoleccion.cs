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
            // Aquí iría la lógica de recolección de materiales, por ejemplo:
            var material = juego.GenerarMaterialAleatorio();
            if (material != null)
            {
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
