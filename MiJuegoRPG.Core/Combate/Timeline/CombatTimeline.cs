namespace MiJuegoRPG.Core.Combate.Timeline
{
    using System.Collections.Generic;
    using System.Linq;
    using MiJuegoRPG.Core.Combate.Acciones;
    using MiJuegoRPG.Core.Combate.Context;
    using MiJuegoRPG.Core.Combate.Eventos;
    using MiJuegoRPG.Core.Combate.Rng;

    public sealed class CombatTimeline
    {
        private readonly List<CombatAction> actions = new();
        private readonly CombatContext context;
        private readonly int baseSeed;
        private int sequenceCounter;

        public CombatTimeline(int seed)
            : this((uint)seed)
        {
        }

        public CombatTimeline(uint? seed = null)
        {
            baseSeed = (int)(seed ?? 0x1234ABCD);
            var rngFactory = new SplitRngFactory(baseSeed);
            context = new CombatContext(rngFactory, new CombatEventLog(), () => CurrentTick);
        }

        public int CurrentTick
        {
            get; private set;
        }

        public CombatEventLog Log => context.Log;

        public void Enqueue(CombatAction action)
        {
            action.Init(CurrentTick, ++sequenceCounter, context.Log);
            actions.Add(action);
            Resort();
        }

        public void RunUntilFinished(int? maxTicks = null)
        {
            var ticksLimit = maxTicks ?? 10000;
            for (var i = 0; i < ticksLimit; i++)
            {
                Tick();
                if (actions.All(a => a.IsFinished))
                {
                    break;
                }
            }
        }

        public void Tick()
        {
            CurrentTick++;

            // Get snapshot sorted
            Resort();
            foreach (var act in actions.Where(a => !a.IsFinished))
            {
                act.Advance(context, CurrentTick);
            }
        }

        public string ComputeDeterminismHash()
        {
            return baseSeed.ToString("X8") + ":" + Log.ComputeDeterminismHash();
        }

        private void Resort()
        {
            actions.Sort((a, b) => a.OrderKey.CompareTo(b.OrderKey));
        }
    }
}
