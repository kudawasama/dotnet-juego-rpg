using System;
using System.Linq;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Pruebas manuales simples (no framework) para validar RarezaMeta y pipeline sombra.
    /// Ejecutar llamando TestRarezaMeta.Probar() desde un punto temporal (p.ej. Program) cuando se necesite.
    /// No se incluye en gameplay normal.
    /// </summary>
    public static class TestRarezaMeta
    {
        public static void Probar()
        {
            Console.WriteLine("[TestRarezaMeta] Iniciando...");
            if (RarezaConfig.Instancia == null)
            {
                Console.WriteLine("[TestRarezaMeta] RarezaConfig no inicializada. Abortando test.");
                return;
            }
            var cfg = RarezaConfig.Instancia;
            var metas = cfg.Metas.Values.ToList();
            if (metas.Count == 0)
            {
                Console.WriteLine("[TestRarezaMeta] Sin metas cargadas.");
                return;
            }

            // 1. Fallback rareza desconocida
            var desconocida = cfg.ObtenerMeta("NoExisteXYZ");
            if (desconocida.PerfMin == 50 && desconocida.PriceMult == 0.5)
                Console.WriteLine("[OK] Fallback desconocida aplica valores seguros.");
            else
                Console.WriteLine("[FAIL] Fallback desconocida no coincide con valores esperados.");

            // 2. Monotonía respecto a PerfAvg: ordenar por PerfAvg asc y pedir PriceMult no decreciente.
            var ordenPerf = metas.OrderBy(m => m.PerfAvg).ToList();
            bool ordenOk = true;
            for (int i = 1; i < ordenPerf.Count; i++)
            {
                var prev = ordenPerf[i - 1];
                var cur = ordenPerf[i];
                if (cur.PriceMult + 1e-6 < prev.PriceMult) // decremento real
                {
                    ordenOk = false;
                    Console.WriteLine($"[WARN] PriceMult decreciente: {prev.Nombre}->{cur.Nombre} {prev.PriceMult:0.000} -> {cur.PriceMult:0.000}");
                }
            }
            Console.WriteLine(ordenOk ? "[OK] PriceMult no decrece con PerfAvg." : "[WARN] Hallados descensos en PriceMult respecto a PerfAvg.");

            Console.WriteLine("[TestRarezaMeta] Fin.");
        }
    }
}
