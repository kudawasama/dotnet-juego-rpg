using MiJuegoRPG.Personaje;
using System;
using System.Collections.Generic;


namespace MiJuegoRPG.Objetos
{
    public abstract class Objeto
    {
        public string Nombre { get; set; }
        public string Rareza { get; set; }
        public string Categoria { get; set; }
        /// <summary>
        /// Identificador opcional de set (para aplicar bonos por umbral de piezas equipadas).
        /// </summary>
        public string? SetId { get; set; }
        /// <summary>
        /// Habilidades que este objeto otorga mientras está equipado.
        /// Se llenan desde los DTOs (HabilidadesOtorgadas) en el generador.
        /// </summary>
        public List<HabilidadOtorgadaRef>? HabilidadesOtorgadas { get; set; }

        public Objeto(string nombre, string rareza = "Normal", string categoria = "Otro")
        {
            Nombre = nombre;
            Rareza = rareza;
            Categoria = categoria;
        }

        public abstract void Usar(MiJuegoRPG.Personaje.Personaje personaje);
    }

    /// <summary>
    /// Referencia ligera a una habilidad otorgada por un equipo.
    /// </summary>
    public class HabilidadOtorgadaRef
    {
        public string Id { get; set; } = string.Empty;
        public int NivelMinimo { get; set; } = 1;
    }
}
