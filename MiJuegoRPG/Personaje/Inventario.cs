using MiJuegoRPG.Objetos;
using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;


namespace MiJuegoRPG.Personaje
{
    public class NewBaseType
    {
        // La clase base puede estar vacía o tener lógica compartida
    }

    public class Inventario : NewBaseType
    {
        public List<Objeto> NuevosObjetos { get; set; }
        public void AgregarObjeto(Objeto objeto)
        {
            NuevosObjetos.Add(objeto);
        }

        public Inventario()
        {
            NuevosObjetos = new List<Objeto>();
        }

        public void MostrarInventario()
        {
            Console.WriteLine("Inventario:");
            foreach (var obj in NuevosObjetos)
            {
                Console.WriteLine($"- {obj.Nombre}");
            }
        }
    }
}
