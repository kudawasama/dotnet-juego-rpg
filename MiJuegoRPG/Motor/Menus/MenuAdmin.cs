using System;
using System.Linq;
using System.Text;
using System.IO;

namespace MiJuegoRPG.Motor.Menus
{
    /// <summary>
    /// Menú administrativo separado para no mezclar comandos con el menú principal del jugador.
    /// </summary>
    public class MenuAdmin
    {
        private readonly Juego juego;
        public MenuAdmin(Juego juego)
        {
            this.juego = juego;
        }

        public void MostrarMenuAdmin()
        {
            while (true)
            {
                juego.Ui.WriteLine("\n=== Menú Administrador ===");
                juego.Ui.WriteLine("1. Teletransportar a sector por Id");
                juego.Ui.WriteLine("2. Modificar reputación global (+/- N)");
                juego.Ui.WriteLine("3. Modificar reputación de facción");
                juego.Ui.WriteLine("4. Ver información de reputación");
                juego.Ui.WriteLine("5. Alternar modo Verbose reputación (actual: " + (juego.reputacionService.Verbose ? "ON" : "OFF") + ")");
                juego.Ui.WriteLine("6. Ajustar nivel (+/- N)");
                juego.Ui.WriteLine("7. Modificar atributo (nombre y delta)");
                juego.Ui.WriteLine("8. Listar clases (desbloqueadas / bloqueadas con motivos)");
                juego.Ui.WriteLine("9. Listar atributos y estadísticas");
                juego.Ui.WriteLine("10. Listar habilidades");
                juego.Ui.WriteLine("11. Listar inventario y equipo");
                juego.Ui.WriteLine("12. Resumen integral");
                juego.Ui.WriteLine("13. Forzar desbloqueo de clase");
                juego.Ui.WriteLine("14. Exportar resumen integral a archivo");
                juego.Ui.WriteLine("0. Volver");
                var op = InputService.LeerOpcion("Opción: ");
                switch (op)
                {
                    case "1":
                        var id = InputService.LeerOpcion("Id de sector destino: ");
                        if (!string.IsNullOrWhiteSpace(id)) juego.TeletransportarASector(id);
                        break;
                    case "2":
                        var txt = InputService.LeerOpcion("Delta reputación global (ej: +10 / -5 / 20): ");
                        if (int.TryParse(txt.Replace("+", string.Empty), out var deltaG))
                        {
                            if (txt.StartsWith("-") || txt.StartsWith("+")) { /* signo ya incluido */ }
                            juego.reputacionService.ModificarReputacion(deltaG * (txt.StartsWith("-") ? -1 : 1));
                        }
                        else juego.Ui.WriteLine("Valor inválido.");
                        break;
                    case "3":
                        var fac = InputService.LeerOpcion("Nombre facción: ");
                        if (string.IsNullOrWhiteSpace(fac)) { juego.Ui.WriteLine("Facción vacía."); break; }
                        var df = InputService.LeerOpcion("Delta (ej: +15 / -4 / 10): ");
                        if (int.TryParse(df.Replace("+", string.Empty), out var deltaF))
                        {
                            juego.reputacionService.ModificarReputacionFaccion(fac, deltaF * (df.StartsWith("-") ? -1 : 1));
                        }
                        else juego.Ui.WriteLine("Valor inválido.");
                        break;
                    case "4":
                        if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador cargado."); break; }
                        juego.Ui.WriteLine($"[REP] Global: {juego.jugador.Reputacion}");
                        if (juego.jugador.ReputacionesFaccion.Count == 0) juego.Ui.WriteLine("[REP] Sin reputaciones de facción.");
                        else foreach (var kv in juego.jugador.ReputacionesFaccion.OrderBy(k => k.Key)) juego.Ui.WriteLine($"[REP] {kv.Key}: {kv.Value}");
                        break;
                    case "5":
                        juego.reputacionService.Verbose = !juego.reputacionService.Verbose;
                        juego.Ui.WriteLine("Verbose reputación ahora: " + (juego.reputacionService.Verbose ? "ON" : "OFF"));
                        break;
                    case "6":
                        if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); break; }
                        var dlv = InputService.LeerOpcion("Delta nivel (ej: +3 / -2 / 5): ");
                        if (int.TryParse(dlv.Replace("+", string.Empty), out var deltaNivel))
                        {
                            bool negativo = dlv.StartsWith("-");
                            if (negativo) deltaNivel *= -1;
                            AjustarNivelAdmin(juego.jugador, deltaNivel);
                        }
                        else juego.Ui.WriteLine("Valor inválido.");
                        break;
                    case "7":
                        if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); break; }
                        juego.Ui.WriteLine("Atributos disponibles: fuerza, destreza, vitalidad, agilidad, suerte, defensa, resistencia, sabiduria, inteligencia, fe, percepcion, persuasion, liderazgo, carisma, voluntad, oscuridad");
                        var entrada = InputService.LeerOpcion("Entrada batch (ej: fuerza+5,inteligencia-3.5) o ENTER para modo clásico: ").Trim();
                        if (!string.IsNullOrEmpty(entrada))
                        {
                            if (ModificarAtributosBatch(juego.jugador, entrada))
                            {
                                var manaRatio = juego.jugador.ManaMaxima > 0 ? (double)juego.jugador.ManaActual / juego.jugador.ManaMaxima : 1.0;
                                juego.jugador.Estadisticas = new Personaje.Estadisticas(juego.jugador.AtributosBase);
                                juego.jugador.ManaActual = (int)(juego.jugador.ManaMaxima * manaRatio);
                                try { if (juego.claseService.Evaluar(juego.jugador)) juego.Ui.WriteLine("[CLASES] Se han desbloqueado nuevas clases tras el cambio de atributo."); } catch { }
                            }
                            break;
                        }

