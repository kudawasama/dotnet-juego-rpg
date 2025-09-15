using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public static class EstadoPersonajePrinter
    {
        public static void MostrarEstadoPersonaje(Personaje.Personaje pj)
        {
            var ui = Juego.ObtenerInstanciaActual()?.Ui;
            var write = new Action<string>(s => { if (ui != null) ui.WriteLine(s); else Console.WriteLine(s); });
            write("\n=== ESTADO DEL PERSONAJE ===");
            write($"Nombre: {pj.Nombre}");
            write($"Clase: {(pj.Clase != null ? pj.Clase.Nombre : "Sin clase")}");
            write($"Título: {pj.Titulo}");
            write($"Nivel: {pj.Nivel}");
            write($"Vida: {pj.Vida}/{pj.VidaMaxima}");
            write($"Maná: {pj.ManaActual}/{pj.ManaMaxima}");
            write($"Energía: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
            write($"Oro: {pj.Oro}");
            int expActual = pj.Experiencia;
            int expSiguiente = pj.ExperienciaSiguienteNivel;
            int expFaltante = expSiguiente - expActual;
            double porcentaje = expSiguiente > 0 ? (double)expActual / expSiguiente * 100.0 : 0.0;
            write($"Experiencia: {expActual} / {expSiguiente} (Faltan {expFaltante})");
            write($"Progreso al siguiente nivel: {porcentaje:F2}%");
            write($"Descansos realizados hoy: {pj.DescansosHoy}");
            write("\n--- Atributos Base ---");
            write("===================================");
            var ab = pj.AtributosBase;
            var atributos = new Dictionary<string, (string abrev, double valor, double exp, double req)>
            {
                {"Fuerza", ("Fza", ab.Fuerza, pj.ExperienciaAtributos[Dominio.Atributo.Fuerza].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Fuerza].Requerida)},
                {"Destreza", ("Dxt", ab.Destreza, pj.ExperienciaAtributos[Dominio.Atributo.Destreza].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Destreza].Requerida)},
                {"Vitalidad", ("Vit", ab.Vitalidad, pj.ExperienciaAtributos[Dominio.Atributo.Vitalidad].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Vitalidad].Requerida)},
                {"Agilidad", ("Agi", ab.Agilidad, pj.ExperienciaAtributos[Dominio.Atributo.Agilidad].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Agilidad].Requerida)},
                {"Suerte", ("Srt", ab.Suerte, pj.ExperienciaAtributos[Dominio.Atributo.Suerte].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Suerte].Requerida)},
                {"Defensa", ("Def", ab.Defensa, pj.ExperienciaAtributos[Dominio.Atributo.Defensa].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Defensa].Requerida)},
                {"Resistencia", ("Res", ab.Resistencia, pj.ExperienciaAtributos[Dominio.Atributo.Resistencia].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Resistencia].Requerida)},
                {"Sabiduría", ("Sab", ab.Sabiduría, 0, 1)},
                {"Inteligencia", ("Int", ab.Inteligencia, pj.ExperienciaAtributos[Dominio.Atributo.Inteligencia].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Inteligencia].Requerida)},
                {"Percepción", ("Per", ab.Percepcion, pj.ExperienciaAtributos[Dominio.Atributo.Percepcion].Progreso, pj.ExperienciaAtributos[Dominio.Atributo.Percepcion].Requerida)},
                {"Persuasión", ("Prs", ab.Persuasion, 0, 1)},
                {"Liderazgo", ("Lid", ab.Liderazgo, 0, 1)},
                {"Carisma", ("Car", ab.Carisma, 0, 1)},
                {"Voluntad", ("Vol", ab.Voluntad, 0, 1)}
            };
            foreach (var atributo in atributos)
            {
                string abrev = atributo.Value.abrev;
                double valor = atributo.Value.valor;
                double exp = atributo.Value.exp;
                double req = atributo.Value.req;
                double bonificador = pj.ObtenerBonificadorAtributo(atributo.Key);
                double total = valor + bonificador;
                double prog = req > 0 ? exp / req * 100.0 : 0.0;
                double faltante = req - exp;
                string textoProg = req > 1 ? $" ({prog:F2}% de {req}, faltan {faltante:F2})" : "";
                write($"{abrev}: {total} (Base: {valor}, Bonif: {bonificador}){textoProg}");
            }
            write("\n--- Estadísticas Físicas ---");
            var est = pj.Estadisticas;
            var estadisticasFisicas = new Dictionary<string, double> {
                {"Ataque", est.Ataque}, {"Defensa Física", est.DefensaFisica}, {"Daño", est.Daño}, {"Crítico", est.Critico},
                {"Evasión", est.Evasion}, {"Velocidad", est.Velocidad}, {"Regeneración", est.Regeneracion}, {"Salud", est.Salud},
                {"Energía", est.Energia}, {"Carga", est.Carga}, {"Poder Ofensivo Físico", est.PoderOfensivoFisico}, {"Poder Defensivo Físico", est.PoderDefensivoFisico}
            };
            foreach (var stat in estadisticasFisicas)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                write($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    write($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        write($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            write("\n--- Estadísticas Mágicas ---");
            var estadisticasMagicas = new Dictionary<string, double> {
                {"Poder Mágico", est.PoderMagico}, {"Defensa Mágica", est.DefensaMagica}, {"Regeneración Mana", est.RegeneracionMana},
                {"Mana", est.Mana}, {"Poder Ofensivo Mágico", est.PoderOfensivoMagico}, {"Poder Defensivo Mágico", est.PoderDefensivoMagico},
                {"Afinidad Elemental", est.AfinidadElemental}, {"Poder Elemental", est.PoderElemental}, {"Resistencia Elemental", est.ResistenciaElemental},
                {"Resistencia Mágica", est.ResistenciaMagica}
            };
            foreach (var stat in estadisticasMagicas)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                write($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    write($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        write($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            write("\n--- Estadísticas Espirituales y Especiales ---");
            var estadisticasEspeciales = new Dictionary<string, double> {
                {"Poder Espiritual", est.PoderEspiritual}, {"Poder Curativo", est.PoderCurativo}, {"Poder de Soporte", est.PoderDeSoporte},
                {"Poder de Control", est.PoderDeControl}, {"Poder de Invocación", est.PoderDeInvocacion}, {"Poder de Transmutación", est.PoderDeTransmutacion},
                {"Poder de Alteración", est.PoderDeAlteracion}, {"Poder de Ilusión", est.PoderDeIlusion}, {"Poder de Conjuración", est.PoderDeConjuracion},
                {"Poder de Destrucción", est.PoderDeDestruccion}, {"Poder de Restauración", est.PoderDeRestauracion}, {"Poder de Transporte", est.PoderDeTransporte},
                {"Poder de Manipulación", est.PoderDeManipulacion}
            };
            foreach (var stat in estadisticasEspeciales)
            {
                double bonificador = pj.ObtenerBonificadorEstadistica(stat.Key);
                double total = stat.Value + bonificador;
                write($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    write($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        write($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            // Indicadores de Supervivencia (27.9): Hambre/Sed/Fatiga/Temperatura con avisos por umbral
            try
            {
                var juego = Juego.ObtenerInstanciaActual();
                var sup = juego?.supervivenciaService;
                if (sup != null)
                {
                    var (wH, wS, wF) = sup.ObtenerUmbralesAdvertencia();
                    var (cH, cS, cF) = sup.ObtenerUmbralesCriticos();
                    string Bar(double v)
                    {
                        // Barra simple 10 segmentos
                        int llenos = (int)Math.Round(v * 10);
                        llenos = Math.Max(0, Math.Min(10, llenos));
                        return new string('#', llenos).PadRight(10, '-');
                    }
                    double pct(double v) => Math.Clamp(v, 0.0, 1.0) * 100.0;

                    write("\n--- Supervivencia ---");
                    var etH = sup.EtiquetaDesdeUmbrales(pj.Hambre, wH, cH);
                    var etS = sup.EtiquetaDesdeUmbrales(pj.Sed, wS, cS);
                    var etF = sup.EtiquetaDesdeUmbrales(pj.Fatiga, wF, cF);
                    write($"Hambre: {pct(pj.Hambre):F0}% [{Bar(pj.Hambre)}] ({etH})");
                    write($"Sed:    {pct(pj.Sed):F0}% [{Bar(pj.Sed)}] ({etS})");
                    write($"Fatiga: {pct(pj.Fatiga):F0}% [{Bar(pj.Fatiga)}] ({etF})");

                    // Temperatura ambiente percibida
                    double t = pj.TempActual;
                    string estadoTemp = sup.EstadoTemperatura(t);
                    write($"Temperatura: {t:F1} °C ({estadoTemp})");
                }
            }
            catch { /* Evitar romper UI si falta config */ }
            if (!InputService.TestMode)
            {
                var ui2 = Juego.ObtenerInstanciaActual()?.Ui;
                if (ui2 != null) ui2.Pause("\nPresiona cualquier tecla para continuar...");
                else { Console.WriteLine("\nPresiona cualquier tecla para continuar..."); Console.ReadKey(); }
            }
        }
    }
}
