using System;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Dominio;

namespace MiJuegoRPG.Motor.Servicios
{
    /// Servicio responsable de aplicar experiencia a atributos según reglas actuales.
    /// NOTA: Centraliza toda la progresión (recolección, entrenamiento, exploración) para evitar lógica duplicada.
    /// Ajustable vía progression.json (valores base y escalados). Factor mínimo evita que la exp llegue a 0.
    public class ProgressionService
    {
    private double expBaseRecoleccion = 0.01;
    private double expBaseEntrenamiento = 0.01;
    private double expBaseExploracion = 0.0025; // valor pequeño incremental para exploración
    // Parametrización extendida (3.3)
    private double escaladoNivelRecoleccion = 1.05;
    private double escaladoNivelEntrenamiento = 1.05;
    private double escaladoNivelExploracion = 1.03;
    private double factorMinExp = 0.0001;
        private readonly Dictionary<Atributo, double> indiceAtributo = new();
        public bool Verbose { get; set; } = true; // permite silenciar mensajes

    private record ProgressionConfig(double ExpBaseRecoleccion, double ExpBaseEntrenamiento, double? ExpBaseExploracion, Dictionary<string,double> Indices, double? EscaladoNivelRecoleccion, double? EscaladoNivelEntrenamiento, double? EscaladoNivelExploracion, double? FactorMinExp);

    public ProgressionService(string? rutaConfig = null) // Carga config de progresión (o usa valores por defecto)
        {
            try
            {
                rutaConfig ??= PathProvider.CombineData("progression.json");
                if (System.IO.File.Exists(rutaConfig))
                {
                    var json = System.IO.File.ReadAllText(rutaConfig);
                    var cfg = System.Text.Json.JsonSerializer.Deserialize<ProgressionConfig>(json);
                    if (cfg != null)
                    {
                        expBaseRecoleccion = cfg.ExpBaseRecoleccion;
                        expBaseEntrenamiento = cfg.ExpBaseEntrenamiento;
                        if (cfg.ExpBaseExploracion.HasValue) expBaseExploracion = cfg.ExpBaseExploracion.Value;
                        if (cfg.EscaladoNivelRecoleccion.HasValue) escaladoNivelRecoleccion = cfg.EscaladoNivelRecoleccion.Value;
                        if (cfg.EscaladoNivelEntrenamiento.HasValue) escaladoNivelEntrenamiento = cfg.EscaladoNivelEntrenamiento.Value;
                        if (cfg.EscaladoNivelExploracion.HasValue) escaladoNivelExploracion = cfg.EscaladoNivelExploracion.Value;
                        if (cfg.FactorMinExp.HasValue) factorMinExp = cfg.FactorMinExp.Value;
                        indiceAtributo.Clear();
                        foreach (var kv in cfg.Indices)
                        {
                            if (Enum.TryParse<Atributo>(kv.Key, true, out var atr))
                                indiceAtributo[atr] = kv.Value;
                        }
                    }
                }
                // Valores por defecto mínimos si faltan
                foreach (Atributo atr in Enum.GetValues(typeof(Atributo)))
                    if (!indiceAtributo.ContainsKey(atr)) indiceAtributo[atr] = 3.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProgressionService] Error cargando configuración: {ex.Message}. Usando valores por defecto.");
                if (indiceAtributo.Count == 0)
                {
                    indiceAtributo[Atributo.Fuerza] = 3.0;
                    indiceAtributo[Atributo.Inteligencia] = 8.0;
                }
            }
        }

    public void AplicarExpRecoleccion(Personaje.Personaje pj, TipoRecoleccion tipo) // Aplica exp a dos atributos según el tipo de recolección
        {
            if (pj == null) return;
            double expBase = expBaseRecoleccion; // configurable
            double indiceNivel = Math.Pow(escaladoNivelRecoleccion, pj.Nivel - 1);
            int minutos = 1; // cada acción = 1 minuto virtual (igual que antes)

            switch (tipo)
            {
                case TipoRecoleccion.Recolectar:
                    // Percepción + Inteligencia
                    GanarAtributoFraccion(pj, Atributo.Percepcion, expBase, indiceNivel, minutos);
                    GanarAtributoFraccion(pj, Atributo.Inteligencia, expBase, indiceNivel, minutos);
                    break;
                case TipoRecoleccion.Minar:
                    // Fuerza + Resistencia
                    GanarAtributoFraccion(pj, Atributo.Fuerza, expBase, indiceNivel, minutos);
                    GanarAtributoFraccion(pj, Atributo.Resistencia, expBase, indiceNivel, minutos);
                    break;
                case TipoRecoleccion.Talar:
                    // Fuerza + Destreza
                    GanarAtributoFraccion(pj, Atributo.Fuerza, expBase, indiceNivel, minutos);
                    GanarAtributoFraccion(pj, Atributo.Destreza, expBase, indiceNivel, minutos);
                    break;
            }
        }

