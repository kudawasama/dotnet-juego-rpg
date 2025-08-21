using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public static class EstadoPersonajePrinter
    {
        public static void MostrarEstadoPersonaje(Personaje.Personaje pj)
        {
            Console.WriteLine("\n=== ESTADO DEL PERSONAJE ===");
            Console.WriteLine($"Nombre: {pj.Nombre}");
            Console.WriteLine($"Clase: {(pj.Clase != null ? pj.Clase.Nombre : "Sin clase")}");
            Console.WriteLine($"Título: {pj.Titulo}");
            Console.WriteLine($"Nivel: {pj.Nivel}");
            Console.WriteLine($"Vida: {pj.Vida}/{pj.VidaMaxima}");
            Console.WriteLine($"Maná: {pj.ManaActual}/{pj.ManaMaxima}");
            Console.WriteLine($"Energía: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
            Console.WriteLine($"Oro: {pj.Oro}");
            int expActual = pj.Experiencia;
            int expSiguiente = pj.ExperienciaSiguienteNivel;
            int expFaltante = expSiguiente - expActual;
            double porcentaje = expSiguiente > 0 ? (double)expActual / expSiguiente * 100.0 : 0.0;
            Console.WriteLine($"Experiencia: {expActual} / {expSiguiente} (Faltan {expFaltante})");
            Console.WriteLine($"Progreso al siguiente nivel: {porcentaje:F2}%");
            Console.WriteLine($"Descansos realizados hoy: {pj.DescansosHoy}");
            Console.WriteLine("\n--- Atributos Base ---");
            Console.WriteLine("===================================");
            var ab = pj.AtributosBase;
            var atributos = new Dictionary<string, (string abrev, double valor, double exp, double req)> {
                {"Fuerza", ("Fza", ab.Fuerza, pj.ExpFuerza, pj.FuerzaExpRequerida)},
                {"Destreza", ("Dxt", ab.Destreza, pj.ExpDestreza, pj.DestrezaExpRequerida)},
                {"Vitalidad", ("Vit", ab.Vitalidad, pj.ExpVitalidad, pj.VitalidadExpRequerida)},
                {"Agilidad", ("Agi", ab.Agilidad, pj.ExpAgilidad, pj.AgilidadExpRequerida)},
                {"Suerte", ("Srt", ab.Suerte, pj.ExpSuerte, pj.SuerteExpRequerida)},
                {"Defensa", ("Def", ab.Defensa, pj.ExpDefensa, pj.DefensaExpRequerida)},
                {"Resistencia", ("Res", ab.Resistencia, pj.ExpResistencia, pj.ResistenciaExpRequerida)},
                {"Sabiduría", ("Sab", ab.Sabiduría, 0, 1)},
                {"Inteligencia", ("Int", ab.Inteligencia, pj.ExpInteligencia, pj.InteligenciaExpRequerida)},
                {"Percepción", ("Per", ab.Percepcion, pj.ExpPercepcion, pj.PercepcionExpRequerida)},
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
                Console.WriteLine($"{abrev}: {total} (Base: {valor}, Bonif: {bonificador}){textoProg}");
            }
            Console.WriteLine("\n--- Estadísticas Físicas ---");
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
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\n--- Estadísticas Mágicas ---");
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
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\n--- Estadísticas Espirituales y Especiales ---");
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
                Console.WriteLine($"{stat.Key}: {stat.Value:F2} ({total:F2})");
                if (bonificador > 0)
                {
                    var fuentes = pj.ObtenerFuentesBonificadorEstadistica(stat.Key);
                    Console.WriteLine($"  Bonificador por equipo:");
                    foreach (var fuente in fuentes)
                    {
                        Console.WriteLine($"    {fuente.Nombre}: +{fuente.Valor}");
                    }
                }
            }

            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}
