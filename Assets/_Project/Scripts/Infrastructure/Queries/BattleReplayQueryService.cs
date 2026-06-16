using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.Infrastructure.Queries
{
    /// <summary>
    /// Loads a full battle replay payload from SQLite.
    /// </summary>
    public sealed class BattleReplayQueryService
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

        private static bool _sqliteInitialized;

        public BattleReplayQueryService(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database path cannot be null or empty.", nameof(databasePath));
            }

            _databasePath = databasePath;
            _connectionString = $"Data Source={_databasePath}";
        }

        public BattleReplayPayload LoadReplay(long battleRunId)
        {
            if (battleRunId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleRunId), "Battle run id must be greater than 0.");
            }

            EnsureSqliteInitialized();

            if (!File.Exists(_databasePath))
            {
                throw new FileNotFoundException("Battle database file was not found.", _databasePath);
            }

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            ReplayHeader header = ReadHeader(connection, battleRunId);
            IReadOnlyList<BattleReplayInitialCharacter> initialCharacters =
                ReadInitialCharacters(connection, battleRunId);
            IReadOnlyList<BattleEvent> events =
                ReadEvents(connection, battleRunId);

            return new BattleReplayPayload(
                header.BattleRunId,
                header.CreatedAtText,
                header.WinnerTeamSide,
                header.TotalRounds,
                header.BattleRulesVersion,
                header.InitialRandomSeed,
                initialCharacters,
                events);
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

        private static ReplayHeader ReadHeader(SqliteConnection connection, long battleRunId)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT
                    Id,
                    CreatedAt,
                    WinnerTeamSide,
                    TotalRounds,
                    BattleRulesVersion,
                    InitialRandomSeed
                FROM BattleRun
                WHERE Id = $battleRunId;
                ";

            command.Parameters.AddWithValue("$battleRunId", battleRunId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                throw new InvalidOperationException($"BattleRun not found. Id={battleRunId}");
            }

            return new ReplayHeader(
                reader.GetInt64(0),
                reader.GetString(1),
                ParseTeamSide(reader.GetString(2)),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetInt32(5));
        }

        private static IReadOnlyList<BattleReplayInitialCharacter> ReadInitialCharacters(
            SqliteConnection connection,
            long battleRunId)
        {
            List<BattleReplayInitialCharacter> result = new List<BattleReplayInitialCharacter>();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT
                    TeamSide,
                    SlotIndex,
                    CharacterClass,
                    InitialHp,
                    Attack,
                    Defense,
                    HealPower,
                    ActionCount
                FROM CharacterInitialRecord
                WHERE BattleRunId = $battleRunId
                ORDER BY TeamSide ASC, SlotIndex ASC;
                ";

            command.Parameters.AddWithValue("$battleRunId", battleRunId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new BattleReplayInitialCharacter(
                    ParseTeamSide(reader.GetString(0)),
                    reader.GetInt32(1),
                    ParseCharacterClass(reader.GetString(2)),
                    reader.GetInt32(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetInt32(7)));
            }

            if (result.Count != 8)
            {
                throw new InvalidOperationException(
                    $"Replay initial character count should be 8, but got {result.Count}.");
            }

            return result;
        }

        private static IReadOnlyList<BattleEvent> ReadEvents(
            SqliteConnection connection,
            long battleRunId)
        {
            List<BattleEvent> result = new List<BattleEvent>();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                SELECT
                    SequenceNo,
                    RoundNo,
                    ActingTeamSide,
                    ActorSlotIndex,
                    ActorClass,
                    WasSkipped,
                    EnemyTargetTeamSide,
                    EnemyTargetSlotIndex,
                    EnemyTargetClass,
                    DamageAmount,
                    EnemyHpBefore,
                    EnemyHpAfter,
                    AllyTargetTeamSide,
                    AllyTargetSlotIndex,
                    AllyTargetClass,
                    HealAmount,
                    AllyHpBefore,
                    AllyHpAfter,
                    WinnerAfterEvent
                FROM BattleEventRecord
                WHERE BattleRunId = $battleRunId
                ORDER BY SequenceNo ASC;
                ";

            command.Parameters.AddWithValue("$battleRunId", battleRunId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(BattleEvent.CreateLoaded(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    ParseTeamSide(reader.GetString(2)),
                    reader.GetInt32(3),
                    ParseCharacterClass(reader.GetString(4)),
                    reader.GetInt32(5) != 0,
                    ReadNullableTeamSide(reader, 6),
                    ReadNullableInt(reader, 7),
                    ReadNullableCharacterClass(reader, 8),
                    reader.GetInt32(9),
                    ReadNullableInt(reader, 10),
                    ReadNullableInt(reader, 11),
                    ReadNullableTeamSide(reader, 12),
                    ReadNullableInt(reader, 13),
                    ReadNullableCharacterClass(reader, 14),
                    reader.GetInt32(15),
                    ReadNullableInt(reader, 16),
                    ReadNullableInt(reader, 17),
                    ReadNullableBattleResult(reader, 18)));
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException($"Replay event stream is empty. BattleRunId={battleRunId}");
            }

            return result;
        }

        private static int? ReadNullableInt(SqliteDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);
        }

        private static TeamSide? ReadNullableTeamSide(SqliteDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? (TeamSide?)null : ParseTeamSide(reader.GetString(index));
        }

        private static CharacterClass? ReadNullableCharacterClass(SqliteDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? (CharacterClass?)null : ParseCharacterClass(reader.GetString(index));
        }

        private static BattleResult ReadNullableBattleResult(SqliteDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? BattleResult.None : ParseBattleResult(reader.GetString(index));
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

        private static BattleResult ParseBattleResult(string value)
        {
            if (Enum.TryParse(value, out BattleResult result))
            {
                return result;
            }

            throw new InvalidOperationException($"Unsupported BattleResult value from database: {value}");
        }

        private sealed class ReplayHeader
        {
            public long BattleRunId { get; }
            public string CreatedAtText { get; }
            public TeamSide WinnerTeamSide { get; }
            public int TotalRounds { get; }
            public string BattleRulesVersion { get; }
            public int InitialRandomSeed { get; }

            public ReplayHeader(
                long battleRunId,
                string createdAtText,
                TeamSide winnerTeamSide,
                int totalRounds,
                string battleRulesVersion,
                int initialRandomSeed)
            {
                BattleRunId = battleRunId;
                CreatedAtText = createdAtText;
                WinnerTeamSide = winnerTeamSide;
                TotalRounds = totalRounds;
                BattleRulesVersion = battleRulesVersion;
                InitialRandomSeed = initialRandomSeed;
            }
        }
    }
}