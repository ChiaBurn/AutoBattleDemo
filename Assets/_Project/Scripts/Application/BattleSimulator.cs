using System;
using System.Collections.Generic;
using TurnBasedBattle.Domain;
using DotNetRandom = System.Random;

namespace TurnBasedBattle.Application
{
    /// <summary>
    /// Pure C# battle simulator.
    /// It mutates BattleRuntime and appends BattleEvent into BattleSession.EventBuffer.
    /// No Unity dependency.
    /// </summary>
    public sealed class BattleSimulator
    {
        private readonly DamageCalculator _damageCalculator;
        private readonly TargetSelector _targetSelector;

        public BattleSimulator()
            : this(new DamageCalculator(), new TargetSelector())
        {
        }

        public BattleSimulator(DamageCalculator damageCalculator, TargetSelector targetSelector)
        {
            _damageCalculator = damageCalculator ?? throw new ArgumentNullException(nameof(damageCalculator));
            _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        }

        public IReadOnlyList<BattleEvent> ResolveNextRound(BattleSession session, DotNetRandom random)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            BattleRuntime runtime = session.Runtime;

            if (runtime.IsFinished)
            {
                return Array.Empty<BattleEvent>();
            }

            runtime.BeginNextRound();

            List<BattleEvent> generatedEvents = new List<BattleEvent>();

            ResolveTeamTurn(session, TeamSide.Left, random, generatedEvents);

            if (!runtime.IsFinished)
            {
                ResolveTeamTurn(session, TeamSide.Right, random, generatedEvents);
            }

            return generatedEvents;
        }

        public IReadOnlyList<BattleEvent> ResolveUntilFinished(
            BattleSession session,
            DotNetRandom random,
            int maxAdditionalRounds = 10000)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (maxAdditionalRounds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAdditionalRounds), "Max additional rounds must be greater than 0.");
            }

            List<BattleEvent> generatedEvents = new List<BattleEvent>();

            int resolvedRounds = 0;

            while (!session.Runtime.IsFinished)
            {
                if (resolvedRounds >= maxAdditionalRounds)
                {
                    throw new InvalidOperationException(
                        $"Battle did not finish within {maxAdditionalRounds} additional rounds. " +
                        "This may indicate a no-progress battle state.");
                }

                IReadOnlyList<BattleEvent> roundEvents = ResolveNextRound(session, random);
                generatedEvents.AddRange(roundEvents);
                resolvedRounds++;
            }

            return generatedEvents;
        }

        private void ResolveTeamTurn(
            BattleSession session,
            TeamSide actingSide,
            DotNetRandom random,
            List<BattleEvent> generatedEvents)
        {
            BattleRuntime runtime = session.Runtime;
            TeamRuntime actingTeam = runtime.GetTeam(actingSide);
            TeamRuntime enemyTeam = runtime.GetOpponentTeam(actingSide);

            for (int slotIndex = 0; slotIndex < actingTeam.Characters.Count; slotIndex++)
            {
                runtime.EvaluateResult();

                if (runtime.IsFinished)
                {
                    return;
                }

                CharacterRuntime actor = actingTeam.GetCharacterBySlotIndex(slotIndex);

                if (!actor.IsAlive)
                {
                    BattleEvent skippedEvent = BattleEvent.CreateSkipped(
                        session.NextSequenceNo,
                        runtime.CurrentRound,
                        actor,
                        runtime.EvaluateResult());

                    session.AddEvent(skippedEvent);
                    generatedEvents.Add(skippedEvent);
                    continue;
                }

                CharacterRuntime enemyTarget = _targetSelector.SelectEnemyTarget(enemyTeam, random);
                CharacterRuntime allyTarget = _targetSelector.SelectAllyTarget(actingTeam);

                int damageAmount = _damageCalculator.CalculateDamage(actor, enemyTarget);
                HpChange enemyHpChange = enemyTarget.ApplyDamage(damageAmount);

                int healAmount = _damageCalculator.CalculateHeal(actor);
                HpChange allyHpChange = allyTarget.ApplyHeal(healAmount);

                BattleResult resultAfterAction = runtime.EvaluateResult();

                BattleEvent actionEvent = BattleEvent.CreateAction(
                    session.NextSequenceNo,
                    runtime.CurrentRound,
                    actor,
                    enemyTarget,
                    damageAmount,
                    enemyHpChange,
                    allyTarget,
                    healAmount,
                    allyHpChange,
                    resultAfterAction);

                session.AddEvent(actionEvent);
                generatedEvents.Add(actionEvent);

                if (runtime.IsFinished)
                {
                    return;
                }
            }
        }
    }
}