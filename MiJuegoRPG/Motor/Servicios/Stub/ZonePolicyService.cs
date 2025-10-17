// Implementación real para ZonePolicyService
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ZonePolicyService
    {
        private readonly Dictionary<string, Dictionary<string, PoliticaZonaDto>> politicas = new();

        public void CargarPoliticas()
        {
            // Buscar archivo en múltiples ubicaciones posibles
            var rutasPosibles = new[]
            {
                Path.Combine("DatosJuego", "config", "zonas_politicas.json"),
                Path.Combine("MiJuegoRPG", "DatosJuego", "config", "zonas_politicas.json"),
                Path.Combine("..", "MiJuegoRPG", "DatosJuego", "config", "zonas_politicas.json")
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
                var config = JsonSerializer.Deserialize<ZonaPoliticaConfig>(json);
                if (config?.Zonas != null)
                {
                    foreach (var zona in config.Zonas)
                    {
                        politicas[zona.Key] = zona.Value;
                    }
                }
            }
        }

        public PoliticaZonaDto ObtenerPolitica(string zona, string accion)
        {
            if (politicas.ContainsKey(zona) && politicas[zona].ContainsKey(accion))
            {
                return politicas[zona][accion];
            }

            // Fallback seguro
            return new PoliticaZonaDto { Permitido = true, DelitoId = null, Risky = false };
        }

        public bool TieneZona(string zona)
        {
            return politicas.ContainsKey(zona);
        }
    }
}
