using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Acciones;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor.Menus
{
    public class MenuRecoleccion
    {
        private Juego juego;
        public MenuRecoleccion(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuRecoleccion(ref bool salir)
        {
            string opcion = "";
            var accionRecoleccion = new AccionRecoleccion(juego);
            while (!salir)
            {
                UIStyle.Header(juego.Ui, "Recolección");
                UIStyle.Hint(juego.Ui, "Filtra por rareza con el menú híbrido");
                juego.Ui.WriteLine("1. Buscar materiales");
                juego.Ui.WriteLine("2. Volver");
                opcion = InputService.LeerOpcion();
                switch (opcion)
                {
                    case "1":
                        accionRecoleccion.RealizarAccionRecoleccion("material");
                        break;
                    case "2":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción no válida.");
                        InputService.Pausa();
                        break;
                }
            }
        }
    }
}
