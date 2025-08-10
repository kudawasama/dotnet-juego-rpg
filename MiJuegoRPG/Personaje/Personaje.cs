using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Interfaces;
using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    public class Personaje : ICombatiente
    {
        // Propiedades del personaje
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

        // Sistema de Experiencia
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int ExperienciaSiguienteNivel { get; set; }
        public int Oro { get; set; }

        // Sistema de profesiones
        public Dictionary<string, int> Profesiones { get; set; } = new Dictionary<string, int> {
            { "Herrero", 0 },
            { "Herbolaria", 0 },
            { "Domador", 0 },
            { "Alquimista", 0 },
            { "Cazador", 0 },
            { "Explorador", 0 }
        };
        public string ProfesionPrincipal { get; set; } = "Sin especialidad";

        public int Defensa => Atributos.Defensa;
        public bool EstaVivo => Vida > 0;

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

        // Clase para progreso de habilidad
        public class HabilidadProgreso
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public int Exp { get; set; } = 0;
            public List<EvolucionHabilidad> Evoluciones { get; set; } = new List<EvolucionHabilidad>();
            public List<string> EvolucionesDesbloqueadas { get; set; } = new List<string>();
        }

        public class EvolucionHabilidad
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public List<CondicionEvolucion> Condiciones { get; set; } = new List<CondicionEvolucion>();
            public string Beneficio { get; set; } = string.Empty;
            public List<string> Mejoras { get; set; } = new List<string>();
        }

        public class CondicionEvolucion
        {
            public string Tipo { get; set; } = string.Empty;
            public int Cantidad { get; set; } = 0;
        }

        public int Atacar(ICombatiente objetivo)
        {
            int danio = Atributos.Fuerza;
            objetivo.RecibirDanio(danio);
            return danio;
        }

        public void RecibirDanio(int danio)
        {
            int danioReal = Math.Max(1, danio - Defensa);
            Vida -= danioReal;
            if (Vida < 0) Vida = 0;
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
            // Valor base de experiencia por "minuto" de entrenamiento
            double expPorMinuto = 0.00001;
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

        // Método para entrenar profesión
        public void EntrenarProfesion(string profesion)
        {
            if (!Profesiones.ContainsKey(profesion))
            {
                Console.WriteLine($"La profesión '{profesion}' no existe.");
                return;
            }
            Profesiones[profesion] += 10;
            Console.WriteLine($"Has entrenado en {profesion}. Progreso: {Profesiones[profesion]}/100");
            if (Profesiones[profesion] >= 100 && ProfesionPrincipal == "Sin especialidad")
            {
                ProfesionPrincipal = profesion;
                Console.WriteLine($"¡Ahora eres especialista en {profesion}!");
            }
        }
    }
}
