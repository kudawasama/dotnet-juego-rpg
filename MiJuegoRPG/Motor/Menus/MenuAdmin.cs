using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;

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

        // Helper local para normalizar textos de rareza igual que GeneradorDeObjetos
        // - Quita acentos y mapea alias comunes a los valores del enum existente
        private static string NormalizarRarezaTextoLocal(string s)
        {
            if (s == null) return "Normal";
            s = s.Replace("ó", "o").Replace("Ó", "O").Replace("ú", "u").Replace("Ú", "U").Replace("á", "a").Replace("Á", "A").Replace("é", "e").Replace("É", "E").Replace("í", "i").Replace("Í", "I");
            if (string.Equals(s, "Comun", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "Común", StringComparison.OrdinalIgnoreCase)) return "Normal"; // mapeo a enum existente
            if (string.Equals(s, "Raro", StringComparison.OrdinalIgnoreCase)) return "Rara";
            return s;
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
                juego.Ui.WriteLine("15. Ajustar tiempo del mundo (+/-minutos o h=HH)");
                juego.Ui.WriteLine("16. Ver cooldowns de encuentros");
                juego.Ui.WriteLine("17. Limpiar cooldowns de encuentros (solo vencidos)");
                juego.Ui.WriteLine("18. Limpiar TODOS los cooldowns de encuentros");
                juego.Ui.WriteLine("19. Ver drops únicos (UniqueOnce)");
                juego.Ui.WriteLine("20. Limpiar TODOS los drops únicos");
                juego.Ui.WriteLine("21. Cambiar clase ACTIVA (sin rebonificar)");
                juego.Ui.WriteLine("22. Dar objeto/equipo/material por nombre");
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
                                try { if (juego.claseService.Evaluar(juego.jugador)) juego.Ui.WriteLine("[CLASES] Se han desbloqueado nuevas clas\nes tras el cambio de atributo."); } catch { }
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
                    case "15":
                        AjustarTiempoDelMundo();
                        break;
                    case "16":
                        ListarCooldownsEncuentros();
                        break;
                    case "17":
                        LimpiarCooldownsEncuentros(true);
                        break;
                    case "18":
                        LimpiarCooldownsEncuentros(false);
                        break;
                    case "19":
                        VerDropsUnicos();
                        break;
                    case "20":
                        LimpiarDropsUnicos();
                        break;
                    case "21":
                        CambiarClaseActiva();
                        break;
                    case "22":
                        DarObjetoPorNombre();
                        break;
                    case "0":
                        return;
                    default:
                        juego.Ui.WriteLine("Opción inválida.");
                        break;
                }
            }
        }

        private void DarObjetoPorNombre()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            // Asegurar catálogos cargados
            try { MiJuegoRPG.Motor.GeneradorObjetos.CargarEquipoAuto(); } catch { }
            try { if (MiJuegoRPG.Objetos.GestorMateriales.MaterialesDisponibles.Count == 0) MiJuegoRPG.Objetos.GestorMateriales.CargarMateriales("materiales.json"); } catch { }
            try { if (MiJuegoRPG.Objetos.GestorPociones.PocionesDisponibles.Count == 0) MiJuegoRPG.Objetos.GestorPociones.CargarPociones("pociones.json"); } catch { }

            juego.Ui.WriteLine("=== Dar objeto/equipo/material ===");
            juego.Ui.WriteLine("Formato: [tipo:]nombre  Ejemplos: 'arma:Espada de Aprendiz', 'material:Madera', 'pocion:Poción de Vida'. Si omites tipo, busco en todo.");
            var entrada = InputService.LeerOpcion("Nombre o filtro: ").Trim();
            if (string.IsNullOrWhiteSpace(entrada)) { juego.Ui.WriteLine("Cancelado."); return; }

            // Atajo: set GM completo
            var lower = entrada.ToLowerInvariant();
            if (lower == "gm:set" || lower == "gm:set-completo" || lower == "gm set" || lower == "gm todo" || lower == "gm:todo")
            {
                EntregarSetGMCompleto();
                return;
            }

            string? prefijo = null; string nombre = entrada;
            var idx = entrada.IndexOf(':');
            if (idx > 0) { prefijo = entrada.Substring(0, idx).Trim().ToLowerInvariant(); nombre = entrada.Substring(idx + 1).Trim(); }

            var pj = juego.jugador;
            var candidatos = new List<(string tipo, string nombre)>();

            // Helpers de agregación rápida
            void AgregarMatches(IEnumerable<string> nombres, string tipo)
            {
                foreach (var n in nombres)
                {
                    if (n.IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0)
                        candidatos.Add((tipo, n));
                }
            }

            // Recolectar catálogo de nombres por tipo
            // Equipo por GeneradorObjetos (acceder a listas internas vía reflexión simple)
            try
            {
                var gen = typeof(MiJuegoRPG.Motor.GeneradorObjetos);
                var campos = new (string campo, string tipo)[] {
                    ("armasDisponibles", "arma"), ("armadurasDisponibles", "armadura"), ("accesoriosDisponibles", "accesorio"),
                    ("botasDisponibles", "botas"), ("cascosDisponibles", "casco"), ("cinturonesDisponibles", "cinturon"),
                    ("collaresDisponibles", "collar"), ("pantalonesDisponibles", "pantalon")
                };
                foreach (var (campo, tipo) in campos)
                {
                    var f = gen.GetField(campo, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var val = f?.GetValue(null) as System.Collections.IEnumerable;
                    if (val == null) continue;
                    var nombres = new List<string>();
                    foreach (var it in val)
                    {
                        var t = it.GetType(); var pn = t.GetProperty("Nombre"); var v = pn?.GetValue(it)?.ToString(); if (!string.IsNullOrWhiteSpace(v)) nombres.Add(v!);
                    }
                    if (prefijo == null || prefijo == tipo)
                        AgregarMatches(nombres, tipo);
                }
            }
            catch { }

            // Materiales y Pociones por gestores
            try { if (prefijo == null || prefijo == "material") AgregarMatches(MiJuegoRPG.Objetos.GestorMateriales.MaterialesDisponibles.ConvertAll(m => m.Nombre), "material"); } catch { }
            try { if (prefijo == null || prefijo == "pocion") AgregarMatches(MiJuegoRPG.Objetos.GestorPociones.PocionesDisponibles.ConvertAll(p => p.Nombre), "pocion"); } catch { }

            if (candidatos.Count == 0)
            {
                juego.Ui.WriteLine("No se encontraron candidatos. Verifica el tipo/nombre o que el catálogo esté cargado.");
                return;
            }

            // Si muchos, mostrar primeros 25
            var lista = candidatos.Distinct().OrderBy(t => t.tipo).ThenBy(t => t.nombre, StringComparer.OrdinalIgnoreCase).ToList();
            const int MAX = 25;
            for (int i = 0; i < Math.Min(MAX, lista.Count); i++) juego.Ui.WriteLine($"{i + 1}. [{lista[i].tipo}] {lista[i].nombre}");
            if (lista.Count > MAX) juego.Ui.WriteLine($"... (+{lista.Count - MAX} más)");

            var pick = InputService.LeerOpcion("Elige # o escribe nombre exacto (ENTER cancela): ").Trim();
            if (string.IsNullOrWhiteSpace(pick)) { juego.Ui.WriteLine("Cancelado."); return; }
            (string tipo, string nombreSel)? elegido = null;
            if (int.TryParse(pick, out var idxSel))
            {
                if (idxSel >= 1 && idxSel <= Math.Min(MAX, lista.Count)) elegido = lista[idxSel - 1];
            }
            else
            {
                elegido = lista.FirstOrDefault(t => string.Equals(t.nombre, pick, StringComparison.OrdinalIgnoreCase));
                if (elegido?.nombreSel == null) elegido = lista.FirstOrDefault(t => t.nombre.IndexOf(pick, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (elegido == null)
            {
                juego.Ui.WriteLine("Selección inválida.");
                return;
            }

            // Construir instancia y agregar al inventario
            var (tipoSel, nombreSel) = elegido.Value;
            try
            {
                Objetos.Objeto? obj = null;
                switch (tipoSel)
                {
                    case "arma":
                        // Crear un arma concreta a partir de la data base: usar GeneradorObjetos con filtro de nombre
                        obj = CrearArmaDesdeNombre(nombreSel);
                        break;
                    case "armadura": obj = CrearArmaduraDesdeNombre(nombreSel); break;
                    case "accesorio": obj = CrearAccesorioDesdeNombre(nombreSel); break;
                    case "botas": obj = CrearBotasDesdeNombre(nombreSel); break;
                    case "casco": obj = CrearCascoDesdeNombre(nombreSel); break;
                    case "cinturon": obj = CrearCinturonDesdeNombre(nombreSel); break;
                    case "collar": obj = CrearCollarDesdeNombre(nombreSel); break;
                    case "pantalon": obj = CrearPantalonDesdeNombre(nombreSel); break;
                    case "material":
                        var mat = MiJuegoRPG.Objetos.GestorMateriales.BuscarMaterialPorNombre(nombreSel);
                        if (mat != null) { pj.Inventario.Agregar(mat, 1); juego.Ui.WriteLine($"Entregado material '{nombreSel}'."); }
                        else juego.Ui.WriteLine("Material no encontrado en catálogo.");
                        return;
                    case "pocion":
                        var poc = MiJuegoRPG.Objetos.GestorPociones.BuscarPocionPorNombre(nombreSel);
                        if (poc != null) { pj.Inventario.AgregarObjeto(poc, 1, pj); juego.Ui.WriteLine($"Entregada poción '{nombreSel}'."); }
                        else juego.Ui.WriteLine("Poción no encontrada en catálogo.");
                        return;
                    default: juego.Ui.WriteLine("Tipo no soportado en esta versión."); return;
                }
                if (obj == null) { juego.Ui.WriteLine("No se pudo instanciar el objeto."); return; }
                pj.Inventario.AgregarObjeto(obj, 1, pj);
                // Preguntar si equipar si es equipo
                if (obj is MiJuegoRPG.Objetos.Arma || obj is MiJuegoRPG.Objetos.Armadura || obj is MiJuegoRPG.Objetos.Accesorio || obj is MiJuegoRPG.Objetos.Botas || obj is MiJuegoRPG.Objetos.Casco || obj is MiJuegoRPG.Objetos.Collar || obj is MiJuegoRPG.Objetos.Cinturon || obj is MiJuegoRPG.Objetos.Pantalon)
                {
                    var eq = InputService.LeerOpcion("¿Equipar inmediatamente? (si/no): ").Trim().ToLowerInvariant();
                    if (eq == "si" || eq == "sí") pj.Inventario.EquiparObjeto(obj, pj);
                }
                juego.Ui.WriteLine($"Entregado '{nombreSel}' ({tipoSel}).");
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"[ADMIN] Error al entregar objeto: {ex.Message}");
            }
        }

        /// <summary>
        /// Entrega el set GM completo (arma, armadura, casco, botas, cinturón, collar, pantalón) y ofrece equipar todo.
        /// </summary>
        private void EntregarSetGMCompleto()
        {
            var pj = juego.jugador; if (pj == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            // nombres exactos según los JSON per-item añadidos
            var piezas = new List<(string tipo, string nombre, Func<string, MiJuegoRPG.Objetos.Objeto?> creador)>
            {
                ("arma", "Estada del Creador", n => CrearArmaDesdeNombre(n)),
                ("armadura", "Armadura DIVINO GM GOD", n => CrearArmaduraDesdeNombre(n)),
                ("casco", "Casco DIVINO GM GOD", n => CrearCascoDesdeNombre(n)),
                ("botas", "Botas DIVINO GM GOD", n => CrearBotasDesdeNombre(n)),
                ("cinturon", "Cinturon DIVINO GM GOD", n => CrearCinturonDesdeNombre(n)),
                ("collar", "Collar DIVINO GM GOD", n => CrearCollarDesdeNombre(n)),
                ("pantalon", "Pantalon DIVINO GM GOD", n => CrearPantalonDesdeNombre(n))
            };

            var entregados = new List<MiJuegoRPG.Objetos.Objeto>();
            foreach (var (tipo, nombre, creador) in piezas)
            {
                try
                {
                    var obj = creador(nombre);
                    if (obj == null) { juego.Ui.WriteLine($"[GM] No se encontró '{nombre}' ({tipo}) en el catálogo."); continue; }
                    pj.Inventario.AgregarObjeto(obj, 1, pj);
                    entregados.Add(obj);
                    juego.Ui.WriteLine($"[GM] Entregado '{nombre}' ({tipo}).");
                }
                catch (Exception ex)
                {
                    juego.Ui.WriteLine($"[GM] Error entregando '{nombre}': {ex.Message}");
                }
            }

            if (entregados.Count == 0) { juego.Ui.WriteLine("[GM] No se entregó ninguna pieza."); return; }

            var resp = InputService.LeerOpcion("¿Equipar TODO automáticamente? (si/no): ").Trim().ToLowerInvariant();
            if (resp == "si" || resp == "sí")
            {
                foreach (var obj in entregados)
                {
                    try { pj.Inventario.EquiparObjeto(obj, pj); } catch { }
                }
                juego.Ui.WriteLine("[GM] Set equipado.");
            }
        }

        private MiJuegoRPG.Objetos.Arma? CrearArmaDesdeNombre(string nombre)
        {
            // Buscar data base por nombre exacto en GeneradorObjetos.armasDisponibles y construir como en GenerarArmaAleatoria, pero sin RNG salvo en rangos
            var genT = typeof(MiJuegoRPG.Motor.GeneradorObjetos);
            var f = genT.GetField("armasDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = f?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null;
            foreach (var it in lista) { var t = it.GetType(); var n = t.GetProperty("Nombre")?.GetValue(it)?.ToString(); if (string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            // Reutilizar lógica de GenerarArmaAleatoria: rareza (permitidas/csv), perfección por rareza y rango, nivel, daño base o rango → construir
            try
            {
                // Extraer propiedades via reflexión (ArmaData)
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                string? rzCsv = baseData.GetType().GetProperty("RarezasPermitidasCsv")?.GetValue(baseData) as string;
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("NivelRequerido")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int danio = (int)(baseData.GetType().GetProperty("Daño")?.GetValue(baseData) ?? 1);
                int? dMin = baseData.GetType().GetProperty("DañoMin")?.GetValue(baseData) as int?;
                int? dMax = baseData.GetType().GetProperty("DañoMax")?.GetValue(baseData) as int?;
                // Determinar rareza elegida respetando CSV
                var candidatas = new List<string>();
                if (!string.IsNullOrWhiteSpace(rzCsv)) foreach (var r in rzCsv.Split(',')) { var sLoc = NormalizarRarezaTextoLocal(r.Trim()); candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(sLoc)); }
                if (candidatas.Count == 0) { candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr)); }
                var rzElegida = candidatas[0];
                // Rango perfección por rareza intersectado con ítem
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!; // método ahora debería aceptar string; si aún acepta enum, se adaptará en refactor posterior
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                // Nivel/danio
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (dMin.HasValue && dMax.HasValue) { danio = new Random().Next(Math.Max(0, dMin.Value), Math.Max(dMin.Value, dMax.Value) + 1); }
                int danioFinal = (int)Math.Round(danio * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("Tipo")?.GetValue(baseData) ?? "UnaMano");
                return new MiJuegoRPG.Objetos.Arma((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, danioFinal, nivel, rzElegida, tipo, perfeccion, 0);
            }
            catch { return null; }
        }

        private MiJuegoRPG.Objetos.Armadura? CrearArmaduraDesdeNombre(string nombre)
        {
            var genT = typeof(MiJuegoRPG.Motor.GeneradorObjetos);
            var f = genT.GetField("armadurasDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = f?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var t = it.GetType(); var n = t.GetProperty("Nombre")?.GetValue(it)?.ToString(); if (string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                string? rzCsv = baseData.GetType().GetProperty("RarezasPermitidasCsv")?.GetValue(baseData) as string;
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int def = (int)(baseData.GetType().GetProperty("Defensa")?.GetValue(baseData) ?? 1);
                int? dMin = baseData.GetType().GetProperty("DefensaMin")?.GetValue(baseData) as int?;
                int? dMax = baseData.GetType().GetProperty("DefensaMax")?.GetValue(baseData) as int?;
                var candidatas = new List<string>();
                if (!string.IsNullOrWhiteSpace(rzCsv)) foreach (var r in rzCsv.Split(',')) { var sLoc = NormalizarRarezaTextoLocal(r.Trim()); candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(sLoc)); }
                if (candidatas.Count == 0) { candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr)); }
                var rzElegida = candidatas[0];
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (dMin.HasValue && dMax.HasValue) { def = new Random().Next(Math.Max(0, dMin.Value), Math.Max(dMin.Value, dMax.Value) + 1); }
                int defFinal = (int)Math.Round(def * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Armadura");
                return new MiJuegoRPG.Objetos.Armadura((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, defFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }

        private MiJuegoRPG.Objetos.Accesorio? CrearAccesorioDesdeNombre(string nombre)
        {
            var genT = typeof(MiJuegoRPG.Motor.GeneradorObjetos);
            var f = genT.GetField("accesoriosDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = f?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var t = it.GetType(); var n = t.GetProperty("Nombre")?.GetValue(it)?.ToString(); if (string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                string? rzCsv = baseData.GetType().GetProperty("RarezasPermitidasCsv")?.GetValue(baseData) as string;
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int bonAtk = (int)(baseData.GetType().GetProperty("BonificacionAtaque")?.GetValue(baseData) ?? 0);
                int bonDef = (int)(baseData.GetType().GetProperty("BonificacionDefensa")?.GetValue(baseData) ?? 0);
                var candidatas = new List<string>();
                if (!string.IsNullOrWhiteSpace(rzCsv)) foreach (var r in rzCsv.Split(',')) { var sLoc = NormalizarRarezaTextoLocal(r.Trim()); candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(sLoc)); }
                if (candidatas.Count == 0) { candidatas.Add(MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr)); }
                var rzElegida = candidatas[0];
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                int bonAtkFinal = (int)Math.Round(bonAtk * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                int bonDefFinal = (int)Math.Round(bonDef * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Accesorio");
                return new MiJuegoRPG.Objetos.Accesorio((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, bonAtkFinal, bonDefFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }

        // Botas/Casco/Cinturón/Collar/Pantalón: se instancian como Armadura-like (defensa base) si existen clases; atajo básico
        private MiJuegoRPG.Objetos.Botas? CrearBotasDesdeNombre(string nombre)
        {
            var t = typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetField("botasDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = t?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var p = it.GetType().GetProperty("Nombre"); var v = p?.GetValue(it)?.ToString(); if (string.Equals(v, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int def = (int)(baseData.GetType().GetProperty("Defensa")?.GetValue(baseData) ?? 1);
                int? dMin = baseData.GetType().GetProperty("DefensaMin")?.GetValue(baseData) as int?;
                int? dMax = baseData.GetType().GetProperty("DefensaMax")?.GetValue(baseData) as int?;
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                var rzElegida = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (dMin.HasValue && dMax.HasValue) { def = new Random().Next(Math.Max(0, dMin.Value), Math.Max(dMin.Value, dMax.Value) + 1); }
                int defFinal = (int)Math.Round(def * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Botas");
                return new MiJuegoRPG.Objetos.Botas((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, defFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }
        private MiJuegoRPG.Objetos.Casco? CrearCascoDesdeNombre(string nombre)
        {
            var t = typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetField("cascosDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = t?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var p = it.GetType().GetProperty("Nombre"); var v = p?.GetValue(it)?.ToString(); if (string.Equals(v, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int def = (int)(baseData.GetType().GetProperty("Defensa")?.GetValue(baseData) ?? 1);
                int? dMin = baseData.GetType().GetProperty("DefensaMin")?.GetValue(baseData) as int?;
                int? dMax = baseData.GetType().GetProperty("DefensaMax")?.GetValue(baseData) as int?;
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                var rzElegida = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (dMin.HasValue && dMax.HasValue) { def = new Random().Next(Math.Max(0, dMin.Value), Math.Max(dMin.Value, dMax.Value) + 1); }
                int defFinal = (int)Math.Round(def * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Casco");
                return new MiJuegoRPG.Objetos.Casco((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, defFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }
        private MiJuegoRPG.Objetos.Cinturon? CrearCinturonDesdeNombre(string nombre)
        {
            var t = typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetField("cinturonesDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = t?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var p = it.GetType().GetProperty("Nombre"); var v = p?.GetValue(it)?.ToString(); if (string.Equals(v, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int carga = (int)(baseData.GetType().GetProperty("BonificacionCarga")?.GetValue(baseData) ?? 1);
                int? cMin = baseData.GetType().GetProperty("BonificacionCargaMin")?.GetValue(baseData) as int?;
                int? cMax = baseData.GetType().GetProperty("BonificacionCargaMax")?.GetValue(baseData) as int?;
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                var rzElegida = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (cMin.HasValue && cMax.HasValue) { carga = new Random().Next(Math.Max(0, cMin.Value), Math.Max(cMin.Value, cMax.Value) + 1); }
                int cargaFinal = (int)Math.Round(carga * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Cinturon");
                return new MiJuegoRPG.Objetos.Cinturon((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, cargaFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }
        private MiJuegoRPG.Objetos.Collar? CrearCollarDesdeNombre(string nombre)
        {
            var t = typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetField("collaresDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = t?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var p = it.GetType().GetProperty("Nombre"); var v = p?.GetValue(it)?.ToString(); if (string.Equals(v, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int bonDef = (int)(baseData.GetType().GetProperty("BonificacionDefensa")?.GetValue(baseData) ?? 0);
                int? bonDefMin = baseData.GetType().GetProperty("BonificacionDefensaMin")?.GetValue(baseData) as int?;
                int? bonDefMax = baseData.GetType().GetProperty("BonificacionDefensaMax")?.GetValue(baseData) as int?;
                int bonEne = (int)(baseData.GetType().GetProperty("BonificacionEnergia")?.GetValue(baseData) ?? 0);
                int? bonEneMin = baseData.GetType().GetProperty("BonificacionEnergiaMin")?.GetValue(baseData) as int?;
                int? bonEneMax = baseData.GetType().GetProperty("BonificacionEnergiaMax")?.GetValue(baseData) as int?;
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                var rzElegida = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (bonDefMin.HasValue && bonDefMax.HasValue) { bonDef = new Random().Next(Math.Max(0, bonDefMin.Value), Math.Max(bonDefMin.Value, bonDefMax.Value) + 1); }
                if (bonEneMin.HasValue && bonEneMax.HasValue) { bonEne = new Random().Next(Math.Max(0, bonEneMin.Value), Math.Max(bonEneMin.Value, bonEneMax.Value) + 1); }
                int bonDefFinal = (int)Math.Round(bonDef * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                int bonEneFinal = (int)Math.Round(bonEne * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Collar");
                return new MiJuegoRPG.Objetos.Collar((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, bonDefFinal, bonEneFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }
        private MiJuegoRPG.Objetos.Pantalon? CrearPantalonDesdeNombre(string nombre)
        {
            var t = typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetField("pantalonesDisponibles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var lista = t?.GetValue(null) as System.Collections.IEnumerable; if (lista == null) return null;
            object? baseData = null; foreach (var it in lista) { var p = it.GetType().GetProperty("Nombre"); var v = p?.GetValue(it)?.ToString(); if (string.Equals(v, nombre, StringComparison.OrdinalIgnoreCase)) { baseData = it; break; } }
            if (baseData == null) return null;
            try
            {
                int perfFija = (int)(baseData.GetType().GetProperty("Perfeccion")?.GetValue(baseData) ?? 50);
                int? pMin = baseData.GetType().GetProperty("PerfeccionMin")?.GetValue(baseData) as int?;
                int? pMax = baseData.GetType().GetProperty("PerfeccionMax")?.GetValue(baseData) as int?;
                int nivel = (int)(baseData.GetType().GetProperty("Nivel")?.GetValue(baseData) ?? 1);
                int? nMin = baseData.GetType().GetProperty("NivelMin")?.GetValue(baseData) as int?;
                int? nMax = baseData.GetType().GetProperty("NivelMax")?.GetValue(baseData) as int?;
                int def = (int)(baseData.GetType().GetProperty("Defensa")?.GetValue(baseData) ?? 1);
                int? dMin = baseData.GetType().GetProperty("DefensaMin")?.GetValue(baseData) as int?;
                int? dMax = baseData.GetType().GetProperty("DefensaMax")?.GetValue(baseData) as int?;
                string rzStr = (string)(baseData.GetType().GetProperty("Rareza")?.GetValue(baseData) ?? "Normal");
                var rzElegida = MiJuegoRPG.Objetos.RarezaHelper.Normalizar(rzStr);
                var rango = (System.ValueTuple<int,int>)typeof(MiJuegoRPG.Motor.GeneradorObjetos).GetMethod("RangoPerfeccionPorRareza", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, new object[] { rzElegida })!;
                int pmin = rango.Item1, pmax = rango.Item2;
                if (pMin.HasValue && pMax.HasValue) { pmin = Math.Max(pmin, pMin.Value); pmax = Math.Min(pmax, pMax.Value); if (pmin > pmax) { pmin = rango.Item1; pmax = rango.Item2; } }
                int perfeccion = (perfFija <= 0 || perfFija > 100 || pMin.HasValue || pMax.HasValue) ? new Random().Next(pmin, pmax + 1) : perfFija;
                if (nMin.HasValue && nMax.HasValue) { nivel = new Random().Next(Math.Max(1, nMin.Value), Math.Max(nMin.Value, nMax.Value) + 1); }
                if (dMin.HasValue && dMax.HasValue) { def = new Random().Next(Math.Max(0, dMin.Value), Math.Max(dMin.Value, dMax.Value) + 1); }
                int defFinal = (int)Math.Round(def * (perfeccion / 50.0), MidpointRounding.AwayFromZero);
                string tipo = (string)(baseData.GetType().GetProperty("TipoObjeto")?.GetValue(baseData) ?? "Pantalon");
                return new MiJuegoRPG.Objetos.Pantalon((string)baseData.GetType().GetProperty("Nombre")!.GetValue(baseData)!, defFinal, nivel, rzElegida, tipo, perfeccion);
            }
            catch { return null; }
        }

        private void VerDropsUnicos()
        {
            try
            {
                var n = Motor.Servicios.DropsService.Count();
                juego.Ui.WriteLine($"[Drops únicos] Total claves marcadas: {n}");
                if (n == 0) return;
                var keys = Motor.Servicios.DropsService.KeysSnapshot();
                // Mostrar un resumen limitado para no saturar
                const int maxMostrar = 30;
                int i = 0;
                foreach (var k in keys)
                {
                    if (i++ >= maxMostrar) { juego.Ui.WriteLine($"... (+{n - maxMostrar} más)"); break; }
                    juego.Ui.WriteLine("- " + k);
                }
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"[ADMIN] Error al listar drops únicos: {ex.Message}");
            }
        }

        private void LimpiarDropsUnicos()
        {
            var conf = InputService.LeerOpcion("Confirmar limpieza de TODOS los drops únicos? (si/no): ").Trim().ToLowerInvariant();
            if (conf != "si" && conf != "sí") { juego.Ui.WriteLine("Cancelado."); return; }
            try
            {
                int n = Motor.Servicios.DropsService.ClearAll();
                juego.Ui.WriteLine($"[Drops únicos] Eliminados: {n}");
            }
            catch (Exception ex)
            {
                juego.Ui.WriteLine($"[ADMIN] Error limpiando drops únicos: {ex.Message}");
            }
        }

        private void CambiarClaseActiva()
        {
            if (juego.jugador == null) { juego.Ui.WriteLine("No hay jugador."); return; }
            var pj = juego.jugador;
            if (pj.ClasesDesbloqueadas == null || pj.ClasesDesbloqueadas.Count == 0)
            {
                juego.Ui.WriteLine("No hay clases desbloqueadas.");
                return;
            }

            var lista = pj.ClasesDesbloqueadas.OrderBy(x => x).ToList();
            juego.Ui.WriteLine("=== Cambiar clase ACTIVA (no rebonifica) ===");
            for (int i = 0; i < lista.Count; i++)
            {
                var nombre = lista[i];
                var marca = pj.Clase != null && string.Equals(pj.Clase.Nombre, nombre, StringComparison.OrdinalIgnoreCase) ? " [ACTIVA]" : string.Empty;
                juego.Ui.WriteLine($"{i + 1}. {nombre}{marca}");
            }

            var entrada = InputService.LeerOpcion("Elige # o nombre (ENTER cancela): ").Trim();
            if (string.IsNullOrWhiteSpace(entrada)) { juego.Ui.WriteLine("Cancelado."); return; }
            string? seleccion = null;
            if (int.TryParse(entrada, out var idx))
            {
                if (idx >= 1 && idx <= lista.Count) seleccion = lista[idx - 1];
            }
            else
            {
                seleccion = lista.FirstOrDefault(n => string.Equals(n, entrada, StringComparison.OrdinalIgnoreCase));
            }
            if (seleccion == null) { juego.Ui.WriteLine("Selección inválida."); return; }

            // Cambiar solo la activa, sin rebonificar
            pj.Clase = new MiJuegoRPG.Personaje.Clase { Nombre = seleccion };
            // Recalcular estadísticas preservando ratio de maná
            try
            {
                var ratio = pj.ManaMaxima > 0 ? (double)pj.ManaActual / pj.ManaMaxima : 1.0;
                pj.Estadisticas = new MiJuegoRPG.Personaje.Estadisticas(pj.AtributosBase);
                pj.ManaActual = (int)(pj.ManaMaxima * ratio);
            }
            catch { }
            juego.Ui.WriteLine($"Clase activa cambiada a '{seleccion}'.");
        }

        private void ListarCooldownsEncuentros()
        {
            if (juego.encuentrosService == null) { juego.Ui.WriteLine("[ADMIN] EncuentrosService no inicializado."); return; }
            var estados = juego.encuentrosService.ObtenerEstadoCooldowns();
            if (estados.Count == 0) { juego.Ui.WriteLine("[ADMIN] No hay cooldowns registrados."); return; }
            juego.Ui.WriteLine("=== Cooldowns de Encuentros ===");
            foreach (var e in estados)
            {
                var rest = e.RestanteMinutos;
                string restTxt = rest <= 0.01 ? "(libre)" : $"restante ~{rest:F1} min";
                juego.Ui.WriteLine($"- {e.Bioma} | {e.Tipo} | {e.Param ?? ""} → último {e.UltimoDisparo:yyyy-MM-dd HH:mm} CD={e.CooldownMinutos?.ToString() ?? "-"} {restTxt}");
            }
        }

        private void LimpiarCooldownsEncuentros(bool soloVencidos)
        {
            if (juego.encuentrosService == null) { juego.Ui.WriteLine("[ADMIN] EncuentrosService no inicializado."); return; }
            int n = juego.encuentrosService.LimpiarCooldowns(soloVencidos);
            juego.Ui.WriteLine(soloVencidos ? $"[ADMIN] Eliminados cooldowns vencidos: {n}" : $"[ADMIN] Eliminados TODOS los cooldowns: {n}");   
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
            // Forzar carga de clases antes de mostrar
            juego.claseService.Cargar();
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
            string PropStr(string n)
            {
                try { return def?.GetType()?.GetProperty(n)?.GetValue(def) as string ?? string.Empty; } catch { return string.Empty; }
            }
            int PropInt(string n)
            {
                try { var v = def?.GetType()?.GetProperty(n)?.GetValue(def); return v is int i ? i : 0; } catch { return 0; }
            }
            var propDictInt = def?.GetType()?.GetProperty("AtributosRequeridos")?.GetValue(def) as System.Collections.IDictionary;
            var propAct = def?.GetType()?.GetProperty("ActividadRequerida")?.GetValue(def) as System.Collections.IDictionary;
            var propStats = def?.GetType()?.GetProperty("EstadisticasRequeridas")?.GetValue(def) as System.Collections.IDictionary;
            var propMis = def?.GetType()?.GetProperty("MisionesRequeridas")?.GetValue(def) as System.Collections.IEnumerable;
            var propClasesPrev = def?.GetType()?.GetProperty("ClasesPrevias")?.GetValue(def) as System.Collections.IEnumerable;
            var propClasesAlguna = def?.GetType()?.GetProperty("ClasesAlguna")?.GetValue(def) as System.Collections.IEnumerable;
            var propExcl = def?.GetType()?.GetProperty("ClasesExcluidas")?.GetValue(def) as System.Collections.IEnumerable;
            var propRepFac = def?.GetType()?.GetProperty("ReputacionFaccionMin")?.GetValue(def) as System.Collections.IDictionary;

            int nivelMin = PropInt("NivelMinimo");
            int repMin = PropInt("ReputacionMinima");
            string misionUnica = PropStr("MisionUnica");
            string objetoUnico = PropStr("ObjetoUnico");

            // Exclusiones
            if (propExcl != null)
            {
                foreach (var ex in propExcl)
                {
                    if (ex == null) continue;
                    if (pj.TieneClase(ex.ToString()!)) razones.Add("Excluida por: " + ex);
                }
            }
            // Clases previas
            if (propClasesPrev != null)
            {
                var faltan = new List<string>();
                foreach (var c in propClasesPrev)
                {
                    if (c == null) continue;
                    if (!pj.TieneClase(c.ToString()!)) faltan.Add(c.ToString()!);
                }
                if (faltan.Count > 0) razones.Add("Faltan clases previas: " + string.Join(", ", faltan));
            }
            // Clases alguna
            if (propClasesAlguna != null)
            {
                bool tieneAlguna = false; var lista = new List<string>();
                foreach (var c in propClasesAlguna)
                {
                    if (c == null) continue;
                    lista.Add(c.ToString()!); if (pj.TieneClase(c.ToString()!)) tieneAlguna = true;
                }
                if (!tieneAlguna && lista.Count > 0) razones.Add("Requiere alguna de: " + string.Join(", ", lista));
            }
            if (nivelMin > 0 && pj != null && pj.Nivel < nivelMin) razones.Add($"Nivel {(pj?.Nivel ?? 0)}/{nivelMin}");
            if (repMin > 0 && pj != null && pj.Reputacion < repMin) razones.Add($"Reputación {(pj?.Reputacion ?? 0)}/{repMin}");
            if (propRepFac != null && pj != null && pj.ReputacionesFaccion != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propRepFac)
                {
                    if (kv.Key == null) continue;
                    pj.ReputacionesFaccion.TryGetValue(kv.Key.ToString()!, out var v);
                    int req = kv.Value is int i ? i : 0;
                    if (v < req) razones.Add($"Reputación {kv.Key} {v}/{req}");
                }
            }
            if (!string.IsNullOrWhiteSpace(misionUnica) && pj != null && pj.MisionesCompletadas != null && !pj.MisionesCompletadas.Any(m => (m?.Id ?? "").Equals(misionUnica, StringComparison.OrdinalIgnoreCase) || (m?.Nombre ?? "").Equals(misionUnica, StringComparison.OrdinalIgnoreCase))) razones.Add("Misión única no completada: " + misionUnica);
            if (!string.IsNullOrWhiteSpace(objetoUnico) && pj != null && pj.Inventario != null && pj.Inventario.NuevosObjetos != null && !pj.Inventario.NuevosObjetos.Any(o => o?.Objeto?.Nombre != null && o.Objeto.Nombre.Equals(objetoUnico, StringComparison.OrdinalIgnoreCase))) razones.Add("Objeto único faltante: " + objetoUnico);
            if (propMis != null && pj != null && pj.MisionesCompletadas != null)
            {
                var faltanMis = new List<string>();
                foreach (var m in propMis)
                {
                    if (m == null) continue;
                    if (!pj.MisionesCompletadas.Any(x => (x?.Id ?? "").Equals(m.ToString(), StringComparison.OrdinalIgnoreCase) || (x?.Nombre ?? "").Equals(m.ToString(), StringComparison.OrdinalIgnoreCase))) faltanMis.Add(m.ToString()!);
                }
                if (faltanMis.Count > 0) razones.Add("Misiones pendientes: " + string.Join(", ", faltanMis));
            }
            if (propDictInt != null && pj != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propDictInt)
                {
                    if (kv.Key == null) continue;
                    double val = ObtenerAtributo(pj, kv.Key.ToString()!);
                    int req = kv.Value is int i ? i : 0;
                    if (val < req) razones.Add($"{kv.Key} {val:F1}/{req}");
                }
            }
            if (propAct != null && pj != null && pj.ContadoresActividad != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propAct)
                {
                    if (kv.Key == null) continue;
                    pj.ContadoresActividad.TryGetValue(kv.Key.ToString()!, out var v);
                    int req = kv.Value is int i ? i : 0;
                    if (v < req) razones.Add($"Actividad {kv.Key} {v}/{req}");
                }
            }
            if (propStats != null && pj != null)
            {
                foreach (System.Collections.DictionaryEntry kv in propStats)
                {
                    if (kv.Key == null) continue;
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
    // Forzar carga de clases antes de mostrar
    juego.claseService.Cargar();
    var pj = juego.jugador;
    // Mostrar lista de clases antes de pedir el nombre
    ListarClases();
    var nombre = InputService.LeerOpcion("Nombre exacto de la clase a forzar (ver arriba): ").Trim();
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

        private void AjustarTiempoDelMundo()
        {
            juego.Ui.WriteLine($"Hora actual del mundo: {juego.FormatoRelojMundo}");
            var val = InputService.LeerOpcion("Ingresa delta en minutos (p.ej., +60, -30) o 'h=HH' para fijar hora: ").Trim();
            if (string.IsNullOrWhiteSpace(val)) { juego.Ui.WriteLine("Sin cambios."); return; }
            if (val.StartsWith("h=", StringComparison.OrdinalIgnoreCase))
            {
                var hh = val.Substring(2);
                if (int.TryParse(hh, out var hora))
                {
                    juego.EstablecerHoraDelDia(hora);
                }
                else juego.Ui.WriteLine("Formato inválido. Ejemplos válidos: h=20, h=5");
                return;
            }
            // delta +/- minutos
            var numTxt = val.Replace("+", string.Empty);
            if (int.TryParse(numTxt, out var delta))
            {
                if (val.StartsWith("-")) delta *= -1;
                juego.AjustarMinutosMundo(delta);
            }
            else juego.Ui.WriteLine("Valor inválido. Usa +N, -N o h=HH.");
        }
    }
}
