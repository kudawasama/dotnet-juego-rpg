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
    private static List<ArmaduraData>? armadurasDisponibles;
    private static List<AccesorioData>? accesoriosDisponibles;
    private static List<BotasData>? botasDisponibles;
    private static List<CinturonData>? cinturonesDisponibles;
    private static List<CollarData>? collaresDisponibles;
    private static List<PantalonData>? pantalonesDisponibles;
        public static void CargarBotas(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                botasDisponibles = JsonSerializer.Deserialize<List<BotasData>>(jsonString);
                Console.WriteLine($"Se cargaron {botasDisponibles?.Count ?? 0} botas del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar botas: {ex.Message}");
                botasDisponibles = new List<BotasData>();
            }
        }

        public static void CargarCinturones(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                cinturonesDisponibles = JsonSerializer.Deserialize<List<CinturonData>>(jsonString);
                Console.WriteLine($"Se cargaron {cinturonesDisponibles?.Count ?? 0} cinturones del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar cinturones: {ex.Message}");
                cinturonesDisponibles = new List<CinturonData>();
            }
        }

        public static void CargarCollares(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                collaresDisponibles = JsonSerializer.Deserialize<List<CollarData>>(jsonString);
                Console.WriteLine($"Se cargaron {collaresDisponibles?.Count ?? 0} collares del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar collares: {ex.Message}");
                collaresDisponibles = new List<CollarData>();
            }
        }

        public static void CargarPantalones(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                pantalonesDisponibles = JsonSerializer.Deserialize<List<PantalonData>>(jsonString);
                Console.WriteLine($"Se cargaron {pantalonesDisponibles?.Count ?? 0} pantalones del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar pantalones: {ex.Message}");
                pantalonesDisponibles = new List<PantalonData>();
            }
        }
        public static Botas GenerarBotasAleatorias(int nivelJugador)
        {
            if (botasDisponibles != null && botasDisponibles.Count > 0)
            {
                var random = new Random();
                var botasData = botasDisponibles[random.Next(botasDisponibles.Count)];
                int defensaFinal = (int)(botasData.Defensa * (botasData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(botasData.Rareza, true, out rareza);
                var botas = new Botas(botasData.Nombre, defensaFinal, botasData.Nivel, rareza, botasData.TipoObjeto, botasData.Perfeccion);
                return botas;
            }
            else
            {
                throw new InvalidOperationException("No hay botas disponibles para generar.");
            }
        }

        public static Cinturon GenerarCinturonAleatorio(int nivelJugador)
        {
            if (cinturonesDisponibles != null && cinturonesDisponibles.Count > 0)
            {
                var random = new Random();
                var cinturonData = cinturonesDisponibles[random.Next(cinturonesDisponibles.Count)];
                int bonifCarga = (int)(cinturonData.BonificacionCarga * (cinturonData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(cinturonData.Rareza, true, out rareza);
                var cinturon = new Cinturon(cinturonData.Nombre, bonifCarga, cinturonData.Nivel, rareza, cinturonData.TipoObjeto, cinturonData.Perfeccion);
                return cinturon;
            }
            else
            {
                throw new InvalidOperationException("No hay cinturones disponibles para generar.");
            }
        }

        public static Collar GenerarCollarAleatorio(int nivelJugador)
        {
            if (collaresDisponibles != null && collaresDisponibles.Count > 0)
            {
                var random = new Random();
                var collarData = collaresDisponibles[random.Next(collaresDisponibles.Count)];
                int bonifDefensa = (int)(collarData.BonificacionDefensa * (collarData.Perfeccion / 50.0));
                int bonifEnergia = (int)(collarData.BonificacionEnergia * (collarData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(collarData.Rareza, true, out rareza);
                var collar = new Collar(collarData.Nombre, bonifDefensa, bonifEnergia, collarData.Nivel, rareza, collarData.TipoObjeto, collarData.Perfeccion);
                return collar;
            }
            else
            {
                throw new InvalidOperationException("No hay collares disponibles para generar.");
            }
        }

        public static Pantalon GenerarPantalonAleatorio(int nivelJugador)
        {
            if (pantalonesDisponibles != null && pantalonesDisponibles.Count > 0)
            {
                var random = new Random();
                var pantalonData = pantalonesDisponibles[random.Next(pantalonesDisponibles.Count)];
                int defensaFinal = (int)(pantalonData.Defensa * (pantalonData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(pantalonData.Rareza, true, out rareza);
                var pantalon = new Pantalon(pantalonData.Nombre, defensaFinal, pantalonData.Nivel, rareza, pantalonData.TipoObjeto, pantalonData.Perfeccion);
                return pantalon;
            }
            else
            {
                throw new InvalidOperationException("No hay pantalones disponibles para generar.");
            }
        }
        public static void CargarArmaduras(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                armadurasDisponibles = JsonSerializer.Deserialize<List<ArmaduraData>>(jsonString);
                Console.WriteLine($"Se cargaron {armadurasDisponibles?.Count ?? 0} armaduras del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar armaduras: {ex.Message}");
                armadurasDisponibles = new List<ArmaduraData>();
            }
        }

        public static void CargarAccesorios(string rutaArchivo)
        {
            try
            {
                string jsonString = File.ReadAllText(rutaArchivo);
                accesoriosDisponibles = JsonSerializer.Deserialize<List<AccesorioData>>(jsonString);
                Console.WriteLine($"Se cargaron {accesoriosDisponibles?.Count ?? 0} accesorios del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar accesorios: {ex.Message}");
                accesoriosDisponibles = new List<AccesorioData>();
            }
        }
        public static Armadura GenerarArmaduraAleatoria(int nivelJugador)
        {
            if (armadurasDisponibles != null && armadurasDisponibles.Count > 0)
            {
                var random = new Random();
                var armaduraData = armadurasDisponibles[random.Next(armadurasDisponibles.Count)];
                int defensaFinal = (int)(armaduraData.Defensa * (armaduraData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(armaduraData.Rareza, true, out rareza);
                var armadura = new Armadura(armaduraData.Nombre, defensaFinal, armaduraData.Nivel, rareza, armaduraData.TipoObjeto, armaduraData.Perfeccion);
                return armadura;
            }
            else
            {
                throw new InvalidOperationException("No hay armaduras disponibles para generar.");
            }
        }

        public static Accesorio GenerarAccesorioAleatorio(int nivelJugador)
        {
            if (accesoriosDisponibles != null && accesoriosDisponibles.Count > 0)
            {
                var random = new Random();
                var accesorioData = accesoriosDisponibles[random.Next(accesoriosDisponibles.Count)];
                int bonifAtaque = (int)(accesorioData.BonificacionAtaque * (accesorioData.Perfeccion / 50.0));
                int bonifDefensa = (int)(accesorioData.BonificacionDefensa * (accesorioData.Perfeccion / 50.0));
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(accesorioData.Rareza, true, out rareza);
                var accesorio = new Accesorio(accesorioData.Nombre, bonifAtaque, bonifDefensa, accesorioData.Nivel, rareza, accesorioData.TipoObjeto, accesorioData.Perfeccion);
                return accesorio;
            }
            else
            {
                throw new InvalidOperationException("No hay accesorios disponibles para generar.");
            }
        }

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
            if (armasDisponibles != null && armasDisponibles.Count > 0)
            {
                var random = new Random();
                // Selección aleatoria ponderada por rareza y perfección
                var armaData = armasDisponibles[random.Next(armasDisponibles.Count)];
                // Ajustar daño según perfección (ejemplo: daño * (perfeccion / 50.0))
                int danoFinal = (int)(armaData.Daño * (armaData.Perfeccion / 50.0));
                // Determinar rareza (si tienes enum, puedes mapearlo aquí)
                MiJuegoRPG.Objetos.Rareza rareza = MiJuegoRPG.Objetos.Rareza.Normal;
                Enum.TryParse(armaData.Rareza, true, out rareza);
                var arma = new Arma(armaData.Nombre, danoFinal, armaData.NivelRequerido, rareza, armaData.Tipo);
                // Puedes guardar la perfección como propiedad extra si tu clase Arma la soporta
                return arma;
            }
            else
            {
                throw new InvalidOperationException("No hay armas disponibles para generar.");
            }
        }
    }
}
