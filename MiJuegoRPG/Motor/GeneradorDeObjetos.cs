using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor
{
    public static class GeneradorObjetos
    {
        private static List<ArmaData>? armasDisponibles;

        public static void CargarArmas(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                armasDisponibles = JsonSerializer.Deserialize<List<ArmaData>>(jsonString);
                Console.WriteLine($"Se cargaron {armasDisponibles?.Count ?? 0} armas del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar armas: {ex.Message}");
                armasDisponibles = new List<ArmaData>();
            }
        }

        public static Arma GenerarArmaAleatoria(int nivelJugador)
        {
            // Lógica para seleccionar un arma aleatoria según el nivel del jugador
            // ...

            // Ejemplo de cómo crear un arma desde los datos del JSON
            if (armasDisponibles != null && armasDisponibles.Count > 0)
            {
                ArmaData armaData = armasDisponibles[0]; // Seleccionamos la primera arma por simplicidad
                return new Arma(armaData.Nombre, armaData.Daño);
            }
            else
            {
                throw new InvalidOperationException("No hay armas disponibles para generar.");
            }
        }
    }
}
