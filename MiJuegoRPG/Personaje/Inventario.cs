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

        /// <summary>
        /// Equipa un objeto (arma, armadura o accesorio) y muestra un aviso centralizado con comparación de estadísticas.
        /// </summary>
        public void EquiparObjeto(Objeto objeto, Personaje? personaje = null)
        {
            string aviso = "";
            string mejora = "";
            if (objeto is MiJuegoRPG.Objetos.Arma armaNueva)
            {
                var armaAnterior = Equipo.Arma as MiJuegoRPG.Objetos.Arma;
                Equipo.Arma = armaNueva;
                aviso += $"Anterior: {(armaAnterior != null ? armaAnterior.Nombre + " (Daño Físico: " + armaAnterior.DañoFisico + ")" : "Ninguna")}\n";
                aviso += $"Nuevo: {armaNueva.Nombre} (Daño Físico: {armaNueva.DañoFisico})\n";
                if (armaAnterior != null)
                {
                    int diff = armaNueva.DañoFisico - armaAnterior.DañoFisico;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} de daño físico)" : (diff < 0 ? $"¡Advertencia! ({diff} de daño físico)" : "Sin cambios en daño físico");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Armadura armaduraNueva)
            {
                var armaduraAnterior = Equipo.Armadura as MiJuegoRPG.Objetos.Armadura;
                Equipo.Armadura = armaduraNueva;
                aviso += $"Anterior: {(armaduraAnterior != null ? armaduraAnterior.Nombre + " (Defensa: " + armaduraAnterior.Defensa + ")" : "Ninguna")}\n";
                aviso += $"Nuevo: {armaduraNueva.Nombre} (Defensa: {armaduraNueva.Defensa})\n";
                if (armaduraAnterior != null)
                {
                    int diff = armaduraNueva.Defensa - armaduraAnterior.Defensa;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} de defensa)" : (diff < 0 ? $"¡Advertencia! ({diff} de defensa)" : "Sin cambios en defensa");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Accesorio accesorioNuevo)
            {
                // Por simplicidad, equipar en Accesorio1 si está vacío, si no en Accesorio2
                var accesorioAnterior = Equipo.Accesorio1 as MiJuegoRPG.Objetos.Accesorio;
                if (Equipo.Accesorio1 == null)
                {
                    Equipo.Accesorio1 = accesorioNuevo;
                }
                else
                {
                    accesorioAnterior = Equipo.Accesorio2 as MiJuegoRPG.Objetos.Accesorio;
                    Equipo.Accesorio2 = accesorioNuevo;
                }
                aviso += $"Anterior: {(accesorioAnterior != null ? accesorioAnterior.Nombre + " (Ataque: " + accesorioAnterior.BonificacionAtaque + ", Defensa: " + accesorioAnterior.BonificacionDefensa + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {accesorioNuevo.Nombre} (Ataque: {accesorioNuevo.BonificacionAtaque}, Defensa: {accesorioNuevo.BonificacionDefensa})\n";
                if (accesorioAnterior != null)
                {
                    int diffAtk = accesorioNuevo.BonificacionAtaque - accesorioAnterior.BonificacionAtaque;
                    int diffDef = accesorioNuevo.BonificacionDefensa - accesorioAnterior.BonificacionDefensa;
                    mejora = (diffAtk > 0 ? $"+{diffAtk} ATK " : (diffAtk < 0 ? $"{diffAtk} ATK " : "")) + (diffDef > 0 ? $"+{diffDef} DEF" : (diffDef < 0 ? $"{diffDef} DEF" : ""));
                    if (string.IsNullOrWhiteSpace(mejora)) mejora = "Sin cambios en bonificaciones";
                }
            }
            else
            {
                aviso = "Este objeto no es equipable.";
            }
            MiJuegoRPG.Motor.AvisosAventura.MostrarAviso(
                "Equipo Equipado",
                objeto.Nombre,
                aviso + (string.IsNullOrWhiteSpace(mejora) ? "" : mejora)
            );
    }
    public List<ObjetoConCantidad> NuevosObjetos { get; set; } = new List<ObjetoConCantidad>();
        public Equipo Equipo { get; set; } = new Equipo();
        public int CapacidadMaxima { get; set; } = 30;

        public void Agregar(Material material, int cantidad = 1)
        {
            AgregarObjeto(material, cantidad);
        }



    public void AgregarObjeto(Objeto objeto, int cantidad = 1, Personaje? personaje = null)
        {
            int totalSlots = NuevosObjetos.Count;
            var existente = NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == objeto.Nombre && o.Objeto.GetType() == objeto.GetType());
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                Console.WriteLine($"Se apiló {cantidad}x {objeto.Nombre} (Total: {existente.Cantidad})");
            }
            else
            {
                if (totalSlots >= CapacidadMaxima)
                {
                    Console.WriteLine("No se puede agregar más objetos. Inventario lleno.");
                    return;
                }
                NuevosObjetos.Add(new ObjetoConCantidad(objeto, cantidad));
                Console.WriteLine($"{objeto.Nombre} ha sido agregado al inventario.");
            }
            // Avisos automáticos si se pasa el personaje
            if (personaje != null)
                MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(personaje);
        }


        public void QuitarObjeto(Objeto objeto, int cantidad = 1)
        {
            var existente = NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == objeto.Nombre && o.Objeto.GetType() == objeto.GetType());
            if (existente != null)
            {
                if (existente.Cantidad > cantidad)
                {
                    existente.Cantidad -= cantidad;
                    Console.WriteLine($"Se quitaron {cantidad}x {objeto.Nombre} (Quedan: {existente.Cantidad})");
                }
                else
                {
                    NuevosObjetos.Remove(existente);
                    Console.WriteLine($"Se eliminó {objeto.Nombre} del inventario.");
                }
            }
            else
            {
                Console.WriteLine($"No tienes {objeto.Nombre} en el inventario.");
            }
        }

        public Inventario()
        {
            NuevosObjetos = new List<ObjetoConCantidad>();
        }

        public void MostrarInventario()
        {
            int totalObjetos = NuevosObjetos.Sum(o => o.Cantidad);
            // Peso total (por ahora 0, para implementar después)
            double pesoTotal = 0;
            Console.WriteLine($"Inventario: {totalObjetos} objetos (Peso: {pesoTotal} / --)");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"{ "Nombre",-20} { "Categoría",-12} { "Cantidad",-8}");
            Console.WriteLine("----------------------------------------");
            foreach (var objCant in NuevosObjetos)
            {
                Console.WriteLine($"{objCant.Objeto.Nombre,-20} {objCant.Objeto.Categoria,-12} {objCant.Cantidad,-8}");
            }
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Capacidad: {NuevosObjetos.Count}/{CapacidadMaxima}");
        }

        public int ContarMaterial(string nombreMaterial)
        {
            return NuevosObjetos
                .Where(o => o.Objeto is MiJuegoRPG.Objetos.Material && o.Objeto.Nombre == nombreMaterial)
                .Sum(o => o.Cantidad);
        }

        public void ConsumirMaterial(string nombreMaterial, int cantidad)
        {
            int restante = cantidad;
            foreach (var objCant in NuevosObjetos.Where(o => o.Objeto is MiJuegoRPG.Objetos.Material && o.Objeto.Nombre == nombreMaterial).ToList())
            {
                if (restante <= 0) break;
                if (objCant.Cantidad > restante)
                {
                    objCant.Cantidad -= restante;
                    restante = 0;
                }
                else
                {
                    restante -= objCant.Cantidad;
                    NuevosObjetos.Remove(objCant);
                }
            }
        }
    }
}
