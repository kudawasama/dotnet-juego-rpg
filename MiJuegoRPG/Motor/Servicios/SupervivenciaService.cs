using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Capa de lectura y acceso a la configuración de supervivencia, data-driven.
    /// No aplica efectos todavía; solo expone datos y helpers básicos.
    /// </summary>
    public class SupervivenciaService
    {
        private SupervivenciaConfig _config = new();
        public SupervivenciaConfig Config => _config;

        public void CargarConfig()
        {
            var path = PathProvider.ConfigPath("supervivencia.json");
            if (!File.Exists(path))
            {
                // Tolerante: deja defaults si no existe
                return;
            }
            var json = File.ReadAllText(path);
            var opts = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
            var cfg = JsonSerializer.Deserialize<SupervivenciaConfig>(json, opts);
            if (cfg != null)
                _config = cfg;
        }

        public MultiplicadoresContexto ObtenerMultiplicadores(string contexto)
        {
            if (string.IsNullOrWhiteSpace(contexto)) return new MultiplicadoresContexto();
            return _config.MultiplicadoresPorContexto.TryGetValue(contexto, out var v) ? v : new MultiplicadoresContexto();
        }

        public ReglasBioma ObtenerReglasBioma(string bioma)
        {
            if (string.IsNullOrWhiteSpace(bioma)) return new ReglasBioma();
            return _config.ReglasPorBioma.TryGetValue(bioma, out var v) ? v : new ReglasBioma();
        }

        public (double umbralHambreWarn, double umbralSedWarn, double umbralFatigaWarn) ObtenerUmbralesAdvertencia()
        {
            var u = _config.Umbrales.Advertencia;
            return (u.Hambre, u.Sed, u.Fatiga);
        }

        public (double umbralHambreCrit, double umbralSedCrit, double umbralFatigaCrit) ObtenerUmbralesCriticos()
        {
            var u = _config.Umbrales.Critico;
            return (u.Hambre, u.Sed, u.Fatiga);
        }

        // Helpers UI/Runtime: etiquetas por umbrales y estado de temperatura
        public string EtiquetaDesdeUmbrales(double valor, double warn, double crit)
        {
            if (valor >= crit) return "CRÍTICO";
            if (valor >= warn) return "ADVERTENCIA";
            return "OK";
        }

        public (string etH, string etS, string etF) EtiquetasHSF(double hambre, double sed, double fatiga)
        {
            var (wH, wS, wF) = ObtenerUmbralesAdvertencia();
            var (cH, cS, cF) = ObtenerUmbralesCriticos();
            return (
                EtiquetaDesdeUmbrales(hambre, wH, cH),
                EtiquetaDesdeUmbrales(sed, wS, cS),
                EtiquetaDesdeUmbrales(fatiga, wF, cF)
            );
        }

        public string EstadoTemperatura(double t)
        {
            var adv = _config.Umbrales.Advertencia;
            var cri = _config.Umbrales.Critico;
            if (t <= cri.Frio) return "HIPOTERMIA";
            if (t < adv.Frio) return "FRÍO";
            if (t >= cri.Calor) return "GOLPE DE CALOR";
            if (t > adv.Calor) return "CALOR";
            return "CONFORT";
        }

        // ---------------- Factores de penalización (27.4) ----------------
        // Combina etiquetas H/S/F: si alguna está en CRÍTICO usa el peor (crítico), si no, si alguna en ADVERTENCIA, usa advertencia; de lo contrario 1.0
        private double FactorDesdeEtiquetas(string etH, string etS, string etF, Func<PjDatos.PenalizacionNivel, double?> selector)
        {
            var pen = _config.Penalizaciones;
            if (pen == null) return 1.0;
            bool anyCrit = etH == "CRÍTICO" || etS == "CRÍTICO" || etF == "CRÍTICO";
            bool anyWarn = etH == "ADVERTENCIA" || etS == "ADVERTENCIA" || etF == "ADVERTENCIA";
            if (anyCrit)
            {
                var v = selector(pen.Critico);
                return v.HasValue ? 1.0 + v.Value : 1.0;
            }
            if (anyWarn)
            {
                var v = selector(pen.Advertencia);
                return v.HasValue ? 1.0 + v.Value : 1.0;
            }
            return 1.0;
        }

        public double FactorEvasion(string etH, string etS, string etF)
            => FactorDesdeEtiquetas(etH, etS, etF, lvl => lvl.Evasion);

        public double FactorPrecision(string etH, string etS, string etF)
            => FactorDesdeEtiquetas(etH, etS, etF, lvl => lvl.Precision);

        public double FactorRegen(string etH, string etS, string etF)
            => FactorDesdeEtiquetas(etH, etS, etF, lvl => lvl.ManaRegen);
    }
}
