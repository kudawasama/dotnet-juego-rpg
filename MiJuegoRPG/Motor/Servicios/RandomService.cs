using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// Servicio centralizado para generación aleatoria.
    /// Permite inyectar una semilla futura para pruebas deterministas.
    public sealed class RandomService
    {
        private static readonly Lazy<RandomService> _instancia = new(() => new RandomService());
        public static RandomService Instancia => _instancia.Value;

    private Random _random;
    private readonly object _lock = new();

        private RandomService()
        {
            _random = new Random();
        }

        public void SetSeed(int seed)
        {
            lock (_lock)
            {
                _random = new Random(seed);
            }
        }

        public int Next(int max)
        {
            lock (_lock) { return _random.Next(max); }
        }
        public int Next(int min, int max)
        {
            lock (_lock) { return _random.Next(min, max); }
        }
        public double NextDouble()
        {
            lock (_lock) { return _random.NextDouble(); }
        }
    }
}
