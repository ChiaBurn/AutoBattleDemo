using System;
using System.Collections.Generic;
using System.Text;
using DotNetRandom = System.Random;
using TurnBasedBattle.Domain;
using UnityEngine;
using TurnBasedBattle.ApplicationServices.Simulation;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.ApplicationServices.Calculators;

namespace TurnBasedBattle.Utilities
{
    /// <summary>
    /// Temporary smoke test for BattleLogFormatter and BattleMetricsCalculator.
    /// Attach this to ApplicationRoot only while verifying Step 3F.
    /// Remove the component after the test passes.
    /// </summary>
    public sealed class BattleFormatterSmokeTest : MonoBehaviour
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
                BattleLogFormatter logFormatter = new BattleLogFormatter();
                BattleMetricsCalculator metricsCalculator = new BattleMetricsCalculator();

                DotNetRandom random = new DotNetRandom(seed);

                IReadOnlyList<BattleEvent> roundEvents = simulator.ResolveNextRound(session, random);
                BattleMetrics metrics = metricsCalculator.Calculate(
                    session,
                    BattlePhase.BattleInProgress,
                    isReplayMode: false);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("[Battle Formatter Smoke Test] Success.");
                builder.AppendLine(logFormatter.FormatBattleStarted(session));

                foreach (BattleEvent battleEvent in roundEvents)
                {
                    builder.AppendLine(logFormatter.FormatEvent(battleEvent));
                }

                builder.AppendLine("---- Metrics ----");
                builder.AppendLine($"模式：{metrics.ModeText}");
                builder.AppendLine($"狀態：{metrics.PhaseText}");
                builder.AppendLine($"目前回合：第 {metrics.CurrentRound} 回合");
                builder.AppendLine($"左隊存活：{metrics.LeftAliveCount} / 4");
                builder.AppendLine($"右隊存活：{metrics.RightAliveCount} / 4");
                builder.AppendLine($"左隊總 HP：{metrics.LeftTotalHp} / {metrics.LeftMaxHp}");
                builder.AppendLine($"右隊總 HP：{metrics.RightTotalHp} / {metrics.RightMaxHp}");
                builder.AppendLine($"勝者：{BattleTextFormatter.ToDisplayName(metrics.Result)}");
                builder.AppendLine($"儲存狀態：{metrics.SaveStatusText}");

                Debug.Log(builder.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Battle Formatter Smoke Test] Failed.\n{exception}");
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