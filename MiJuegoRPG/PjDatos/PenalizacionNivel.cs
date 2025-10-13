using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class PenalizacionNivel
    {
        // Reducción porcentual por atributo base (0.05 = -5%)
        public Dictionary<string, double>? ReduccionAtributos
        {
            get; set;
        }
        // Factores acumulables por etiqueta para stats (negativos reducen)
        public double? Precision
        {
            get; set;
        }
        public double? Evasion
        {
            get; set;
        }
        // Opcional: si el JSON lo provee, penalización a regen de maná
        public double? ManaRegen
        {
            get; set;
        }
        // Otros campos existentes
        public double? MitigacionEnergia
        {
            get; set;
        }
    }
}