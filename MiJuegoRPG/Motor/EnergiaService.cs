using System;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class EnergiaService
    {
        private const int EnergiaMaximaDefault = 100;
        private const int CostoRecoleccion = 10;
        private const int MinRecuperacion = 0;

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


        public bool GastarEnergiaRecoleccion(Personaje.Personaje pj)
        {
            if (pj == null) return false;

            if (pj.EnergiaActual < CostoRecoleccion)
            {
                Console.WriteLine("No tienes suficiente energía para recolectar. Debes descansar en una posada.");
                return false;
            }

            pj.EnergiaActual -= CostoRecoleccion;
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
