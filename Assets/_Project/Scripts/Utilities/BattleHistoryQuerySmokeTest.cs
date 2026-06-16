using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;
using TurnBasedBattle.Infrastructure.Queries;
using TurnBasedBattle.Infrastructure.Records;
using UnityEngine;

namespace TurnBasedBattle.Utilities
{
    /// <summary>
    /// Temporary smoke test for reading saved battle history from SQLite.
    /// Attach this to ApplicationRoot only while verifying Step 5B.
    /// Remove the component after the test passes.
    /// </summary>
    public sealed class BattleHistoryQuerySmokeTest : MonoBehaviour
    {
        private const string DatabaseFileName = "battle_runs.db";

        private void Start()
        {
            RunSmokeTest();
        }

        private void RunSmokeTest()
        {
            try
            {
                string databasePath = Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    DatabaseFileName);

                BattleHistoryQueryService queryService = new BattleHistoryQueryService(databasePath);
                IReadOnlyList<BattleRunSummaryRecord> records = queryService.GetLatestBattleRuns(limit: 10);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("[Battle History Query Smoke Test] Success.");
                builder.AppendLine($"DatabasePath={databasePath}");
                builder.AppendLine($"RecordCount={records.Count}");

                foreach (BattleRunSummaryRecord record in records)
                {
                    builder.AppendLine(
                        $"Run #{record.BattleRunId} | " +
                        $"{record.CreatedAtText} | " +
                        $"Winner={BattleTextFormatter.ToDisplayName(record.WinnerTeamSide)} | " +
                        $"Rounds={record.TotalRounds} | " +
                        $"Rules={record.BattleRulesVersion}");

                    builder.AppendLine($"  Left : {FormatClassOrder(record.LeftOrder)}");
                    builder.AppendLine($"  Right: {FormatClassOrder(record.RightOrder)}");
                }

                Debug.Log(builder.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Battle History Query Smoke Test] Failed.\n{exception}");
            }
        }

        private static string FormatClassOrder(IEnumerable<CharacterClass> order)
        {
            return string.Join(" ¡÷ ", order.Select(BattleTextFormatter.ToDisplayName));
        }
    }
}