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
            public string Id { get; set; } = string.Empty;
            public MatchDef Match { get; set; } = new();
            public List<ThresholdDef> Thresholds { get; set; } = new();
        }
        public class MatchDef
        {
            public string? SetId
            {
                get; set;
            }
            public string? NameContains
            {
                get; set;
            }
        }
        public class ThresholdDef
        {
            public int Piezas
            {
                get; set;
            }
            public List<BonoDef>? Bonos
            {
                get; set;
            }
            public List<HabilidadDef>? Habilidades
            {
                get; set;
            }
        }
        public class BonoDef
        {
            public string Estadistica { get; set; } = string.Empty; public double Valor
            {
                get; set;
            }
        }
        public class HabilidadDef
        {
            public string Id { get; set; } = string.Empty; public int NivelMinimo { get; set; } = 1;
        }

        private readonly List<SetDef> sets = new();
        public static SetBonusService Instancia { get; } = new SetBonusService();

        private SetBonusService()
        {
            CargarSets();
        }

        public void CargarSets()
        {
            sets.Clear();
            try
            {
                string dir = PathProvider.CombineData(Path.Combine("Equipo", "sets"));
                if (!Directory.Exists(dir))
                    return;
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var def = JsonSerializer.Deserialize<SetDef>(json, opts);
                        if (def != null)
                            sets.Add(def);
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

        public (Dictionary<string, double> bonos, List<(string id, int nivel)> habilidades) CalcularBonosYHabilidades(IEnumerable<Objeto> equipados)
        {
            var bonos = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var habs = new List<(string id, int nivel)>();
            var lista = equipados.ToList();
            foreach (var set in sets)
            {
                // Contar piezas que matchean este set
                int count = 0;
                foreach (var obj in lista)
                {
                    bool ok = false;
                    if (!string.IsNullOrWhiteSpace(set.Match.SetId) && !string.IsNullOrWhiteSpace(obj.SetId))
                    {
                        ok = string.Equals(obj.SetId, set.Match.SetId, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (!string.IsNullOrWhiteSpace(set.Match.NameContains))
                    {
                        ok = obj.Nombre?.IndexOf(set.Match.NameContains, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    if (ok)
                        count++;
                }
                if (count <= 0)
                    continue;
                foreach (var th in set.Thresholds.OrderBy(t => t.Piezas))
                {
                    if (count >= th.Piezas)
                    {
                        if (th.Bonos != null)
                        {
                            foreach (var b in th.Bonos)
                            {
                                bonos.TryGetValue(b.Estadistica, out var v);
                                bonos[b.Estadistica] = v + b.Valor;
                            }
                        }
                        if (th.Habilidades != null)
                        {
                            foreach (var h in th.Habilidades)
                            {
                                if (!string.IsNullOrWhiteSpace(h.Id))
                                    habs.Add((h.Id, Math.Max(1, h.NivelMinimo)));
                            }
                        }
                    }
                }
            }
            return (bonos, habs);
        }
    }
}
