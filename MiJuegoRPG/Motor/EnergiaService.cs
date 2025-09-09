using System;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class EnergiaService
    {
        private const int EnergiaMaximaDefault = 100;
        private int costoFijoLegacy = 10; // respaldo si no se puede calcular dinámico
        private const int MinRecuperacion = 0;
        // Config dinámica
        private EnergiaConfig config = new();
        private bool configCargada = false;

        private record EnergiaConfig(
            Dictionary<string,int>? BasePorTipo,
            Dictionary<string,double>? ModHerramienta,
            Dictionary<string,double>? ModBioma,
            Dictionary<string,string>? AtributoRelevantePorTipo,
            int UmbralAtributo,
            double FactorReduccionAtributo,
            int CostoMinimo,
            int CostoMaximo,
            Dictionary<string,double>? ModClase // Nuevo: modificadores específicos por nombre de clase/profesión
            )
        {
            public EnergiaConfig(): this(new(), new(), new(), new(), 25, 0.4, 3, 25, new()) {}
        }

        public void CargarConfig()
        {
            if (configCargada) return;
            try
            {
                var ruta = System.IO.Path.Combine(Juego.ObtenerRutaRaizProyecto(), "MiJuegoRPG", "DatosJuego", "energia.json");
                if (System.IO.File.Exists(ruta))
                {
                    var json = System.IO.File.ReadAllText(ruta);
                    var cfg = System.Text.Json.JsonSerializer.Deserialize<EnergiaConfig>(json);
                    if (cfg != null) config = cfg;
                }
                configCargada = true;
            }
            catch { /* usar defaults */ }
        }

        public void InicializarEnergia(Personaje.Personaje pj)
        {
            if (pj == null) return;

            pj.EnergiaMaxima = EnergiaMaximaDefault;
            pj.EnergiaActual = EnergiaMaximaDefault;
            pj.DescansosHoy = 0;
            pj.UltimaFechaDescanso = DateTime.Now.Date;
            pj.UltimaRecuperacionPasiva = DateTime.Now;
        }

        public void MostrarEnergia(Personaje.Personaje jugador)
            {
                Console.WriteLine($"Energía actual: {jugador.EnergiaActual}/{jugador.EnergiaMaxima}");
            }


        public int CalcularCostoAccion(Personaje.Personaje pj, string tipoAccion, string? bioma, string? herramientaInferida, MiJuegoRPG.Dominio.Atributo? atrRelevante = null, double? valorAtributo = null)
        {
            CargarConfig();
            // Base
            int baseTipo = config.BasePorTipo != null && config.BasePorTipo.TryGetValue(tipoAccion, out var b) ? b : costoFijoLegacy;
            double modHerr = 0.0;
            if (!string.IsNullOrWhiteSpace(herramientaInferida) && config.ModHerramienta != null && config.ModHerramienta.TryGetValue(herramientaInferida, out var mh)) modHerr = mh;
            double modBioma = 0.0;
            if (!string.IsNullOrWhiteSpace(bioma) && config.ModBioma != null && config.ModBioma.TryGetValue(bioma, out var mb)) modBioma = mb;
            // Atributo relevante
            string? atrStr = null;
            if (config.AtributoRelevantePorTipo != null && config.AtributoRelevantePorTipo.TryGetValue(tipoAccion, out var a)) atrStr = a;
            double modAtr = 0.0;
            if (atrStr != null)
            {
                // Mapear atributo
                MiJuegoRPG.Dominio.Atributo atrEnum;
                if (Enum.TryParse<MiJuegoRPG.Dominio.Atributo>(atrStr, true, out atrEnum))
                {
                    double valor = valorAtributo ?? ObtenerValorAtributo(pj, atrEnum);
                    // Si el atributo supera el umbral, reducir coste hasta FactorReduccionAtributo (porcentaje)
                    if (valor > config.UmbralAtributo)
                    {
                        double exceso = valor - config.UmbralAtributo;
                        modAtr = -Math.Min(config.FactorReduccionAtributo, exceso / (config.UmbralAtributo * 5.0));
                    }
                }
            }
            double modClase = 0.0;
            if (pj?.Clase != null && !string.IsNullOrWhiteSpace(pj.Clase.Nombre) && config.ModClase != null)
            {
                if (config.ModClase.TryGetValue(pj.Clase.Nombre, out var mc))
                    modClase += mc; // Clase principal
            }
            // Bonificadores dinámicos de clases emergentes
            try
            {
                var juegoRef = Juego.Instancia ?? Juego.ObtenerInstanciaActual();
                if (pj != null && juegoRef?.claseService != null)
                {
                    foreach (var bono in juegoRef.claseService.Bonificadores(pj))
                    {
                        if (bono.Key.Equals("Energia.ModClase", StringComparison.OrdinalIgnoreCase)) modClase += bono.Value;
                        else if (bono.Key.Equals($"Energia.ModAccion.{tipoAccion}", StringComparison.OrdinalIgnoreCase)) modClase += bono.Value;
                    }
                }
            } catch { }
            double costo = baseTipo * (1 + modHerr + modBioma + modAtr + modClase);
            if (costo < config.CostoMinimo) costo = config.CostoMinimo;
            if (costo > config.CostoMaximo) costo = config.CostoMaximo;
            return (int)Math.Round(costo);
        }

        private static double ObtenerValorAtributo(Personaje.Personaje pj, MiJuegoRPG.Dominio.Atributo atr)
        {
            if (pj == null) return 0;
            var a = pj.AtributosBase;
            return atr switch
            {
                MiJuegoRPG.Dominio.Atributo.Fuerza => a.Fuerza,
                MiJuegoRPG.Dominio.Atributo.Inteligencia => a.Inteligencia,
                MiJuegoRPG.Dominio.Atributo.Destreza => a.Destreza,
                MiJuegoRPG.Dominio.Atributo.Resistencia => a.Resistencia,
                MiJuegoRPG.Dominio.Atributo.Defensa => a.Defensa,
                MiJuegoRPG.Dominio.Atributo.Vitalidad => a.Vitalidad,
                MiJuegoRPG.Dominio.Atributo.Agilidad => a.Agilidad,
                MiJuegoRPG.Dominio.Atributo.Suerte => a.Suerte,
                MiJuegoRPG.Dominio.Atributo.Percepcion => a.Percepcion,
                _ => 0
            };
        }

        public bool GastarEnergiaRecoleccion(Personaje.Personaje pj, string tipoAccion, string? bioma, string? herramientaInferida)
        {
            if (pj == null) return false;
            int costo = CalcularCostoAccion(pj, tipoAccion, bioma, herramientaInferida);
            if (pj.EnergiaActual < costo)
            {
                Console.WriteLine($"No tienes suficiente energía. Requiere {costo} y tienes {pj.EnergiaActual}.");
                return false;
            }
            pj.EnergiaActual -= costo;
            Console.WriteLine($"Energía gastada: {costo}. Queda {pj.EnergiaActual}/{pj.EnergiaMaxima}.");
            return true;
        }

        public void RecuperarEnergiaDescanso(Personaje.Personaje pj)
        {
            if (pj == null) return;

            // Reiniciar contador si ha cambiado el día
            if (pj.UltimaFechaDescanso.Date < DateTime.Now.Date)
            {
                pj.DescansosHoy = 0;
                pj.UltimaFechaDescanso = DateTime.Now.Date;
            }

            // Cálculo de porcentaje según número de descansos
            int porcentaje = 100 - (pj.DescansosHoy * 10);
            if (porcentaje < MinRecuperacion) porcentaje = MinRecuperacion;

            int energiaARecuperar = (pj.EnergiaMaxima * porcentaje) / 100;

            pj.EnergiaActual += energiaARecuperar;
            if (pj.EnergiaActual > pj.EnergiaMaxima)
                pj.EnergiaActual = pj.EnergiaMaxima;

            pj.DescansosHoy++;

            Console.WriteLine($"Descansas en la posada y recuperas {energiaARecuperar} puntos de energía ({porcentaje}%).");
            Console.WriteLine($"Energía actual: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
        }

        public void RecuperacionPasiva(Personaje.Personaje pj)
        {
            if (pj == null) return;

            TimeSpan tiempoTranscurrido = DateTime.Now - pj.UltimaRecuperacionPasiva;
            int minutos = (int)tiempoTranscurrido.TotalMinutes;

            if (minutos >= 10)
            {
                int puntosARecuperar = minutos / 10; // 1 punto cada 10 min
                pj.EnergiaActual += puntosARecuperar;

                if (pj.EnergiaActual > pj.EnergiaMaxima)
                    pj.EnergiaActual = pj.EnergiaMaxima;

                pj.UltimaRecuperacionPasiva = DateTime.Now;

                Console.WriteLine($"Recuperaste {puntosARecuperar} puntos de energía de manera pasiva.");
            }
        }
    }
}
