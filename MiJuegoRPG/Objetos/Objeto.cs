using MiJuegoRPG.Personaje;
using System;
using System.Collections.Generic;


namespace MiJuegoRPG.Objetos
{
    public abstract class Objeto
    {
        public string Nombre { get; set; }

        public Objeto(string nombre)
        {
            Nombre = nombre;
        }

        public abstract void Usar(MiJuegoRPG.Personaje.Personaje personaje); 
    }
}
