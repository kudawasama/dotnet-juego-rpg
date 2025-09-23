using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    public class MotorInventario
    {
        private Juego juego;
        private IUserInterface ui => Juego.ObtenerInstanciaActual()?.Ui ?? new ConsoleUserInterface();
        public MotorInventario(Juego juego)
        {
            this.juego = juego;
        }
        public void GestionarInventario()
        {
            if (juego.jugador == null)
            {
                ui.WriteLine("No hay personaje cargado.");
                InputService.Pausa();
                return;
            }
            bool salir = false;
            while (!salir)
            {
                // Limpieza visual (si la UI soporta clear, se podría extender IUserInterface)
                if (juego.jugador.Inventario.NuevosObjetos.Count == 0)
                {
                    UIStyle.Header(ui, "Inventario");
                    ui.WriteLine("Tu inventario está vacío.");
                    InputService.Pausa("Presiona cualquier tecla para volver al menú...");
                    return;
                }
                juego.jugador.Inventario.MostrarInventario();
                ui.WriteLine("\nOpciones:");
                ui.WriteLine("1. Usar objeto");
                ui.WriteLine("2. Equipar objeto");
                ui.WriteLine("3. Desequipar objeto");
                ui.WriteLine("0. Salir");
                string opcion = InputService.LeerOpcion("Selecciona una opción: ");
                switch (opcion)
                {
                    case "1":
                        UsarObjeto();
                        break;
                    case "2":
                        EquiparObjeto();
                        break;
                    case "3":
                        DesequiparObjeto();
                        break;
                    case "0":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
    }
    private void UsarObjeto()
        {
            var input = InputService.LeerOpcion("Ingresa el número del objeto a usar: ");
            if (juego.jugador != null && juego.jugador.Inventario != null && juego.jugador.Inventario.NuevosObjetos != null &&
                int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Pocion pocion)
                {
                    bool confirmar = ui.Confirm($"¿Usar {pocion.Nombre} para curar {pocion.Curacion} HP? (s/n): ");
                    if (confirmar)
                    {
                        juego.jugador.Vida = Math.Min(juego.jugador.Vida + pocion.Curacion, juego.jugador.VidaMaxima);
                        objCant.Cantidad--;
                        if (objCant.Cantidad <= 0)
                            juego.jugador.Inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                        ui.WriteLine($"Usaste {pocion.Nombre} y recuperaste {pocion.Curacion} puntos de vida.");
                    }
                    else
                    {
                        ui.WriteLine("Acción cancelada.");
                    }
                }
                else
                {
                    ui.WriteLine($"No puedes usar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                ui.WriteLine("Selección inválida.");
            }
            InputService.Pausa();
        }

        private void EquiparObjeto()
        {
            var input = InputService.LeerOpcion("Ingresa el número del objeto a equipar: ");
            if (juego.jugador != null && int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Arma arma)
                {
                    juego.jugador.Inventario.Equipo.Arma = arma;
                    ui.WriteLine($"Has equipado {arma.Nombre} como arma.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Casco")
                {
                    juego.jugador.Inventario.Equipo.Casco = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como casco.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Armadura")
                {
                    juego.jugador.Inventario.Equipo.Armadura = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como armadura.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Pantalon")
                {
                    juego.jugador.Inventario.Equipo.Pantalon = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como pantalón.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Botas")
                {
                    juego.jugador.Inventario.Equipo.Zapatos = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como botas.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Collar")
                {
                    juego.jugador.Inventario.Equipo.Collar = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como collar.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Cinturon")
                {
                    juego.jugador.Inventario.Equipo.Cinturon = objCant.Objeto;
                    ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como cinturón.");
                    juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                }
                else if (objCant.Objeto.Categoria == "Accesorio")
                {
                    if (juego.jugador.Inventario.Equipo.Accesorio1 == null)
                    {
                        juego.jugador.Inventario.Equipo.Accesorio1 = objCant.Objeto;
                        ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 1.");
                        juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                    }
                    else if (juego.jugador.Inventario.Equipo.Accesorio2 == null)
                    {
                        juego.jugador.Inventario.Equipo.Accesorio2 = objCant.Objeto;
                        ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 2.");
                        juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador);
                    }
                    else
                    {
                        ui.WriteLine("Ya tienes ambos accesorios equipados. Desequipa uno primero.");
                    }
                }
                else
                {
                    ui.WriteLine($"No puedes equipar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                ui.WriteLine("Selección inválida.");
            }
            InputService.Pausa();
        }

        private void DesequiparObjeto()
        {
            ui.WriteLine("¿Qué tipo de equipo deseas desequipar?");
            ui.WriteLine("1. Arma\n2. Casco\n3. Armadura\n4. Pantalón\n5. Botas\n6. Collar\n7. Cinturón\n8. Accesorio 1\n9. Accesorio 2\n0. Cancelar");
            string opcion = InputService.LeerOpcion("Selecciona una opción: ");
            if (juego.jugador != null)
            {
                switch (opcion)
                {
                    case "1": juego.jugador.Inventario.Equipo.Arma = null; ui.WriteLine("Arma desequipada."); break;
                    case "2": juego.jugador.Inventario.Equipo.Casco = null; ui.WriteLine("Casco desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "3": juego.jugador.Inventario.Equipo.Armadura = null; ui.WriteLine("Armadura desequipada."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "4": juego.jugador.Inventario.Equipo.Pantalon = null; ui.WriteLine("Pantalón desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "5": juego.jugador.Inventario.Equipo.Zapatos = null; ui.WriteLine("Botas desequipadas."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "6": juego.jugador.Inventario.Equipo.Collar = null; ui.WriteLine("Collar desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "7": juego.jugador.Inventario.Equipo.Cinturon = null; ui.WriteLine("Cinturón desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "8": juego.jugador.Inventario.Equipo.Accesorio1 = null; ui.WriteLine("Accesorio 1 desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "9": juego.jugador.Inventario.Equipo.Accesorio2 = null; ui.WriteLine("Accesorio 2 desequipado."); juego.jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.jugador); break;
                    case "0": ui.WriteLine("Cancelado."); break;
                    default: ui.WriteLine("Opción no válida."); break;
                }
            }
            else
            {
                ui.WriteLine("No hay equipo o inventario para modificar.");
            }
            InputService.Pausa();
        }
    }
}
