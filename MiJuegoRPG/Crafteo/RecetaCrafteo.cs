using System.Collections.Generic;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Crafteo
{
    public class RecetaCrafteo
    {
        public required string Nombre
        {
            get; set;
        }
        public List<(string nombreMaterial, int cantidad)> Materiales { get; set; } = new();
        public required Objeto ObjetoResultado
        {
            get; set;
        }
    }
}