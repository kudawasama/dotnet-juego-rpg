using System;
using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    /// <summary>
    /// Clase para manejar múltiples personajes.
    /// </summary>
    public class DatosGuardado
    {
        public List<MiJuegoRPG.Personaje.Personaje> Personajes { get; set; } = new List<MiJuegoRPG.Personaje.Personaje>();
        public DateTime FechaUltimaModificacion { get; set; } = DateTime.Now;
    }
}