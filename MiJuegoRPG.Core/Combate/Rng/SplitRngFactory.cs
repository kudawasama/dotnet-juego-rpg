using System.Collections.Generic;
using MiJuegoRPG.Core.Combate.Enums;

namespace MiJuegoRPG.Core.Combate.Rng
{
    public interface IRng
    {
        int NextInt(int maxExclusive);
        double NextDouble();
    }

    public interface IRngFactory
    {
        IRng GetStream(RngStreamId id);
    }

    internal sealed class SplitRngFactory : IRngFactory
    {
        private readonly int baseSeed;
        private readonly Dictionary<RngStreamId, IRng> streams = new();

        public SplitRngFactory(int baseSeed) => this.baseSeed = baseSeed;

        public IRng GetStream(RngStreamId id)
        {
            if (streams.TryGetValue(id, out var existing))
            {
                return existing;
            }

            var seeded = new XorShift32(baseSeed + ((int)id * 10007));
            streams[id] = seeded;
            return seeded;
        }

        private sealed class XorShift32 : IRng
        {
            private const uint FallbackSeed = 0xA341316C;
            private uint state;

            public XorShift32(int seed)
            {
                state = seed == 0 ? FallbackSeed : (uint)seed;
                if (state == 0)
                {
                    state = FallbackSeed;
                }
            }

            private uint Next()
            {
                uint x = state;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                state = x;
                return x;
            }

            public int NextInt(int maxExclusive) => (int)(Next() % (uint)maxExclusive);
            public double NextDouble() => Next() / (double)uint.MaxValue;
        }
    }
}
