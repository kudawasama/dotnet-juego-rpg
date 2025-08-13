using MiJuegoRPG.Objetos;
using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;


namespace MiJuegoRPG.Personaje
{
    public class Equipo
    {
        public Objeto? Arma { get; set; }
        public Objeto? Casco { get; set; }
        public Objeto? Armadura { get; set; }
        public Objeto? Pantalon { get; set; }
        public Objeto? Zapatos { get; set; }
        public Objeto? Collar { get; set; }
        public Objeto? Cinturon { get; set; }
        public Objeto? Accesorio1 { get; set; }
        public Objeto? Accesorio2 { get; set; }
    }
    public class NewBaseType
    {
        // La clase base puede estar vacía o tener lógica compartida
    }

    public class Inventario : NewBaseType
    {
    public List<ObjetoConCantidad> NuevosObjetos { get; set; } = new List<ObjetoConCantidad>();
    public List<Objeto> Objetos { get; set; } = new List<Objeto>();
    public Equipo Equipo { get; set; } = new Equipo();
        public void AgregarObjeto(Objeto objeto, int cantidad = 1)
        {
            var existente = NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == objeto.Nombre && o.Objeto.GetType() == objeto.GetType());
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                Console.WriteLine($"Se apiló {cantidad}x {objeto.Nombre} (Total: {existente.Cantidad})");
            }
            else
            {
                NuevosObjetos.Add(new ObjetoConCantidad(objeto, cantidad));
                Console.WriteLine($"{objeto.Nombre} ha sido agregado al inventario.");
            }
        }

        public Inventario()
        {
            NuevosObjetos = new List<ObjetoConCantidad>();
        }

        public void MostrarInventario()
        {
            Console.WriteLine("Inventario:");
            foreach (var objCant in NuevosObjetos)
            {
                Console.WriteLine($"- {objCant.Objeto.Nombre} x{objCant.Cantidad}");
            }
        }
    }
}
