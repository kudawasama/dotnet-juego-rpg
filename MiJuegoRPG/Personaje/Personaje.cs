using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Interfaces;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Motor;
namespace MiJuegoRPG.Personaje
{
    public class Personaje : ICombatiente
    {
        // Completar misión y procesar recompensas
        public void CompletarMision(string misionId)
        {
            var mision = MisionesActivas.FirstOrDefault(m => m.Id == misionId);
            if (mision != null)
            {
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
            }
            else
            {
                Console.WriteLine("No tienes esa misión activa.");
            }
        }
        public MiJuegoRPG.Motor.Ubicacion? UbicacionActual { get; set; }
        // ...existing code...
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
        // ...existing code...
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
        // ...existing code...
    public string Nombre { get; set; } = string.Empty;
    public Clase? Clase { get; set; }
    public int Vida { get; set; }
    public int VidaMaxima { get; set; }
    public Inventario Inventario { get; set; } = new Inventario();
    public int VidaActual => Vida;
    public AtributosBase Atributos => Clase != null ? Clase.Atributos : AtributosBase;
    public AtributosBase AtributosBase { get; set; } = new AtributosBase();
    public string Titulo { get; set; } = "Sin título";
    public string ClaseDesbloqueada { get; set; } = "Sin clase";

        // Experiencia por atributo
    public double ExpFuerza { get; set; }
    public double ExpMagia { get; set; }
    public double ExpAgilidad { get; set; }
    public double ExpInteligencia { get; set; }
    public double ExpResistencia { get; set; }
    public double ExpDefensa { get; set; }
    public double ExpVitalidad { get; set; }
    public double ExpSuerte { get; set; }
    public double ExpDestreza { get; set; }
    public double ExpPercepcion { get; set; }

        // Experiencia requerida para subir cada atributo
    public double FuerzaExpRequerida { get; set; } = 1.0;
    public double MagiaExpRequerida { get; set; } = 1.0;
    public double AgilidadExpRequerida { get; set; } = 1.0;
    public double InteligenciaExpRequerida { get; set; } = 1.0;
    public double ResistenciaExpRequerida { get; set; } = 1.0;
    public double DefensaExpRequerida { get; set; } = 1.0;
    public double VitalidadExpRequerida { get; set; } = 1.0;
    public double SuerteExpRequerida { get; set; } = 1.0;
    public double DestrezaExpRequerida { get; set; } = 1.0;
    public double PercepcionExpRequerida { get; set; } = 1.0;

        // Sistema de Experiencia
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int ExperienciaSiguienteNivel { get; set; }
        public int Oro { get; set; }

        // Sistema de profesiones (OBSOLETO, mantener solo si se usa en alguna lógica especial)
        // public Dictionary<string, int> Profesiones { get; set; } = new Dictionary<string, int> {
        //     { "Herrero", 0 },
        //     { "Herbolaria", 0 },
        //     { "Domador", 0 },
        //     { "Alquimista", 0 },
        //     { "Cazador", 0 },
        //     { "Explorador", 0 }
        // };
        // public string ProfesionPrincipal { get; set; } = "Sin especialidad";

        // Implementación de ICombatiente
        public int Defensa => (int)Atributos.Defensa;
        public bool EstaVivo => Vida > 0;
        public int Atacar(ICombatiente objetivo)
        {
            int danio = (int)Atributos.Fuerza;
            objetivo.RecibirDanio(danio);
            Console.WriteLine($"{Nombre} ataca y causa {danio} de daño a {objetivo.Nombre}");
            return danio;
        }
        public void RecibirDanio(int danio)
        {
            double danioReal = Math.Max(1, danio - Defensa);
            Vida -= (int)danioReal;
            if (Vida < 0) Vida = 0;
            Console.WriteLine($"{Nombre} recibió {danioReal} de daño. Vida restante: {Vida}");
        }

        // Habilidades aprendidas y progreso
        public Dictionary<string, HabilidadProgreso> Habilidades { get; set; } = new Dictionary<string, HabilidadProgreso>();

        public void UsarHabilidad(string habilidadId)
        {
            if (Habilidades.TryGetValue(habilidadId, out var progreso))
            {
                progreso.Exp++;
                Console.WriteLine($"Usaste la habilidad {progreso.Nombre}. Exp actual: {progreso.Exp}");
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
                    Console.WriteLine($"¡{progreso.Nombre} evolucionó a {evo.Nombre}! Beneficio: {evo.Beneficio}");
                }
            }
        }

