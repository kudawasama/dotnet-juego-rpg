using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PersonajeModel = MiJuegoRPG.Personaje.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    // Tipos base de encuentros durante exploración
    public enum TipoEncuentro
    {
        Nada,
        BotinComun,
        Materiales,
        NPC,
        CombateComunes,
        CombateBioma,
        MiniJefe,
        MazmorraRara,
        EventoRaro
    }

    public class EntradaEncuentro
    {
        public TipoEncuentro Tipo
        {
            get; set;
        }
        public int Peso
        {
            get; set;
        } // peso relativo
        // Parámetros opcionales (p. ej., subtipos: "raton","lobo","cofre_pequeno")
        public string? Param
        {
            get; set;
        }
        // Requisitos suaves (nivel mínimo, killcount, hora del día, reputación, etc.)
        public int? MinNivel
        {
            get; set;
        }
        public int? MinKills
        {
            get; set;
        }
        // Franja horaria opcional [0-23]. Si ambos null → sin restricción.
        // Si HoraMin <= HoraMax: ventana directa (p.ej., 6..18). Si cruza medianoche (HoraMin > HoraMax): válido si hora >= HoraMin o hora <= HoraMax.
        public int? HoraMin
        {
            get; set;
        }
        public int? HoraMax
        {
            get; set;
        }
        // Probabilidad directa de activación (0..1). Si se define, se evalúa primero y, si pasa el roll, se prioriza este encuentro.
        public double? Chance
        {
            get; set;
        }
        // Prioridad opcional para resolver colisiones entre múltiples entradas con Chance. Mayor → más prioridad.
        public int? Prioridad
        {
            get; set;
        }
        // Cooldown opcional en minutos: si se define, tras activarse este encuentro no puede volver a salir hasta que pase el tiempo indicado.
        public int? CooldownMinutos
        {
            get; set;
        }
    }

    public class TablaEncuentrosBioma
    {
        public string Bioma { get; set; } = string.Empty;
        public List<EntradaEncuentro> Entradas { get; set; } = new();
    }

    public class EncuentroResuelto
    {
        public TipoEncuentro Tipo
        {
            get; set;
        }
        public string? Param
        {
            get; set;
        }
    }

    public class EncuentrosService
    {
        private readonly Dictionary<string, TablaEncuentrosBioma> tablas = new(StringComparer.OrdinalIgnoreCase);
        private readonly RandomService rng = RandomService.Instancia;
        // Para pruebas/QA: permite inyectar la hora actual sin depender de Juego/DateTime.Now
        public Func<int>? HoraActualProvider
        {
            get; set;
        }
        // Para pruebas/QA: permite inyectar la fecha/hora actual completa para evaluar cooldowns
        public Func<DateTime>? FechaActualProvider
        {
            get; set;
        }

        // Registro de último disparo por clave (bioma|tipo|param)
        private readonly Dictionary<string, DateTime> ultimoDisparo = new();

        private int ObtenerHoraActual()
        {
            if (HoraActualProvider != null)
                return HoraActualProvider.Invoke();
            return MiJuegoRPG.Motor.Juego.InstanciaActual?.FechaActual.Hour ?? DateTime.Now.Hour;
        }

        private DateTime ObtenerFechaActual()
        {
            if (FechaActualProvider != null)
                return FechaActualProvider.Invoke();
            return MiJuegoRPG.Motor.Juego.InstanciaActual?.FechaActual ?? DateTime.Now;
        }

        private static string ClaveEncuentro(string bioma, EntradaEncuentro e)
        {
            var p = e.Param ?? string.Empty;
            return $"{bioma}|{e.Tipo}|{p}";
        }

        private bool EstaEnCooldown(string bioma, EntradaEncuentro e, DateTime ahora)
        {
            if (e.CooldownMinutos == null || e.CooldownMinutos!.Value <= 0)
                return false;
            var clave = ClaveEncuentro(bioma, e);
            if (ultimoDisparo.TryGetValue(clave, out var t))
            {
                var dt = ahora - t;
                return dt.TotalMinutes < e.CooldownMinutos!.Value;
            }
            return false;
        }

        private void RegistrarDisparo(string bioma, EntradaEncuentro e, DateTime ahora)
        {
            if (e.CooldownMinutos == null || e.CooldownMinutos!.Value <= 0)
                return;
            var clave = ClaveEncuentro(bioma, e);
            ultimoDisparo[clave] = ahora;
        }

        // Persistencia ligera (para guardado/carga): exportar/importar registro de últimos disparos
        public Dictionary<string, DateTime> ExportarCooldowns()
        {
            return new Dictionary<string, DateTime>(ultimoDisparo);
        }

        public void ImportarCooldowns(Dictionary<string, DateTime> data)
        {
            ultimoDisparo.Clear();
            if (data == null)
                return;
            foreach (var kv in data)
            {
                ultimoDisparo[kv.Key] = kv.Value;
            }
        }

        // DTO para inspección desde herramientas/admin
        public class EncuentroCooldownEstado
        {
            public string Clave { get; set; } = string.Empty; // bioma|tipo|param
            public string Bioma { get; set; } = string.Empty;
            public TipoEncuentro Tipo
            {
                get; set;
            }
            public string? Param
            {
                get; set;
            }
            public DateTime UltimoDisparo
            {
                get; set;
            }
            public int? CooldownMinutos
            {
                get; set;
            }
            public double RestanteMinutos
            {
                get; set;
            }
        }

        // Listado de cooldowns con restante calculado según tablas cargadas
        public List<EncuentroCooldownEstado> ObtenerEstadoCooldowns()
        {
            var ahora = ObtenerFechaActual();
            var lista = new List<EncuentroCooldownEstado>();
            foreach (var kv in ultimoDisparo)
            {
                var partes = kv.Key.Split('|');
                string bioma = partes.Length > 0 ? partes[0] : string.Empty;
                string tipoStr = partes.Length > 1 ? partes[1] : string.Empty;
                string? param = partes.Length > 2 ? partes[2] : null;
                TipoEncuentro tipo = Enum.TryParse<TipoEncuentro>(tipoStr, true, out var t) ? t : TipoEncuentro.Nada;
                int? cd = null;
                if (tablas.TryGetValue(bioma, out var tabla))
                {
                    var e = tabla.Entradas.FirstOrDefault(x => x.Tipo == tipo && string.Equals(x.Param ?? string.Empty, param ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                    cd = e?.CooldownMinutos;
                }
                double restante = 0;
                if (cd.HasValue && cd.Value > 0)
                {
                    var dt = ahora - kv.Value;
                    restante = Math.Max(0.0, cd.Value - dt.TotalMinutes);
                }
                lista.Add(new EncuentroCooldownEstado
                {
                    Clave = kv.Key,
                    Bioma = bioma,
                    Tipo = tipo,
                    Param = param,
                    UltimoDisparo = kv.Value,
                    CooldownMinutos = cd,
                    RestanteMinutos = restante
                });
            }
            return lista.OrderByDescending(x => x.RestanteMinutos).ThenBy(x => x.Clave, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // Limpia cooldowns: si soloVencidos=true, solo los que ya no tienen restante
        public int LimpiarCooldowns(bool soloVencidos)
        {
            var ahora = ObtenerFechaActual();
            int removidos = 0;
            if (!soloVencidos)
            {
                removidos = ultimoDisparo.Count;
                ultimoDisparo.Clear();
                return removidos;
            }
            var claves = ultimoDisparo.Keys.ToList();
            foreach (var clave in claves)
            {
                var partes = clave.Split('|');
                string bioma = partes.Length > 0 ? partes[0] : string.Empty;
                string tipoStr = partes.Length > 1 ? partes[1] : string.Empty;
                string? param = partes.Length > 2 ? partes[2] : null;
                TipoEncuentro tipo = Enum.TryParse<TipoEncuentro>(tipoStr, true, out var t) ? t : TipoEncuentro.Nada;
                int? cd = null;
                if (tablas.TryGetValue(bioma, out var tabla))
                {
                    var e = tabla.Entradas.FirstOrDefault(x => x.Tipo == tipo && string.Equals(x.Param ?? string.Empty, param ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                    cd = e?.CooldownMinutos;
                }
                if (!cd.HasValue || cd.Value <= 0)
                {
                    // Si no hay cooldown configurado, considerar no vigente → limpiar
                    ultimoDisparo.Remove(clave);
                    removidos++;
                }
                else
                {
                    var t0 = ultimoDisparo[clave];
                    var dt = ahora - t0;
                    if (dt.TotalMinutes >= cd.Value)
                    {
                        ultimoDisparo.Remove(clave);
                        removidos++;
                    }
                }
            }
            return removidos;
        }

        public void RegistrarTabla(TablaEncuentrosBioma tabla)
        {
            tablas[tabla.Bioma] = tabla;
        }

        public bool CargarDesdeJsonPorDefecto()
        {
            try
            {
                var ruta = PathProvider.CombineData("eventos", "encuentros.json");
                if (!File.Exists(ruta))
                    return false;
                var json = File.ReadAllText(ruta);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                opts.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var tablas = JsonSerializer.Deserialize<List<TablaEncuentrosBioma>>(json, opts) ?? new List<TablaEncuentrosBioma>();
                this.tablas.Clear();
                foreach (var t in tablas)
                    RegistrarTabla(t);
                Logger.Info($"[Encuentros] Tablas cargadas desde JSON ({this.tablas.Count} biomas). Ruta: {ruta}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Encuentros] Error cargando JSON: {ex.Message}");
                return false;
            }
        }

        public void RegistrarTablasPorDefecto()
        {
            // Pesos pensados para progresión lenta: comunes muy probables, raros escasos
            RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "Bosque",
                Entradas = new List<EntradaEncuentro>
                {
                    new() { Tipo = TipoEncuentro.Nada, Peso = 25 },
                    new() { Tipo = TipoEncuentro.BotinComun, Peso = 12, Param = "cofre_pequeno" },
                    new() { Tipo = TipoEncuentro.Materiales, Peso = 18 },
                    new() { Tipo = TipoEncuentro.NPC, Peso = 5 },
                    new() { Tipo = TipoEncuentro.CombateComunes, Peso = 20, Param = "rata|conejo" },
                    new() { Tipo = TipoEncuentro.CombateBioma, Peso = 15, Param = "lobo" },
                    new() { Tipo = TipoEncuentro.MiniJefe, Peso = 4, Param = "lider_manada:lobo", MinKills = 12 },
                    new() { Tipo = TipoEncuentro.MazmorraRara, Peso = 1 },
                    new() { Tipo = TipoEncuentro.EventoRaro, Peso = 0 }
                }
            });

            RegistrarTabla(new TablaEncuentrosBioma
            {
                Bioma = "Montaña",
                Entradas = new List<EntradaEncuentro>
                {
                    new() { Tipo = TipoEncuentro.Nada, Peso = 20 },
                    new() { Tipo = TipoEncuentro.BotinComun, Peso = 8, Param = "cofre_piedra" },
                    new() { Tipo = TipoEncuentro.Materiales, Peso = 22 },
                    new() { Tipo = TipoEncuentro.NPC, Peso = 3 },
                    new() { Tipo = TipoEncuentro.CombateComunes, Peso = 18, Param = "rata_cueva|murcielago" },
                    new() { Tipo = TipoEncuentro.CombateBioma, Peso = 20, Param = "lobo_montana|golem_pequeno" },
                    new() { Tipo = TipoEncuentro.MiniJefe, Peso = 3, Param = "alfa_lobo", MinKills = 10 },
                    new() { Tipo = TipoEncuentro.MazmorraRara, Peso = 2 },
                    new() { Tipo = TipoEncuentro.EventoRaro, Peso = 0 }
                }
            });
        }

        // Resolver clásico (compatibilidad): sin stats del jugador
        public EncuentroResuelto Resolver(string bioma, int nivelJugador, Func<string, int?> getKillCount)
        {
            if (!tablas.TryGetValue(bioma, out var tabla))
            {
                // Fallback si no hay tabla: preferir comunes
                tabla = new TablaEncuentrosBioma
                {
                    Bioma = bioma,
                    Entradas = new List<EntradaEncuentro>
                    {
                        new() { Tipo = TipoEncuentro.Materiales, Peso = 40 },
                        new() { Tipo = TipoEncuentro.CombateComunes, Peso = 30, Param = "rata" },
                        new() { Tipo = TipoEncuentro.CombateBioma, Peso = 20 },
                        new() { Tipo = TipoEncuentro.MazmorraRara, Peso = 1 },
                        new() { Tipo = TipoEncuentro.BotinComun, Peso = 9 }
                    }
                };
            }

            var hora = ObtenerHoraActual();
            var ahora = ObtenerFechaActual();
            var candidatas = tabla.Entradas.Where(e => (e.MinNivel == null || nivelJugador >= e.MinNivel)).ToList();
            // Filtrar por kills si aplica
            candidatas = candidatas.Where(e => e.MinKills == null || (e.Param != null && getKillCount(e.Param.Split(':').Last()) >= e.MinKills)).ToList();
            // Filtrar por hora si aplica
            candidatas = candidatas.Where(e => CumpleHora(e, hora)).ToList();
            // Filtrar por cooldown si aplica
            candidatas = candidatas.Where(e => !EstaEnCooldown(bioma, e, ahora)).ToList();

            // 1) Evaluar entradas con Chance (si pasa el roll, gana). Si hay varias que pasan, usar Prioridad y luego Peso como desempate.
            var conChance = candidatas.Where(e => e.Chance != null).ToList();
            var pasaron = new List<EntradaEncuentro>();
            foreach (var e in conChance)
            {
                double p = Math.Clamp(e.Chance!.Value, 0.0, 1.0);
                if (rng.NextDouble() < p)
                    pasaron.Add(e);
            }
            if (pasaron.Count > 0)
            {
                var elegido = pasaron
                    .OrderByDescending(e => e.Prioridad ?? 0)
                    .ThenByDescending(e => Math.Max(0, e.Peso))
                    .First();
                RegistrarDisparo(bioma, elegido, ahora);
                return new EncuentroResuelto { Tipo = elegido.Tipo, Param = elegido.Param };
            }

            // 2) Fallback: selección ponderada entre las entradas sin Chance
            var sinChance = candidatas.Where(e => e.Chance == null).ToList();
            int totalPeso = sinChance.Sum(e => Math.Max(0, e.Peso));
            if (totalPeso <= 0)
                return new EncuentroResuelto { Tipo = TipoEncuentro.Nada };
            int r = rng.Next(1, totalPeso + 1);
            int acumulado = 0;
            foreach (var e in sinChance)
            {
                acumulado += Math.Max(0, e.Peso);
                if (r <= acumulado)
                {
                    RegistrarDisparo(bioma, e, ahora);
                    return new EncuentroResuelto { Tipo = e.Tipo, Param = e.Param };
                }
            }
            return new EncuentroResuelto { Tipo = TipoEncuentro.Nada };
        }

        // Resolver con modificadores por estadísticas/habilidades/clase y kills
        public EncuentroResuelto Resolver(string bioma, PersonajeModel jugador, Func<string, int?> getKillCount)
        {
            if (!tablas.TryGetValue(bioma, out var tabla))
            {
                RegistrarTablasPorDefecto();
                tablas.TryGetValue(bioma, out tabla);
            }
            if (tabla == null)
            {
                // último fallback
                return Resolver(bioma, jugador.Nivel, getKillCount);
            }

            // Filtrar por nivel
            var hora2 = ObtenerHoraActual();
            var ahora2 = ObtenerFechaActual();
            var candidatas = tabla.Entradas.Where(e => (e.MinNivel == null || jugador.Nivel >= e.MinNivel)).ToList();
            // Gating por kills: mini-jefe u otros con MinKills
            candidatas = candidatas.Where(e => e.MinKills == null || (e.Param != null && getKillCount(e.Param.Split(':').Last()) >= e.MinKills)).ToList();
            // Filtro horario
            candidatas = candidatas.Where(e => CumpleHora(e, hora2)).ToList();
            // Filtro cooldown
            candidatas = candidatas.Where(e => !EstaEnCooldown(bioma, e, ahora2)).ToList();

            // 1) Intentar activar entradas con Chance
            var conChance = candidatas.Where(e => e.Chance != null).ToList();
            var activadas = new List<EntradaEncuentro>();
            foreach (var e in conChance)
            {
                double p = Math.Clamp(e.Chance!.Value, 0.0, 1.0);
                if (rng.NextDouble() < p)
                    activadas.Add(e);
            }
            if (activadas.Count > 0)
            {
                var elegido = activadas
                    .OrderByDescending(e => e.Prioridad ?? 0)
                    .ThenByDescending(e => Math.Max(0, e.Peso))
                    .First();
                RegistrarDisparo(bioma, elegido, ahora2);
                return new EncuentroResuelto { Tipo = elegido.Tipo, Param = elegido.Param };
            }

            // 2) Fallback: calcular pesos efectivos con modificadores entre entradas sin Chance
            var sinChance = candidatas.Where(e => e.Chance == null).ToList();
            var pesos = new List<(EntradaEncuentro e, int peso)>();
            foreach (var e in sinChance)
            {
                double mod = CalcularModificador(e, jugador, getKillCount, bioma);
                int p = (int)Math.Round(Math.Max(0, e.Peso) * mod);
                if (p > 0)
                    pesos.Add((e, p));
            }
            int total = pesos.Sum(x => x.peso);
            if (total <= 0)
                return new EncuentroResuelto { Tipo = TipoEncuentro.Nada };
            int r = rng.Next(1, total + 1);
            int acc = 0;
            foreach (var par in pesos)
            {
                acc += par.peso;
                if (r <= acc)
                {
                    RegistrarDisparo(bioma, par.e, ahora2);
                    return new EncuentroResuelto { Tipo = par.e.Tipo, Param = par.e.Param };
                }
            }
            return new EncuentroResuelto { Tipo = TipoEncuentro.Nada };
        }

        private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);

        private static bool CumpleHora(EntradaEncuentro e, int hora)
        {
            if (e.HoraMin == null && e.HoraMax == null)
                return true;
            int? min = e.HoraMin;
            int? max = e.HoraMax;
            if (min == null && max != null)
                return hora <= max.Value; // Solo límite superior
            if (min != null && max == null)
                return hora >= min.Value; // Solo límite inferior
            if (min == null && max == null)
                return true;
            int mi = Math.Clamp(min!.Value, 0, 23);
            int ma = Math.Clamp(max!.Value, 0, 23);
            if (mi <= ma)
                return hora >= mi && hora <= ma; // ventana directa
            // Cruza medianoche (ej.: 20..4) → válido si hora >= mi o hora <= ma
            return hora >= mi || hora <= ma;
        }

        private double CalcularModificador(EntradaEncuentro e, PersonajeModel jugador, Func<string, int?> getKillCount, string bioma)
        {
            // Base 1.0, aumentos suaves por atributos/skills (sin romper progresión lenta)
            double f = 1.0;
            var atr = jugador.AtributosBase;
            double suerte = atr.Suerte;
            double agi = atr.Agilidad;
            double per = atr.Percepcion;
            double des = atr.Destreza;

            switch (e.Tipo)
            {
                case TipoEncuentro.BotinComun:
                case TipoEncuentro.Materiales:
                    f += Clamp((per + suerte) / 300.0, 0.0, 0.5); // hasta +50%
                    break;
                case TipoEncuentro.NPC:
                case TipoEncuentro.EventoRaro:
                case TipoEncuentro.MazmorraRara:
                    f += Clamp(suerte / 400.0, 0.0, 0.25); // hasta +25%
                    break;
                case TipoEncuentro.CombateComunes:
                case TipoEncuentro.CombateBioma:
                    f += Clamp((agi + des) / 300.0, 0.0, 0.3); // hasta +30%
                    break;
                case TipoEncuentro.MiniJefe:
                    {
                        // Solo considerar si cumple MinKills; luego aumenta leve por kills extra y suerte
                        int req = e.MinKills ?? 0;
                        string clave = e.Param?.Split(':').Last() ?? string.Empty; // p.ej., "lobo"
                        int k = string.IsNullOrWhiteSpace(clave) ? 0 : (getKillCount(clave) ?? 0);
                        if (k < req)
                            return 0.0; // no elegible
                        int extra = Math.Max(0, k - req);
                        f += Clamp((0.02 * extra) + (suerte / 500.0), 0.0, 0.5); // máx. +50%
                    }
                    break;
            }
            // Ganchos futuros: clase/skills/buffs (se pueden añadir multiplicadores aquí)
            return f;
        }
    }
}
