using System;
using System.Linq;

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
                Console.WriteLine("\n=== Menú Administrador ===");
                Console.WriteLine("1. Teletransportar a sector por Id");
                Console.WriteLine("2. Modificar reputación global (+/- N)");
                Console.WriteLine("3. Modificar reputación de facción");
                Console.WriteLine("4. Ver información de reputación");
                Console.WriteLine("5. Alternar modo Verbose reputación (actual: " + (juego.reputacionService.Verbose ? "ON" : "OFF") + ")");
                Console.WriteLine("6. Ajustar nivel (+/- N)");
                Console.WriteLine("7. Modificar atributo (nombre y delta)");
                Console.WriteLine("8. Listar clases (desbloqueadas / bloqueadas con motivos)");
                Console.WriteLine("9. Listar atributos y estadísticas");
                Console.WriteLine("10. Listar habilidades");
                Console.WriteLine("11. Listar inventario y equipo");
                Console.WriteLine("12. Resumen integral");
                Console.WriteLine("0. Volver");
                Console.Write("Opción: ");
                var op = Console.ReadLine()?.Trim();
                switch (op)
                {
                    case "1":
                        Console.Write("Id de sector destino: ");
                        var id = Console.ReadLine()?.Trim() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(id)) juego.TeletransportarASector(id);
                        break;
                    case "2":
                        Console.Write("Delta reputación global (ej: +10 / -5 / 20): ");
                        var txt = Console.ReadLine()?.Trim() ?? "0";
                        if (int.TryParse(txt.Replace("+", string.Empty), out var deltaG))
                        {
                            if (txt.StartsWith("-") || txt.StartsWith("+")) { /* signo ya incluido */ }
                            juego.reputacionService.ModificarReputacion(deltaG * (txt.StartsWith("-") ? -1 : 1));
                        }
                        else Console.WriteLine("Valor inválido.");
                        break;
                    case "3":
                        Console.Write("Nombre facción: ");
                        var fac = Console.ReadLine()?.Trim() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(fac)) { Console.WriteLine("Facción vacía."); break; }
                        Console.Write("Delta (ej: +15 / -4 / 10): ");
                        var df = Console.ReadLine()?.Trim() ?? "0";
                        if (int.TryParse(df.Replace("+", string.Empty), out var deltaF))
                        {
                            juego.reputacionService.ModificarReputacionFaccion(fac, deltaF * (df.StartsWith("-") ? -1 : 1));
                        }
                        else Console.WriteLine("Valor inválido.");
                        break;
                    case "4":
                        if (juego.jugador == null) { Console.WriteLine("No hay jugador cargado."); break; }
                        Console.WriteLine($"[REP] Global: {juego.jugador.Reputacion}");
                        if (juego.jugador.ReputacionesFaccion.Count == 0) Console.WriteLine("[REP] Sin reputaciones de facción.");
                        else foreach (var kv in juego.jugador.ReputacionesFaccion.OrderBy(k => k.Key)) Console.WriteLine($"[REP] {kv.Key}: {kv.Value}");
                        break;
                    case "5":
                        juego.reputacionService.Verbose = !juego.reputacionService.Verbose;
                        Console.WriteLine("Verbose reputación ahora: " + (juego.reputacionService.Verbose ? "ON" : "OFF"));
                        break;
                    case "6":
                        if (juego.jugador == null) { Console.WriteLine("No hay jugador."); break; }
                        Console.Write("Delta nivel (ej: +3 / -2 / 5): ");
                        var dlv = Console.ReadLine()?.Trim() ?? "0";
                        if (int.TryParse(dlv.Replace("+", string.Empty), out var deltaNivel))
                        {
                            bool negativo = dlv.StartsWith("-");
                            if (negativo) deltaNivel *= -1;
                            AjustarNivelAdmin(juego.jugador, deltaNivel);
                        }
                        else Console.WriteLine("Valor inválido.");
                        break;
                    case "7":
                        if (juego.jugador == null) { Console.WriteLine("No hay jugador."); break; }
                        Console.WriteLine("Atributos disponibles: fuerza, destreza, vitalidad, agilidad, suerte, defensa, resistencia, sabiduria, inteligencia, fe, percepcion, persuasion, liderazgo, carisma, voluntad, oscuridad");
                        Console.Write("Nombre atributo: ");
                        var atr = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
                        Console.Write("Delta (ej: +5 / -3 / 2.5): ");
                        var dat = Console.ReadLine()?.Trim() ?? "0";
                        if (double.TryParse(dat.Replace("+", string.Empty), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var deltaAtr))
                        {
                            if (dat.StartsWith("-")) deltaAtr *= -1;
                            if (ModificarAtributo(juego.jugador, atr, deltaAtr))
                            {
                                Console.WriteLine($"Atributo '{atr}' modificado en {deltaAtr}.");
                                // Recalcular estadísticas y mana
                                var manaRatio = juego.jugador.ManaMaxima > 0 ? (double)juego.jugador.ManaActual / juego.jugador.ManaMaxima : 1.0;
                                juego.jugador.Estadisticas = new Personaje.Estadisticas(juego.jugador.AtributosBase);
                                juego.jugador.ManaActual = (int)(juego.jugador.ManaMaxima * manaRatio);
                                try {
                                    if (juego.claseService.Evaluar(juego.jugador))
                                        Console.WriteLine("[CLASES] Se han desbloqueado nuevas clases tras el cambio de atributo.");
                                } catch { }
                            }
                            else Console.WriteLine("Atributo desconocido.");
                        }
                        else Console.WriteLine("Valor inválido.");
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
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
        }

        private void AjustarNivelAdmin(Personaje.Personaje pj, int delta)
        {
            if (delta == 0) { Console.WriteLine("Delta 0, sin cambios."); return; }
            int nuevo = pj.Nivel + delta;
            if (nuevo < 1) nuevo = 1;
            if (nuevo > 500) nuevo = 500; // límite arbitrario
            if (nuevo == pj.Nivel) { Console.WriteLine("Nivel sin cambio efectivo."); return; }
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
                Console.WriteLine($"Nivel ajustado manualmente a {pj.Nivel}.");
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

        // --- Listados y diagnósticos ---
        private void ListarClases()
        {
            if (juego.jugador == null) { Console.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            Console.WriteLine("=== Clases Desbloqueadas ===");
            if (pj.ClasesDesbloqueadas.Count == 0) Console.WriteLine("(ninguna)");
            else foreach (var c in pj.ClasesDesbloqueadas.OrderBy(c=>c)) Console.WriteLine("- " + c);

            // Necesitamos acceder a las definiciones internas: usar reflejo sobre claseService
            var campoDefs = typeof(Motor.Servicios.ClaseDinamicaService).GetField("defs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDefs == null) { Console.WriteLine("No se pudieron obtener definiciones de clases."); return; }
            var defs = campoDefs.GetValue(juego.claseService) as System.Collections.IEnumerable;
            if (defs == null) return;
            Console.WriteLine("\n=== Clases Bloqueadas (motivos) ===");
            foreach (var def in defs)
            {
                var tipo = def.GetType();
                string nombre = tipo.GetProperty("Nombre")?.GetValue(def)?.ToString() ?? "?";
                if (pj.TieneClase(nombre)) continue;
                var razones = MotivosBloqueoClase(pj, def);
                Console.WriteLine("- " + nombre + (razones.Count == 0 ? " (emergente no alcanzada)" : ": " + string.Join("; ", razones)));
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
            if (juego.jugador == null) { Console.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            Console.WriteLine("=== Atributos Base ===");
            foreach (var p in pj.AtributosBase.GetType().GetProperties())
            {
                Console.WriteLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.AtributosBase)):F2}");
            }
            Console.WriteLine("=== Estadísticas ===");
            foreach (var p in pj.Estadisticas.GetType().GetProperties().Take(25)) // limitar para no saturar
            {
                Console.WriteLine($"{p.Name}: {Convert.ToDouble(p.GetValue(pj.Estadisticas)):F2}");
            }
        }

        private void ListarHabilidades()
        {
            if (juego.jugador == null) { Console.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            Console.WriteLine("=== Habilidades ===");
            if (pj.Habilidades.Count == 0) { Console.WriteLine("(ninguna)"); return; }
            foreach (var kv in pj.Habilidades)
            {
                var h = kv.Value;
                Console.WriteLine($"- {h.Nombre} (ID {h.Id}) Nv {h.Nivel} Exp {h.Exp}");
            }
        }

        private void ListarInventarioYEquipo()
        {
            if (juego.jugador == null) { Console.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            Console.WriteLine("=== Inventario (objetos) ===");
            if (pj.Inventario.NuevosObjetos.Count == 0) Console.WriteLine("(vacío)");
            else foreach (var o in pj.Inventario.NuevosObjetos) Console.WriteLine($"- {o.Objeto.Nombre} x{o.Cantidad}");
            Console.WriteLine("=== Equipo ===");
            var eq = pj.Inventario.Equipo;
            void Imprimir(string label, object? item) { Console.WriteLine(label + ": " + (item == null ? "(libre)" : ((MiJuegoRPG.Objetos.Objeto)item).Nombre)); }
            Imprimir("Arma", eq.Arma); Imprimir("Casco", eq.Casco); Imprimir("Armadura", eq.Armadura); Imprimir("Pantalon", eq.Pantalon); Imprimir("Botas", eq.Zapatos); Imprimir("Collar", eq.Collar); Imprimir("Cinturon", eq.Cinturon); Imprimir("Accesorio1", eq.Accesorio1); Imprimir("Accesorio2", eq.Accesorio2);
        }

        private void ResumenIntegral()
        {
            ListarClases();
            Console.WriteLine();
            ListarAtributosYEstadisticas();
            Console.WriteLine();
            ListarHabilidades();
            Console.WriteLine();
            ListarInventarioYEquipo();
            Console.WriteLine();
            // Reputación
            if (juego.jugador != null)
            {
                Console.WriteLine("=== Reputación ===");
                Console.WriteLine($"Global: {juego.jugador.Reputacion}");
                foreach (var kv in juego.jugador.ReputacionesFaccion.OrderBy(k=>k.Key)) Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }
    }
}
