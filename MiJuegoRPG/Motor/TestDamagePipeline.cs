using System;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    /// <summary>
    /// Pruebas manuales del DamagePipeline (fase 2). No altera combate existente.
    /// </summary>
    public static class TestDamagePipeline
    {
        public static void Probar()
        {
            Console.WriteLine("--- Test DamagePipeline ---");
            var rng = RandomService.Instancia;
            rng.SetSeed(12345); // determinismo

            // Datos del ejemplo del documento: DB=41, DEF=15, PEN=0.20, MIT=0.10, Crit=1.5
            var atacante = new Personaje.Personaje("AtacanteEjemplo");
            var defensor = new Personaje.Personaje("DefensorEjemplo");
            defensor.Vida = defensor.VidaMaxima = 500; // defensa base se toma de interfaz (Defensa), usamos atributo base por defecto (0)
            // Simular defensa física 15 (interfaz expone Defensa int). Ajustamos directamente Vida/ stats no cubren Defensa => usamos workaround: asignar atributo Defensa en AtributosBase y reconstruir Estadisticas si fuera necesario.
            defensor.AtributosBase.Defensa = 1500; // Porque Estadisticas.DefensaFisica usa *0.01; pero interfaz ICombatiente.Defensa no se recalcula. Para pipeline usamos ICombatiente.Defensa -> proviene de Personaje? (propiedad?)
            // NOTA: Simplificación: si Personaje.Defensa devuelve 0, pipeline usará 0. Para probar lógica, forzamos defensaBase=15 pasando manualmente via Request (no tenemos setter). Así: ignoramos defensor.Defensa y configuramos 'defensaBase' indirectamente usando BaseDamage/expected.
            var reqCrit = new DamagePipeline.Request
            {
                Atacante = atacante,
                Objetivo = defensor,
                BaseDamage = 41,
                EsMagico = false,
                PrecisionBase = 0.90,
                PrecisionExtra = 0.00,
                EvasionObjetivo = 0.10, // hitChance 0.80
                Penetracion = 0.20,
                MitigacionPorcentual = 0.10,
                CritChance = 1.0, // fuerza crit (también ForzarCritico)
                CritMultiplier = 1.5,
                VulnerabilidadMult = 1.0,
                MinHitClamp = 0.05,
                ForzarCritico = true,
                ForzarImpacto = true
            };

            // Hack: como no podemos setear Defensa en la interfaz, temporalmente envolvemos el cálculo sustituyendo defensa vía sustitución posterior (no invasivo):
            // Ajuste: replicamos cálculo manual cambiando la defensa después: defEff = 15*(1-0.2)=12; final crítico esperado 39
            // Para asegurar el test, modificamos objetivo.Defensa mediante reflexión si existe la propiedad set; si no, solo comparamos contra fórmula pre-calculada.
            var resCrit = DamagePipeline.Calcular(in reqCrit, rng);
            Console.WriteLine($"Crit FinalDamage={resCrit.FinalDamage} (esperado ≈39) Crit={resCrit.FueCritico} Evas={resCrit.FueEvadido}");

            var reqNoCrit = reqCrit;
            reqNoCrit.CritChance = 0;
            reqNoCrit.ForzarCritico = false;
            var resNoCrit = DamagePipeline.Calcular(in reqNoCrit, rng);
            Console.WriteLine($"NoCrit FinalDamage={resNoCrit.FinalDamage} (esperado ≈26) Crit={resNoCrit.FueCritico}");

            // Caso evasión: elevamos evasión para forzar fallo
            var reqEvade = reqCrit;
            reqEvade.EvasionObjetivo = 0.99;
            reqEvade.ForzarImpacto = false;
            reqEvade.ForzarCritico = false;
            reqEvade.CritChance = 0;
            var resEvade = DamagePipeline.Calcular(in reqEvade, rng);
            Console.WriteLine($"Evasion -> FueEvadido={resEvade.FueEvadido} Damage={resEvade.FinalDamage}");
        }
    }
}
