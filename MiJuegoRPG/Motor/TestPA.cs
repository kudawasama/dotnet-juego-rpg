using System;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Pruebas ligeras (manuales) del cálculo de PA. Patrón similar a TestGeneradorObjetos.
    /// NOTA: No es un framework de tests formal aún.
    /// </summary>
    public static class TestPA
    {
        public static void Probar()
        {
            var cfg = CombatConfig.LoadOrDefault();
            Console.WriteLine("--- Test PA ---");

            // Caso básico (stats bajos)
            var p1 = new Personaje.Personaje("TesterA") { Nivel = 5 };
            p1.Atributos.Agilidad = 20; p1.Atributos.Destreza = 15;
            int pa1 = ActionPointService.ComputePA(p1, cfg);
            Console.WriteLine($"Caso A (bajo): PA={pa1} (esperado ~2)");

            // Caso rápido (tope PAMax)
            var p2 = new Personaje.Personaje("TesterB") { Nivel = 18 };
            p2.Atributos.Agilidad = 70; p2.Atributos.Destreza = 55;
            int pa2 = ActionPointService.ComputePA(p2, cfg);
            Console.WriteLine($"Caso B (rápido): PA={pa2} (esperado {cfg.PAMax})");

            // Edge (clamp superior)
            var p3 = new Personaje.Personaje("TesterC") { Nivel = 120 };
            p3.Atributos.Agilidad = 500; p3.Atributos.Destreza = 500;
            int pa3 = ActionPointService.ComputePA(p3, cfg);
            Console.WriteLine($"Caso C (clamp): PA={pa3} (esperado {cfg.PAMax})");
        }
    }
}
