using MiJuegoRPG.Personaje;
using System;
using System.Collections.Generic;


namespace MiJuegoRPG.Objetos
{
    public abstract class Objeto
    {
        public string Nombre { get; set; }
        public Rareza Rareza { get; set; }
        public string Categoria { get; set; }

        public Objeto(string nombre, Rareza rareza = Rareza.Normal, string categoria = "Otro")
        {
            Nombre = nombre;
            Rareza = rareza;
            Categoria = categoria;
        }

        public abstract void Usar(MiJuegoRPG.Personaje.Personaje personaje);
    }
}
