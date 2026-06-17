using System;
using DotNetRandom = System.Random;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.AI
{
    /// <summary>
    /// AI-only battle simulator.
    ///
    /// This simulator is optimized for Monte Carlo evaluation:
    /// - It mutates BattleRuntime directly.
    /// - It does not create BattleEvent.
    /// - It does not append to EventBuffer.
    /// - It does not produce replay data.
    ///
    /// Use this only for AI win-rate estimation.
    /// Do not use this for player-facing battle execution.
    /// </summary>
    public sealed class FastBattleOutcomeSimulator
    {
        public BattleResult ResolveUntilFinished(
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
                throw new ArgumentOutOfRangeException(
                    nameof(maxAdditionalRounds),
                    "Max additional rounds must be greater than 0.");
            }

            int resolvedRounds = 0;
            BattleRuntime runtime = session.Runtime;

            while (!runtime.IsFinished)
            {
                if (resolvedRounds >= maxAdditionalRounds)
                {
                    throw new InvalidOperationException(
                        $"Battle did not finish within {maxAdditionalRounds} additional rounds. " +
                        "This may indicate a no-progress battle state.");
                }

                runtime.BeginNextRound();

                ResolveTeamTurn(runtime, TeamSide.Left, random);

                if (!runtime.IsFinished)
                {
                    ResolveTeamTurn(runtime, TeamSide.Right, random);
                }

                resolvedRounds++;
            }

            return runtime.Result;
        }

        private static void ResolveTeamTurn(
            BattleRuntime runtime,
            TeamSide actingSide,
            DotNetRandom random)
        {
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
                    continue;
                }

                CharacterRuntime enemyTarget = SelectRandomAliveEnemy(enemyTeam, random);
                CharacterRuntime allyTarget = SelectLowestHpAliveAlly(actingTeam);

                int baseDamage = actor.Attack - enemyTarget.Defense;
                int damageAmount = Math.Max(0, baseDamage) * actor.ActionCount;
                enemyTarget.ApplyDamage(damageAmount);

                int healAmount = actor.HealPower * actor.ActionCount;
                allyTarget.ApplyHeal(healAmount);

                runtime.EvaluateResult();

                if (runtime.IsFinished)
                {
                    return;
                }
            }
        }

        private static CharacterRuntime SelectRandomAliveEnemy(
            TeamRuntime enemyTeam,
            DotNetRandom random)
        {
            int aliveCount = 0;

            for (int i = 0; i < enemyTeam.Characters.Count; i++)
            {
                if (enemyTeam.Characters[i].IsAlive)
                {
                    aliveCount++;
                }
            }

            if (aliveCount <= 0)
            {
                throw new InvalidOperationException(
                    "Cannot select enemy target because no enemy is alive.");
            }

            int selectedAliveIndex = random.Next(0, aliveCount);
            int currentAliveIndex = 0;

            for (int i = 0; i < enemyTeam.Characters.Count; i++)
            {
                CharacterRuntime candidate = enemyTeam.Characters[i];

                if (!candidate.IsAlive)
                {
                    continue;
                }

                if (currentAliveIndex == selectedAliveIndex)
                {
                    return candidate;
                }

                currentAliveIndex++;
            }

            throw new InvalidOperationException("Failed to select random alive enemy.");
        }

        private static CharacterRuntime SelectLowestHpAliveAlly(TeamRuntime allyTeam)
        {
            CharacterRuntime selected = null;

            for (int i = 0; i < allyTeam.Characters.Count; i++)
            {
                CharacterRuntime candidate = allyTeam.Characters[i];

                if (!candidate.IsAlive)
                {
                    continue;
                }

                if (selected == null ||
                    candidate.CurrentHp < selected.CurrentHp ||
                    candidate.CurrentHp == selected.CurrentHp &&
                    candidate.SlotIndex < selected.SlotIndex)
                {
                    selected = candidate;
                }
            }

            if (selected == null)
            {
                throw new InvalidOperationException(
                    "Cannot select ally target because no ally is alive.");
            }

            return selected;
        }
    }
}