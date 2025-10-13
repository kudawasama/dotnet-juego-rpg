using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class ArmaduraData
    {
        public required string Nombre
        {
            get; set;
        }

        public int Defensa
        {
            get; set;
        }

        public int Nivel
        {
            get; set;
        }

        public string? SetId
        {
            get; set;
        }

        // v2 opcional: soportar rango de nivel y defensa
        public int? NivelMin
        {
            get; set;
        }

        public int? NivelMax
        {
            get; set;
        }

        public int? DefensaMin
        {
            get; set;
        }

        public int? DefensaMax
        {
            get; set;
        }

        public required string TipoObjeto
        {
            get; set;
        }

        public string Rareza { get; set; } = "Comun";

        // v2 opcional: rarezas permitidas por ítem
        public string? RarezasPermitidasCsv
        {
            get; set;
        }

        public int Perfeccion { get; set; } = 50;

        // v2 opcional: rango de perfección
        public int? PerfeccionMin
        {
            get; set;
        }

        public int? PerfeccionMax
        {
            get; set;
        }

        // Habilidades y efectos opcionales otorgados por la pieza
        public List<EfectoData>? Efectos
        {
            get; set;
        }

        public List<HabilidadOtorgadaData>? HabilidadesOtorgadas
        {
            get; set;
        }

        // Metadatos opcionales
        public int? Valor
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

        public System.Collections.Generic.List<string>? Tags
        {
            get; set;
        }
    }
}
