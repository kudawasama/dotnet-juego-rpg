using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Interfaces;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Motor;
using System.Text.Json.Serialization; // Necesario para [JsonIgnore]
namespace MiJuegoRPG.Personaje
{
    public class Personaje : ICombatiente, IEvadible
    {
    // Atributos base del personaje (faltaba propiedad pública explícita para uso en servicios)
    public AtributosBase AtributosBase { get; set; } = new AtributosBase();
    // Alias para compatibilidad con código existente que usa 'Atributos'
    public AtributosBase Atributos => AtributosBase;

    // Propiedades básicas (algunas eran referenciadas pero no estaban definidas en este archivo)
    public string Nombre { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public int Vida { get; set; }
    public int VidaMaxima { get; set; }
    // Alias de compatibilidad con código antiguo que usaba VidaActual
    public int VidaActual { get => Vida; set => Vida = value; }
    public Inventario Inventario { get; set; } = new Inventario();
    public Clase? Clase { get; set; }
    // Compatibilidad: algunos scripts referencian ClaseDesbloqueada
    public string? ClaseDesbloqueada { get; set; }
        // IDs de desbloqueos ya notificados (para avisos automáticos)
        public HashSet<string> DesbloqueosNotificados { get; set; } = new HashSet<string>();


        // Completar misión y procesar recompensas
    public void CompletarMision(string misionId)
        {
            var mision = MisionesActivas.FirstOrDefault(m => m.Id == misionId);
            if (mision != null)
            {
                // Validar requisitos antes de completar
                bool cumpleTodos = true;
                if (mision.Requisitos != null && mision.Requisitos.Count > 0)
                {
                    foreach (var req in mision.Requisitos)
                    {
                        // Soporta requisitos tipo "clave:valor" o solo "clave"
                        var partes = req.Split(':');
                        string clave = partes[0].Trim();
                        object? valor = null;
                        if (partes.Length > 1)
                        {
                            valor = partes[1].Trim();
                            // Intenta convertir valor a int si es posible
                            if (int.TryParse(valor.ToString(), out int valorInt))
                                valor = valorInt;
                        }
                        // Si no hay valor, se asume que es un objeto que debe estar en el inventario
                        bool cumple;
                        if (valor == null)
                        {
                            // Buscar el objeto en el inventario (nombre parcial, insensible a mayúsculas)
                            cumple = Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.Contains(clave, StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            cumple = CumpleRequisito(clave, valor);
                        }
                        if (!cumple)
                        {
                            cumpleTodos = false;
                            Console.WriteLine($"No cumples el requisito: {req}");
                        }
                    }
                }
                if (!cumpleTodos)
                {
                    Console.WriteLine("No puedes completar la misión porque no cumples todos los requisitos.");
                    return;
                }
                // Procesar recompensas
                if (mision.Recompensas != null)
                {
                    // Si es la misión de Elena, dejar elegir arma
                    if (mision.Id == "BAI-INI-001")
                    {
                        var armas = mision.Recompensas;
                        Console.WriteLine("Elena: ¿Qué arma te gustaría tener?");
                        for (int i = 0; i < armas.Count; i++)
                            Console.WriteLine($"{i + 1}. {armas[i]}");
                        Console.Write("Selecciona una opción: ");
                        if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion > 0 && seleccion <= armas.Count)
                        {
                            string armaElegida = armas[seleccion - 1];
                            Inventario.AgregarObjeto(new MiJuegoRPG.Objetos.Arma(armaElegida, 5));
                            Console.WriteLine($"¡Has recibido: {armaElegida}!");
                        }
                        else
                        {
                            Console.WriteLine("Selección inválida. No se entregó arma.");
                        }
                    }
                    else
                    {
                        foreach (var recompensa in mision.Recompensas)
                        {
                            if (recompensa.ToLower().Contains("oro"))
                            {
                                Oro += 100; // Ejemplo, puedes ajustar según la recompensa
                                Console.WriteLine("¡Has recibido 100 monedas de oro!");
                            }
                            if (recompensa.ToLower().Contains("espada"))
                            {
                                Inventario.AgregarObjeto(new MiJuegoRPG.Objetos.Arma("Espada de Misión", 20));
                                Console.WriteLine("¡Has recibido una Espada de Misión!");
                            }
                            // Agrega más lógica según el tipo de recompensa
                        }
                    }
                }
                // Mover a completadas
                MisionesCompletadas.Add(mision);
                MisionesActivas.Remove(mision);
                Console.WriteLine($"¡Misión completada: {mision.Nombre}!");
                try { MiJuegoRPG.Motor.Servicios.BusEventos.Instancia.Publicar(new MiJuegoRPG.Motor.Servicios.EventoMisionCompletada(mision.Id, mision.Nombre)); } catch { }

                // Revisar desbloqueos automáticos después de completar misión
                MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(this);
            }
            else
            {
                Console.WriteLine("No tienes esa misión activa.");
            }
        }
        // PROPIEDADES Energía
        public int EnergiaActual { get; set; } = 100;
        public int EnergiaMaxima { get; set; } = 100;
        public int DescansosHoy { get; set; } = 0;
        public DateTime UltimaFechaDescanso { get; set; }
        public DateTime UltimaRecuperacionPasiva { get; set; }
        public int UltimoDiaDescanso { get; set; } = 0;

    // Ubicacion Actual (obsoleto, usar UbicacionActualId para persistencia de sector)
    public MiJuegoRPG.Motor.Ubicacion? UbicacionActual { get; set; }

    // Nuevo: ID del sector actual para persistencia y carga
    public string? UbicacionActualId { get; set; }

        // Métodos de bonificadores y objetos equipados
        public double ObtenerBonificadorAtributo(string atributo)
        {
            double total = 0;
            foreach (var obj in ObtenerObjetosEquipados())
            {
                if (obj is MiJuegoRPG.Interfaces.IBonificadorAtributo boni)
                {
                    total += boni.ObtenerBonificador(atributo);
                }
            }
            return total;
        }

        public List<FuenteBonificador> ObtenerFuentesBonificadorAtributo(string atributo)
        {
            var fuentes = new List<FuenteBonificador>();
            foreach (var obj in ObtenerObjetosEquipados())
            {
                if (obj is MiJuegoRPG.Interfaces.IBonificadorAtributo boni)
                {
                    double valor = boni.ObtenerBonificador(atributo);
                    if (valor != 0)
                        fuentes.Add(new FuenteBonificador(obj.Nombre, "Atributo", atributo, valor));
                }
            }
            return fuentes;
        }

        public double ObtenerBonificadorEstadistica(string estadistica)
        {
            double total = 0;
            foreach (var obj in ObtenerObjetosEquipados())
            {
                if (obj is MiJuegoRPG.Interfaces.IBonificadorEstadistica boni)
                {
                    total += boni.ObtenerBonificador(estadistica);
                }
            }
            return total;
        }

        public List<FuenteBonificador> ObtenerFuentesBonificadorEstadistica(string estadistica)
        {
            var fuentes = new List<FuenteBonificador>();
            foreach (var obj in ObtenerObjetosEquipados())
            {
                if (obj is MiJuegoRPG.Interfaces.IBonificadorEstadistica boni)
                {
                    double valor = boni.ObtenerBonificador(estadistica);
                    if (valor != 0)
                        fuentes.Add(new FuenteBonificador(obj.Nombre, "Estadistica", estadistica, valor));
                }
            }
            return fuentes;
        }

        public List<Objetos.Objeto> ObtenerObjetosEquipados()
        {
            var lista = new List<Objetos.Objeto>();
            var eq = Inventario.Equipo;
            if (eq.Arma != null) lista.Add(eq.Arma);
            if (eq.Casco != null) lista.Add(eq.Casco);
            if (eq.Armadura != null) lista.Add(eq.Armadura);
            if (eq.Pantalon != null) lista.Add(eq.Pantalon);
            if (eq.Zapatos != null) lista.Add(eq.Zapatos);
            if (eq.Collar != null) lista.Add(eq.Collar);
            if (eq.Cinturon != null) lista.Add(eq.Cinturon);
            if (eq.Accesorio1 != null) lista.Add(eq.Accesorio1);
            if (eq.Accesorio2 != null) lista.Add(eq.Accesorio2);
            return lista;
        }
        // Índices de dificultad por atributo
        public Dictionary<string, double> IndiceAtributo = new Dictionary<string, double>
        {
            { "fuerza", 3.0 },        // Más accesible para guerreros
            { "inteligencia", 8.0 },  // Muy difícil para magos, requiere especialización
            { "destreza", 3.5 },      // Exploradores y pícaros más ágiles
            { "suerte", 12.0 },       // Extremadamente difícil, atributo raro
            { "defensa", 5.5 },       // Tanques y defensivos
            { "vitalidad", 9.0 },     // Muy difícil, solo tanques y guerreros la suben rápido
            { "agilidad", 4.5 }       // Exploradores y pícaros la suben más fácil
        };
        public Estadisticas Estadisticas { get; set; } = new Estadisticas();
        // Propiedades del personaje
        // Misiones activas y completadas
        public List<MisionConId> MisionesActivas { get; set; } = new List<MisionConId>();
        public List<MisionConId> MisionesCompletadas { get; set; } = new List<MisionConId>();
        // Sistema dinámico de clases y actividad
        public Dictionary<string,int> ContadoresActividad { get; set; } = new();
        public HashSet<string> ClasesDesbloqueadas { get; set; } = new();
        public void RegistrarActividad(string clave, int inc = 1)
        {
            if (string.IsNullOrWhiteSpace(clave)) return;
            ContadoresActividad.TryGetValue(clave, out var v);
            ContadoresActividad[clave] = v + inc;
        }
        // Contadores persistentes de muertes: claves "global:tipo" y "bioma:<bioma>:tipo"
        public Dictionary<string,int> KillCounters { get; set; } = new();
        public void IncrementarKill(string tipo, string? bioma = null)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return;
            tipo = tipo.Trim().ToLowerInvariant();
            // Global
            var keyG = $"global:{tipo}";
            KillCounters.TryGetValue(keyG, out var g); KillCounters[keyG] = g + 1;
            // Por bioma si aplica
            if (!string.IsNullOrWhiteSpace(bioma))
            {
                var keyB = $"bioma:{bioma.Trim().ToLowerInvariant()}:{tipo}";
                KillCounters.TryGetValue(keyB, out var b); KillCounters[keyB] = b + 1;
            }
        }
        public int GetKills(string tipo, string? bioma = null)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return 0;
            tipo = tipo.Trim().ToLowerInvariant();
            int total = 0;
            var keyG = $"global:{tipo}";
            if (KillCounters.TryGetValue(keyG, out var g)) total += g;
            if (!string.IsNullOrWhiteSpace(bioma))
            {
                var keyB = $"bioma:{bioma.Trim().ToLowerInvariant()}:{tipo}";
                if (KillCounters.TryGetValue(keyB, out var b)) total += b;
            }
            return total;
        }
        public bool TieneClase(string nombre) => ClasesDesbloqueadas.Contains(nombre) || (Clase != null && Clase.Nombre == nombre);
        public bool DesbloquearClase(string nombre)
        {
            if (TieneClase(nombre)) return false;
            ClasesDesbloqueadas.Add(nombre);
            try { MiJuegoRPG.Motor.AvisosAventura.MostrarAviso("Clase Desbloqueada", nombre, $"Has obtenido la clase {nombre}!"); } catch { Console.WriteLine($"[CLASE] Desbloqueada: {nombre}"); }
            return true;
        }
        // Clase auxiliar para misiones con Id y condiciones
        public class MisionConId
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string UbicacionNPC { get; set; } = string.Empty;
            public List<string> Requisitos { get; set; } = new List<string>();
            public List<string> Recompensas { get; set; } = new List<string>();
            public int ExpNivel { get; set; } = 0;
            public Dictionary<string, int> ExpAtributos { get; set; } = new Dictionary<string, int>();
            public string Estado { get; set; } = string.Empty;
            public string SiguienteMisionId { get; set; } = string.Empty;
            public List<string> Condiciones { get; set; } = new List<string>();
        }
        // Experiencia unificada por atributo (nuevo sistema 3.1)
        public Dictionary<MiJuegoRPG.Dominio.Atributo, ExpAtributo> ExperienciaAtributos { get; set; } = new Dictionary<MiJuegoRPG.Dominio.Atributo, ExpAtributo>();

    // Campos legacy de experiencia (mantener temporalmente para compatibilidad con guardados antiguos)
    // LEGACY: Marcados con JsonIgnore para no volver a persistirlos en nuevas partidas.
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpFuerza { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpMagia { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpAgilidad { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpInteligencia { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpResistencia { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpDefensa { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpVitalidad { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpSuerte { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpDestreza { get; set; }
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ExpPercepcion { get; set; }

    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double FuerzaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double MagiaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double AgilidadExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double InteligenciaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double ResistenciaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double DefensaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double VitalidadExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double SuerteExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double DestrezaExpRequerida { get; set; } = 1.0;
    [JsonIgnore][Obsolete("Usar ExperienciaAtributos")] public double PercepcionExpRequerida { get; set; } = 1.0;

        // Sistema de Experiencia
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int ExperienciaSiguienteNivel { get; set; }
        public int Oro { get; set; }

        // Estados de supervivencia (27.x)
        // Valores normalizados 0..1 para Hambre, Sed y Fatiga. TempActual en °C (aprox.).
        public double Hambre { get; set; } = 0.0;
        public double Sed { get; set; } = 0.0;
        public double Fatiga { get; set; } = 0.0;
        public double TempActual { get; set; } = 20.0; // Ambiente templado por defecto

        public void ClampEstadosSupervivencia()
        {
            static double C(double v) => Math.Max(0.0, Math.Min(1.0, v));
            Hambre = C(Hambre);
            Sed = C(Sed);
            Fatiga = C(Fatiga);
            // TempActual sin clamp duro: se regula vía reglas de bioma y recuperación
        }

        // Estados previos de etiqueta (no persistentes) para avisos de transición de umbral
        [JsonIgnore] public string UltHambreEstado { get; set; } = "OK";
        [JsonIgnore] public string UltSedEstado { get; set; } = "OK";
        [JsonIgnore] public string UltFatigaEstado { get; set; } = "OK";

    // Reputación global básica y reputaciones por facción (extensible)
    public int Reputacion { get; set; } = 0; // usada por ReputacionMinima genérica
    public Dictionary<string,int> ReputacionesFaccion { get; set; } = new();

        // Preferencias por partida (QoL): verbosidad del logger
        public bool PreferenciaLoggerEnabled { get; set; } = true;
        public string PreferenciaLoggerLevel { get; set; } = "Info"; // "Error"|"Warn"|"Info"|"Debug"

        // Control de descansos por día


        // Implementación de ICombatiente
        public int Defensa => (int)Atributos.Defensa;
        public int DefensaMagica => (int)Estadisticas.DefensaMagica;
        public bool EstaVivo => Vida > 0;

        // Ataque físico
        public int AtacarFisico(ICombatiente objetivo)
        {
            // Si el objetivo puede evadir, chequear antes de aplicar daño
            if (objetivo is IEvadible evasivo && evasivo.IntentarEvadir(false))
            {
                // Mensajería de combate centralizada vía DamageResolver (evitar duplicados aquí)
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[{Nombre}] ataque físico evadido por {objetivo.Nombre}");
                return 0;
            }
            int danio = (int)(Atributos.Fuerza + Estadisticas.Ataque + ObtenerBonificadorAtributo("Fuerza") + ObtenerBonificadorEstadistica("Ataque"));
            objetivo.RecibirDanioFisico(danio);
            MiJuegoRPG.Motor.Servicios.Logger.Debug($"{Nombre} ataca físicamente y causa {danio} de daño a {objetivo.Nombre}");
            return danio;
        }

        // Ataque mágico
        public int AtacarMagico(ICombatiente objetivo)
        {
            if (objetivo is IEvadible evasivo && evasivo.IntentarEvadir(true))
            {
                // Mensajería de combate centralizada vía DamageResolver (evitar duplicados aquí)
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[{Nombre}] ataque mágico evadido por {objetivo.Nombre}");
                return 0;
            }
            int danio = (int)(Atributos.Inteligencia + Estadisticas.PoderMagico + ObtenerBonificadorAtributo("Inteligencia") + ObtenerBonificadorEstadistica("Poder Mágico"));
            objetivo.RecibirDanioMagico(danio);
            MiJuegoRPG.Motor.Servicios.Logger.Debug($"{Nombre} lanza un ataque mágico y causa {danio} de daño a {objetivo.Nombre}");
            return danio;
        }

        // Recibir daño físico
        public void RecibirDanioFisico(int danio)
        {
            // Oportunidad de evadir ataques físicos entrantes
            if (IntentarEvadir(false))
            {
                // Mensajería de combate centralizada vía DamageResolver y acciones
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[{Nombre}] evadió un ataque físico entrante");
                return;
            }
            double defensaTotal = Defensa + ObtenerBonificadorAtributo("Defensa") + ObtenerBonificadorEstadistica("Defensa Física");
            // Aplicar penetración si está activa: reduce la defensa efectiva antes de mitigar
            if (GameplayToggles.PenetracionEnabled)
            {
                double pen = MiJuegoRPG.Motor.Servicios.CombatAmbientContext.GetPenetracion();
                defensaTotal = System.Math.Max(0.0, defensaTotal * (1.0 - pen));
            }
            double danioReal = Math.Max(1, danio - defensaTotal);
            Vida -= (int)danioReal;
            if (Vida < 0) Vida = 0;
            MiJuegoRPG.Motor.Servicios.Logger.Debug($"{Nombre} recibió {danioReal} de daño físico. Vida restante: {Vida}");
        }

        // Recibir daño mágico
        public void RecibirDanioMagico(int danio)
        {
            if (IntentarEvadir(true))
            {
                // Mensajería de combate centralizada vía DamageResolver y acciones
                MiJuegoRPG.Motor.Servicios.Logger.Debug($"[{Nombre}] evadió un ataque mágico entrante");
                return;
            }
            double defensaMagicaTotal = DefensaMagica + ObtenerBonificadorAtributo("Resistencia") + ObtenerBonificadorEstadistica("Defensa Mágica");
            if (GameplayToggles.PenetracionEnabled)
            {
                double pen = MiJuegoRPG.Motor.Servicios.CombatAmbientContext.GetPenetracion();
                defensaMagicaTotal = System.Math.Max(0.0, defensaMagicaTotal * (1.0 - pen));
            }
            double danioReal = Math.Max(1, danio - defensaMagicaTotal);
            Vida -= (int)danioReal;
            if (Vida < 0) Vida = 0;
            MiJuegoRPG.Motor.Servicios.Logger.Debug($"{Nombre} recibió {danioReal} de daño mágico. Vida restante: {Vida}");
        }

        // Implementación de IEvadible en jugador
        public bool IntentarEvadir(bool esAtaqueMagico)
        {
            // Base desde Estadisticas.Evasion (0..1) + bonificadores de equipo
            double baseEv = Estadisticas?.Evasion ?? 0.0;
            // Permitir bonificadores por estadística "Evasion" en equipo
            baseEv += ObtenerBonificadorEstadistica("Evasion");
            // Pequeño ajuste si el ataque es mágico (generalmente más difícil de evadir)
            if (esAtaqueMagico) baseEv *= 0.8;

            // Penalización por estados de Supervivencia (27.4): sólo si hay servicio y config cargada
            try
            {
                var juego = MiJuegoRPG.Motor.Juego.ObtenerInstanciaActual();
                var sup = juego?.supervivenciaService;
                if (sup != null)
                {
                    var (etH, etS, etF) = sup.EtiquetasHSF(Hambre, Sed, Fatiga);
                    double factor = sup.FactorEvasion(etH, etS, etF); // típicamente <= 1.0
                    baseEv *= factor;
                }
            }
            catch { /* tolerante: si no hay config, no penaliza */ }

            baseEv = Math.Clamp(baseEv, 0.0, 0.5); // cap conservador jugador
            var rng = MiJuegoRPG.Motor.Servicios.RandomService.Instancia;
            return rng.NextDouble() < baseEv;
        }

        // Habilidades aprendidas y progreso
        public Dictionary<string, HabilidadProgreso> Habilidades { get; set; } = new Dictionary<string, HabilidadProgreso>();

        // Clase auxiliar para el progreso de habilidades
        public class HabilidadProgreso
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public int Exp { get; set; }
            public int Nivel { get; set; }
            public List<EvolucionHabilidad> Evoluciones { get; set; } = new List<EvolucionHabilidad>();
            public HashSet<string> EvolucionesDesbloqueadas { get; set; } = new HashSet<string>();
            public Dictionary<string, int>? AtributosNecesarios { get; set; } // <-- Agregado para requisitos de atributos
        }

        public class EvolucionHabilidad
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Beneficio { get; set; } = string.Empty;
            public List<CondicionEvolucion> Condiciones { get; set; } = new List<CondicionEvolucion>();
        }

        public class CondicionEvolucion
        {
            public string Tipo { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        public void UsarHabilidad(string habilidadId)
        {
            if (Habilidades.TryGetValue(habilidadId, out var progreso))
            {
                // Verifica requisitos antes de usar la habilidad
                if (progreso.AtributosNecesarios != null && !CumpleRequisitosHabilidad(progreso.AtributosNecesarios))
                {
                    Console.WriteLine($"No cumples los requisitos para usar la habilidad {progreso.Nombre}.");
                    return;
                }
                int nivelAnterior = progreso.Nivel;
                progreso.Exp++;
                Console.WriteLine($"Usaste la habilidad {progreso.Nombre}. Exp actual: {progreso.Exp}");
                if (progreso.Nivel > nivelAnterior)
                {
                    MiJuegoRPG.Motor.AvisosAventura.MostrarAviso(
                        "Nivel de Habilidad",
                        progreso.Nombre,
                        $"¡La habilidad ha subido a nivel {progreso.Nivel}!");
                }
                RevisarEvolucionHabilidad(habilidadId);
            }
            else
            {
                Console.WriteLine($"No tienes la habilidad {habilidadId}.");
            }
        }

        public void RevisarEvolucionHabilidad(string habilidadId)
        {
            if (!Habilidades.TryGetValue(habilidadId, out var progreso)) return;
            var evoluciones = progreso.Evoluciones;
            foreach (var evo in evoluciones)
            {
                bool cumple = true;
                foreach (var cond in evo.Condiciones)
                {
                    if (cond.Tipo == "NvHabilidad" && progreso.Exp < cond.Cantidad) cumple = false;
                    if (cond.Tipo == "NvJugador" && Nivel < cond.Cantidad) cumple = false;
                    if (cond.Tipo == "Ataque" && AtributosBase.Fuerza < cond.Cantidad) cumple = false;
                }
                if (cumple && !progreso.EvolucionesDesbloqueadas.Contains(evo.Id))
                {
                    progreso.EvolucionesDesbloqueadas.Add(evo.Id);
                    MiJuegoRPG.Motor.AvisosAventura.MostrarAviso(
                        "Evolución de Habilidad",
                        $"{progreso.Nombre} → {evo.Nombre}",
                        $"Beneficio: {evo.Beneficio}");
                }
            }
        }

        public void AprenderHabilidad(HabilidadProgreso habilidad) // Método para aprender habilidades
        {
            // Verifica requisitos antes de aprender la habilidad
            if (habilidad.AtributosNecesarios != null && !CumpleRequisitosHabilidad(habilidad.AtributosNecesarios))
            {
                Console.WriteLine($"No cumples los requisitos para aprender la habilidad {habilidad.Nombre}.");
                return;
            }
            if (!Habilidades.ContainsKey(habilidad.Id))
            {
                Habilidades.Add(habilidad.Id, habilidad);
                Console.WriteLine($"Aprendiste la habilidad {habilidad.Nombre}");
                // Avisos automáticos al aprender habilidad
                MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(this);
            }
        }
        
        public void CambiarClase(Clase nuevaClase) // Cuando cambies la clase, llama a los avisos automáticos
        {
            Clase = nuevaClase;
            Console.WriteLine($"¡Has cambiado de clase a {nuevaClase.Nombre}!");
            MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(this);
        }


        public Personaje(string nombre) // Constructor Personaje
        {
            Nombre = nombre;
            VidaMaxima = 100;
            Vida = VidaMaxima;
            Inventario = new Inventario();
            Nivel = 1;
            Experiencia = 0;
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);
            Oro = 0;
            Clase = null;
            Estadisticas = new Estadisticas(AtributosBase);
            ManaActual = ManaMaxima;
            // Inicializar experiencia atributos nueva estructura
            foreach (MiJuegoRPG.Dominio.Atributo atr in Enum.GetValues(typeof(MiJuegoRPG.Dominio.Atributo)))
            {
                ExperienciaAtributos[atr] = new ExpAtributo();
            }
            MigrarExperienciaLegacy();
        }

        private bool _migracionLegacyHecha = false;
        /// <summary>
        /// Migra valores de experiencia legacy (ExpFuerza, etc.) al diccionario unificado si existen.
        /// Se ejecuta una sola vez; deja los campos legacy intactos para compatibilidad de lectura pero ya no se usan.
        /// </summary>
        private void MigrarExperienciaLegacy()
        {
            if (_migracionLegacyHecha) return;
            bool habiaDatos = false;
            void Copiar(MiJuegoRPG.Dominio.Atributo atr, double exp, double req)
            {
                if (exp > 0)
                {
                    var data = ExperienciaAtributos[atr];
                    data.Progreso = exp;
                    if (req > 0) data.Requerida = req;
                    habiaDatos = true;
                }
            }
            // Desactivar advertencias de obsolescencia dentro del bloque de migración
#pragma warning disable CS0618
            Copiar(MiJuegoRPG.Dominio.Atributo.Fuerza, ExpFuerza, FuerzaExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Inteligencia, ExpInteligencia, InteligenciaExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Agilidad, ExpAgilidad, AgilidadExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Resistencia, ExpResistencia, ResistenciaExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Defensa, ExpDefensa, DefensaExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Vitalidad, ExpVitalidad, VitalidadExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Suerte, ExpSuerte, SuerteExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Destreza, ExpDestreza, DestrezaExpRequerida);
            Copiar(MiJuegoRPG.Dominio.Atributo.Percepcion, ExpPercepcion, PercepcionExpRequerida);
#pragma warning restore CS0618
            if (habiaDatos)
                Console.WriteLine("[Migración] Experiencia legacy migrada al nuevo sistema.");
            _migracionLegacyHecha = true;
        }

        private int CalcularExperienciaNecesaria(int nivel) // Método para calcular la experiencia necesaria para el siguiente nivel
        {
            return nivel * nivel * 200;
        }

        
    // Método Entrenar eliminado (migrado a ProgressionService). Mantener referencia si se necesitara compatibilidad retro.


        public void GanarExperiencia(int cantidad) // Método para ganar experiencia
        {
            Experiencia += cantidad;
            Console.WriteLine($"Has ganado {cantidad} puntos de experiencia. Total actual: {Experiencia}");
            if (Experiencia >= ExperienciaSiguienteNivel)
            {
                SubirNivel();
            }
        }

        public void GanarOro(int cantidad) // Método para ganar oro
        {
            Oro += cantidad;
            Console.WriteLine($"Has ganado {cantidad} monedas de oro. Total actual: {Oro}");
        }

        private void SubirNivel() // Método para subir de nivel
        {
            Nivel++;
            Experiencia -= ExperienciaSiguienteNivel;
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);
            VidaMaxima += 10;
            Vida = VidaMaxima;
            Console.WriteLine($"¡Has subido al nivel {Nivel}! Vida máxima ahora: {VidaMaxima}");
            // Revisar desbloqueos automáticos después de subir de nivel
            MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(this);
            try { MiJuegoRPG.Motor.Servicios.BusEventos.Instancia.Publicar(new MiJuegoRPG.Motor.Servicios.EventoNivelSubido(Nivel)); } catch { }
        }

        
        public bool PuedeAccederMision(Mision mision) // Verifica si el personaje puede acceder a una misión según condiciones y progreso
        {
            // Si la misión ya está activa o completada, no mostrarla como accesible
            if (MisionesActivas != null && MisionesActivas.Any(ms => ms.Id == mision.Id))
                return false;
            if (MisionesCompletadas != null && MisionesCompletadas.Any(ms => ms.Id == mision.Id))
                return false;
            // Si la misión tiene condiciones, verificar que todas estén cumplidas
            if (mision.Condiciones != null && mision.Condiciones.Count > 0)
            {
                foreach (var condicion in mision.Condiciones)
                {
                    // Ejemplo: "Completar BAI-REC-002" => buscar en MisionesCompletadas
                    var partes = condicion.Split(' ');
                    if (partes.Length == 2 && partes[0] == "Completar")
                    {
                        var idCondicion = partes[1];
                        if (MisionesCompletadas == null || !MisionesCompletadas.Any(ms => ms.Id == idCondicion))
                            return false;
                    }
                }
            }
            // Si no hay condiciones o todas se cumplen, la misión es accesible
            return true;
        }

        
        public bool CumpleRequisito(string clave, object valor) // Verifica si el personaje cumple un requisito específico (para rutas, misiones, etc.)
        {
            var requisito = clave.ToLower();
            return requisito switch
            {
                "barco" => Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.Contains("barco", StringComparison.OrdinalIgnoreCase)),
                "oro" => valor is int oroRequerido && Oro >= oroRequerido,
                "nivel" => valor is int nivelRequerido && Nivel >= nivelRequerido,
                "fuerza" => valor is int fuerzaRequerida && AtributosBase.Fuerza >= fuerzaRequerida,
                "inteligencia" => valor is int inteligenciaRequerida && AtributosBase.Inteligencia >= inteligenciaRequerida,
                "misioncompletada" => valor is string idMision && MisionesCompletadas.Any(m => m.Id == idMision),
                // Agrega más casos según los requisitos que uses en el juego
                _ => false,
            };

        }

        // Propiedades de maná
        public int ManaActual { get; set; }
        public int ManaMaxima => (int)Estadisticas.Mana;   

        

        public bool GastarMana(int cantidad) // Método para gastar maná
        {
            if (ManaActual >= cantidad)
            {
                ManaActual -= cantidad;
                return true;
            }
            return false;
        }

            public void RecuperarMana(int cantidad) // Método para recuperar maná
            {
                ManaActual = Math.Min(ManaActual + cantidad, ManaMaxima);
            }

                // Verifica si el personaje cumple los requisitos de atributos para una habilidad
        public bool CumpleRequisitosHabilidad(Dictionary<string, int> atributosNecesarios)
        {
            foreach (var req in atributosNecesarios)
            {
                // Normaliza el nombre del atributo para evitar problemas de mayúsculas/minúsculas
                string clave = req.Key.ToLower();
                int valorNecesario = req.Value;
                int valorActual = 0;
                switch (clave)
                {
                    case "fuerza": valorActual = (int)AtributosBase.Fuerza; break;
                    case "inteligencia": valorActual = (int)AtributosBase.Inteligencia; break;
                    case "destreza": valorActual = (int)AtributosBase.Destreza; break;
                    case "suerte": valorActual = (int)AtributosBase.Suerte; break;
                    case "defensa": valorActual = (int)AtributosBase.Defensa; break;
                    case "vitalidad": valorActual = (int)AtributosBase.Vitalidad; break;
                    case "agilidad": valorActual = (int)AtributosBase.Agilidad; break;
                    case "resistencia": valorActual = (int)AtributosBase.Resistencia; break;
                    case "percepcion": valorActual = (int)AtributosBase.Percepcion; break;
                    case "sabiduria": valorActual = (int)AtributosBase.Sabiduría; break;
                    case "fe": valorActual = (int)AtributosBase.Fe; break;
                    case "carisma": valorActual = (int)AtributosBase.Carisma; break;
                    case "liderazgo": valorActual = (int)AtributosBase.Liderazgo; break;
                    case "persuasion": valorActual = (int)AtributosBase.Persuasion; break;
                    default: continue; // Si el atributo no existe, lo ignora
                }
                if (valorActual < valorNecesario)
                {
                    Console.WriteLine($"No cumples el requisito: {req.Key} ({valorActual}/{valorNecesario})");
                    return false;
                }
            }
            return true;
        }

    }
}
