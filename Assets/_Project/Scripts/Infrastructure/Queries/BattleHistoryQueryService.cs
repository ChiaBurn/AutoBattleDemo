using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using TurnBasedBattle.Domain;
using TurnBasedBattle.Infrastructure.Records;

namespace TurnBasedBattle.Infrastructure.Queries
{
    /// <summary>
    /// Read-only query service for battle history list.
    ///
    /// This service reads battle summaries only.
    /// Full replay loading will be implemented separately.
    /// </summary>
    public sealed class BattleHistoryQueryService
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

        private static bool _sqliteInitialized;

        public BattleHistoryQueryService(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database path cannot be null or empty.", nameof(databasePath));
            }

            _databasePath = databasePath;
            _connectionString = $"Data Source={_databasePath}";
        }

        public IReadOnlyList<BattleRunSummaryRecord> GetLatestBattleRuns(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than 0.");
            }

            EnsureSqliteInitialized();

            if (!File.Exists(_databasePath))
            {
                return Array.Empty<BattleRunSummaryRecord>();
            }

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            if (!TableExists(connection, "BattleRun") ||
                !TableExists(connection, "CharacterInitialRecord"))
            {
                return Array.Empty<BattleRunSummaryRecord>();
            }

            List<BattleRunHeader> headers = ReadBattleRunHeaders(connection, limit);
            List<BattleRunSummaryRecord> result = new List<BattleRunSummaryRecord>();

            foreach (BattleRunHeader header in headers)
            {
                IReadOnlyList<CharacterClass> leftOrder = ReadTeamOrder(
                    connection,
                    header.BattleRunId,
                    TeamSide.Left);

                IReadOnlyList<CharacterClass> rightOrder = ReadTeamOrder(
                    connection,
                    header.BattleRunId,
                    TeamSide.Right);

                result.Add(new BattleRunSummaryRecord(
                    header.BattleRunId,
                    header.CreatedAtText,
                    header.WinnerTeamSide,
                    header.TotalRounds,
                    header.BattleRulesVersion,
                    header.AiApplied,
                    header.AiWinRate,
                    header.AiSimulationCount,
                    leftOrder,
                    rightOrder));
            }

            return result;
        }

        private static void EnsureSqliteInitialized()
        {
            if (_sqliteInitialized)
            {
                return;
            }

            SQLitePCL.Batteries_V2.Init();
            _sqliteInitialized = true;
        }

        private static bool TableExists(SqliteConnection connection, string tableName)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $tableName;
                ";

            command.Parameters.AddWithValue("$tableName", tableName);

            object result = command.ExecuteScalar();
            long count = Convert.ToInt64(result);

            return count > 0;
        }

        private static List<BattleRunHeader> ReadBattleRunHeaders(
            SqliteConnection connection,
            int limit)
        {
            List<BattleRunHeader> result = new List<BattleRunHeader>();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT
                    Id,
                    CreatedAt,
                    WinnerTeamSide,
                    TotalRounds,
                    BattleRulesVersion,
                    AiApplied,
                    AiWinRate,
                    AiSimulationCount
                FROM BattleRun
                ORDER BY Id DESC
                LIMIT $limit;
                ";

            command.Parameters.AddWithValue("$limit", limit);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long battleRunId = reader.GetInt64(0);
                string createdAtText = reader.GetString(1);
                TeamSide winnerTeamSide = ParseTeamSide(reader.GetString(2));
                int totalRounds = reader.GetInt32(3);
                string battleRulesVersion = reader.GetString(4);
                bool aiApplied = reader.GetInt32(5) != 0;
                double? aiWinRate = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6);
                int? aiSimulationCount = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);

                result.Add(new BattleRunHeader(
                    battleRunId,
                    createdAtText,
                    winnerTeamSide,
                    totalRounds,
                    battleRulesVersion,
                    aiApplied,
                    aiWinRate,
                    aiSimulationCount));
            }

            return result;
        }

        private static IReadOnlyList<CharacterClass> ReadTeamOrder(
            SqliteConnection connection,
            long battleRunId,
            TeamSide teamSide)
        {
            List<CharacterClass> result = new List<CharacterClass>();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT CharacterClass
                FROM CharacterInitialRecord
                WHERE BattleRunId = $battleRunId
                  AND TeamSide = $teamSide
                ORDER BY SlotIndex ASC;
                ";

            command.Parameters.AddWithValue("$battleRunId", battleRunId);
            command.Parameters.AddWithValue("$teamSide", teamSide.ToString());

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(ParseCharacterClass(reader.GetString(0)));
            }

            return result;
        }

        private static TeamSide ParseTeamSide(string value)
        {
            if (Enum.TryParse(value, out TeamSide result))
            {
                return result;
            }

            throw new InvalidOperationException($"Unsupported TeamSide value from database: {value}");
        }

        private static CharacterClass ParseCharacterClass(string value)
        {
            if (Enum.TryParse(value, out CharacterClass result))
            {
                return result;
            }

            throw new InvalidOperationException($"Unsupported CharacterClass value from database: {value}");
        }

        private sealed class BattleRunHeader
        {
            public long BattleRunId { get; }
            public string CreatedAtText { get; }
            public TeamSide WinnerTeamSide { get; }
            public int TotalRounds { get; }
            public string BattleRulesVersion { get; }
            public bool AiApplied { get; }
            public double? AiWinRate { get; }
            public int? AiSimulationCount { get; }

            public BattleRunHeader(
                long battleRunId,
                string createdAtText,
                TeamSide winnerTeamSide,
                int totalRounds,
                string battleRulesVersion,
                bool aiApplied,
                double? aiWinRate,
                int? aiSimulationCount)
            {
                BattleRunId = battleRunId;
                CreatedAtText = createdAtText;
                WinnerTeamSide = winnerTeamSide;
                TotalRounds = totalRounds;
                BattleRulesVersion = battleRulesVersion;
                AiApplied = aiApplied;
                AiWinRate = aiWinRate;
                AiSimulationCount = aiSimulationCount;
            }
        }
    }
}