using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    // Clase para almacenar los datos de un arma.
    public class ArmaData
    {
        public required string Nombre
        {
            get; set;
        }

        public int Daño
        {
            get; set;
        }

        public int NivelRequerido
        {
            get; set;
        }

        public string? SetId
        {
            get; set;
        }

        // Alternativa a NivelRequerido: permitir rango de nivel
        public int? NivelMin
        {
            get; set;
        }

        public int? NivelMax
        {
            get; set;
        }

        public int Valor
        {
            get; set;
        }

        public required string Tipo
        {
            get; set;
        } // Ejemplo: "Espada", "Arco", "Bastón"

        public string Rareza { get; set; } = "Comun";

        // Permitir restringir rarezas por ítem (CSV o lista, se normaliza a CSV por compatibilidad)
        public string? RarezasPermitidasCsv
        {
            get; set;
        }

        // Perfección fija o rango
        public int Perfeccion { get; set; } = 50;

        public int? PerfeccionMin
        {
            get; set;
        }

        public int? PerfeccionMax
        {
            get; set;
        }

        // Daño por canales y/o rango (si no se usa, se toma "Daño" como base)
        public int? DañoMin
        {
            get; set;
        }

        public int? DañoMax
        {
            get; set;
        }

        public int? DañoFisico
        {
            get; set;
        }

        public int? DañoMagico
        {
            get; set;
        }

        public Dictionary<string, int>? DañoElemental
        {
            get; set;
        }

        // Crítico y atributos de combate
        public double? CriticoProbabilidad
        {
            get; set;
        }

        public double? CriticoMultiplicador
        {
            get; set;
        }

        public double? Penetracion
        {
            get; set;
        }

        public double? Precision
        {
            get; set;
        }

        public double? VelocidadAtaque
        {
            get; set;
        }

        // Bonificadores y stats
        public Dictionary<string, int>? BonificadoresAtributos
        {
            get; set;
        }

        public Dictionary<string, int>? BonificadoresEstadisticas
        {
            get; set;
        }

        // Efectos y habilidades
        public List<EfectoData>? Efectos
        {
            get; set;
        }

        public List<HabilidadOtorgadaData>? HabilidadesOtorgadas
        {
            get; set;
        }

        // Requisitos, economía y metadatos
        public Dictionary<string, int>? Requisitos
        {
            get; set;
        } // p.ej. {"Fuerza":5, "Nivel":2}

        public int? ValorCompra
        {
            get; set;
        }

        public int? ValorVenta
        {
            get; set;
        }

        public double? Peso
        {
            get; set;
        }

        public int? Durabilidad
        {
            get; set;
        }

        public string? Descripcion
        {
            get; set;
        }

        public List<string>? Tags
        {
            get; set;
        }
    }
}
