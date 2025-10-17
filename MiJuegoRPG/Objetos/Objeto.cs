using MiJuegoRPG.Personaje;
using System;
using System.Collections.Generic;


namespace MiJuegoRPG.Objetos
{
    public abstract class Objeto
    {
        public string Nombre
        {
            get; set;
        }
        public string Rareza
        {
            get; set;
        }
        public string Categoria
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets identificador opcional de set (para aplicar bonos por umbral de piezas equipadas).
        /// </summary>
        public string? SetId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets habilidades que este objeto otorga mientras está equipado.
        /// Se llenan desde los DTOs (HabilidadesOtorgadas) en el generador.
        /// </summary>
        public List<HabilidadOtorgadaRef>? HabilidadesOtorgadas
        {
            get; set;
        }

        public Objeto(string nombre, string rareza = "Normal", string categoria = "Otro")
        {
            Nombre = nombre;
            Rareza = rareza;
            Categoria = categoria;
        }

        public abstract void Usar(MiJuegoRPG.Personaje.Personaje personaje);
    }

    // SA1402: HabilidadOtorgadaRef se movió a HabilidadOtorgadaRef.cs para cumplir con SA1402 (un tipo por archivo)
}
