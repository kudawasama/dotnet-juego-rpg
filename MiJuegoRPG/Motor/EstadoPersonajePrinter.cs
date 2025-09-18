using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor.Servicios;

namespace MiJuegoRPG.Motor
{
    public static class EstadoPersonajePrinter
    {
        public static void MostrarEstadoPersonaje(Personaje.Personaje pj, bool detallado = false)
        {
            var juego = Juego.ObtenerInstanciaActual();
            var ui = juego?.Ui;
            void W(string s) { if (ui != null) ui.WriteLine(s); else Console.WriteLine(s); }

            // Encabezado profesional
            UIStyle.Header(ui!, "Estado del Personaje");
            UIStyle.Hint(ui!, juego?.FormatoRelojMundo ?? "");
            // Resumen compacto
            var clase = pj.Clase?.Nombre ?? "Sin clase";
            UIStyle.SubHeader(ui!, $"{pj.Nombre} — {clase} • Nivel {pj.Nivel} • {pj.Titulo}");

            // Helper barra 20 segmentos
            string Bar(int actual, int max)
            {
                max = Math.Max(1, max);
                double ratio = Math.Clamp(actual / (double)max, 0.0, 1.0);
                int llenos = (int)System.Math.Round(ratio * 20);
                return new string('#', llenos).PadRight(20, '-');
            }

            // Recursos principales
            W($"Vida   [{Bar(pj.Vida, pj.VidaMaxima)}] {pj.Vida}/{pj.VidaMaxima}");
            W($"Maná   [{Bar(pj.ManaActual, pj.ManaMaxima)}] {pj.ManaActual}/{pj.ManaMaxima}");
            W($"Energía[{Bar(pj.EnergiaActual, pj.EnergiaMaxima)}] {pj.EnergiaActual}/{pj.EnergiaMaxima}");

            // Experiencia
            int expActual = pj.Experiencia;
            int expSiguiente = pj.ExperienciaSiguienteNivel;
            int expFaltante = System.Math.Max(0, expSiguiente - expActual);
            double pct = expSiguiente > 0 ? (double)expActual / expSiguiente : 0.0;
            W($"XP     [{new string('#', (int)System.Math.Round(pct * 20)).PadRight(20, '-')}] {expActual}/{expSiguiente} (faltan {expFaltante})");
            W($"Oro: {pj.Oro} • Descansos hoy: {pj.DescansosHoy}");

            // Atributos clave (compacto) con progreso
            UIStyle.SubHeader(ui!, "Atributos");
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
            foreach (var kv in atributos)
            {
                var nombre = kv.Key; var (abrev, valor, exp, req) = kv.Value;
                double bonif = pj.ObtenerBonificadorAtributo(nombre);
                double total = valor + bonif;
                string p = req > 1 ? $" ({(exp/req*100.0):F1}% XP)" : string.Empty;
                W($"{abrev}: {total:F2} (Base {valor:F2}{(bonif!=0? $", +{bonif:F2}": "")}){p}");
            }

            // Estadísticas clave (resumidas)
            UIStyle.SubHeader(ui!, "Estadísticas");
            var est = pj.Estadisticas;
            void Stat(string nombre, double baseVal)
            {
                double bon = pj.ObtenerBonificadorEstadistica(nombre);
                double tot = baseVal + bon;
                W($"{nombre}: {tot:F2}{(bon!=0? $" (Base {baseVal:F2}, +{bon:F2})" : "")}");
            }
            Stat("Ataque", est.Ataque);
            Stat("Defensa Física", est.DefensaFisica);
            Stat("Poder Mágico", est.PoderMagico);
            Stat("Defensa Mágica", est.DefensaMagica);
            Stat("Crítico", est.Critico);
            Stat("Evasión", est.Evasion);
            Stat("Velocidad", est.Velocidad);
            Stat("Regeneración", est.Regeneracion);
            Stat("Regeneración Mana", est.RegeneracionMana);
            Stat("Resistencia Elemental", est.ResistenciaElemental);
            Stat("Resistencia Mágica", est.ResistenciaMagica);

            // Supervivencia (compacto en sección)
            try
            {
                var sup = juego?.supervivenciaService;
                if (sup != null)
                {
                    UIStyle.SubHeader(ui!, "Supervivencia");
                    var (wH, wS, wF) = sup.ObtenerUmbralesAdvertencia();
                    var (cH, cS, cF) = sup.ObtenerUmbralesCriticos();
                    string BarFrac(double v)
                    {
                        int llenos = (int)Math.Round(Math.Clamp(v,0.0,1.0) * 10);
                        llenos = Math.Max(0, Math.Min(10, llenos));
                        return new string('#', llenos).PadRight(10, '-');
                    }
                    double Pct(double v) => Math.Clamp(v, 0.0, 1.0) * 100.0;
                    var etH = sup.EtiquetaDesdeUmbrales(pj.Hambre, wH, cH);
                    var etS = sup.EtiquetaDesdeUmbrales(pj.Sed, wS, cS);
                    var etF = sup.EtiquetaDesdeUmbrales(pj.Fatiga, wF, cF);
                    W($"Hambre: {Pct(pj.Hambre):F0}% [{BarFrac(pj.Hambre)}] ({etH})");
                    W($"Sed:    {Pct(pj.Sed):F0}% [{BarFrac(pj.Sed)}] ({etS})");
                    W($"Fatiga: {Pct(pj.Fatiga):F0}% [{BarFrac(pj.Fatiga)}] ({etF})");
                    double t = pj.TempActual;
                    string estadoTemp = sup.EstadoTemperatura(t);
                    W($"Temperatura: {t:F1} °C ({estadoTemp})");
                }
            }
            catch { /* Evitar romper UI si falta config */ }

            // Si se solicita modo detallado, mostrar equipo equipado con formato compacto-profesional
            if (detallado)
            {
                try
                {
                    UIStyle.SubHeader(ui!, "Equipo");
                    var eq = pj.Inventario.Equipo;
                    void Slot(string nombre, MiJuegoRPG.Objetos.Objeto? obj)
                    {
                        if (obj == null) { W($"{nombre}: —"); return; }
                        // Línea principal: Nombre del objeto
                        W($"{nombre}: {obj.Nombre}");
                        // Detalles según tipo conocido (si aplica)
                        switch (obj)
                        {
                            case MiJuegoRPG.Objetos.Arma arma:
                                W($"  Daño Físico: {arma.DañoFisico} • Daño Mágico: {arma.DañoMagico}");
                                W($"  Rareza: {arma.Rareza} • Perfección: {arma.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Casco casco:
                                W($"  Rareza: {casco.Rareza} • Perfección: {casco.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Armadura arm:
                                W($"  Rareza: {arm.Rareza} • Perfección: {arm.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Pantalon pan:
                                W($"  Rareza: {pan.Rareza} • Perfección: {pan.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Botas bot:
                                W($"  Rareza: {bot.Rareza} • Perfección: {bot.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Collar col:
                                W($"  Rareza: {col.Rareza} • Perfección: {col.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Cinturon cin:
                                W($"  Rareza: {cin.Rareza} • Perfección: {cin.Perfeccion}");
                                break;
                            case MiJuegoRPG.Objetos.Accesorio acc:
                                W($"  Rareza: {acc.Rareza} • Perfección: {acc.Perfeccion}");
                                break;
                        }
                    }
                    Slot("Arma", eq.Arma);
                    Slot("Casco", eq.Casco);
                    Slot("Armadura", eq.Armadura);
                    Slot("Pantalón", eq.Pantalon);
                    Slot("Zapatos", eq.Zapatos);
                    Slot("Collar", eq.Collar);
                    Slot("Cinturón", eq.Cinturon);
                    Slot("Accesorio 1", eq.Accesorio1);
                    Slot("Accesorio 2", eq.Accesorio2);
                }
                catch { /* tolerante: si falta algún tipo, omitir detalles */ }
            }

            if (!InputService.TestMode) ui?.Pause("\nPresiona cualquier tecla para continuar...");
        }
    }
}
