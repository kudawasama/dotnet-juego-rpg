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

    // SA1402: TasasConfig se movió a TasasConfig.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: MultiplicadoresContexto se movió a MultiplicadoresContexto.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: UmbralesConfig se movió a UmbralesConfig.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: UmbralValores se movió a UmbralesConfig.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ReglasBioma se movió a ReglasBioma.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: BonoRefugio se movió a BonoRefugio.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: ConsumoConfig se movió a ConsumoConfig.cs para cumplir con SA1402 (un tipo por archivo)

    // SA1402: ComidaAguaReduce se movió a ComidaAguaReduce.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: FatigaReduce se movió a FatigaReduce.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: MedicoReduce se movió a MedicoReduce.cs para cumplir con SA1402 (un tipo por archivo)

    // ---------------- Penalizaciones ----------------
    // SA1402: PenalizacionesConfig se movió a PenalizacionesConfig.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: PenalizacionNivel se movió a PenalizacionNivel.cs para cumplir con SA1402 (un tipo por archivo)
}
