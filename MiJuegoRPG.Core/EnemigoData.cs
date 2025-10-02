using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class EnemigoData
    {
        public string Nombre { get; set; } = string.Empty;
        public int VidaBase { get; set; }
        public int AtaqueBase { get; set; }
        public int DefensaBase { get; set; }
        public int DefensaMagicaBase { get; set; }
        public int Nivel { get; set; }
        public int ExperienciaRecompensa { get; set; }
        public int OroRecompensa { get; set; }
        public Familia Familia { get; set; }
        public string Rareza { get; set; } = "Comun";
        public string Categoria { get; set; } = "Normal";
        public string ArmaNombre { get; set; } = string.Empty;
        public Dictionary<string, bool>? Inmunidades { get; set; }
        public double? MitigacionFisicaPorcentaje { get; set; }
        public double? MitigacionMagicaPorcentaje { get; set; }
        public List<string>? Tags { get; set; }
        public string? Id { get; set; }
        public double? SpawnChance { get; set; }
        public int? SpawnWeight { get; set; }
        public Dictionary<string, double>? ResistenciasElementales { get; set; }
        public Dictionary<string, double>? VulnerabilidadesElementales { get; set; }
        public Dictionary<string, int>? DanioElementalBase { get; set; }
        public EquipoInicialData? EquipoInicial { get; set; }
        public List<DropRuleData>? Drops { get; set; }
        public double? EvasionFisica { get; set; }
        public double? EvasionMagica { get; set; }
    }

    public class EquipoInicialData
    {
        public string? Arma { get; set; }
        public string? Armadura { get; set; }
        public string? Casco { get; set; }
        public string? Botas { get; set; }
        public string? Accesorio { get; set; }
    }

    public class DropRuleData
    {
        public string Tipo { get; set; } = "Material";
        public string Nombre { get; set; } = string.Empty;
        public string? Rareza { get; set; }
        public double Chance { get; set; } = 0.05;
        public int CantidadMin { get; set; } = 1;
        public int CantidadMax { get; set; } = 1;
        public bool UniqueOnce { get; set; } = false;
    }
}
