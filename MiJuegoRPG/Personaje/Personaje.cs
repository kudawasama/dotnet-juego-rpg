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
        public Clase Clase { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public Inventario Inventario { get; set; }
        public int VidaActual => Vida;
        public AtributosBase Atributos => Clase.Atributos;


        // Sistema de Experiencia
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int ExperienciaSiguienteNivel { get; set; }
        public int Oro { get; set; }

           


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

        public Personaje(string nombre, Clase clase)
        {
            Nombre = nombre;
            Clase = clase;
            VidaMaxima = (int)clase.Estadisticas.Salud;
            Vida = VidaMaxima;
            Inventario = new Inventario();

            // Inicializar sistema de experiencia
            Nivel = 1;
            Experiencia = 0;
            ExperienciaSiguienteNivel = CalcularExperienciaNecesaria(Nivel + 1);
            Oro = 0;

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

  
        

        public void MostrarEstado()
        {
            Console.WriteLine($"=== {Nombre} ===");
            Console.WriteLine($"Clase: {Clase.Nombre}");
            Console.WriteLine($"Vida: {Vida}/{VidaMaxima}");
            Console.WriteLine($"Atributos:");
            Console.WriteLine($"  Fuerza: {Clase.Atributos.Fuerza}");
            Console.WriteLine($"  Agilidad: {Clase.Atributos.Agilidad}");
            Console.WriteLine($"  Inteligencia: {Clase.Atributos.Inteligencia}");
            Console.WriteLine($"  Defensa: {Clase.Atributos.Defensa}");
            Console.WriteLine($"  Suerte: {Clase.Atributos.Suerte}");
        }

        public void Curar(int cantidadCuracion)
        {
            int vidaAnterior = Vida;
            Vida += cantidadCuracion;

            // Limitar la vida al máximo
            if (Vida > VidaMaxima)
                Vida = VidaMaxima;

            int vidaCurada = Vida - vidaAnterior;
            Console.WriteLine($"{Nombre} se curó {vidaCurada} puntos de vida. Vida actual: {Vida}/{VidaMaxima}");

        }
    }
}
