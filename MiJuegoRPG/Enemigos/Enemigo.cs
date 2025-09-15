using System;
using System.Collections.Generic;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Enemigos
{
    // Clase base abstracta para todos los enemigos del juego.
    public abstract class Enemigo : ICombatiente, IEvadible
    {

        // Implementación básica de ICombatiente
        public string Nombre { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public int Defensa { get; private set; }
        public int DefensaMagica { get; set; }
        public bool EstaVivo => Vida > 0;
        public int Ataque { get; set; }
    // Etiqueta simple de tipo (p.ej., "lobo", "rata", "golem") para contadores/encuentros
    public string? Tag { get; set; }
    // Mitigaciones porcentuales adicionales (0..1). No sustituyen Defensa, se aplican después.
    public double MitigacionFisicaPorcentaje { get; set; } = 0.0; // ej. 0.2 = 20%
    public double MitigacionMagicaPorcentaje { get; set; } = 0.0;
    // Inmunidades por palabra clave ("veneno", "sangrado", etc.)
    public Dictionary<string, bool> Inmunidades { get; private set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
    // Resistencias elementales específicas 0..0.9 (mitigación adicional por tipo)
    public Dictionary<string, double> ResistenciasElementales { get; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    // Daño elemental plano adicional por tipo (informativo para futuro cálculo detallado)
    public Dictionary<string, int> DanioElementalBase { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Evasión (0..0.95) configurable, por tipo de ataque
        public double EvasionFisica { get; set; } = 0.0;
        public double EvasionMagica { get; set; } = 0.0;
        public bool IntentarEvadir(bool esAtaqueMagico)
        {
            var rng = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            double p = esAtaqueMagico ? EvasionMagica : EvasionFisica;
            p = Math.Clamp(p, 0.0, 0.95);
            return rng.NextDouble() < p;
        }

        // Implementación explícita de los métodos de ICombatiente
        public virtual int AtacarFisico(ICombatiente objetivo)
        {
            // Chequeo de evasión del objetivo
            if (objetivo is IEvadible evasivo && evasivo.IntentarEvadir(false))
            {
                var ui = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual()?.Ui;
                ui?.WriteLine($"¡{objetivo.Nombre} evadió el ataque físico de {Nombre}!");
                return 0;
            }
            int danio = Ataque;
            objetivo.RecibirDanioFisico(danio);
            return danio;
        }

        public virtual int AtacarMagico(ICombatiente objetivo)
        {
            if (objetivo is IEvadible evasivo && evasivo.IntentarEvadir(true))
            {
                var ui = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual()?.Ui;
                ui?.WriteLine($"¡{objetivo.Nombre} evadió el hechizo de {Nombre}!");
                return 0;
            }
            int danio = Ataque; // Puedes ajustar la lógica si tienes un atributo de ataque mágico
            objetivo.RecibirDanioMagico(danio);
            return danio;
        }

        public virtual void RecibirDanioFisico(int danioFisico)
        {
            int danioTrasDef = Math.Max(1, danioFisico - Defensa);
            if (MitigacionFisicaPorcentaje > 0)
            {
                danioTrasDef = (int)Math.Max(1, Math.Round(danioTrasDef * (1.0 - Math.Clamp(MitigacionFisicaPorcentaje, 0.0, 0.9)), MidpointRounding.AwayFromZero));
            }
            int danioReal = danioTrasDef;
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }

        public virtual void RecibirDanioMagico(int danioMagico)
        {
            int danioTrasDef = Math.Max(1, danioMagico - DefensaMagica);
            if (MitigacionMagicaPorcentaje > 0)
            {
                danioTrasDef = (int)Math.Max(1, Math.Round(danioTrasDef * (1.0 - Math.Clamp(MitigacionMagicaPorcentaje, 0.0, 0.9)), MidpointRounding.AwayFromZero));
            }
            int danioReal = danioTrasDef;
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }
    // Drop de objetos
    public List<MiJuegoRPG.Objetos.Objeto> ObjetosDrop { get; set; } = new List<MiJuegoRPG.Objetos.Objeto>();
    public Dictionary<string, double> ProbabilidadesDrop { get; set; } = new Dictionary<string, double>();
    // Metadatos de drop por nombre de ítem
    public Dictionary<string, (int min, int max)> RangoCantidadDrop { get; } = new Dictionary<string, (int min, int max)>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> DropsUniqueOnce { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    // Identificador lógico de data para clave de UniqueOnce (Id o Nombre)
    public string IdData { get; set; } = string.Empty;

        // Constantes para ajustar la dificultad del drop
        public const double BASE_DROP_RATE = 0.05; // 5% base
        public const double RAREZA_MULTIPLIER_Rota = 0.1;
        public const double RAREZA_MULTIPLIER_Pobre = 0.3;
        public const double RAREZA_MULTIPLIER_Normal = 0.5;
        public const double RAREZA_MULTIPLIER_Superior = 0.7;
        public const double RAREZA_MULTIPLIER_Rara = 0.85;
        public const double RAREZA_MULTIPLIER_Legendaria = 0.95;
        public const double RAREZA_MULTIPLIER_Ornamentada = 1.0;

        public MiJuegoRPG.Objetos.Objeto? IntentarDrop()
        {
            var random = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            foreach (var obj in ObjetosDrop)
            {
                // Si hay probabilidad específica configurada para este objeto, úsala
                if (ProbabilidadesDrop != null && ProbabilidadesDrop.TryGetValue(obj.Nombre, out var chanceCfg))
                {
                    // Bloqueo por UniqueOnce antes de sortear
                    if (DropsUniqueOnce.Contains(obj.Nombre))
                    {
                        var clave = MiJuegoRPG.Motor.Servicios.DropsService.ClaveUnique(string.IsNullOrWhiteSpace(IdData) ? Nombre : IdData, obj.Nombre);
                        if (MiJuegoRPG.Motor.Servicios.DropsService.Marcado(clave))
                            continue; // ya obtenido en esta partida
                    }
                    if (random.NextDouble() < Math.Clamp(chanceCfg, 0.0, 1.0))
                        return obj;
                    else
                        continue;
                }

                double rate = BASE_DROP_RATE;
                // Ajuste por rareza
                switch (obj.Rareza)
                {
                    case Rareza.Rota: rate *= RAREZA_MULTIPLIER_Rota; break;
                    case Rareza.Pobre: rate *= RAREZA_MULTIPLIER_Pobre; break;
                    case Rareza.Normal: rate *= RAREZA_MULTIPLIER_Normal; break;
                    case Rareza.Superior: rate *= RAREZA_MULTIPLIER_Superior; break;
                    case Rareza.Rara: rate *= RAREZA_MULTIPLIER_Rara; break;
                    case Rareza.Legendaria: rate *= RAREZA_MULTIPLIER_Legendaria; break;
                    case Rareza.Ornamentada: rate *= RAREZA_MULTIPLIER_Ornamentada; break;
                }
                // Ajuste por nivel del monstruo y del objeto
                // Ejemplo: si el objeto es de nivel mucho mayor que el monstruo, reduce el rate
                int nivelMonstruo = this.Nivel;
                int nivelObjeto = (obj is MiJuegoRPG.Objetos.Arma arma) ? arma.Nivel : nivelMonstruo;
                if (nivelObjeto > nivelMonstruo)
                {
                    rate *= 0.5;
                }
                // Drop aleatorio
                if (random.NextDouble() < rate)
                {
                    return obj;
                }
            }
            return null;
        }

        // Helpers para configuración data-driven
        public void EstablecerInmunidad(string tipo, bool inmune)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return;
            Inmunidades[tipo] = inmune;
        }
        public void EstablecerMitigacionElemental(string tipo, double valor)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return;
            ResistenciasElementales[tipo] = Math.Clamp(valor, 0.0, 0.9);
        }
        public void AgregarDanioElementalBase(string tipo, int cantidad)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return;
            if (DanioElementalBase.ContainsKey(tipo)) DanioElementalBase[tipo] += Math.Max(0, cantidad);
            else DanioElementalBase[tipo] = Math.Max(0, cantidad);
        }
    
        // Propiedades del enemigo. 'set' es privado para evitar cambios externos.
        // Defensa ya está arriba
        public int Nivel { get; private set; }
        public int ExperienciaRecompensa { get; private set; }
        public int OroRecompensa { get; private set; }
        
               

        // Constructor modificado. Ahora recibe atributos base y el nivel.
        // Las variables del constructor deben tener los mismos nombres que las propiedades.
        protected Enemigo(string nombre, int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase, int nivel, int experienciaRecompensa, int oroRecompensa)
        {
            Nombre = nombre;
            Nivel = nivel;
            ExperienciaRecompensa = experienciaRecompensa;
            OroRecompensa = oroRecompensa;

            // Llamamos a un método para calcular los atributos basados en el nivel.
            CalcularAtributos(vidaBase, ataqueBase, defensaBase, defensaMagicaBase);
        }

        // Nuevo método privado para calcular los atributos escalados.
        private void CalcularAtributos(int vidaBase, int ataqueBase, int defensaBase, int defensaMagicaBase)
        {
            int factorEscalado = Nivel;

            VidaMaxima = vidaBase + (vidaBase * factorEscalado / 2);
            Vida = VidaMaxima;

            Ataque = ataqueBase + (ataqueBase * factorEscalado / 2);
            Defensa = defensaBase + (defensaBase * factorEscalado / 2);
            DefensaMagica = defensaMagicaBase + (defensaMagicaBase * factorEscalado / 2);
        }


        // Método para recibir daño.
        public void RecibirDanio(int danio)
        {
            int danioReal = Math.Max(1, danio - Defensa);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
        }
        
        // Método para dar recompensas.
        public void DarRecompensas(MiJuegoRPG.Personaje.Personaje jugador)
        {
            if (!EstaVivo)
            {
                var ui = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual()?.Ui;
                ui?.WriteLine($"El {Nombre} ha sido derrotado.");
                jugador.GanarExperiencia(ExperienciaRecompensa);
                jugador.GanarOro(OroRecompensa);
                try
                {
                    // Incrementar contadores de muertes: global y por bioma actual
                    var juego = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual();
                    var bioma = juego?.mapa?.UbicacionActual?.Region;
                    var tipo = (Tag ?? Nombre).ToLowerInvariant();
                    jugador.IncrementarKill(tipo, bioma);
                }
                catch { }
                var drop = IntentarDrop();
                if (drop != null)
                {
                    // Determinar cantidad según metadatos (con clamps defensivos)
                    int cant = 1;
                    if (RangoCantidadDrop.TryGetValue(drop.Nombre, out var rango))
                    {
                        int min = Math.Max(1, rango.min);
                        int max = Math.Max(min, rango.max);
                        // clamp anti-exploit (progresión lenta): máximo 3 por kill salvo drops de calidad Rota/Pobre
                        int hardCap = (drop.Rareza == Objetos.Rareza.Rota || drop.Rareza == Objetos.Rareza.Pobre) ? 5 : 3;
                        max = Math.Min(max, hardCap);
                        if (min > max) min = max;
                        cant = (min == max) ? min : MiJuegoRPG.Motor.Servicios.RandomService.Instancia.Next(min, max + 1);
                    }
                    // Encolar UniqueOnce si aplica y sorteo múltiple terminó
                    bool esUnique = DropsUniqueOnce.Contains(drop.Nombre);
                    if (esUnique)
                    {
                        cant = 1; // unique no se multiplica
                        var clave = MiJuegoRPG.Motor.Servicios.DropsService.ClaveUnique(string.IsNullOrWhiteSpace(IdData) ? Nombre : IdData, drop.Nombre);
                        // Marca y persiste por GuardadoService al guardar partida
                        if (!MiJuegoRPG.Motor.Servicios.DropsService.Marcado(clave))
                        {
                            MiJuegoRPG.Motor.Servicios.DropsService.MarcarSiNoExiste(clave);
                        }
                        else
                        {
                            // Ya existía marcado (carrera rara). Evitar duplicado.
                            cant = 0;
                        }
                    }
                    if (cant > 0)
                    {
                        ui?.WriteLine($"¡Has obtenido {cant}x: {drop.Nombre} ({drop.Rareza})!");
                        try { jugador.Inventario.AgregarObjeto(drop, cant, jugador); } catch { }
                    }
                }
            }
        }
    }
}