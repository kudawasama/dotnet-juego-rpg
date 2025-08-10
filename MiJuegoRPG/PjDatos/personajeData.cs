// Importamos las librerías necesarias
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Habilidades;
using System.Collections.Generic;

// Definimos el namespace (espacio de nombres) para organizar el código
namespace MiJuegoRPG.PjDatos
{
    // Esta clase representa los datos del personaje que queremos guardar.
    // Es como una "fotocopia" de los datos relevantes del Personaje.
    public class PersonajeData
    {
        // Propiedades de la clase Personaje que vamos a guardar.
        // Las propiedades son como variables que tienen un getter y un setter.
        public required string Nombre { get; set; }
        public required string ClaseNombre { get; set; }
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int ExperienciaSiguienteNivel { get; set; }
        public int Oro { get; set; }
        public int VidaActual { get; set; }
        public int VidaMaxima { get; set; }
        public int Defensa { get; set; }
        public bool EstaVivo { get; set; }

        // Aquí guardaremos los atributos base, como Fuerza, Destreza, etc.
        // La clase AtributosBase probablemente se encuentra en tu carpeta 'Personaje'.
        public required AtributosBase Atributos { get; set; }
        
        // También la clase del personaje, que contiene las estadísticas de base.
        public required Clase Clase { get; set; }
        
        // Y el inventario, que contiene los objetos.
        public required Inventario Inventario { get; set; }
        
        // Opcional: Si quieres guardar las habilidades aprendidas.
        public List<Habilidad> Habilidades { get; set; } = new List<Habilidad>();
    }
}