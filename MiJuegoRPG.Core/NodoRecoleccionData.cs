using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class NodoRecoleccionData
    {
        public string? Nombre { get; set; }
        public List<MaterialEntrada>? Materiales { get; set; }
    }

    public class MaterialEntrada
    {
        public string? Nombre { get; set; }
        public int Cantidad { get; set; }
    }
}