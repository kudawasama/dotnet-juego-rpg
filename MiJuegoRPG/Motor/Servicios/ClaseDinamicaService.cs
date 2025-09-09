using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ClaseDinamicaService
    {
        private List<ClaseData> defs = new();
        private bool cargado = false;
        private readonly object _lock = new();
        private readonly Juego juego;
        public ClaseDinamicaService(Juego juego) { this.juego = juego; }

        private string RutaJson()
        {
            return System.IO.Path.Combine(Juego.ObtenerRutaRaizProyecto(), "MiJuegoRPG", "DatosJuego", "clases_dinamicas.json");
        }

        public void Cargar()
        {
            if (cargado) return;
            lock (_lock)
            {
                if (cargado) return;
                try
                {
                    var ruta = RutaJson();
                    if (System.IO.File.Exists(ruta))
                    {
                        var json = System.IO.File.ReadAllText(ruta);
                        var opciones = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var lista = System.Text.Json.JsonSerializer.Deserialize<List<ClaseData>>(json, opciones);
                        if (lista != null) defs = lista;
                    }
                }
                catch { }
                cargado = true;
            }
        }

        public IEnumerable<ClaseData> ObtenerClasesActivas(Personaje.Personaje pj)
        {
            Cargar();
            return defs.Where(d => pj.TieneClase(d.Nombre));
        }

        public bool Evaluar(Personaje.Personaje pj)
        {
            if (pj == null) return false;
            Cargar();
            bool algunCambio = false;
            foreach (var def in defs)
            {
                if (pj.TieneClase(def.Nombre)) continue;
                if (BloqueadaPorExclusion(pj, def)) continue;
                if (!CumpleHardRequirements(pj, def))
                {
                    // Chequear modo emergente si definido
                    if (def.PesoEmergenteMin > 0 && CalculaScoreEmergente(pj, def) >= def.PesoEmergenteMin)
                    {
                        if (pj.DesbloquearClase(def.Nombre)) algunCambio = true;
                    }
                    continue;
                }
                if (pj.DesbloquearClase(def.Nombre))
                {
                    AplicarBonosAtributoInicial(pj, def);
                    algunCambio = true;
                }
            }
            return algunCambio;
        }

        private bool BloqueadaPorExclusion(Personaje.Personaje pj, ClaseData def)
        {
            if (def.ClasesExcluidas == null || def.ClasesExcluidas.Count == 0) return false;
            foreach (var ex in def.ClasesExcluidas)
                if (pj.TieneClase(ex)) return true;
            return false;
        }

        private bool CumpleHardRequirements(Personaje.Personaje pj, ClaseData def)
        {
            // Clases previas (todas)
            if (def.ClasesPrevias != null && def.ClasesPrevias.Count > 0)
            {
                foreach (var c in def.ClasesPrevias)
                    if (!pj.TieneClase(c)) return false;
            }
            // Clases alguna
            if (def.ClasesAlguna != null && def.ClasesAlguna.Count > 0)
            {
                if (!def.ClasesAlguna.Any(c => pj.TieneClase(c))) return false;
            }
            // Nivel mínimo
            if (def.NivelMinimo > 0 && pj.Nivel < def.NivelMinimo) return false;
            // Atributos
            if (def.AtributosRequeridos != null && def.AtributosRequeridos.Count > 0)
            {
                foreach (var kv in def.AtributosRequeridos)
                {
                    double valor = ObtenerAtributoActual(pj, kv.Key);
                    if (valor < kv.Value) return false;
                }
            }
            // Actividad
            if (def.ActividadRequerida != null && def.ActividadRequerida.Count > 0)
            {
                foreach (var kv in def.ActividadRequerida)
                {
                    pj.ContadoresActividad.TryGetValue(kv.Key, out var v);
                    if (v < kv.Value) return false;
                }
            }
            // Habilidades nivel (placeholder: si no hay sistema detallado, se ignora hasta que exista)
            if (def.HabilidadesNivelRequerido != null && def.HabilidadesNivelRequerido.Count > 0)
            {
                foreach (var kv in def.HabilidadesNivelRequerido)
                {
                    if (!pj.Habilidades.TryGetValue(kv.Key, out var hab) || hab.Nivel < kv.Value) return false;
                }
            }
            // Estadísticas requeridas
            if (def.EstadisticasRequeridas != null && def.EstadisticasRequeridas.Count > 0)
            {
                foreach (var kv in def.EstadisticasRequeridas)
                {
                    double valor = ObtenerEstadisticaActual(pj, kv.Key);
                    if (valor < kv.Value) return false;
                }
            }
            // Reputación mínima (stub hasta que exista propiedad real)
            if (def.ReputacionMinima != 0)
            {
                int reputacionActual = ObtenerReputacion(pj);
                if (reputacionActual < def.ReputacionMinima) return false;
            }
            // Misiones requeridas
            if (def.MisionesRequeridas != null && def.MisionesRequeridas.Count > 0)
            {
                foreach (var m in def.MisionesRequeridas)
                {
                    if (!TieneMisionCompletada(pj, m)) return false;
                }
            }
            // Misión única
            if (!string.IsNullOrWhiteSpace(def.MisionUnica))
            {
                if (!TieneMisionCompletada(pj, def.MisionUnica)) return false;
            }
            // Objeto único
            if (!string.IsNullOrWhiteSpace(def.ObjetoUnico))
            {
                if (!TieneObjeto(pj, def.ObjetoUnico)) return false;
            }
            return true;
        }

        private double CalculaScoreEmergente(Personaje.Personaje pj, ClaseData def)
        {
            double totalPesos = 0; double sumCumplido = 0;
            if (def.ActividadRequerida != null)
            {
                foreach (var kv in def.ActividadRequerida)
                {
                    totalPesos += 1;
                    pj.ContadoresActividad.TryGetValue(kv.Key, out var v);
                    if (v >= kv.Value) sumCumplido += 1; else sumCumplido += Math.Min(1.0, (double)v / Math.Max(1, kv.Value));
                }
            }
            if (def.AtributosRequeridos != null)
            {
                foreach (var kv in def.AtributosRequeridos)
                {
                    totalPesos += 1;
                    double valor = ObtenerAtributoActual(pj, kv.Key);
                    if (valor >= kv.Value) sumCumplido += 1; else sumCumplido += Math.Min(1.0, valor / Math.Max(1, kv.Value));
                }
            }
            if (totalPesos <= 0) return 0;
            return sumCumplido / totalPesos;
        }

        private double ObtenerAtributoActual(Personaje.Personaje pj, string nombre)
        {
            var a = pj.AtributosBase;
            return nombre.ToLower() switch
            {
                "fuerza" => a.Fuerza,
                "inteligencia" => a.Inteligencia,
                "destreza" => a.Destreza,
                "resistencia" => a.Resistencia,
                "defensa" => a.Defensa,
                "vitalidad" => a.Vitalidad,
                "agilidad" => a.Agilidad,
                "suerte" => a.Suerte,
                "percepcion" => a.Percepcion,
                "sabiduria" => a.Sabiduría,
                "fe" => a.Fe,
                "carisma" => a.Carisma,
                "liderazgo" => a.Liderazgo,
                "persuasion" => a.Persuasion,
                "voluntad" => a.Voluntad,
                "oscuridad" => a.Oscuridad,
                _ => 0
            };
        }

        private void AplicarBonosAtributoInicial(Personaje.Personaje pj, ClaseData def)
        {
            if (def.AtributosGanados == null) return;
            foreach (var kv in def.AtributosGanados)
            {
                switch (kv.Key.ToLower())
                {
                    case "fuerza": pj.AtributosBase.Fuerza += kv.Value; break;
                    case "inteligencia": pj.AtributosBase.Inteligencia += kv.Value; break;
                    case "destreza": pj.AtributosBase.Destreza += kv.Value; break;
                    case "resistencia": pj.AtributosBase.Resistencia += kv.Value; break;
                    case "defensa": pj.AtributosBase.Defensa += kv.Value; break;
                    case "vitalidad": pj.AtributosBase.Vitalidad += kv.Value; break;
                    case "agilidad": pj.AtributosBase.Agilidad += kv.Value; break;
                    case "suerte": pj.AtributosBase.Suerte += kv.Value; break;
                    case "percepcion": pj.AtributosBase.Percepcion += kv.Value; break;
                    case "sabiduria": pj.AtributosBase.Sabiduría += kv.Value; break;
                    case "fe": pj.AtributosBase.Fe += kv.Value; break;
                    case "carisma": pj.AtributosBase.Carisma += kv.Value; break;
                    case "liderazgo": pj.AtributosBase.Liderazgo += kv.Value; break;
                    case "persuasion": pj.AtributosBase.Persuasion += kv.Value; break;
                    case "voluntad": pj.AtributosBase.Voluntad += kv.Value; break;
                    case "oscuridad": pj.AtributosBase.Oscuridad += kv.Value; break;
                }
            }
        }

        public IEnumerable<KeyValuePair<string,double>> Bonificadores(Personaje.Personaje pj)
        {
            Cargar();
            foreach (var def in defs)
            {
                if (!pj.TieneClase(def.Nombre)) continue;
                if (def.Bonificadores == null) continue;
                foreach (var kv in def.Bonificadores)
                    yield return kv;
            }
        }
        // --- Helpers adicionales ---
        private double ObtenerEstadisticaActual(Personaje.Personaje pj, string nombre)
        {
            var e = pj.Estadisticas;
            switch (nombre.ToLower())
            {
                case "critico": return e.Critico;
                case "ataque": return e.Ataque;
                case "poderofensivomagico": return e.PoderOfensivoMagico;
                case "podermagico": return e.PoderMagico;
                case "podercurativo": return e.PoderCurativo;
                case "poderofensivofisico": return e.PoderOfensivoFisico;
                case "defensafisica": return e.DefensaFisica;
                case "poderdeinvocacion": return e.PoderDeInvocacion;
                case "poderelemental": return e.PoderElemental;
                case "poderdivino": return e.PoderEspiritual; // alias
                default: return 0;
            }
        }
        private int ObtenerReputacion(Personaje.Personaje pj)
        {
            return pj.Reputacion;
        }
        private bool TieneMisionCompletada(Personaje.Personaje pj, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            return pj.MisionesCompletadas.Any(m => string.Equals(m.Nombre, nombre, StringComparison.OrdinalIgnoreCase) || string.Equals(m.Id, nombre, StringComparison.OrdinalIgnoreCase));
        }
        private bool TieneObjeto(Personaje.Personaje pj, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            return pj.Inventario.NuevosObjetos.Any(o => string.Equals(o.Objeto.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
        }
    }
}
