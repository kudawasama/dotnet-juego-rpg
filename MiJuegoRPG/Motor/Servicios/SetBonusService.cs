using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Carga definiciones de sets y calcula bonos según piezas equipadas.
    /// </summary>
    public class SetBonusService
    {
        public class SetDef
        {
            public string id { get; set; } = string.Empty;
            public MatchDef match { get; set; } = new();
            public List<ThresholdDef> thresholds { get; set; } = new();
        }
        public class MatchDef
        {
            public string? setId { get; set; }
            public string? nameContains { get; set; }
        }
        public class ThresholdDef
        {
            public int piezas { get; set; }
            public List<BonoDef>? bonos { get; set; }
            public List<HabilidadDef>? habilidades { get; set; }
        }
        public class BonoDef { public string estadistica { get; set; } = string.Empty; public double valor { get; set; } }
        public class HabilidadDef { public string id { get; set; } = string.Empty; public int nivelMinimo { get; set; } = 1; }

    private readonly List<SetDef> _sets = new();
        public static SetBonusService Instancia { get; } = new SetBonusService();

        private SetBonusService()
        {
            CargarSets();
        }

        public void CargarSets()
        {
            _sets.Clear();
            try
            {
                string dir = PathProvider.CombineData(Path.Combine("Equipo", "sets"));
                if (!Directory.Exists(dir)) return;
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var def = JsonSerializer.Deserialize<SetDef>(json, opts);
                        if (def != null) _sets.Add(def);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SetBonus] Ignorando '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetBonus] Error cargando sets: {ex.Message}");
            }
        }

        public (Dictionary<string,double> bonos, List<(string id,int nivel)> habilidades) CalcularBonosYHabilidades(IEnumerable<Objeto> equipados)
        {
            var bonos = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var habs = new List<(string id, int nivel)>();
            var lista = equipados.ToList();
            foreach (var set in _sets)
            {
                // Contar piezas que matchean este set
                int count = 0;
                foreach (var obj in lista)
                {
                    bool ok = false;
                    if (!string.IsNullOrWhiteSpace(set.match.setId) && !string.IsNullOrWhiteSpace(obj.SetId))
                    {
                        ok = string.Equals(obj.SetId, set.match.setId, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (!string.IsNullOrWhiteSpace(set.match.nameContains))
                    {
                        ok = obj.Nombre?.IndexOf(set.match.nameContains, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    if (ok) count++;
                }
                if (count <= 0) continue;
                foreach (var th in set.thresholds.OrderBy(t => t.piezas))
                {
                    if (count >= th.piezas)
                    {
                        if (th.bonos != null)
                        {
                            foreach (var b in th.bonos)
                            {
                                bonos.TryGetValue(b.estadistica, out var v);
                                bonos[b.estadistica] = v + b.valor;
                            }
                        }
                        if (th.habilidades != null)
                        {
                            foreach (var h in th.habilidades)
                            {
                                if (!string.IsNullOrWhiteSpace(h.id))
                                    habs.Add((h.id, Math.Max(1, h.nivelMinimo)));
                            }
                        }
                    }
                }
            }
            return (bonos, habs);
        }
    }
}
