using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Habilidades
{
    
    public class GestorHabilidades
    {
        // Sube experiencia, nivel y evalúa evolución de la habilidad
        public void UsarHabilidad(HabilidadProgreso habilidad, Personaje.Personaje personaje, double expGanada = 1.0)
        {
            if (habilidad == null) return;

            // Sumar experiencia
            habilidad.Exp += (int)expGanada;

            // Subir de nivel si corresponde
            while (habilidad.Exp >= ExpParaNivel(habilidad.Nivel))
            {
                habilidad.Exp -= (int)ExpParaNivel(habilidad.Nivel);
                
                Console.WriteLine($"¡{habilidad.Nombre} subió a nivel {habilidad.Nivel}!");
            }

            // Evaluar evolución
            if (PuedeEvolucionar(habilidad, personaje))
            {
                string nuevaId = habilidad.Evoluciones[0].Id; // Asumiendo que la primera evolución es la deseada
                if (!string.IsNullOrEmpty(nuevaId))
                {
                    Console.WriteLine($"¡{habilidad.Nombre} ha evolucionado a {nuevaId}!");
                    habilidad.Id = nuevaId;
                    habilidad.Nombre = nuevaId; // O busca el nombre real de la habilidad evolucionada
                    habilidad.Exp = 0;
                }
            }
        }

        // Experiencia necesaria para subir de nivel (puedes ajustar la fórmula)
            // Experiencia necesaria para subir de nivel (progresión exponencial)
            private double ExpParaNivel(int nivel)
            {
                // Puedes ajustar el 1.2 para más o menos dificultad
                return 10 * Math.Pow(1.4, nivel);
            }

        // Verifica si cumple condiciones de evolución
        private bool PuedeEvolucionar(HabilidadProgreso habilidad, Personaje.Personaje personaje)
        {
            if (habilidad.Evoluciones == null) return false;
            foreach (var cond in habilidad.Evoluciones[0].Condiciones)
            {
                // Ejemplo: condición por nivel
                if (cond.Tipo == "Nivel" && habilidad.Nivel < cond.Cantidad)
                    return false;
                // Puedes agregar más condiciones aquí (misión, atributo, etc.)
            }
            return true;
        }
    }
}
