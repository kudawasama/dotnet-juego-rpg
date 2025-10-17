// Implementación real para DelitosService
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios
{
    public class DelitosService
    {
        private readonly Dictionary<string, DelitoConfig> delitos = new();
        private readonly RandomService randomService;

        public DelitosService()
        {
            this.randomService = RandomService.Instancia;
        }

        public DelitosService(RandomService rng)
        {
            this.randomService = rng;
        }

        public void CargarDelitos()
        {
            // Buscar archivo en múltiples ubicaciones posibles
            var rutasPosibles = new[]
            {
                Path.Combine("DatosJuego", "config", "delitos.json"),
                Path.Combine("MiJuegoRPG", "DatosJuego", "config", "delitos.json"),
                Path.Combine("..", "MiJuegoRPG", "DatosJuego", "config", "delitos.json")
            };

            string? configPath = null;
            foreach (var ruta in rutasPosibles)
            {
                if (File.Exists(ruta))
                {
                    configPath = ruta;
                    break;
                }
            }

            if (configPath != null)
            {
                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<DelitosConfig>(json);
                if (config?.Delitos != null)
                {
                    foreach (var delito in config.Delitos)
                    {
                        delitos[delito.Key] = delito.Value;
                    }
                }
            }
        }

        public ResultadoDelito? AplicarDelito(string delitoId, MiJuegoRPG.Personaje.Personaje personaje)
        {
            if (!delitos.ContainsKey(delitoId)) return null;

            var delito = delitos[delitoId];

            // Determinar si la penalización es global o de facción
            var fac = delito.FaccionAfectada;
            bool aplicarGlobal = string.IsNullOrWhiteSpace(fac) || fac.Equals("ciudad", StringComparison.OrdinalIgnoreCase);
            if (aplicarGlobal)
            {
                // Penalización global
                personaje.Reputacion += delito.ReputacionPenalty;
            }
            else
            {
                // Penalización a facción específica
                if (!personaje.ReputacionesFaccion.ContainsKey(fac))
                {
                    personaje.ReputacionesFaccion[fac] = 0;
                }
                personaje.ReputacionesFaccion[fac] += delito.ReputacionPenalty;
            }

            // Aplicar multa aleatoria
            var multa = randomService.Next(delito.MultaMax - delito.MultaMin + 1) + delito.MultaMin;
            personaje.Oro = Math.Max(0, personaje.Oro - multa);

            return new ResultadoDelito
            {
                DelitoId = delitoId,
                AlertaCiudad = delito.ActivaAlerta,
                ReputacionCambiada = delito.ReputacionPenalty,
                MultaAplicada = multa
            };
        }

        public void AplicarConsecuencia(string delitoId)
        {
            // Método legacy - se mantiene por compatibilidad
        }
    }
}
