using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Ubicacion
    {
        public Ubicacion()
        {
            Id = "";
            Nombre = "";
            Tipo = "";
            Descripcion = "";
        }
        public Ubicacion(PjDatos.SectorData data)
        {
            Id = data.Id;
            Nombre = data.Nombre;
            Tipo = data.Tipo;
            Descripcion = data.Descripcion;
            CiudadPrincipal = data.CiudadPrincipal;
            EsCentroCiudad = data.EsCentroCiudad;
            ParteCiudad = data.ParteCiudad;
            // Copiar otros campos relevantes si es necesario
        }
        public string Id
        {
            get; set;
        }
        public string Nombre
        {
            get; set;
        }
        public string Tipo
        {
            get; set;
        } // Ciudad, Ruta, Mazmorra, etc.
        public string Descripcion
        {
            get; set;
        }
        public List<Ruta> Rutas { get; set; } = new List<Ruta>();
        public List<string> EventosPosibles { get; set; } = new List<string>();
        public bool Desbloqueada { get; set; } = false;
        public bool Visitada { get; set; } = false;
        public Dictionary<string, object> Requisitos { get; set; } = new Dictionary<string, object>();

        // Nuevos metadatos de ciudad
        public bool CiudadPrincipal { get; set; } = false;
        public bool EsCentroCiudad { get; set; } = false;
        public string? ParteCiudad { get; set; } = null;
    }

    public class Ruta
    {
        public Ruta()
        {
            Nombre = "";
            Destino = "";
        }
        public string Nombre
        {
            get; set;
        }
        public string Destino
        {
            get; set;
        }
        public bool Desbloqueada { get; set; } = false;
        public Dictionary<string, object> Requisitos { get; set; } = new Dictionary<string, object>();
        public string Estado { get; set; } = "Insegura"; // Segura/Insegura
    }

    public class EstadoMundo
    {
        public List<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
        public int DiaActual { get; set; } = 1;
        public int HoraActual { get; set; } = 8;
        public List<string> EventosGlobales { get; set; } = new List<string>();
        public Dictionary<string, bool> MazmorrasCompletadas { get; set; } = new Dictionary<string, bool>();
    }
}
