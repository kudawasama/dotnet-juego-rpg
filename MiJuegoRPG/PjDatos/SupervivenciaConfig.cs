using System.Collections.Generic;

namespace MiJuegoRPG.PjDatos
{
    public class SupervivenciaConfig
    {
        public TasasConfig Tasas { get; set; } = new TasasConfig();
        public Dictionary<string, MultiplicadoresContexto> MultiplicadoresPorContexto { get; set; } = new();
        public UmbralesConfig Umbrales { get; set; } = new UmbralesConfig();
        public Dictionary<string, ReglasBioma> ReglasPorBioma { get; set; } = new();
        public Dictionary<string, BonoRefugio> BonosRefugio { get; set; } = new();
        public ConsumoConfig Consumo { get; set; } = new ConsumoConfig();
        // NUEVO: Penalizaciones por umbral (advertencia/crítico)
        public PenalizacionesConfig Penalizaciones { get; set; } = new PenalizacionesConfig();
    }

    public class TasasConfig
    {
        public double HambrePorHora { get; set; }
        public double SedPorHora { get; set; }
        public double FatigaPorHora { get; set; }
        public double TempRecuperacionPorHora { get; set; }
    }

    public class MultiplicadoresContexto
    {
        public double Hambre { get; set; } = 1.0;
        public double Sed { get; set; } = 1.0;
        public double Fatiga { get; set; } = 1.0;
    }

    public class UmbralesConfig
    {
        public UmbralValores Advertencia { get; set; } = new UmbralValores();
        public UmbralValores Critico { get; set; } = new UmbralValores();
    }

    public class UmbralValores
    {
        public double Hambre { get; set; }
        public double Sed { get; set; }
        public double Fatiga { get; set; }
        public double Frio { get; set; }
        public double Calor { get; set; }
    }

    public class ReglasBioma
    {
        public double TempDia { get; set; }
        public double TempNoche { get; set; }
        public double SedMultiplier { get; set; } = 1.0;
        public double HambreMultiplier { get; set; } = 1.0;
        public double FatigaMultiplier { get; set; } = 1.0;
    }

    public class BonoRefugio
    {
        public double TempPlus { get; set; }
        public double FatigaRecuperacion { get; set; }
    }

    public class ConsumoConfig
    {
        public ComidaAguaReduce Comida { get; set; } = new ComidaAguaReduce();
        public ComidaAguaReduce Agua { get; set; } = new ComidaAguaReduce();
        public FatigaReduce Descanso { get; set; } = new FatigaReduce();
        public MedicoReduce Medico { get; set; } = new MedicoReduce();
    }

    public class ComidaAguaReduce
    {
        public double HambreReduce { get; set; }
        public double SedReduce { get; set; }
    }

    public class FatigaReduce
    {
        // La propiedad no puede llamarse igual que la clase; mapeamos el nombre JSON para compatibilidad
        [System.Text.Json.Serialization.JsonPropertyName("FatigaReduce")]
        public double Valor { get; set; }
    }

    public class MedicoReduce
    {
        public bool SangradoReduce { get; set; }
        public bool InfeccionReduce { get; set; }
    }

    // ---------------- Penalizaciones ----------------
    public class PenalizacionesConfig
    {
        public PenalizacionNivel Advertencia { get; set; } = new PenalizacionNivel();
        public PenalizacionNivel Critico { get; set; } = new PenalizacionNivel();
    }

    public class PenalizacionNivel
    {
        // Reducción porcentual por atributo base (0.05 = -5%)
        public Dictionary<string, double>? ReduccionAtributos { get; set; }
        // Factores acumulables por etiqueta para stats (negativos reducen)
        public double? Precision { get; set; }
        public double? Evasion { get; set; }
        // Opcional: si el JSON lo provee, penalización a regen de maná
        public double? ManaRegen { get; set; }
        // Otros campos existentes
        public double? MitigacionEnergia { get; set; }
    }
}