                        var atr = InputService.LeerOpcion("Nombre atributo: ").ToLower();
                        var dat = InputService.LeerOpcion("Delta (ej: +5 / -3 / 2.5): ");
                        if (double.TryParse(dat.Replace("+", string.Empty), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var deltaAtr))
                        {
                            if (dat.StartsWith("-")) deltaAtr *= -1;
                            if (ModificarAtributo(juego.jugador, atr, deltaAtr))
                            {
                                ClampAtributos(juego.jugador);
                                juego.Ui.WriteLine($"Atributo '{atr}' modificado en {deltaAtr}.");
                                // Recalcular estadísticas y mana
                                var manaRatio = juego.jugador.ManaMaxima > 0 ? (double)juego.jugador.ManaActual / juego.jugador.ManaMaxima : 1.0;
                                juego.jugador.Estadisticas = new Personaje.Estadisticas(juego.jugador.AtributosBase);
                                juego.jugador.ManaActual = (int)(juego.jugador.ManaMaxima * manaRatio);
                                try {
                                    if (juego.claseService.Evaluar(juego.jugador))
                                        juego.Ui.WriteLine("[CLASES] Se han desbloqueado nuevas clases tras el cambio de atributo.");
                                } catch { }
                            }
                            else juego.Ui.WriteLine("Atributo desconocido.");
                        }
                        else juego.Ui.WriteLine("Valor inválido.");
                        break;
                    case "8":
                        ListarClases();
                        break;
                    case "9":
                        ListarAtributosYEstadisticas();
                        break;
                    case "10":
                        ListarHabilidades();
                        break;
                    case "11":
                        ListarInventarioYEquipo();
                        break;
                    case "12":
                        ResumenIntegral();
                        break;
                    case "13":
                        ForzarClase();
                        break;
                    case "14":
                        ExportarResumenIntegral();
                        break;
                    case "0":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción inválida.");
                        break;
                }
            }
        }

        private void AjustarNivelAdmin(Personaje.Personaje pj, int delta)
        {
            if (delta == 0) { juego.Ui.WriteLine("Delta 0, sin cambios."); return; }
            int nuevo = pj.Nivel + delta;
            if (nuevo < 1) nuevo = 1;
            if (nuevo > 500) nuevo = 500; // límite arbitrario
            if (nuevo == pj.Nivel) { juego.Ui.WriteLine("Nivel sin cambio efectivo."); return; }
            if (nuevo > pj.Nivel)
            {
                while (pj.Nivel < nuevo)
                {
                    // Reutilizar lógica interna: Ganar experiencia suficiente para subir
                    pj.GanarExperiencia(pj.ExperienciaSiguienteNivel - pj.Experiencia);
                }
            }
            else // bajar nivel
            {
                pj.Nivel = nuevo;
                pj.Experiencia = 0;
                pj.ExperienciaSiguienteNivel = (int)Math.Pow(pj.Nivel + 1, 2) * 200;
                pj.VidaMaxima = 100 + (pj.Nivel - 1) * 10; // simplificación retroactiva
                pj.Vida = pj.VidaMaxima;
                juego.Ui.WriteLine($"Nivel ajustado manualmente a {pj.Nivel}.");
            }
        }

        private bool ModificarAtributo(Personaje.Personaje pj, string nombre, double delta)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            var a = pj.AtributosBase;
            // normalizar acentos potenciales
            switch (nombre)
            {
                case "fuerza": a.Fuerza += delta; break;
                case "destreza": a.Destreza += delta; break;
                case "vitalidad": a.Vitalidad += delta; break;
                case "agilidad": a.Agilidad += delta; break;
                case "suerte": a.Suerte += delta; break;
                case "defensa": a.Defensa += delta; break;
                case "resistencia": a.Resistencia += delta; break;
                case "sabiduria": a.Sabiduría += delta; break;
                case "inteligencia": a.Inteligencia += delta; break;
                case "fe": a.Fe += delta; break;
                case "percepcion": a.Percepcion += delta; break;
                case "persuasion": a.Persuasion += delta; break;
                case "liderazgo": a.Liderazgo += delta; break;
                case "carisma": a.Carisma += delta; break;
                case "voluntad": a.Voluntad += delta; break;
                case "oscuridad": a.Oscuridad += delta; break;
                default: return false;
            }
            return true;
        }

        private void ClampAtributos(Personaje.Personaje pj)
        {
            var a = pj.AtributosBase;
            a.Fuerza = Math.Max(0, a.Fuerza);
            a.Destreza = Math.Max(0, a.Destreza);
            a.Vitalidad = Math.Max(0, a.Vitalidad);
            a.Agilidad = Math.Max(0, a.Agilidad);
            a.Suerte = Math.Max(0, a.Suerte);
            a.Defensa = Math.Max(0, a.Defensa);
            a.Resistencia = Math.Max(0, a.Resistencia);
            a.Sabiduría = Math.Max(0, a.Sabiduría);
            a.Inteligencia = Math.Max(0, a.Inteligencia);
            a.Fe = Math.Max(0, a.Fe);
            a.Percepcion = Math.Max(0, a.Percepcion);
            a.Persuasion = Math.Max(0, a.Persuasion);
            a.Liderazgo = Math.Max(0, a.Liderazgo);
            a.Carisma = Math.Max(0, a.Carisma);
            a.Voluntad = Math.Max(0, a.Voluntad);
            a.Oscuridad = Math.Max(0, a.Oscuridad);
        }

        private bool ModificarAtributosBatch(Personaje.Personaje pj, string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada)) return false;
            int aplicados = 0; var desconocidos = new System.Collections.Generic.List<string>();
            var partes = entrada.Split(',');
            foreach (var raw in partes)
            {
                var t = (raw ?? string.Empty).Trim(); if (t.Length == 0) continue;
                t = t.Replace(" ", string.Empty).ToLower();
                string attr; double delta;
                int idxMas = t.IndexOf('+');
                int idxMenos = t.LastIndexOf('-');
                if (idxMas > 0)
                {
                    attr = t.Substring(0, idxMas);
                    var num = t.Substring(idxMas + 1);
                    if (!double.TryParse(num, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out delta)) { desconocidos.Add(t); continue; }
                }
                else if (idxMenos > 0)
                {
                    attr = t.Substring(0, idxMenos);
                    var num = t.Substring(idxMenos + 1);
                    if (!double.TryParse(num, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out delta)) { desconocidos.Add(t); continue; }
                    delta *= -1;
                }
                else { desconocidos.Add(t); continue; }

                if (ModificarAtributo(pj, attr, delta)) aplicados++; else desconocidos.Add(attr);
            }
            if (aplicados > 0)
            {
                ClampAtributos(pj);
                juego.Ui.WriteLine($"Aplicados: {aplicados}." + (desconocidos.Count > 0 ? $" Desconocidos: {string.Join(", ", desconocidos)}" : string.Empty));
                return true;
            }
            juego.Ui.WriteLine(desconocidos.Count > 0 ? $"Ningún cambio. Desconocidos: {string.Join(", ", desconocidos)}" : "Ningún cambio aplicado.");
            return false;
        }

        // --- Listados y diagnósticos ---
        private void ListarClases()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            juego.Ui.WriteLine("=== Clases Desbloqueadas ===");
            if (pj.ClasesDesbloqueadas.Count == 0) juego.Ui.WriteLine("(ninguna)");
            else foreach (var c in pj.ClasesDesbloqueadas.OrderBy(c=>c)) juego.Ui.WriteLine("- " + c);

            // Necesitamos acceder a las definiciones internas: usar reflejo sobre claseService
            var campoDefs = typeof(Motor.Servicios.ClaseDinamicaService).GetField("defs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDefs == null) { juego.Ui.WriteLine("No se pudieron obtener definiciones de clases."); return; }
            var defs = campoDefs.GetValue(juego.claseService) as System.Collections.IEnumerable;
            if (defs == null) return;
            juego.Ui.WriteLine("\n=== Clases Bloqueadas (motivos) ===");
            foreach (var def in defs)
            {
                var tipo = def.GetType();
                string nombre = tipo.GetProperty("Nombre")?.GetValue(def)?.ToString() ?? "?";
                if (pj.TieneClase(nombre)) continue;
                var razones = MotivosBloqueoClase(pj, def);
                juego.Ui.WriteLine("- " + nombre + (razones.Count == 0 ? " (emergente no alcanzada)" : ": " + string.Join("; ", razones)));
            }
        }

        private List<string> MotivosBloqueoClase(Personaje.Personaje pj, object def)
        {
            var razones = new List<string>();
            string PropStr(string n) => def.GetType().GetProperty(n)?.GetValue(def) as string ?? string.Empty;
            int PropInt(string n) { var v = def.GetType().GetProperty(n)?.GetValue(def); return v is int i ? i : 0; }
            var propDictInt = def.GetType().GetProperty("AtributosRequeridos")?.GetValue(def) as System.Collections.IDictionary;
            var propAct = def.GetType().GetProperty("ActividadRequerida")?.GetValue(def) as System.Collections.IDictionary;
            var propStats = def.GetType().GetProperty("EstadisticasRequeridas")?.GetValue(def) as System.Collections.IDictionary;
            var propMis = def.GetType().GetProperty("MisionesRequeridas")?.GetValue(def) as System.Collections.IEnumerable;
            var propClasesPrev = def.GetType().GetProperty("ClasesPrevias")?.GetValue(def) as System.Collections.IEnumerable;
            var propClasesAlguna = def.GetType().GetProperty("ClasesAlguna")?.GetValue(def) as System.Collections.IEnumerable;
            var propExcl = def.GetType().GetProperty("ClasesExcluidas")?.GetValue(def) as System.Collections.IEnumerable;
            var propRepFac = def.GetType().GetProperty("ReputacionFaccionMin")?.GetValue(def) as System.Collections.IDictionary;

            int nivelMin = PropInt("NivelMinimo");
            int repMin = PropInt("ReputacionMinima");
            string misionUnica = PropStr("MisionUnica");
            string objetoUnico = PropStr("ObjetoUnico");

            // Exclusiones
            if (propExcl != null)
                foreach (var ex in propExcl) if (pj.TieneClase(ex.ToString()!)) razones.Add("Excluida por: " + ex);
            // Clases previas
            if (propClasesPrev != null)
            {
                var faltan = new List<string>();
                foreach (var c in propClasesPrev) if (!pj.TieneClase(c.ToString()!)) faltan.Add(c.ToString()!);
                if (faltan.Count > 0) razones.Add("Faltan clases previas: " + string.Join(", ", faltan));
            }
            // Clases alguna
            if (propClasesAlguna != null)
            {
                bool tieneAlguna = false; var lista = new List<string>();
                foreach (var c in propClasesAlguna) { lista.Add(c.ToString()!); if (pj.TieneClase(c.ToString()!)) tieneAlguna = true; }
                if (!tieneAlguna && lista.Count > 0) razones.Add("Requiere alguna de: " + string.Join(", ", lista));
            }
            if (nivelMin > 0 && pj.Nivel < nivelMin) razones.Add($"Nivel {pj.Nivel}/{nivelMin}");
            if (repMin > 0 && pj.Reputacion < repMin) razones.Add($"Reputación {pj.Reputacion}/{repMin}");
            if (propRepFac != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propRepFac)
                {
                    pj.ReputacionesFaccion.TryGetValue(kv.Key.ToString()!, out var v);
                    int req = kv.Value is int i ? i : 0;
                    if (v < req) razones.Add($"Reputación {kv.Key} {v}/{req}");
                }
            }
            if (!string.IsNullOrWhiteSpace(misionUnica) && !pj.MisionesCompletadas.Any(m => m.Id.Equals(misionUnica, StringComparison.OrdinalIgnoreCase) || m.Nombre.Equals(misionUnica, StringComparison.OrdinalIgnoreCase))) razones.Add("Misión única no completada: " + misionUnica);
            if (!string.IsNullOrWhiteSpace(objetoUnico) && !pj.Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.Equals(objetoUnico, StringComparison.OrdinalIgnoreCase))) razones.Add("Objeto único faltante: " + objetoUnico);
            if (propMis != null)
            {
                var faltanMis = new List<string>();
                foreach (var m in propMis)
                {
                    if (!pj.MisionesCompletadas.Any(x => x.Id.Equals(m.ToString(), StringComparison.OrdinalIgnoreCase) || x.Nombre.Equals(m.ToString(), StringComparison.OrdinalIgnoreCase))) faltanMis.Add(m.ToString()!);
                }
                if (faltanMis.Count > 0) razones.Add("Misiones pendientes: " + string.Join(", ", faltanMis));
            }
            if (propDictInt != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propDictInt)
                {
                    double val = ObtenerAtributo(pj, kv.Key.ToString()!);
                    int req = kv.Value is int i ? i : 0;
                    if (val < req) razones.Add($"{kv.Key} {val:F1}/{req}");
                }
            }
            if (propAct != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propAct)
                {
                    pj.ContadoresActividad.TryGetValue(kv.Key.ToString()!, out var v);
                    int req = kv.Value is int i ? i : 0;
                    if (v < req) razones.Add($"Actividad {kv.Key} {v}/{req}");
                }
            }
            if (propStats != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propStats)
                {
                    double val = ObtenerEstadistica(pj, kv.Key.ToString()!);
                    int req = kv.Value is int i ? i : 0;
                    if (val < req) razones.Add($"Stat {kv.Key} {val:F1}/{req}");
                }
            }
            return razones;
        }

        private double ObtenerAtributo(Personaje.Personaje pj, string nombre)
        {
            var a = pj.AtributosBase; nombre = nombre.ToLower();
            return nombre switch
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
        private double ObtenerEstadistica(Personaje.Personaje pj, string nombre)
        {
            var e = pj.Estadisticas; nombre = nombre.ToLower();
            return nombre switch
            {
                "critico" => e.Critico,
                "ataque" => e.Ataque,
                "poderofensivomagico" => e.PoderOfensivoMagico,
                "podermagico" => e.PoderMagico,
                "podercurativo" => e.PoderCurativo,
                "poderofensivofisico" => e.PoderOfensivoFisico,
                "defensafisica" => e.DefensaFisica,
                _ => 0
            };
        }

        private void ListarAtributosYEstadisticas()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            juego.Ui.WriteLine("=== Atributos Base ===");
            foreach (var p in pj.AtributosBase.GetType().GetProperties())
            {
                juego.Ui.WriteLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.AtributosBase)):F2}");
            }
            juego.Ui.WriteLine("=== Estadísticas ===");
            foreach (var p in pj.Estadisticas.GetType().GetProperties().Take(25)) // limitar para no saturar
            {
                juego.Ui.WriteLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.Estadisticas)):F2}");
            }
        }

        private void ListarHabilidades()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            juego.Ui.WriteLine("=== Habilidades ===");
            if (pj.Habilidades.Count == 0) { juego.Ui.WriteLine("(ninguna)"); return; }
            foreach (var kv in pj.Habilidades)
            {
                var h = kv.Value;
                juego.Ui.WriteLine($"- {h.Nombre} (ID {h.Id}) Nv {h.Nivel} Exp {h.Exp}");
            }
        }

        private void ListarInventarioYEquipo()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            juego.Ui.WriteLine("=== Inventario (objetos) ===");
            if (pj.Inventario.NuevosObjetos.Count == 0) juego.Ui.WriteLine("(vacío)");
            else foreach (var o in pj.Inventario.NuevosObjetos) juego.Ui.WriteLine($"- {o.Objeto.Nombre} x{o.Cantidad}");
            juego.Ui.WriteLine("=== Equipo ===");
            var eq = pj.Inventario.Equipo;
            void Imprimir(string label, object? item) { juego.Ui.WriteLine(label + ": " + (item == null ? "(libre)" : ((MiJuegoRPG.Objetos.Objeto)item).Nombre)); }
            Imprimir("Arma", eq.Arma); Imprimir("Casco", eq.Casco); Imprimir("Armadura", eq.Armadura); Imprimir("Pantalon", eq.Pantalon); Imprimir("Botas", eq.Zapatos); Imprimir("Collar", eq.Collar); Imprimir("Cinturon", eq.Cinturon); Imprimir("Accesorio1", eq.Accesorio1); Imprimir("Accesorio2", eq.Accesorio2);
        }

        private void ResumenIntegral()
        {
            ListarClases();
            juego.Ui.WriteLine();
            ListarAtributosYEstadisticas();
            juego.Ui.WriteLine();
            ListarHabilidades();
            juego.Ui.WriteLine();
            ListarInventarioYEquipo();
            juego.Ui.WriteLine();
            // Reputación
            if (juego.jugador != null)
            {
                juego.Ui.WriteLine("=== Reputación ===");
                juego.Ui.WriteLine($"Global: {juego.jugador.Reputacion}");
                foreach (var kv in juego.jugador.ReputacionesFaccion.OrderBy(k=>k.Key)) juego.Ui.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }

        private void ForzarClase()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            var nombre = InputService.LeerOpcion("Nombre exacto de la clase a forzar: ").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { juego.Ui.WriteLine("Nombre vacío."); return; }

            // Obtener definiciones mediante reflexión
            var campoDefs = typeof(Motor.Servicios.ClaseDinamicaService).GetField("defs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDefs == null) { juego.Ui.WriteLine("No se pudieron leer definiciones de clases."); return; }
            var defs = campoDefs.GetValue(juego.claseService) as System.Collections.IEnumerable;
            if (defs == null) { juego.Ui.WriteLine("Sin definiciones cargadas."); return; }

            object? defEncontrada = null;
            foreach (var def in defs)
            {
                var tipo = def.GetType();
                string? nom = tipo.GetProperty("Nombre")?.GetValue(def)?.ToString();
                if (!string.IsNullOrWhiteSpace(nom) && string.Equals(nom, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    defEncontrada = def; break;
                }
            }
            if (defEncontrada == null)
            {
                juego.Ui.WriteLine("Clase no encontrada. Ver opción 'Listar clases' para nombres.");
                return;
            }
            if (pj.TieneClase(nombre)) { juego.Ui.WriteLine("Ya tenías esta clase."); return; }

            // Desbloquear y aplicar bonos iniciales usando reflexión al método privado
            if (pj.DesbloquearClase(nombre))
            {
                try
                {
                    var metodo = typeof(Motor.Servicios.ClaseDinamicaService).GetMethod(
                        "AplicarBonosAtributoInicial",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    metodo?.Invoke(juego.claseService, new object[] { pj, defEncontrada });

                    // Recalcular estadísticas y mantener ratio de maná
                    try
                    {
                        var manaRatio = pj.ManaMaxima > 0 ? (double)pj.ManaActual / pj.ManaMaxima : 1.0;
                        pj.Estadisticas = new Personaje.Estadisticas(pj.AtributosBase);
                        pj.ManaActual = (int)(pj.ManaMaxima * manaRatio);
                    }
                    catch { /* si no hay maná en este build */ }

                    // Reevaluar por posibles cadenas de desbloqueo
                    try { if (juego.claseService.Evaluar(pj)) juego.Ui.WriteLine("[CLASES] Desbloqueos adicionales tras forzar clase."); } catch { }

                    juego.Ui.WriteLine($"Clase '{nombre}' forzada y bonos aplicados.");
                }
                catch (Exception ex)
                {
                    juego.Ui.WriteLine($"Se forzó la clase pero no se pudieron aplicar bonos automáticamente ({ex.Message}).");
                }
            }
            else
            {
                juego.Ui.WriteLine("No se pudo forzar el desbloqueo (quizás ya estaba desbloqueada).");
            }
        }

        private void ExportarResumenIntegral()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            try
            {
                var sb = new StringBuilder();
                var pj = juego.jugador;
                // Clases
                sb.AppendLine("=== Clases Desbloqueadas ===");
                if (pj.ClasesDesbloqueadas.Count == 0) sb.AppendLine("(ninguna)");
                else foreach (var c in pj.ClasesDesbloqueadas.OrderBy(c=>c)) sb.AppendLine("- " + c);

                var campoDefs = typeof(Motor.Servicios.ClaseDinamicaService).GetField("defs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var defs = campoDefs?.GetValue(juego.claseService) as System.Collections.IEnumerable;
                if (defs != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== Clases Bloqueadas (motivos) ===");
                    foreach (var def in defs)
                    {
                        var tipo = def.GetType();
                        string nombre = tipo.GetProperty("Nombre")?.GetValue(def)?.ToString() ?? "?";
                        if (pj.TieneClase(nombre)) continue;
                        var razones = MotivosBloqueoClase(pj, def);
                        sb.AppendLine("- " + nombre + (razones.Count == 0 ? " (emergente no alcanzada)" : ": " + string.Join("; ", razones)));
                    }
                }

                // Atributos y estadísticas
                sb.AppendLine();
                sb.AppendLine("=== Atributos Base ===");
                foreach (var p in pj.AtributosBase.GetType().GetProperties())
                    sb.AppendLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.AtributosBase)):F2}");
                sb.AppendLine("=== Estadísticas ===");
                foreach (var p in pj.Estadisticas.GetType().GetProperties().Take(25))
                    sb.AppendLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.Estadisticas)):F2}");

                // Habilidades
                sb.AppendLine();
                sb.AppendLine("=== Habilidades ===");
                if (pj.Habilidades.Count == 0) sb.AppendLine("(ninguna)");
                else foreach (var kv in pj.Habilidades)
                {
                    var h = kv.Value; sb.AppendLine($"- {h.Nombre} (ID {h.Id}) Nv {h.Nivel} Exp {h.Exp}");
                }

                // Inventario y equipo
                sb.AppendLine();
                sb.AppendLine("=== Inventario (objetos) ===");
                if (pj.Inventario.NuevosObjetos.Count == 0) sb.AppendLine("(vacío)");
                else foreach (var o in pj.Inventario.NuevosObjetos) sb.AppendLine($"- {o.Objeto.Nombre} x{o.Cantidad}");
                sb.AppendLine("=== Equipo ===");
                var eq = pj.Inventario.Equipo;
                string Nom(object? item) => item == null ? "(libre)" : ((MiJuegoRPG.Objetos.Objeto)item).Nombre;
                sb.AppendLine($"Arma: {Nom(eq.Arma)}"); sb.AppendLine($"Casco: {Nom(eq.Casco)}"); sb.AppendLine($"Armadura: {Nom(eq.Armadura)}");
                sb.AppendLine($"Pantalon: {Nom(eq.Pantalon)}"); sb.AppendLine($"Botas: {Nom(eq.Zapatos)}"); sb.AppendLine($"Collar: {Nom(eq.Collar)}");
                sb.AppendLine($"Cinturon: {Nom(eq.Cinturon)}"); sb.AppendLine($"Accesorio1: {Nom(eq.Accesorio1)}"); sb.AppendLine($"Accesorio2: {Nom(eq.Accesorio2)}");

                // Reputación
                sb.AppendLine(); sb.AppendLine("=== Reputación ===");
                sb.AppendLine($"Global: {pj.Reputacion}");
                foreach (var kv in pj.ReputacionesFaccion.OrderBy(k=>k.Key)) sb.AppendLine($"{kv.Key}: {kv.Value}");

                // Guardar archivo
                var raiz = Juego.ObtenerRutaRaizProyecto();
                var dir = Path.Combine(raiz, "MiJuegoRPG", "logs", "admin");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
                juego.Ui.WriteLine($"[ADMIN] Resumen exportado a {file}");
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"[ADMIN] Error exportando resumen: {ex.Message}");
            }
        }
    }
}
