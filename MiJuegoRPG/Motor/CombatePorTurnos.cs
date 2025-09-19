using System;
using System.Linq;
using System.Collections.Generic;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    public class CombatePorTurnos
    {
        private ICombatiente jugador;
        private List<ICombatiente> enemigos;
        private IUserInterface ui => Juego.ObtenerInstanciaActual()?.Ui ?? new ConsoleUserInterface();
    // Efectos activos por combatiente
    private Dictionary<ICombatiente, List<MiJuegoRPG.Interfaces.IEfecto>> efectosActivos = new();
    // Reglas de acciones (cooldowns, recursos)
    private readonly ActionRulesService actionRules = new();

        // Facilita pruebas de loop (no usado en juego normal)
        public int? MaxIteraciones { get; set; } = null;

        // Constructor para combate múltiple
        public CombatePorTurnos(ICombatiente jugador, List<ICombatiente> enemigos)
        {
            this.jugador = jugador;
            this.enemigos = enemigos;
        }

        // Constructor para combate clásico (uno a uno)
        public CombatePorTurnos(ICombatiente jugador, ICombatiente enemigo)
        {
            this.jugador = jugador;
            this.enemigos = new List<ICombatiente> { enemigo };
        }

        public void IniciarCombate()
        {
            UIStyle.Header(ui, "Combate por Turnos");
            ui.WriteLine($"Enemigos: {string.Join(", ", enemigos.Select(e => e.Nombre))}");
            ui.WriteLine($"¡Comienza el combate entre {jugador.Nombre} y {string.Join(", ", enemigos.Select(e => e.Nombre))}!");
            MostrarEstadoCombate();
            ui.WriteLine("----------------------------------------\n");

            bool turnoJugador = true;
            int turno = 0;
            while (jugador.EstaVivo && enemigos.Any(e => e.EstaVivo) && (MaxIteraciones == null || turno < MaxIteraciones))
            {
                if (turnoJugador)
                {
                    UIStyle.SubHeader(ui, $"Turno de {jugador.Nombre}");
                    ui.WriteLine("1. Atacar");
                    ui.WriteLine("2. Usar poción (si tienes)");
                    ui.WriteLine("3. Huir");
                    ui.WriteLine("4. Habilidad (Físico/Mágico)");
                    ui.WriteLine("5. Habilidad: Aplicar Veneno (demo)");
                    var accion = InputService.LeerOpcion("Elige una acción: ");

                    switch (accion)
                    {
                        case "1":
                            {
                                // Elegir enemigo vivo
                                var enemigosVivos = enemigos.Where(e => e.EstaVivo).ToList();
                                ICombatiente? objetivo = null;
                                if (enemigosVivos.Count == 1)
                                {
                                    objetivo = enemigosVivos[0];
                                }
                                else
                                {
                                    UIStyle.Hint(ui, "Elige enemigo a atacar:");
                                    for (int i = 0; i < enemigosVivos.Count; i++)
                                        ui.WriteLine($"{i + 1}. {enemigosVivos[i].Nombre} ({enemigosVivos[i].Vida}/{enemigosVivos[i].VidaMaxima} HP)");
                                    var sel = InputService.LeerOpcion("Elige enemigo a atacar: ");
                                    if (int.TryParse(sel, out int idx) && idx > 0 && idx <= enemigosVivos.Count)
                                    {
                                        objetivo = enemigosVivos[idx - 1];
                                    }
                                }
                                if (objetivo == null)
                                {
                                    ui.WriteLine("Selección inválida, pierdes el turno.");
                                }
                                else
                                {
                                    var accionFisica = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                                    var res = accionFisica.Ejecutar(jugador, objetivo);
                                    foreach (var m in res.Mensajes) ui.WriteLine(m);
                                    RegistrarEfectos(res);
                                }
                                break;
                            }
                        case "2":
                            {
                                // Usar poción como acción de combate con gating: no perder turno en indisponible/cancelación
                                if (jugador is MiJuegoRPG.Personaje.Personaje pj)
                                {
                                    var pociones = pj.Inventario.NuevosObjetos
                                        .Where(o => o.Objeto is MiJuegoRPG.Objetos.Pocion && o.Cantidad > 0)
                                        .ToList();
                                    if (pociones.Count == 0)
                                    {
                                        ui.WriteLine("No tienes pociones disponibles.");
                                        continue; // no perder turno si no hay
                                    }
                                    UIStyle.Hint(ui, "Elige una poción para usar:");
                                    for (int i = 0; i < pociones.Count; i++)
                                    {
                                        var p = (MiJuegoRPG.Objetos.Pocion)pociones[i].Objeto;
                                        ui.WriteLine($"{i + 1}. {p.Nombre} (+{p.Curacion} HP) x{pociones[i].Cantidad}");
                                    }
                                    var selP = InputService.LeerOpcion("Ingresa el número de la poción: ");
                                    if (int.TryParse(selP, out int idxP) && idxP > 0 && idxP <= pociones.Count)
                                    {
                                        var pocion = (MiJuegoRPG.Objetos.Pocion)pociones[idxP - 1].Objeto;
                                        if (ui.Confirm($"¿Usar {pocion.Nombre} para curar {pocion.Curacion} HP? (s/n): "))
                                        {
                                            var usar = new MiJuegoRPG.Motor.Acciones.UsarPocionAccion(idxP - 1);
                                            if (!TryEjecutarAccion(jugador, jugador, usar, out var msg))
                                            {
                                                if (!string.IsNullOrEmpty(msg)) ui.WriteLine(msg);
                                                continue; // si no se pudo (debería ser raro), no perder turno
                                            }
                                        }
                                        else
                                        {
                                            ui.WriteLine("Acción cancelada.");
                                            continue; // no perder turno al cancelar
                                        }
                                    }
                                    else
                                    {
                                        ui.WriteLine("Selección inválida.");
                                        continue; // no perder turno en selección inválida
                                    }
                                }
                                else
                                {
                                    ui.WriteLine("No puedes usar pociones con este combatiente.");
                                    continue; // mantener consistencia de gating
                                }
                            }
                            break;
                        case "3":
                            ui.WriteLine($"{jugador.Nombre} intenta huir...");
                            if (MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(100) < 60) // 60% probabilidad de huir
                            {
                                ui.WriteLine("¡Has logrado huir del combate!");
                                // Volver al menú de ubicación
                                return;
                            }
                            else
                            {
                                ui.WriteLine("No lograste huir, pierdes el turno.");
                            }
                            break;
                        case "4":
                            {
                                // Submenú de habilidades simples: Físico (por interfaz) o Mágico (cálculo si es Personaje)
                                ui.WriteLine("Habilidades:");
                                MiJuegoRPG.Interfaces.IAccionCombate tempFis = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                                MiJuegoRPG.Interfaces.IAccionCombate tempMag = new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion();
                                // Sufijos y marcado dinámico según estado
                                bool fisEnCd = actionRules.EstaEnCooldown(jugador, tempFis, out var cdfMenu) && cdfMenu > 0;
                                bool fisSinRec = !actionRules.TieneRecursos(jugador, tempFis, out var msgResFisMenu);
                                string sufFis = string.Empty;
                                if (fisEnCd) sufFis += $" (CD {cdfMenu})";
                                if (fisSinRec && !string.IsNullOrEmpty(msgResFisMenu)) sufFis += " (Sin maná)";
                                string marcaFis = (fisEnCd || fisSinRec) ? "[X] " : "";

                                bool magEnCd = actionRules.EstaEnCooldown(jugador, tempMag, out var cdmMenu) && cdmMenu > 0;
                                bool magSinRec = !actionRules.TieneRecursos(jugador, tempMag, out var msgResMagMenu);
                                string sufMag = string.Empty;
                                if (magEnCd) sufMag += $" (CD {cdmMenu})";
                                if (magSinRec && !string.IsNullOrEmpty(msgResMagMenu)) sufMag += " (Sin maná)";
                                string marcaMag = (magEnCd || magSinRec) ? "[X] " : "";

                                ui.WriteLine($"1. {marcaFis}{tempFis.Nombre} (Costo: {tempFis.CostoMana}, CD: {tempFis.CooldownTurnos}){sufFis}");
                                ui.WriteLine($"2. {marcaMag}{tempMag.Nombre} (Costo: {tempMag.CostoMana}, CD: {tempMag.CooldownTurnos}){sufMag}");
                                var selHab = InputService.LeerOpcion("Elige habilidad: ");
                                var enemigosVivos = enemigos.Where(e => e.EstaVivo).ToList();
                                if (enemigosVivos.Count == 0) { ui.WriteLine("No hay objetivos."); continue; }
                                ICombatiente? objetivo = null;
                                if (enemigosVivos.Count == 1) objetivo = enemigosVivos[0];
                                else
                                {
                                    UIStyle.Hint(ui, "Elige objetivo:");
                                    for (int i = 0; i < enemigosVivos.Count; i++)
                                        ui.WriteLine($"{i + 1}. {enemigosVivos[i].Nombre} ({enemigosVivos[i].Vida}/{enemigosVivos[i].VidaMaxima} HP)");
                                    var selObj = InputService.LeerOpcion("Número de objetivo: ");
                                    if (int.TryParse(selObj, out int idxO) && idxO > 0 && idxO <= enemigosVivos.Count)
                                        objetivo = enemigosVivos[idxO - 1];
                                }
                                if (objetivo == null) { ui.WriteLine("Selección inválida."); continue; }
                                if (selHab == "1")
                                {
                                    var accionFisica = new MiJuegoRPG.Motor.Acciones.AtaqueFisicoAccion();
                                    if (!TryEjecutarAccion(jugador, objetivo, accionFisica, out var msg))
                                    {
                                        ui.WriteLine(msg);
                                        continue;
                                    }
                                }
                                else if (selHab == "2")
                                {
                                    var accionMagica = new MiJuegoRPG.Motor.Acciones.AtaqueMagicoAccion();
                                    if (!TryEjecutarAccion(jugador, objetivo, accionMagica, out var msg))
                                    {
                                        ui.WriteLine(msg);
                                        continue;
                                    }
                                }
                                else
                                {
                                    ui.WriteLine("Habilidad no válida.");
                                    continue;
                                }
                            }
                            break;
                        case "5":
                            {
                                // Habilidad: Aplicar Veneno (demo)
                                var enemigosVivos = enemigos.Where(e => e.EstaVivo).ToList();
                                if (enemigosVivos.Count == 0) { ui.WriteLine("No hay objetivos."); continue; }
                                ICombatiente? objetivo = null;
                                if (enemigosVivos.Count == 1) objetivo = enemigosVivos[0];
                                else
                                {
                                    UIStyle.Hint(ui, "Elige objetivo:");
                                    for (int i = 0; i < enemigosVivos.Count; i++)
                                        ui.WriteLine($"{i + 1}. {enemigosVivos[i].Nombre} ({enemigosVivos[i].Vida}/{enemigosVivos[i].VidaMaxima} HP)");
                                    var selObj = InputService.LeerOpcion("Número de objetivo: ");
                                    if (int.TryParse(selObj, out int idxO) && idxO > 0 && idxO <= enemigosVivos.Count)
                                        objetivo = enemigosVivos[idxO - 1];
                                }
                                if (objetivo == null) { ui.WriteLine("Selección inválida."); continue; }
                                var veneno = new MiJuegoRPG.Motor.Acciones.AplicarVenenoAccion();
                                if (!TryEjecutarAccion(jugador, objetivo, veneno, out var msg))
                                {
                                    ui.WriteLine(msg);
                                    continue;
                                }
                            }
                            break;
                        default:
                            ui.WriteLine("Acción inválida, pierdes el turno.");
                            break;
                    }
                }
                else
                {
                    // Cada enemigo vivo ataca al jugador
                    foreach (var enemigo in enemigos.Where(e => e.EstaVivo))
                    {
                        UIStyle.SubHeader(ui, $"Turno de {enemigo.Nombre}");
                        var resolver = new DamageResolver();
                        var res = resolver.ResolverAtaqueFisico(enemigo, jugador);
                        foreach (var m in res.Mensajes) ui.WriteLine(m);
                        RegistrarEfectos(res);
                    }
                }

                // Mostrar estado después de cada turno
                MostrarEstadoCombate();
                // Aplicar efectos al inicio del siguiente turno (tick y limpiar expirados)
                ProcesarEfectos(jugador);
                foreach (var ene in enemigos) ProcesarEfectos(ene);
                // Avanzar cooldowns del actor que acaba de jugar
                if (turnoJugador)
                    actionRules.AvanzarCooldownsDe(jugador);
                else
                    foreach (var ene in enemigos.Where(e => e.EstaVivo)) actionRules.AvanzarCooldownsDe(ene);
                // Regeneración de maná (lenta) por turno para todos los combatientes vivos
                if (jugador.EstaVivo)
                {
                    var rec = actionRules.RegenerarManaTurno(jugador);
                    if (rec > 0) ui.WriteLine($"{jugador.Nombre} recupera {rec} de maná.");
                }
                foreach (var ene in enemigos.Where(e => e.EstaVivo))
                {
                    var rec = actionRules.RegenerarManaTurno(ene);
                    if (rec > 0) ui.WriteLine($"{ene.Nombre} recupera {rec} de maná.");
                }
                turnoJugador = !turnoJugador;
                turno++;
            }

            // Resultado del combate
            if (!jugador.EstaVivo)
            {
                ui.WriteLine($"{jugador.Nombre} ha sido derrotado.");
            }
            else
            {
                ui.WriteLine($"Has derrotado a todos los enemigos!");
                // Dar recompensas por cada enemigo derrotado
                foreach (var enemigo in enemigos)
                {
                    if (!enemigo.EstaVivo && jugador is MiJuegoRPG.Personaje.Personaje personaje && enemigo is MiJuegoRPG.Enemigos.Enemigo enemigoReal)
                    {
                        enemigoReal.DarRecompensas(personaje);
                    }
                }
            }
        }

        private void MostrarEstadoCombate()
        {
            UIStyle.Hint(ui, "\n=== ESTADO DEL COMBATE ===");
            var infoJugador = $"{jugador.Nombre}: {jugador.Vida}/{jugador.VidaMaxima} HP";
            if (jugador is MiJuegoRPG.Personaje.Personaje pj)
            {
                infoJugador += $" | Mana {pj.ManaActual}/{pj.ManaMaxima}";
            }
            if (efectosActivos.TryGetValue(jugador, out var efJ) && efJ.Count > 0)
            {
                infoJugador += " | Efectos: " + string.Join(", ", efJ.Select(e => $"{e.Nombre}({e.TurnosRestantes}t)"));
            }
            ui.WriteLine(infoJugador);
            foreach (var enemigo in enemigos)
            {
                var infoE = $"{enemigo.Nombre}: {enemigo.Vida}/{enemigo.VidaMaxima} HP";
                if (efectosActivos.TryGetValue(enemigo, out var efE) && efE.Count > 0)
                {
                    infoE += " | Efectos: " + string.Join(", ", efE.Select(e => $"{e.Nombre}({e.TurnosRestantes}t)"));
                }
                ui.WriteLine(infoE);
            }
            ui.WriteLine("----------------------------------------\n");
        }

        private void RegistrarEfectos(MiJuegoRPG.Interfaces.ResultadoAccion res)
        {
            if (res.EfectosAplicados == null || res.EfectosAplicados.Count == 0) return;
            if (!efectosActivos.TryGetValue(res.Objetivo, out var lista))
            {
                lista = new List<MiJuegoRPG.Interfaces.IEfecto>();
                efectosActivos[res.Objetivo] = lista;
            }
            lista.AddRange(res.EfectosAplicados);
        }

        private void ProcesarEfectos(ICombatiente objetivo)
        {
            if (!efectosActivos.TryGetValue(objetivo, out var lista) || lista.Count == 0) return;
            var restantes = new List<MiJuegoRPG.Interfaces.IEfecto>();
            foreach (var ef in lista)
            {
                foreach (var msg in ef.Tick(objetivo)) ui.WriteLine(msg);
                if (ef.AvanzarTurno()) restantes.Add(ef);
                else ui.WriteLine($"El efecto {ef.Nombre} en {objetivo.Nombre} se disipa.");
            }
            efectosActivos[objetivo] = restantes;
        }

        /// <summary>
        /// Ejecuta una acción solo si está disponible (sin cooldown activo y con recursos suficientes).
        /// En éxito aplica cooldown, consume recursos y registra efectos. Devuelve true si se ejecutó.
        /// Si falla, no consume turno en el menú llamante (éste debe usar 'continue').
        /// </summary>
        internal bool TryEjecutarAccion(ICombatiente actor, ICombatiente objetivo, MiJuegoRPG.Interfaces.IAccionCombate accion, out string mensaje)
        {
            mensaje = string.Empty;
            if (actionRules.EstaEnCooldown(actor, accion, out var cd) && cd > 0)
            {
                mensaje = $"Habilidad en cooldown por {cd} turnos.";
                return false;
            }
            if (!actionRules.TieneRecursos(actor, accion, out var msg))
            {
                mensaje = string.IsNullOrEmpty(msg) ? "Recursos insuficientes." : msg;
                return false;
            }
            if (!actionRules.ConsumirRecursos(actor, accion))
            {
                mensaje = "No se pudieron consumir los recursos requeridos.";
                return false;
            }
            var res = accion.Ejecutar(actor, objetivo);
            foreach (var m in res.Mensajes) ui.WriteLine(m);
            RegistrarEfectos(res);
            actionRules.AplicarCooldown(actor, accion);
            return true;
        }

        // (Cooldowns) Lógica movida a ActionRulesService
    }
}