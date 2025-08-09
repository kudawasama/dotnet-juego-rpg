using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Enemigos;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor
{
    public static class GeneradorEnemigos
    {
        private static readonly Random random = new Random();
        private static List<EnemigoData>? enemigosDisponibles;

        // Este es el método que falta en tu archivo.
        // Se encarga de leer el archivo JSON y cargar los datos de los enemigos.
        public static void CargarEnemigos(string rutaArchivo)
        {
            // Mostrar la ruta recibida
            Console.WriteLine($"[GeneradorEnemigos] Ruta recibida: {rutaArchivo}");
            // Forzar ruta a la carpeta del proyecto
            var dirProyecto = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? "");
            string rutaBase = dirProyecto?.FullName ?? Environment.CurrentDirectory;
            rutaArchivo = Path.Combine(rutaBase, "MiJuegoRPG", "PjDatos", Path.GetFileName(rutaArchivo));
            Console.WriteLine($"[GeneradorEnemigos] Ruta forzada: {rutaArchivo}");
            // Mostrar la ruta final usada
            Console.WriteLine($"[GeneradorEnemigos] Ruta final usada: {rutaArchivo}");
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                enemigosDisponibles = JsonSerializer.Deserialize<List<EnemigoData>>(jsonString, options);
                Console.WriteLine($"Se cargaron {enemigosDisponibles?.Count ?? 0} enemigos del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar enemigos: {ex.Message}");
                enemigosDisponibles = new List<EnemigoData>();
            }
        }
        
        // Método para generar un enemigo aleatorio basado en el JSON.
        public static Enemigo GenerarEnemigoAleatorio(MiJuegoRPG.Personaje.Personaje jugador)
        {
            if (enemigosDisponibles == null || enemigosDisponibles.Count == 0)
            {
                Console.WriteLine("No se encontraron enemigos. Generando Goblin por defecto.");
                // Creamos un enemigo por defecto si no hay JSON
                return new EnemigoEstandar("Goblin", 50, 10, 5, 1, 5, 5);
            }

            var enemigosApropiados = enemigosDisponibles
                .Where(e => e.Nivel <= jugador.Nivel + 2) // Filtra enemigos para que no sean demasiado difíciles
                .ToList();
            
            if (!enemigosApropiados.Any())
            {
                Console.WriteLine("No se encontraron enemigos apropiados. Generando Goblin por defecto.");
                return new EnemigoEstandar("Goblin", 50, 10, 5, 1, 5, 5);
            }

            int indice = random.Next(0, enemigosApropiados.Count);
            EnemigoData enemigoData = enemigosApropiados[indice];

            // Buscar arma por nombre si existe en el JSON del enemigo
            Objetos.Arma? arma = null;
            if (!string.IsNullOrWhiteSpace(enemigoData.ArmaNombre))
            {
                arma = Objetos.GestorArmas.BuscarArmaPorNombre(enemigoData.ArmaNombre);
            }

            var enemigo = new EnemigoEstandar(
                enemigoData.Nombre,
                enemigoData.VidaBase,
                enemigoData.AtaqueBase,
                enemigoData.DefensaBase,
                enemigoData.Nivel,
                enemigoData.ExperienciaRecompensa,
                enemigoData.OroRecompensa
            );
            if (arma != null)
            {
                enemigo.ArmaEquipada = arma;
            }

            // Ejemplo de drops básicos según tipo de enemigo
            if (enemigo.Nombre.ToLower().Contains("goblin"))
            {
                var armaDrop = new MiJuegoRPG.Objetos.Arma("Espada Oxidada", 5, nivel: 1, rareza: MiJuegoRPG.Objetos.Rareza.Normal, categoria: "UnaMano");
                enemigo.ObjetosDrop.Add(armaDrop);
                MiJuegoRPG.Objetos.GestorArmas.GuardarArmaSiNoExiste(armaDrop);

                var pocionDrop = new MiJuegoRPG.Objetos.Pocion("Poción Pequeña", 10, MiJuegoRPG.Objetos.Rareza.Pobre);
                enemigo.ObjetosDrop.Add(pocionDrop);
                MiJuegoRPG.Objetos.GestorPociones.GuardarPocionSiNoExiste(pocionDrop);
            }
            else if (enemigo.Nombre.ToLower().Contains("slime"))
            {
                var materialDrop = new MiJuegoRPG.Objetos.Material("Gelatina", MiJuegoRPG.Objetos.Rareza.Rota);
                enemigo.ObjetosDrop.Add(materialDrop);
                MiJuegoRPG.Objetos.GestorMateriales.GuardarMaterialSiNoExiste(materialDrop);
            }
            else if (enemigo.Nombre.ToLower().Contains("golem"))
            {
                var armaDrop = new MiJuegoRPG.Objetos.Arma("Martillo Pesado", 20, nivel: enemigo.Nivel, rareza: MiJuegoRPG.Objetos.Rareza.Rara, categoria: "DosManos");
                enemigo.ObjetosDrop.Add(armaDrop);
                MiJuegoRPG.Objetos.GestorArmas.GuardarArmaSiNoExiste(armaDrop);
            }
            // Puedes agregar más lógica para otros tipos de enemigos

            return enemigo;
        }

        // Método para iniciar el combate
        public static void IniciarCombate(MiJuegoRPG.Personaje.Personaje jugador, Enemigo enemigo)
        {
            Console.Clear();
            Console.WriteLine($"¡Un {enemigo.Nombre} salvaje ha aparecido!");

            var combate = new CombatePorTurnos(jugador, enemigo);
            combate.IniciarCombate();
            
            Console.WriteLine("\nEl combate ha terminado. Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}