using System;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class EnergiaService

    {
        private const int EnergiaMaximaDefault = 100;
        private const int CostoRecoleccion = 10;
        private const int RecuperacionPorDescanso = 20;

        // Inicializa la energía del personaje si no está inicializada
        public void InicializarEnergia(Personaje.Personaje pj)
        {
            if (pj.EnergiaMaxima <= 0) pj.EnergiaMaxima = EnergiaMaximaDefault;
            if (pj.EnergiaActual > pj.EnergiaMaxima || pj.EnergiaActual < 0)
                pj.EnergiaActual = pj.EnergiaMaxima;
        }

        // Intenta gastar energía para una acción de recolección
        public bool GastarEnergiaRecoleccion(Personaje.Personaje pj)
        {
            InicializarEnergia(pj);
            if (pj.EnergiaActual < CostoRecoleccion)
            {
                Console.WriteLine("No tienes suficiente energía para recolectar. Descansa o usa un objeto para recuperarla.");
                return false;
            }
            pj.EnergiaActual -= CostoRecoleccion;
            Console.WriteLine($"Energía restante: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
            return true;
        }

        // Recupera energía (por descanso, objeto, etc.)
        public void RecuperarEnergia(Personaje.Personaje pj, int cantidad = RecuperacionPorDescanso)
        {
            InicializarEnergia(pj);
            pj.EnergiaActual = Math.Min(pj.EnergiaActual + cantidad, pj.EnergiaMaxima);
            Console.WriteLine($"Energía recuperada. Actual: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
        }

        // Mostrar energía en menús
        public void MostrarEnergia(Personaje.Personaje pj)
        {
            InicializarEnergia(pj);
            Console.WriteLine($"Energía: {pj.EnergiaActual}/{pj.EnergiaMaxima}");
        }
        // Opción de descanso para recuperar energía
        public void Descansar(Personaje.Personaje pj)
        {
            // Obtener el día actual del juego (ejemplo: suponiendo que hay una propiedad estatica Juego.DiaActual)
            int diaActual = Juego.DiaActual;
            // Penalización progresiva: cada descanso adicional recupera menos energía
            int recuperacionBase = 20;
            int penalizacion = pj.DescansosHoy * 5; // Cada descanso extra recupera 5 menos
            int cantidadRecuperar = Math.Max(5, recuperacionBase - penalizacion); // Nunca menos de 5
            RecuperarEnergia(pj, cantidadRecuperar);
            pj.DescansosHoy++;
            Console.WriteLine($"Has descansado. Recuperas {cantidadRecuperar} de energía. Descansos hoy: {pj.DescansosHoy}");
        }
    }   
}
