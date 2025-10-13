using System;

namespace MiJuegoRPG.Motor.Servicios
{
    // Servicio centralizado para generación aleatoria.
    // Permite inyectar una semilla futura para pruebas deterministas.
    public sealed class RandomService
    {
        private static readonly Lazy<RandomService> instancia = new(() => new RandomService());
        public static RandomService Instancia => instancia.Value;

        private Random random;
        private readonly object @lock = new();

        private RandomService()
        {
            random = new Random();
        }

        public void SetSeed(int seed)
        {
            lock (@lock)
            {
                random = new Random(seed);
            }
        }

        public int Next(int max)
        {
            lock (@lock)
            {
                return random.Next(max);
            }
        }
        public int Next(int min, int max)
        {
            lock (@lock)
            {
                return random.Next(min, max);
            }
        }
        public double NextDouble()
        {
            lock (@lock)
            {
                return random.NextDouble();
            }
        }
    }
}
