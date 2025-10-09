using System;

namespace MiJuegoRPG.Core.Combate.Orden
{
    public readonly struct ActionOrderKey : IComparable<ActionOrderKey>
    {
        public ActionOrderKey(int scheduledTick, byte phaseOrdinal, byte priorityTier, int speedScore, int sequence, int actorId)
        {
            ScheduledTick = scheduledTick;
            PhaseOrdinal = phaseOrdinal;
            PriorityTier = priorityTier;
            SpeedScore = speedScore;
            Sequence = sequence;
            ActorId = actorId;
        }

        public int ScheduledTick { get; }
        public byte PhaseOrdinal { get; }
        public byte PriorityTier { get; }
        public int SpeedScore { get; }
        public int Sequence { get; }
        public int ActorId { get; }

        public int CompareTo(ActionOrderKey o)
        {
            int c;
            if ((c = ScheduledTick.CompareTo(o.ScheduledTick)) != 0) return c;
            if ((c = PhaseOrdinal.CompareTo(o.PhaseOrdinal)) != 0) return c;
            if ((c = PriorityTier.CompareTo(o.PriorityTier)) != 0) return c;
            if ((c = o.SpeedScore.CompareTo(SpeedScore)) != 0) return c; // mayor velocidad primero
            if ((c = Sequence.CompareTo(o.Sequence)) != 0) return c;
            return ActorId.CompareTo(o.ActorId);
        }

        public override string ToString() =>
            $"T={ScheduledTick} P={PhaseOrdinal} Pr={PriorityTier} S={SpeedScore} Seq={Sequence} A={ActorId}";
    }
}
