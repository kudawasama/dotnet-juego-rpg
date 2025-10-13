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
        private IUserInterface Ui => Juego.ObtenerInstanciaActual()?.Ui ?? new ConsoleUserInterface();
        public MotorInventario(Juego juego)
        {
            this.juego = juego;
        }
        public void GestionarInventario()
        {
            if (juego.Jugador == null)
            {
                Ui.WriteLine("No hay personaje cargado.");
                InputService.Pausa();
                return;
            }
            bool salir = false;
            while (!salir)
            {
                // Limpieza visual (si la UI soporta clear, se podría extender IUserInterface)
                if (juego.Jugador.Inventario.NuevosObjetos.Count == 0)
                {
                    UIStyle.Header(Ui, "Inventario");
                    Ui.WriteLine("Tu inventario está vacío.");
                    InputService.Pausa("Presiona cualquier tecla para volver al menú...");
                    return;
                }
                juego.Jugador.Inventario.MostrarInventario();
                Ui.WriteLine("\nOpciones:");
                Ui.WriteLine("1. Usar objeto");
                Ui.WriteLine("2. Equipar objeto");
                Ui.WriteLine("3. Desequipar objeto");
                Ui.WriteLine("0. Salir");
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
            if (juego.Jugador != null && juego.Jugador.Inventario != null && juego.Jugador.Inventario.NuevosObjetos != null &&
                int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.Jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.Jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Pocion pocion)
                {
                    bool confirmar = Ui.Confirm($"¿Usar {pocion.Nombre} para curar {pocion.Curacion} HP? (s/n): ");
                    if (confirmar)
                    {
                        juego.Jugador.Vida = Math.Min(juego.Jugador.Vida + pocion.Curacion, juego.Jugador.VidaMaxima);
                        objCant.Cantidad--;
                        if (objCant.Cantidad <= 0)
                            juego.Jugador.Inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                        Ui.WriteLine($"Usaste {pocion.Nombre} y recuperaste {pocion.Curacion} puntos de vida.");
                    }
                    else
                    {
                        Ui.WriteLine("Acción cancelada.");
                    }
                }
                else
                {
                    Ui.WriteLine($"No puedes usar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                Ui.WriteLine("Selección inválida.");
            }
            InputService.Pausa();
        }

        private void EquiparObjeto()
        {
            var input = InputService.LeerOpcion("Ingresa el número del objeto a equipar: ");
            if (juego.Jugador != null && int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.Jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.Jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Arma arma)
                {
                    juego.Jugador.Inventario.Equipo.Arma = arma;
                    Ui.WriteLine($"Has equipado {arma.Nombre} como arma.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Casco")
                {
                    juego.Jugador.Inventario.Equipo.Casco = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como casco.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Armadura")
                {
                    juego.Jugador.Inventario.Equipo.Armadura = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como armadura.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Pantalon")
                {
                    juego.Jugador.Inventario.Equipo.Pantalon = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como pantalón.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Botas")
                {
                    juego.Jugador.Inventario.Equipo.Zapatos = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como botas.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Collar")
                {
                    juego.Jugador.Inventario.Equipo.Collar = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como collar.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Cinturon")
                {
                    juego.Jugador.Inventario.Equipo.Cinturon = objCant.Objeto;
                    Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} como cinturón.");
                    juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                }
                else if (objCant.Objeto.Categoria == "Accesorio")
                {
                    if (juego.Jugador.Inventario.Equipo.Accesorio1 == null)
                    {
                        juego.Jugador.Inventario.Equipo.Accesorio1 = objCant.Objeto;
                        Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 1.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                    }
                    else if (juego.Jugador.Inventario.Equipo.Accesorio2 == null)
                    {
                        juego.Jugador.Inventario.Equipo.Accesorio2 = objCant.Objeto;
                        Ui.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 2.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                    }
                    else
                    {
                        Ui.WriteLine("Ya tienes ambos accesorios equipados. Desequipa uno primero.");
                    }
                }
                else
                {
                    Ui.WriteLine($"No puedes equipar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                Ui.WriteLine("Selección inválida.");
            }
            InputService.Pausa();
        }

        private void DesequiparObjeto()
        {
            Ui.WriteLine("¿Qué tipo de equipo deseas desequipar?");
            Ui.WriteLine("1. Arma\n2. Casco\n3. Armadura\n4. Pantalón\n5. Botas\n6. Collar\n7. Cinturón\n8. Accesorio 1\n9. Accesorio 2\n0. Cancelar");
            string opcion = InputService.LeerOpcion("Selecciona una opción: ");
            if (juego.Jugador != null)
            {
                switch (opcion)
                {
                    case "1":
                        juego.Jugador.Inventario.Equipo.Arma = null;
                        Ui.WriteLine("Arma desequipada.");
                        break;
                    case "2":
                        juego.Jugador.Inventario.Equipo.Casco = null;
                        Ui.WriteLine("Casco desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "3":
                        juego.Jugador.Inventario.Equipo.Armadura = null;
                        Ui.WriteLine("Armadura desequipada.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "4":
                        juego.Jugador.Inventario.Equipo.Pantalon = null;
                        Ui.WriteLine("Pantalón desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "5":
                        juego.Jugador.Inventario.Equipo.Zapatos = null;
                        Ui.WriteLine("Botas desequipadas.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "6":
                        juego.Jugador.Inventario.Equipo.Collar = null;
                        Ui.WriteLine("Collar desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "7":
                        juego.Jugador.Inventario.Equipo.Cinturon = null;
                        Ui.WriteLine("Cinturón desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "8":
                        juego.Jugador.Inventario.Equipo.Accesorio1 = null;
                        Ui.WriteLine("Accesorio 1 desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "9":
                        juego.Jugador.Inventario.Equipo.Accesorio2 = null;
                        Ui.WriteLine("Accesorio 2 desequipado.");
                        juego.Jugador.Inventario.SincronizarHabilidadesYBonosSet(juego.Jugador);
                        break;
                    case "0":
                        Ui.WriteLine("Cancelado.");
                        break;
                    default:
                        Ui.WriteLine("Opción no válida.");
                        break;
                }
            }
            else
            {
                Ui.WriteLine("No hay equipo o inventario para modificar.");
            }
            InputService.Pausa();
        }
    }
}
