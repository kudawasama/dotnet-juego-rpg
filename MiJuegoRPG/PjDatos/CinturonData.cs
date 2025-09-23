using System.Collections.Generic;
namespace MiJuegoRPG.PjDatos
{
    public class CinturonData
    {
        public required string Nombre { get; set; }
        public int BonificacionCarga { get; set; }
        public int Nivel { get; set; }
        public string? SetId { get; set; }
        public int? NivelMin { get; set; }
        public int? NivelMax { get; set; }
        public int? BonificacionCargaMin { get; set; }
        public int? BonificacionCargaMax { get; set; }
        public required string TipoObjeto { get; set; }
        public string Rareza { get; set; } = "Comun";
        public string? RarezasPermitidasCsv { get; set; }
        public int Perfeccion { get; set; } = 50;
        public int? PerfeccionMin { get; set; }
        public int? PerfeccionMax { get; set; }
        public List<EfectoData>? Efectos { get; set; }
        public List<HabilidadOtorgadaData>? HabilidadesOtorgadas { get; set; }
        public int? Valor { get; set; }
        public int? ValorVenta { get; set; }
        public double? Peso { get; set; }
        public int? Durabilidad { get; set; }
        public string? Descripcion { get; set; }
        public System.Collections.Generic.List<string>? Tags { get; set; }
    }
}
