using System;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.PjDatos
{
    public class PersonajeData
    {
        public string Nombre { get; set; } = "";
        public string ClaseNombre { get; set; } = "";
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
    }
}