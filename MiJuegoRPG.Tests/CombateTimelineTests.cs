namespace MiJuegoRPG.Tests
{
    using System.Linq;
    using FluentAssertions;
    using MiJuegoRPG.Core.Combate.Acciones;
    using MiJuegoRPG.Core.Combate.Eventos;
    using MiJuegoRPG.Core.Combate.Timeline;
    using Xunit;

    public class CombateTimelineTests
    {
        /// <summary>
        /// Verifica que ActionStart se loguea en tick 0 y que DamageApplied ocurre en el mismo tick que ActionImpact (tras completar cast).
        /// </summary>
        [Fact]
        public void ActionStart_Y_DamageApplied_Ticks_Correctos()
        {
            var timeline = new CombatTimeline(777);
            timeline.Enqueue(new SimpleAttackAction(1, 2, castTicks: 2, recoveryTicks: 1, baseDamage: 30));

            // Tick 0: solo encolado (ActionStart en tick 0)
            var startEvent = timeline.Log.Events.Should()
                .ContainSingle(e => e.Type == CombatEventType.ActionStart)
                .Subject;
            startEvent.Tick.Should().Be(0);

            // Avanzar 1 tick (no impacto aún)
            timeline.Tick();
            timeline.Log.Events.Count(e => e.Type == CombatEventType.ActionImpact).Should().Be(0);

            // Avanzar segundo tick -> se produce impacto
            timeline.Tick();
            var impact = timeline.Log.Events.First(e => e.Type == CombatEventType.ActionImpact);
            impact.Tick.Should().Be(2); // castTicks=2 => impacto al inicio del tick 2 tras avanzar
            var dmg = timeline.Log.Events.First(e => e.Type == CombatEventType.DamageApplied);
            dmg.Tick.Should().Be(2);
        }

        /// <summary>
        /// Con distintas seeds el hash debe divergir (algún evento produce diferencias por RNG particionado).
        /// </summary>
        [Fact]
        public void Determinismo_SeedsDistintos_DanHashesDistintos()
        {
            var a = new CombatTimeline(111);
            var b = new CombatTimeline(222);
            a.Enqueue(new SimpleAttackAction(1, 2, castTicks: 2, recoveryTicks: 1, baseDamage: 40));
            b.Enqueue(new SimpleAttackAction(1, 2, castTicks: 2, recoveryTicks: 1, baseDamage: 40));

            for (int i = 0; i < 8; i++)
            {
                a.Tick();
                b.Tick();
            }

            var hashA = a.ComputeDeterminismHash();
            var hashB = b.ComputeDeterminismHash();
            hashA.Should().NotBe(hashB, "semillas diferentes deben diverger en algún evento (crit RNG u orden interno)");
        }

        /// <summary>
        /// Misma seed y mismas acciones deben producir hash idéntico (determinismo fuerte).
        /// </summary>
        [Fact]
        public void Determinismo_MismaSeed_MismoHash()
        {
            var t1 = new CombatTimeline(12345);
            var t2 = new CombatTimeline(12345);
            t1.Enqueue(new SimpleAttackAction(1, 2, castTicks: 2, recoveryTicks: 1, baseDamage: 50));
            t2.Enqueue(new SimpleAttackAction(1, 2, castTicks: 2, recoveryTicks: 1, baseDamage: 50));

            for (int i = 0; i < 10; i++)
            {
                t1.Tick();
                t2.Tick();
            }

            t1.ComputeDeterminismHash().Should().Be(t2.ComputeDeterminismHash());
        }

        /// <summary>
        /// Dos impactos simultáneos deben resolverse de forma determinista: SpeedScore (igual) -> SequenceId.
        /// </summary>
        [Fact]
        public void Colision_Simultanea_OrdenDeterminista_PorVelocidadYPrioridad()
        {
            var timeline = new CombatTimeline(999);
            timeline.Enqueue(new SimpleAttackAction(1, 2, 2, 1, 40));
            timeline.Enqueue(new SimpleAttackAction(2, 1, 2, 1, 45));

            for (int i = 0; i < 5; i++)
            {
                timeline.Tick();
            }

            // Ambos impactos deben existir; hash estable
            timeline.Log.Events.Should().NotBeEmpty();
            var impacts = timeline.Log.Events.Where(e => e.Type == CombatEventType.ActionImpact).ToList();
            impacts.Count.Should().Be(2);

            // Orden determinista: según SpeedScore (igual ahora) cae en SequenceId (primero encolado primero)
            impacts[0].ActorId.Should().Be(1);
            impacts[1].ActorId.Should().Be(2);
        }
    }
}
