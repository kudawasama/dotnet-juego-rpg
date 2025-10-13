namespace MiJuegoRPG.Core.Combate.Eventos
{
    using System.Collections.Generic;

    public sealed class CombatEventLog
    {
        private readonly List<CombatEvent> events = new(256);

        public IReadOnlyList<CombatEvent> Events => events;

        public void Add(CombatEvent ev) => events.Add(ev);

        public int ComputeDeterministicHash()
        {
            unchecked
            {
                int h = 17;
                foreach (var e in events)
                {
                    h = (h * 31) + e.Tick;
                    h = (h * 31) + (int)e.Type;
                    h = (h * 31) + e.ActorId;
                    h = (h * 31) + e.TargetId;
                    h = (h * 31) + e.RefActionSeq;
                    h = (h * 31) + e.V1;
                    h = (h * 31) + e.V2;
                }

                return h;
            }
        }

        public string ComputeDeterminismHash() => ComputeDeterministicHash().ToString("X8");
    }
}
