using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Sector
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public bool Descubierto { get; set; } = false;
        public List<string> Eventos { get; set; } = new List<string>();
        public List<string> EnemigosPosibles { get; set; } = new List<string>();
        public List<string> ObjetosPosibles { get; set; } = new List<string>();
    }

    public class Mapa
    {
        private Dictionary<string, PjDatos.SectorData> sectores = new Dictionary<string, PjDatos.SectorData>();
        public PjDatos.SectorData UbicacionActual { get; private set; }
        public Dictionary<string, bool> SectoresDescubiertos { get; private set; } = new Dictionary<string, bool>();

        public List<PjDatos.SectorData> ObtenerSectores()
        {
            return new List<PjDatos.SectorData>(sectores.Values);
        }

        public Mapa(Dictionary<string, PjDatos.SectorData> sectoresDict)
        {
            sectores = sectoresDict ?? new Dictionary<string, PjDatos.SectorData>();
                PjDatos.SectorData? ubicacionInicial = null;
                foreach (var sector in sectores.Values)
                {
                    if (sector.CiudadInicial)
                    {
                        ubicacionInicial = sector;
                        break;
                    }
                }
                if (ubicacionInicial == null)
                {
                    ubicacionInicial = sectores.Count > 0 ? new List<PjDatos.SectorData>(sectores.Values)[0] : null;
                }
                if (ubicacionInicial == null)
                {
                    throw new InvalidOperationException("No se encontró ninguna ubicación válida en el mapa. Verifica los archivos de mapa.");
                }
                UbicacionActual = ubicacionInicial;
                SectoresDescubiertos[UbicacionActual.Id] = true;
        }

    // Método de carga por archivo eliminado, ahora se usa el constructor con diccionario

        public void MoverseA(string idSectorDestino)
        {
            if (sectores.TryGetValue(idSectorDestino, out var sectorDestino))
            {
                if (UbicacionActual.Conexiones.Contains(sectorDestino.Id))
                {
                    UbicacionActual = sectorDestino;
                    Console.WriteLine($"Te has movido a {UbicacionActual.Nombre}.");
                    if (!SectoresDescubiertos.ContainsKey(UbicacionActual.Id))
                        SectoresDescubiertos[UbicacionActual.Id] = true;
                }
                else
                {
                    Console.WriteLine("No puedes viajar directamente a esa ubicación.");
                }
            }
        }

        public List<PjDatos.SectorData> ObtenerSectoresAdyacentes()
        {
            var adyacentes = new List<PjDatos.SectorData>();
            foreach (var idConexion in UbicacionActual.Conexiones)
            {
                if (sectores.ContainsKey(idConexion))
                {
                    var sector = sectores[idConexion];
                    if (sector != null)
                        adyacentes.Add(sector);
                }
            }
            return adyacentes;
        }

        public void MostrarMapa()
        {
            Console.WriteLine($"Mapa actual:");
            int idx = 1;
            foreach (var s in sectores.Values)
            {
                var estado = SectoresDescubiertos.ContainsKey(s.Id) && SectoresDescubiertos[s.Id] ? "Descubierto" : "Sin explorar";
                Console.WriteLine($"{idx}. {s.Nombre} - {estado}");
                idx++;
            }
        }
    }
}
