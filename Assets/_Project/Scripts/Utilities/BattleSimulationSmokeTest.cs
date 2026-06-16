using System;
using System.Collections.Generic;
using System.Text;
using DotNetRandom = System.Random;
using TurnBasedBattle.Application;
using TurnBasedBattle.Domain;
using UnityEngine;

namespace TurnBasedBattle.Utilities
{
    /// <summary>
    /// Temporary smoke test for the pure C# battle simulator.
    /// Attach this to ApplicationRoot only while verifying Step 3E.
    /// Remove the component after the test passes.
    /// </summary>
    public sealed class BattleSimulationSmokeTest : MonoBehaviour
    {
        private void Start()
        {
            RunSmokeTest();
        }

        private void RunSmokeTest()
        {
            try
            {
                const int seed = 12345;

                BattleSession session = CreateTestSession(seed);
                BattleSimulator simulator = new BattleSimulator();
                DotNetRandom random = new DotNetRandom(seed);

                IReadOnlyList<BattleEvent> roundEvents = simulator.ResolveNextRound(session, random);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("[Battle Simulation Smoke Test] Success.");
                builder.AppendLine($"CurrentRound={session.Runtime.CurrentRound}");
                builder.AppendLine($"GeneratedEvents={roundEvents.Count}");
                builder.AppendLine($"BattleResult={session.Runtime.Result}");
                builder.AppendLine($"LeftTeamHp={session.Runtime.LeftTeam.TotalCurrentHp}/{session.Runtime.LeftTeam.TotalMaxHp}");
                builder.AppendLine($"RightTeamHp={session.Runtime.RightTeam.TotalCurrentHp}/{session.Runtime.RightTeam.TotalMaxHp}");

                foreach (BattleEvent battleEvent in roundEvents)
                {
                    if (battleEvent.WasSkipped)
                    {
                        builder.AppendLine(
                            $"Event#{battleEvent.SequenceNo}: Round {battleEvent.RoundNo}, " +
                            $"{battleEvent.ActingTeamSide} slot {battleEvent.ActorSlotIndex} {battleEvent.ActorClass} skipped.");
                    }
                    else
                    {
                        builder.AppendLine(
                            $"Event#{battleEvent.SequenceNo}: Round {battleEvent.RoundNo}, " +
                            $"{battleEvent.ActingTeamSide} slot {battleEvent.ActorSlotIndex} {battleEvent.ActorClass} " +
                            $"hit {battleEvent.EnemyTargetTeamSide} slot {battleEvent.EnemyTargetSlotIndex} {battleEvent.EnemyTargetClass} " +
                            $"for {battleEvent.DamageAmount}, HP {battleEvent.EnemyHpBefore}->{battleEvent.EnemyHpAfter}; " +
                            $"heal {battleEvent.AllyTargetTeamSide} slot {battleEvent.AllyTargetSlotIndex} {battleEvent.AllyTargetClass} " +
                            $"for {battleEvent.HealAmount}, HP {battleEvent.AllyHpBefore}->{battleEvent.AllyHpAfter}.");
                    }
                }

                Debug.Log(builder.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Battle Simulation Smoke Test] Failed.\n{exception}");
            }
        }

        private static BattleSession CreateTestSession(int seed)
        {
            TeamRuntime leftTeam = TeamRuntime.CreateDefault(
                TeamSide.Left,
                new List<CharacterClass>
                {
                    CharacterClass.Warrior,
                    CharacterClass.Elf,
                    CharacterClass.Mage,
                    CharacterClass.Priest
                });

            TeamRuntime rightTeam = TeamRuntime.CreateDefault(
                TeamSide.Right,
                new List<CharacterClass>
                {
                    CharacterClass.Elf,
                    CharacterClass.Mage,
                    CharacterClass.Warrior,
                    CharacterClass.Priest
                });

            BattleRuntime runtime = new BattleRuntime(leftTeam, rightTeam);
            return new BattleSession(runtime, seed);
        }
    }
}