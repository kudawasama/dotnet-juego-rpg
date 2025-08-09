using System;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Personaje
{
    public class Personaje : ICombatiente
    {
        // Propiedades del personaje
        public string Nombre { get; set; }
        public Clase? Clase { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public Inventario Inventario { get; set; }
        public int VidaActual => Vida;
        public AtributosBase Atributos => Clase != null ? Clase.Atributos : AtributosBase;

        // Nuevo: Atributos base independientes
        public AtributosBase AtributosBase { get; set; }
        public string Titulo { get; set; } = "Sin título";
        public string ClaseDesbloqueada { get; set; } = "Sin clase";

        // Experiencia por atributo
        public int ExpFuerza { get; set; }
        public int ExpMagia { get; set; }
        public int ExpAgilidad { get; set; }
        public int ExpInteligencia { get; set; }
        public int ExpResistencia { get; set; }

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

        public void GanarExperiencia(int exp)
        {
            Experiencia += exp;
            Console.WriteLine($"{Nombre} ganó {exp} puntos de experiencia.");

            // verificar si sube de nivel
            if (Experiencia >= ExperienciaSiguienteNivel)
            {
                SubirNivel();
            }
        }

        public void GanarOro(int cantidad)
        {
            Oro += cantidad;
            Console.WriteLine($"{Nombre} ganó {cantidad} monedas de oro!");
        }

        private void SubirNivel()
        {
            Nivel++;
            Console.WriteLine($"¡{Nombre} subió al nivel {Nivel}!");

            // Aumentar estadísticas al subir de nivel
            int bonusVida = 10 + (Atributos.Vitalidad / 2);
            VidaMaxima += bonusVida;
            Vida = VidaMaxima; // Curar completamente al subir de nivel

            // Calcular experiencia para el siguiente nivel
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);

            Console.WriteLine($"Vida máxima aumentó en {bonusVida}! Nueva vida máxima: {VidaMaxima}");
        }

        private int CalcularExperienciaNecesaria(int nivel)
        {
            // Fórmula exponencial para experiencia requerida
            return nivel * nivel * 200;
        }

        // Método para entrenar atributos y desbloquear clases/títulos
        public void Entrenar(string atributo)
        {
            switch (atributo.ToLower())
            {
                case "fuerza":
                    AtributosBase.Fuerza += 1;
                    ExpFuerza += 2;
                    GanarExperiencia(2);
                    break;
                case "resistencia":
                    AtributosBase.Resistencia += 1;
                    ExpResistencia += 2;
                    GanarExperiencia(2);
                    break;
                case "inteligencia":
                    AtributosBase.Inteligencia += 1;
                    ExpInteligencia += 2;
                    GanarExperiencia(2);
                    break;
                case "destreza":
                    AtributosBase.Destreza += 1;
                    ExpAgilidad += 2;
                    GanarExperiencia(2);
                    break;
                case "magia":
                    ExpMagia += 2;
                    GanarExperiencia(2);
                    break;
                case "suerte":
                    AtributosBase.Suerte += 1;
                    GanarExperiencia(2);
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
