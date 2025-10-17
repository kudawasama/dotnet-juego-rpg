namespace MiJuegoRPG.PjDatos
{
    public class AccesorioData
    {
        public required string Nombre
        {
            get; set;
        }

        public int BonificacionAtaque
        {
            get; set;
        }

        public int BonificacionDefensa
        {
            get; set;
        }

        public string? SetId
        {
            get; set;
        }

        // Permite tanto un "Nivel" plano como un rango NivelMin/NivelMax
        public int Nivel
        {
            get; set;
        }

        public int? NivelMin
        {
            get; set;
        }

        public int? NivelMax
        {
            get; set;
        }

        public required string TipoObjeto
        {
            get; set;
        }

        // Rareza puede venir como string simple o lista CSV a través de RarezasPermitidasCsv
        public string Rareza { get; set; } = "Comun";

        public string? RarezasPermitidasCsv
        {
            get; set;
        }

        // Perfeccion puede venir en un rango PerfeccionMin/PerfeccionMax o valor fijo
        public int Perfeccion { get; set; } = 50;

        public int? PerfeccionMin
        {
            get; set;
        }

        public int? PerfeccionMax
        {
            get; set;
        }

        public string? Descripcion
        {
            get; set;
        }
    }
}