        /// Aplica micro-experiencia por exploración de sector.
        /// Regla: Percepción siempre gana una fracción; si es primera visita, pequeño bonus a Agilidad.
    public void AplicarExpExploracion(Personaje.Personaje pj, bool primeraVisita) // Micro exp al explorar, bonus si primera vez
        {
            if (pj == null) return;
            double indiceNivel = Math.Pow(escaladoNivelExploracion, pj.Nivel - 1);
            double basePercepcion = expBaseExploracion / indiceNivel;
            if (basePercepcion < 0.00005) basePercepcion = 0.00005;
            IncrementarAtributo(pj, Atributo.Percepcion, basePercepcion);
            if (Verbose)
                Console.WriteLine($"Exploración: +{basePercepcion:F5} Percepción");
            if (primeraVisita)
            {
                double bonusAgilidad = basePercepcion * 0.5;
                IncrementarAtributo(pj, Atributo.Agilidad, bonusAgilidad);
                if (Verbose)
                    Console.WriteLine($"Exploración (primera visita): +{bonusAgilidad:F5} Agilidad");
            }
        }

        /// Aplica progreso de entrenamiento para un atributo específico (equivalente a Personaje.Entrenar pero centralizado).
        /// Cada invocación simula 1 "minuto" virtual tal como hacía la versión previa.
    public void AplicarEntrenamiento(Personaje.Personaje pj, Atributo atributo, int minutos = 1) // Simula entrenamiento incremental por "minuto virtual"
        {
            if (pj == null) return;
            double expBase = expBaseEntrenamiento;
            double indiceNivel = Math.Pow(escaladoNivelEntrenamiento, pj.Nivel - 1);
            double indiceAtr = indiceAtributo.TryGetValue(atributo, out var idx) ? idx : 1.0;
            double valorActual = ObtenerValorAtributo(pj, atributo);
            double factorEscalado = 0.05; // igual que antes
            double expPorMinuto = expBase / (indiceNivel * indiceAtr * (1 + valorActual * factorEscalado));
            if (expPorMinuto < 0.0001) expPorMinuto = 0.0001;
            for (int i = 0; i < minutos; i++)
            {
                var expData = pj.ExperienciaAtributos[atributo];
                expData.Progreso += expPorMinuto;
                MostrarProgreso(atributo.ToString(), expData.Progreso, expData.Requerida);
                if (expData.Progreso >= expData.Requerida)
                {
                    IncrementarAtributoBase(pj, atributo, 1);
                    expData.Progreso = 0;
                    expData.Requerida *= 1.2;
                    MostrarSubida(atributo.ToString(), ObtenerValorAtributo(pj, atributo), expData.Requerida);
                }
            }
        }

        private void MostrarProgreso(string nombre, double exp, double req)
        {
            if (Verbose)
                Console.WriteLine($"Entrenando {nombre}... Progreso: {exp:F5}/{req:F2}");
        }

        private void MostrarSubida(string nombre, double nuevoValor, double reqNueva)
        {
            Console.WriteLine($"¡{nombre} subió a {nuevoValor}! Próximo nivel requiere {reqNueva:F2} exp.");
            // Emitir evento de subida (atributo). "nombre" aquí es el string del enum.
            try { BusEventos.Instancia.Publicar(new EventoAtributoSubido(nombre, nuevoValor)); } catch { /* silencioso */ }
        }

    private void GanarAtributoFraccion(Personaje.Personaje pj, Atributo atributo, double expBase, double indiceNivel, int minutos) // Fórmula fraccional escalada por nivel y valor actual
        {
            double valorActual = ObtenerValorAtributo(pj, atributo);
            if (valorActual <= 0) valorActual = 1.0; // salvaguarda como en código original
            double indice = 1.0 + (valorActual / 10.0);
            double expFraccion = expBase / (indiceNivel * indice);
            if (expFraccion < factorMinExp) expFraccion = factorMinExp; // mínimo configurable
            // En la implementación previa se acumulaba en ExpX y luego se volcaba; aquí sumamos directo para simplificar.
            IncrementarAtributo(pj, atributo, expFraccion * minutos);
            if (Verbose)
                Console.WriteLine($"Has ganado {expFraccion * minutos:F4} exp de {atributo}.");
        }

        private static double ObtenerValorAtributo(Personaje.Personaje pj, Atributo atr)
        {
            var a = pj.AtributosBase; //a significa: atributos base del personaje. a.Fuerza significa el valor base de fuerza
            return atr switch
            {
                Atributo.Fuerza => a.Fuerza,
                Atributo.Inteligencia => a.Inteligencia,
                Atributo.Destreza => a.Destreza,
                Atributo.Resistencia => a.Resistencia,
                Atributo.Defensa => a.Defensa,
                Atributo.Vitalidad => a.Vitalidad,
                Atributo.Agilidad => a.Agilidad,
                Atributo.Suerte => a.Suerte,
                Atributo.Percepcion => a.Percepcion,
                _ => 0
            };
        }

        private static void IncrementarAtributo(Personaje.Personaje pj, Atributo atr, double cantidad)
        {
            var a = pj.AtributosBase;
            switch (atr)
            {
                case Atributo.Fuerza: a.Fuerza += cantidad; break;
                case Atributo.Inteligencia: a.Inteligencia += cantidad; break;
                case Atributo.Destreza: a.Destreza += cantidad; break;
                case Atributo.Resistencia: a.Resistencia += cantidad; break;
                case Atributo.Defensa: a.Defensa += cantidad; break;
                case Atributo.Vitalidad: a.Vitalidad += cantidad; break;
                case Atributo.Agilidad: a.Agilidad += cantidad; break;
                case Atributo.Suerte: a.Suerte += cantidad; break;
                case Atributo.Percepcion: a.Percepcion += cantidad; break;
            }
        }

        private static void IncrementarAtributoBase(Personaje.Personaje pj, Atributo atr, double incremento)
        {
            IncrementarAtributo(pj, atr, incremento);
        }
    }
}
