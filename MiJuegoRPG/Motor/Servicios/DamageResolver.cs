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
            int vidaAntes = objetivo.Vida;
            // Paso 0 (opcional): Chequeo de precisión previa al ataque físico.
            // Si está activo el toggle global y el ejecutor es un Personaje, usamos su estadística de Precisión.
            // En caso de fallar, no delegamos al método de ataque y devolvemos un resultado con 0 daño.
            if (GameplayToggles.PrecisionCheckEnabled && ejecutor is MiJuegoRPG.Personaje.Personaje pjPrec)
            {
                double pHit = System.Math.Clamp(pjPrec.Estadisticas.Precision, 0.0, 0.95);
                var rng0 = RandomService.Instancia;
                bool acierta = rng0.NextDouble() < pHit;
                if (!acierta)
                {
                    var miss = new ResultadoAccion
                    {
                        NombreAccion = "Ataque Físico",
                        Ejecutor = ejecutor,
                        Objetivo = objetivo,
                        DanioBase = 0,
                        DanioReal = 0,
                        EsMagico = false,
                        ObjetivoDerrotado = !objetivo.EstaVivo,
                        FueEvadido = true,
                    };
                    miss.Mensajes.Add($"{ejecutor.Nombre} falla el Ataque Físico sobre {objetivo.Nombre} (precisión insuficiente).");
                    return miss;
                }
            }

            // Ejecuta el ataque físico según la lógica vigente. Si el toggle de penetración está activo
            // y el ejecutor es Personaje, propagamos su penetración al receptor mediante contexto ambiental.
            int danio;
            if (GameplayToggles.PenetracionEnabled && (ejecutor is MiJuegoRPG.Personaje.Personaje pjPen))
            {
                double pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danio = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarFisico(objetivo));
            }
            else
            {
                danio = ejecutor.AtacarFisico(objetivo);
            }
            int danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Físico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danioAplicado,
                EsMagico = false,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };

            // Si no hubo daño aplicado, interpretamos como evasión/fallo (o mitigación total);
            // más preciso que depender del valor retornado por AtacarFisico (que puede ser pre-defensas).
            res.FueEvadido = danioAplicado == 0;

            // Crítico (no intrusivo): si el ejecutor es Personaje, usar su estadística CritChance/Critico como probabilidad (0..1 aprox.)
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is MiJuegoRPG.Personaje.Personaje pj)
            {
                // Preferir CritChance si está disponible (>0); de lo contrario usar 'Critico' legacy.
                double raw = pj.Estadisticas.CritChance > 0 ? pj.Estadisticas.CritChance : pj.Estadisticas.Critico;
                // Clamp conservador: 0..0.95; si CritChance>=1.0, consideramos crítico forzado (útil para pruebas deterministas)
                pCrit = System.Math.Clamp(raw, 0.0, 0.95);
                if (raw >= 1.0)
                {
                    pCrit = 1.0; // fuerza crítico
                    forceCrit = true;
                }
            }
            var rng = RandomService.Instancia;
            bool fueCrit = danioAplicado > 0 && (forceCrit || rng.NextDouble() < pCrit);
            res.FueCritico = fueCrit;

            // Mensajes base (mantener compatibilidad con pruebas que revisan el primer mensaje)
            res.Mensajes.Add($"{ejecutor.Nombre} usa Ataque Físico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño.");
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

        /// <summary>
        /// Resuelve un ataque mágico delegando el cálculo principal al ejecutor.
        /// Mantiene el daño actual (no altera fórmulas), anota metadatos y mensajes.
        /// </summary>
        public ResultadoAccion ResolverAtaqueMagico(ICombatiente ejecutor, ICombatiente objetivo)
        {
            int vidaAntes = objetivo.Vida;

            // Ejecuta el ataque mágico según la lógica vigente; aplica el mismo mecanismo de penetración
            // para defensa mágica si el toggle está activo.
            int danio;
            if (GameplayToggles.PenetracionEnabled && (ejecutor is MiJuegoRPG.Personaje.Personaje pjPen))
            {
                double pen = System.Math.Clamp(pjPen.Estadisticas.Penetracion, 0.0, 0.9);
                danio = CombatAmbientContext.WithPenetracion(pen, () => ejecutor.AtacarMagico(objetivo));
            }
            else
            {
                danio = ejecutor.AtacarMagico(objetivo);
            }
            int danioAplicado = System.Math.Max(0, vidaAntes - objetivo.Vida);

            var res = new ResultadoAccion
            {
                NombreAccion = "Ataque Mágico",
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = danio,
                DanioReal = danioAplicado,
                EsMagico = true,
                ObjetivoDerrotado = !objetivo.EstaVivo,
            };

            // Si no hubo daño aplicado, interpretamos como evasión/fallo (o mitigación total)
            res.FueEvadido = danioAplicado == 0;

            // Crítico (no intrusivo) igual que en físico
            double pCrit = 0.0;
            bool forceCrit = false;
            if (ejecutor is MiJuegoRPG.Personaje.Personaje pj)
            {
                double raw = pj.Estadisticas.CritChance > 0 ? pj.Estadisticas.CritChance : pj.Estadisticas.Critico;
                pCrit = System.Math.Clamp(raw, 0.0, 0.95);
                if (raw >= 1.0)
                {
                    pCrit = 1.0; // fuerza crítico en pruebas
                    forceCrit = true;
                }
            }
            var rng = RandomService.Instancia;
            bool fueCrit = danioAplicado > 0 && (forceCrit || rng.NextDouble() < pCrit);
            res.FueCritico = fueCrit;

            // Mensajes
            res.Mensajes.Add($"{ejecutor.Nombre} lanza Ataque Mágico sobre {objetivo.Nombre} y causa {res.DanioReal} de daño mágico.");
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
