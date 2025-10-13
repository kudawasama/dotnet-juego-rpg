namespace MiJuegoRPG.Core.Combate.Context
{
    using MiJuegoRPG.Core.Combate.Eventos;
    using MiJuegoRPG.Core.Combate.Rng;

    public sealed class CombatContext
    {
        private readonly System.Func<int> currentTickProvider;

        public CombatContext(IRngFactory rng, CombatEventLog log, System.Func<int> currentTickProvider)
        {
            Rng = rng;
            Log = log;
            this.currentTickProvider = currentTickProvider;
        }

        public IRngFactory Rng
        {
            get;
        }

        public CombatEventLog Log
        {
            get;
        }

        public int CurrentTick => currentTickProvider();
    }
}
