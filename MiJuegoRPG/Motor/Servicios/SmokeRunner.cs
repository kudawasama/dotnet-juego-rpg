namespace MiJuegoRPG.Motor.Servicios
{
    using System;
    using MiJuegoRPG.Enemigos;
    using MiJuegoRPG.Motor.Acciones;
    using PJ = MiJuegoRPG.Personaje;

    /// <summary>
    /// Ejecuta recorridos mínimos (smoke) para validar que los flujos base no crashean.
    /// No persiste nada ni entra a UI.
    /// </summary>
    public static class SmokeRunner
    {
        /// <summary>
        /// Crea un personaje básico, genera un enemigo estándar y ejecuta 1 ataque físico.
        /// Imprime un reporte compacto y retorna 0 si todo terminó sin excepción.
        /// </summary>
        /// <returns>
        /// 0 si el recorrido smoke finaliza correctamente; 1 si se captura alguna excepción.
        /// </returns>
        public static int RunCombateSmoke()
        {
            try
            {
                // Configuración determinista y verbosa (para inspección manual)
                RandomService.Instancia.SetSeed(12345);
                GameplayToggles.PrecisionCheckEnabled = false;
                GameplayToggles.PenetracionEnabled = false;
                GameplayToggles.CombatVerbose = true;

                // Personaje mínimo
                var pj = new PJ.Personaje("Smoke_Tester");

                // Asegurar estadísticas básicas inicializadas
                try
                {
                    pj.Estadisticas.Precision = 1.0; // evitar fallo por precisión si se activa accidentalmente
                }
                catch
                {
                    // Ignorar si el modelo ya inicializa estas propiedades
                }

                // Enemigo sencillo
                var enemigo = new EnemigoEstandar(
                    nombre: "Dummy",
                    vidaBase: 30,
                    ataqueBase: 3,
                    defensaBase: 1,
                    defensaMagicaBase: 0,
                    nivel: 1,
                    experienciaRecompensa: 0,
                    oroRecompensa: 0);

                int vidaAntes = enemigo.Vida;
                var accion = new AtaqueFisicoAccion();
                var res = accion.Ejecutar(pj, enemigo);
                int vidaDespues = enemigo.Vida;

                // Reporte compacto
                Console.WriteLine("=== SMOKE COMBATE ===");
                Console.WriteLine($"Atacante: {pj.Nombre}");
                Console.WriteLine($"Objetivo: {enemigo.Nombre} (HP {vidaAntes}->{vidaDespues})");
                Console.WriteLine($"DanioBase={res.DanioBase} DanioReal={res.DanioReal} Crit={res.FueCritico} Evasion={res.FueEvadido}");
                if (res.Mensajes != null && res.Mensajes.Count > 0)
                {
                    Console.WriteLine("Mensajes:");

                    // Mostrar hasta 3 líneas para no saturar
                    for (int i = 0; i < res.Mensajes.Count && i < 3; i++)
                    {
                        Console.WriteLine("- " + res.Mensajes[i]);
                    }
                }

                Console.WriteLine("Resultado: SMOKE OK");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Resultado: SMOKE FAIL");
                Console.WriteLine("Error: " + ex.Message);
                return 1;
            }
        }
    }
}