        public void AprenderHabilidad(HabilidadProgreso habilidad)
        {
            if (!Habilidades.ContainsKey(habilidad.Id))
            {
                Habilidades.Add(habilidad.Id, habilidad);
                Console.WriteLine($"Aprendiste la habilidad {habilidad.Nombre}");
            }
        }

        public Personaje(string nombre)
        {
            Nombre = nombre;
            VidaMaxima = 100;
            Vida = VidaMaxima;
            Inventario = new Inventario();
            Nivel = 1;
            Experiencia = 0;
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);
            Oro = 0;
            AtributosBase = new AtributosBase();
            Clase = null;
        }

        private int CalcularExperienciaNecesaria(int nivel)
        {
            return nivel * nivel * 200;
        }

        // Método para entrenar atributos y desbloquear clases/títulos
        public void Entrenar(string atributo)
        {
            // Sistema estandarizado: experiencia base * índice de atributo * índice de nivel
            double expBase = 0.01; // Puedes ajustar este valor base
            double indiceNivel = Math.Pow(1.05, Nivel - 1); // Cada nivel aumenta la dificultad un 5%
            double indiceAtributo = IndiceAtributo.ContainsKey(atributo.ToLower()) ? IndiceAtributo[atributo.ToLower()] : 1.0;
            double expPorMinuto = expBase / (indiceNivel * indiceAtributo);
            if (expPorMinuto < 0.0001) expPorMinuto = 0.0001; // Límite mínimo absoluto
            int minutos = 1; // Simulación: cada acción equivale a 1 minuto
            switch (atributo.ToLower())
            {
                case "fuerza":
                    ExpFuerza += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Fuerza... Progreso: {ExpFuerza:F5}/{FuerzaExpRequerida:F2}");
                    if (ExpFuerza >= FuerzaExpRequerida)
                    {
                        AtributosBase.Fuerza += 1;
                        ExpFuerza = 0;
                        FuerzaExpRequerida *= 1.2; // Aumenta dificultad
                        Console.WriteLine($"¡Fuerza subió a {AtributosBase.Fuerza}! Próximo nivel requiere {FuerzaExpRequerida:F2} exp.");
                    }
                    break;
                case "inteligencia":
                    ExpInteligencia += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Inteligencia... Progreso: {ExpInteligencia:F5}/{InteligenciaExpRequerida:F2}");
                    if (ExpInteligencia >= InteligenciaExpRequerida)
                    {
                        AtributosBase.Inteligencia += 1;
                        ExpInteligencia = 0;
                        InteligenciaExpRequerida *= 1.2;
                        Console.WriteLine($"¡Inteligencia subió a {AtributosBase.Inteligencia}! Próximo nivel requiere {InteligenciaExpRequerida:F2} exp.");
                    }
                    break;
                case "destreza":
                    ExpDestreza += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Destreza... Progreso: {ExpDestreza:F5}/{DestrezaExpRequerida:F2}");
                    if (ExpDestreza >= DestrezaExpRequerida)
                    {
                        AtributosBase.Destreza += 1;
                        ExpDestreza = 0;
                        DestrezaExpRequerida *= 1.2;
                        Console.WriteLine($"¡Destreza subió a {AtributosBase.Destreza}! Próximo nivel requiere {DestrezaExpRequerida:F2} exp.");
                    }
                    break;
                case "magia":
                    ExpMagia += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Magia... Progreso: {ExpMagia:F5}/{MagiaExpRequerida:F2}");
                    if (ExpMagia >= MagiaExpRequerida)
                    {
                        // Si tienes atributo Magia, súbelo aquí
                        ExpMagia = 0;
                        MagiaExpRequerida *= 1.2;
                        Console.WriteLine($"¡Magia subió de nivel! Próximo nivel requiere {MagiaExpRequerida:F2} exp.");
                    }
                    break;
                case "suerte":
                    ExpSuerte += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Suerte... Progreso: {ExpSuerte:F5}/{SuerteExpRequerida:F2}");
                    if (ExpSuerte >= SuerteExpRequerida)
                    {
                        AtributosBase.Suerte += 1;
                        ExpSuerte = 0;
                        SuerteExpRequerida *= 1.2;
                        Console.WriteLine($"¡Suerte subió a {AtributosBase.Suerte}! Próximo nivel requiere {SuerteExpRequerida:F2} exp.");
                    }
                    break;
                case "defensa":
                    ExpDefensa += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Defensa... Progreso: {ExpDefensa:F5}/{DefensaExpRequerida:F2}");
                    if (ExpDefensa >= DefensaExpRequerida)
                    {
                        AtributosBase.Defensa += 1;
                        ExpDefensa = 0;
                        DefensaExpRequerida *= 1.2;
                        Console.WriteLine($"¡Defensa subió a {AtributosBase.Defensa}! Próximo nivel requiere {DefensaExpRequerida:F2} exp.");
                    }
                    break;
                case "vitalidad":
                    ExpVitalidad += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Vitalidad... Progreso: {ExpVitalidad:F5}/{VitalidadExpRequerida:F2}");
                    if (ExpVitalidad >= VitalidadExpRequerida)
                    {
                        AtributosBase.Vitalidad += 1;
                        ExpVitalidad = 0;
                        VitalidadExpRequerida *= 1.2;
                        Console.WriteLine($"¡Vitalidad subió a {AtributosBase.Vitalidad}! Próximo nivel requiere {VitalidadExpRequerida:F2} exp.");
                    }
                    break;
                case "agilidad":
                    ExpAgilidad += expPorMinuto * minutos;
                    Console.WriteLine($"Entrenando Agilidad... Progreso: {ExpAgilidad:F5}/{AgilidadExpRequerida:F2}");
                    if (ExpAgilidad >= AgilidadExpRequerida)
                    {
                        AtributosBase.Agilidad += 1;
                        ExpAgilidad = 0;
                        AgilidadExpRequerida *= 1.2;
                        Console.WriteLine($"¡Agilidad subió a {AtributosBase.Agilidad}! Próximo nivel requiere {AgilidadExpRequerida:F2} exp.");
                    }
                    break;
            }
        }

        // Método para entrenar profesión (OBSOLETO, eliminar si no se usa en el juego)
        // public void EntrenarProfesion(string profesion)
        // {
        //     if (!Profesiones.ContainsKey(profesion))
        //     {
        //         Console.WriteLine($"La profesión '{profesion}' no existe.");
        //         return;
        //     }
        //     Profesiones[profesion] += 10;
        //     Console.WriteLine($"Has entrenado en {profesion}. Progreso: {Profesiones[profesion]}/100");
        //     if (Profesiones[profesion] >= 100 && ProfesionPrincipal == "Sin especialidad")
        //     {
        //         ProfesionPrincipal = profesion;
        //         Console.WriteLine($"¡Ahora eres especialista en {profesion}!");
        //     }
        // }

        public void GanarExperiencia(int cantidad)
        {
            Experiencia += cantidad;
            Console.WriteLine($"Has ganado {cantidad} puntos de experiencia. Total actual: {Experiencia}");
            if (Experiencia >= ExperienciaSiguienteNivel)
            {
                SubirNivel();
            }
        }

        public void GanarOro(int cantidad)
        {
            Oro += cantidad;
            Console.WriteLine($"Has ganado {cantidad} monedas de oro. Total actual: {Oro}");
        }

        private void SubirNivel()
        {
            Nivel++;
            Experiencia -= ExperienciaSiguienteNivel;
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);
            VidaMaxima += 10;
            Vida = VidaMaxima;
            Console.WriteLine($"¡Has subido al nivel {Nivel}! Vida máxima ahora: {VidaMaxima}");
        }

        // Verifica si el personaje puede acceder a una misión según condiciones y progreso
        public bool PuedeAccederMision(Mision mision)
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

        // Verifica si el personaje cumple un requisito específico (para rutas, misiones, etc.)
        public bool CumpleRequisito(string clave, object valor)
        {
            switch (clave.ToLower())
            {
                case "barco":
                    // Ejemplo: si el requisito es tener barco
                    // Puedes validar si el jugador tiene un objeto "Barco" en el inventario
                    return Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.ToLower().Contains("barco"));
                case "oro":
                    // Si el requisito es tener cierta cantidad de oro
                    if (valor is int oroRequerido)
                        return Oro >= oroRequerido;
                    break;
                case "nivel":
                    if (valor is int nivelRequerido)
                        return Nivel >= nivelRequerido;
                    break;
                case "fuerza":
                    if (valor is int fuerzaRequerida)
                        return AtributosBase.Fuerza >= fuerzaRequerida;
                    break;
                // Agrega más casos según los requisitos que uses en el juego
                default:
                    // Si el requisito no se reconoce, por defecto no se cumple
                    return false;
            }
            return false;
        }
    }
}
