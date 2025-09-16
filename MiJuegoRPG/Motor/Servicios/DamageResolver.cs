using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Resolver de daño mínimo: delega el cálculo principal al método existente del ejecutor
    /// (AtacarFisico/AtacarMagico) para no romper fórmulas actuales, y añade metadatos (crítico)
    /// y mensajes complementarios de forma no intrusiva.
    /// </summary>
    public class DamageResolver
    {
        /// <summary>
        /// Resuelve un ataque físico aprovechando la lógica existente del ejecutor.
        /// No modifica el daño retornado actualmente; solo anota si fue crítico.
        /// </summary>
        public ResultadoAccion ResolverAtaqueFisico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            // Ejecuta el ataque físico según la lógica vigente (incluye evasión/defensas actuales)
            int danio = ejecutor.AtacarFisico(objetivo);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Físico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danio,
                EsMagico = false,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };

            // Si el daño retornado es 0, en la implementación actual de Personaje/Enemigo significa que el objetivo evadió
            // (el cálculo de daño bruto siempre es > 0 salvo evasión previa al impacto)
            res.FueEvadido = danio == 0;

            // Crítico (no intrusivo): si el ejecutor es Personaje, usar su estadística Critico como probabilidad (0..1 aprox.)
            double pCrit = 0.0;
            if (ejecutor is MiJuegoRPG.Personaje.Personaje pj)
            {
                // La estadística 'Critico' ya existe; clamp conservador para no romper balance
                pCrit = System.Math.Clamp(pj.Estadisticas.Critico, 0.0, 0.5);
            }
            var rng = RandomService.Instancia;
            bool fueCrit = danio > 0 && rng.NextDouble() < pCrit;
            res.FueCritico = fueCrit;

            // Mensajes base (mantener compatibilidad con pruebas que revisan el primer mensaje)
            res.Mensajes.Add($"{ejecutor.Nombre} usa Ataque Físico sobre {objetivo.Nombre} y causa {danio} de daño.");
            if (res.FueEvadido)
            {
                res.Mensajes.Add("¡El objetivo evadió el ataque!");
            }
            else if (fueCrit)
            {
                res.Mensajes.Add("¡Golpe crítico!");
            }

            return res;
        }
    }
}
