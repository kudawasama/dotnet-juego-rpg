using System;
using System.Collections.Generic;
using MiJuegoRPG.Motor;

namespace MiJuegoRPG.Motor
{
    public class MotorRutas
    {
        private Juego juego;
        public MotorRutas(Juego juego)
        {
            this.juego = juego;
        }
        public void MostrarMenuRutas()
        {
            var ubicacionActual = juego.Mapa.UbicacionActual;
            var sectores = juego.Mapa.ObtenerSectores();
            Console.Clear();
            Console.WriteLine(juego.FormatoRelojMundo);
            Console.WriteLine($"Sectores conectados desde {ubicacionActual.Nombre}:");
            var sectoresConectados = new List<PjDatos.SectorData>();
            foreach (var idConexion in ubicacionActual.Conexiones)
            {
                var conectado = sectores.Find(s => s.Id == idConexion);
                if (conectado != null)
                    sectoresConectados.Add(conectado);
            }
            int i = 1;
            foreach (var sector in sectoresConectados)
            {
                Console.WriteLine($"{i}. {sector.Nombre} - {sector.Descripcion}");
                i++;
            }
            Console.WriteLine($"0. Volver");
            Console.Write("Selecciona el sector al que deseas viajar: ");
            var opcion = Console.ReadLine();
            if (opcion == "0")
                return;
            if (int.TryParse(opcion, out int idx) && idx > 0 && idx <= sectoresConectados.Count)
            {
                var destino = sectoresConectados[idx - 1];
                if (juego.Mapa.MoverseA(destino.Id))
                {
                    Console.WriteLine($"Te has movido a: {destino.Nombre}");
                    Console.WriteLine(destino.Descripcion);
                    Console.WriteLine("Presiona cualquier tecla para continuar...");
                    Console.ReadKey();
                    juego.MostrarMenuPorUbicacion();
                    return;
                }
                else
                {
                    Console.WriteLine("No puedes moverte a ese sector.");
                }
            }
            else
            {
                Console.WriteLine("Opción no válida.");
            }
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}
