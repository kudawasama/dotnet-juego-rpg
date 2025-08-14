using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MotorInventario
    {
        private Juego juego;
        public MotorInventario(Juego juego)
        {
            this.juego = juego;
        }
        public void GestionarInventario()
        {
            if (juego.jugador == null)
            {
                Console.WriteLine("No hay personaje cargado.");
                Console.ReadKey();
                return;
            }
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== Inventario ===");
                if (juego.jugador.Inventario.NuevosObjetos.Count == 0)
                {
                    Console.WriteLine("Tu inventario está vacío.");
                    Console.WriteLine("Presiona cualquier tecla para volver al menú...");
                    Console.ReadKey();
                    return;
                }
                for (int i = 0; i < juego.jugador.Inventario.NuevosObjetos.Count; i++)
                {
                    var objCant = juego.jugador.Inventario.NuevosObjetos[i];
                    Console.WriteLine($"{i + 1}. {objCant.Objeto.Nombre} x{objCant.Cantidad}");
                }
                Console.WriteLine("\nOpciones:");
                Console.WriteLine("1. Usar objeto");
                Console.WriteLine("2. Equipar objeto");
                Console.WriteLine("3. Desequipar objeto");
                Console.WriteLine("0. Salir");
                Console.Write("Selecciona una opción: ");
                string opcion = Console.ReadLine() ?? "";
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
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void UsarObjeto()
        {
            Console.Write("Ingresa el número del objeto a usar: ");
            var input = Console.ReadLine();
            if (juego.jugador != null && juego.jugador.Inventario != null && juego.jugador.Inventario.NuevosObjetos != null &&
                int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Pocion pocion)
                {
                    juego.jugador.Vida = Math.Min(juego.jugador.Vida + pocion.Curacion, juego.jugador.VidaMaxima);
                    objCant.Cantidad--;
                    if (objCant.Cantidad <= 0)
                        juego.jugador.Inventario.NuevosObjetos.RemoveAt(seleccion - 1);
                    Console.WriteLine($"Usaste {pocion.Nombre} y recuperaste {pocion.Curacion} puntos de vida.");
                }
                else
                {
                    Console.WriteLine($"No puedes usar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                Console.WriteLine("Selección inválida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        private void EquiparObjeto()
        {
            Console.Write("Ingresa el número del objeto a equipar: ");
            var input = Console.ReadLine();
            if (juego.jugador != null && int.TryParse(input, out int seleccion) && seleccion > 0 && seleccion <= juego.jugador.Inventario.NuevosObjetos.Count)
            {
                var objCant = juego.jugador.Inventario.NuevosObjetos[seleccion - 1];
                if (objCant.Objeto is Arma arma)
                {
                    juego.jugador.Inventario.Equipo.Arma = arma;
                    Console.WriteLine($"Has equipado {arma.Nombre} como arma.");
                }
                else if (objCant.Objeto.Categoria == "Casco")
                {
                    juego.jugador.Inventario.Equipo.Casco = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como casco.");
                }
                else if (objCant.Objeto.Categoria == "Armadura")
                {
                    juego.jugador.Inventario.Equipo.Armadura = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como armadura.");
                }
                else if (objCant.Objeto.Categoria == "Pantalon")
                {
                    juego.jugador.Inventario.Equipo.Pantalon = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como pantalón.");
                }
                else if (objCant.Objeto.Categoria == "Botas")
                {
                    juego.jugador.Inventario.Equipo.Zapatos = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como botas.");
                }
                else if (objCant.Objeto.Categoria == "Collar")
                {
                    juego.jugador.Inventario.Equipo.Collar = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como collar.");
                }
                else if (objCant.Objeto.Categoria == "Cinturon")
                {
                    juego.jugador.Inventario.Equipo.Cinturon = objCant.Objeto;
                    Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} como cinturón.");
                }
                else if (objCant.Objeto.Categoria == "Accesorio")
                {
                    if (juego.jugador.Inventario.Equipo.Accesorio1 == null)
                    {
                        juego.jugador.Inventario.Equipo.Accesorio1 = objCant.Objeto;
                        Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 1.");
                    }
                    else if (juego.jugador.Inventario.Equipo.Accesorio2 == null)
                    {
                        juego.jugador.Inventario.Equipo.Accesorio2 = objCant.Objeto;
                        Console.WriteLine($"Has equipado {objCant.Objeto.Nombre} en accesorio 2.");
                    }
                    else
                    {
                        Console.WriteLine("Ya tienes ambos accesorios equipados. Desequipa uno primero.");
                    }
                }
                else
                {
                    Console.WriteLine($"No puedes equipar {objCant.Objeto.Nombre}.");
                }
            }
            else
            {
                Console.WriteLine("Selección inválida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        private void DesequiparObjeto()
        {
            Console.WriteLine("¿Qué tipo de equipo deseas desequipar?");
            Console.WriteLine("1. Arma\n2. Casco\n3. Armadura\n4. Pantalón\n5. Botas\n6. Collar\n7. Cinturón\n8. Accesorio 1\n9. Accesorio 2\n0. Cancelar");
            Console.Write("Selecciona una opción: ");
            string opcion = Console.ReadLine() ?? "";
            if (juego.jugador != null)
            {
                switch (opcion)
                {
                    case "1": juego.jugador.Inventario.Equipo.Arma = null; Console.WriteLine("Arma desequipada."); break;
                    case "2": juego.jugador.Inventario.Equipo.Casco = null; Console.WriteLine("Casco desequipado."); break;
                    case "3": juego.jugador.Inventario.Equipo.Armadura = null; Console.WriteLine("Armadura desequipada."); break;
                    case "4": juego.jugador.Inventario.Equipo.Pantalon = null; Console.WriteLine("Pantalón desequipado."); break;
                    case "5": juego.jugador.Inventario.Equipo.Zapatos = null; Console.WriteLine("Botas desequipadas."); break;
                    case "6": juego.jugador.Inventario.Equipo.Collar = null; Console.WriteLine("Collar desequipado."); break;
                    case "7": juego.jugador.Inventario.Equipo.Cinturon = null; Console.WriteLine("Cinturón desequipado."); break;
                    case "8": juego.jugador.Inventario.Equipo.Accesorio1 = null; Console.WriteLine("Accesorio 1 desequipado."); break;
                    case "9": juego.jugador.Inventario.Equipo.Accesorio2 = null; Console.WriteLine("Accesorio 2 desequipado."); break;
                    case "0": Console.WriteLine("Cancelado."); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
            else
            {
                Console.WriteLine("No hay equipo o inventario para modificar.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}
