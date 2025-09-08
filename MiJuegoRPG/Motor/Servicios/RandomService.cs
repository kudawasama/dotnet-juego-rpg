using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// Servicio centralizado para generación aleatoria.
    /// Permite inyectar una semilla futura para pruebas deterministas.
    public sealed class RandomService
    {
        private static readonly Lazy<RandomService> _instancia = new(() => new RandomService());
        public static RandomService Instancia => _instancia.Value;

        private readonly Random _random;

        private RandomService()
        {
            _random = new Random();
        }

        public int Next(int max) => _random.Next(max);
        public int Next(int min, int max) => _random.Next(min, max);
        public double NextDouble() => _random.NextDouble();
    }
}
