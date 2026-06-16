using System;
using System.IO;
using Microsoft.Data.Sqlite;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.Infrastructure
{
    /// <summary>
    /// SQLite persistence service for completed battles.
    ///
    /// Responsibility:
    /// 1. Create database schema if needed.
    /// 2. Save BattleRun, CharacterInitialRecord, and BattleEventRecord in one transaction.
    ///
    /// This service does not depend on UnityEngine.
    /// The caller decides the database file path.
    /// </summary>
    public sealed class BattlePersistenceService
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

        private static bool _sqliteInitialized;

        public BattlePersistenceService(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database path cannot be null or empty.", nameof(databasePath));
            }

            _databasePath = databasePath;
            _connectionString = $"Data Source={_databasePath}";
        }

        public BattleSaveResult SaveCompletedBattle(BattleSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (!session.Runtime.IsFinished)
            {
                throw new InvalidOperationException("Only completed battles can be saved.");
            }

            if (session.Runtime.Result == BattleResult.None)
            {
                throw new InvalidOperationException("Completed battle must have a winner.");
            }

            EnsureSqliteInitialized();
            EnsureDatabaseDirectoryExists();

            DateTime savedAt = DateTime.Now;

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            using SqliteCommand pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
            pragmaCommand.ExecuteNonQuery();

            EnsureSchema(connection);

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                long battleRunId = InsertBattleRun(connection, transaction, session, savedAt);
                InsertCharacterInitialRecords(connection, transaction, battleRunId, session.Runtime.LeftTeam);
                InsertCharacterInitialRecords(connection, transaction, battleRunId, session.Runtime.RightTeam);
                InsertBattleEventRecords(connection, transaction, battleRunId, session);

                transaction.Commit();

                return new BattleSaveResult(battleRunId, savedAt, _databasePath);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        private void EnsureDatabaseDirectoryExists()
        {
            string directory = Path.GetDirectoryName(_databasePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void EnsureSchema(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS BattleRun
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CreatedAt TEXT NOT NULL,
                    WinnerTeamSide TEXT NOT NULL,
                    TotalRounds INTEGER NOT NULL,
                    BattleRulesVersion TEXT NOT NULL,
                    InitialRandomSeed INTEGER NOT NULL,
                    AiApplied INTEGER NOT NULL,
                    AiWinRate REAL NULL,
                    AiSimulationCount INTEGER NULL
                );

                CREATE TABLE IF NOT EXISTS CharacterInitialRecord
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BattleRunId INTEGER NOT NULL,
                    TeamSide TEXT NOT NULL,
                    SlotIndex INTEGER NOT NULL,
                    CharacterClass TEXT NOT NULL,
                    InitialHp INTEGER NOT NULL,
                    Attack INTEGER NOT NULL,
                    Defense INTEGER NOT NULL,
                    HealPower INTEGER NOT NULL,
                    ActionCount INTEGER NOT NULL,
                    FOREIGN KEY (BattleRunId) REFERENCES BattleRun(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS BattleEventRecord
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BattleRunId INTEGER NOT NULL,
                    SequenceNo INTEGER NOT NULL,
                    RoundNo INTEGER NOT NULL,
                    ActingTeamSide TEXT NOT NULL,
                    ActorSlotIndex INTEGER NOT NULL,
                    ActorClass TEXT NOT NULL,
                    WasSkipped INTEGER NOT NULL,
                    EnemyTargetTeamSide TEXT NULL,
                    EnemyTargetSlotIndex INTEGER NULL,
                    EnemyTargetClass TEXT NULL,
                    DamageAmount INTEGER NOT NULL,
                    EnemyHpBefore INTEGER NULL,
                    EnemyHpAfter INTEGER NULL,
                    AllyTargetTeamSide TEXT NULL,
                    AllyTargetSlotIndex INTEGER NULL,
                    AllyTargetClass TEXT NULL,
                    HealAmount INTEGER NOT NULL,
                    AllyHpBefore INTEGER NULL,
                    AllyHpAfter INTEGER NULL,
                    WinnerAfterEvent TEXT NULL,
                    FOREIGN KEY (BattleRunId) REFERENCES BattleRun(Id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS IX_CharacterInitialRecord_BattleRunId
                ON CharacterInitialRecord (BattleRunId);

                CREATE INDEX IF NOT EXISTS IX_BattleEventRecord_BattleRunId_SequenceNo
                ON BattleEventRecord (BattleRunId, SequenceNo);
                ";

            command.ExecuteNonQuery();
        }

        private static long InsertBattleRun(
            SqliteConnection connection,
            SqliteTransaction transaction,
            BattleSession session,
            DateTime savedAt)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                @"
                INSERT INTO BattleRun
                (
                    CreatedAt,
                    WinnerTeamSide,
                    TotalRounds,
                    BattleRulesVersion,
                    InitialRandomSeed,
                    AiApplied,
                    AiWinRate,
                    AiSimulationCount
                )
                VALUES
                (
                    $createdAt,
                    $winnerTeamSide,
                    $totalRounds,
                    $battleRulesVersion,
                    $initialRandomSeed,
                    $aiApplied,
                    $aiWinRate,
                    $aiSimulationCount
                );

                SELECT last_insert_rowid();
                ";

            AddParameter(command, "$createdAt", savedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            AddParameter(command, "$winnerTeamSide", GetWinnerTeamSideText(session.Runtime.Result));
            AddParameter(command, "$totalRounds", session.Runtime.CurrentRound);
            AddParameter(command, "$battleRulesVersion", session.BattleRulesVersion);
            AddParameter(command, "$initialRandomSeed", session.InitialRandomSeed);
            AddParameter(command, "$aiApplied", session.AiApplied ? 1 : 0);
            AddParameter(command, "$aiWinRate", session.AiWinRate.HasValue ? session.AiWinRate.Value : DBNull.Value);
            AddParameter(command, "$aiSimulationCount", session.AiSimulationCount.HasValue ? session.AiSimulationCount.Value : DBNull.Value);

            object result = command.ExecuteScalar();

            if (result == null)
            {
                throw new InvalidOperationException("Failed to retrieve BattleRun Id.");
            }

            return Convert.ToInt64(result);
        }

        private static void InsertCharacterInitialRecords(
            SqliteConnection connection,
            SqliteTransaction transaction,
            long battleRunId,
            TeamRuntime team)
        {
            foreach (CharacterRuntime character in team.Characters)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText =
                    @"
                    INSERT INTO CharacterInitialRecord
                    (
                        BattleRunId,
                        TeamSide,
                        SlotIndex,
                        CharacterClass,
                        InitialHp,
                        Attack,
                        Defense,
                        HealPower,
                        ActionCount
                    )
                    VALUES
                    (
                        $battleRunId,
                        $teamSide,
                        $slotIndex,
                        $characterClass,
                        $initialHp,
                        $attack,
                        $defense,
                        $healPower,
                        $actionCount
                    );
                    ";

                AddParameter(command, "$battleRunId", battleRunId);
                AddParameter(command, "$teamSide", character.TeamSide.ToString());
                AddParameter(command, "$slotIndex", character.SlotIndex);
                AddParameter(command, "$characterClass", character.Class.ToString());
                AddParameter(command, "$initialHp", character.MaxHp);
                AddParameter(command, "$attack", character.Attack);
                AddParameter(command, "$defense", character.Defense);
                AddParameter(command, "$healPower", character.HealPower);
                AddParameter(command, "$actionCount", character.ActionCount);

                command.ExecuteNonQuery();
            }
        }

        private static void InsertBattleEventRecords(
            SqliteConnection connection,
            SqliteTransaction transaction,
            long battleRunId,
            BattleSession session)
        {
            foreach (BattleEvent battleEvent in session.EventBuffer)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText =
                    @"
                    INSERT INTO BattleEventRecord
                    (
                        BattleRunId,
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
                    )
                    VALUES
                    (
                        $battleRunId,
                        $sequenceNo,
                        $roundNo,
                        $actingTeamSide,
                        $actorSlotIndex,
                        $actorClass,
                        $wasSkipped,
                        $enemyTargetTeamSide,
                        $enemyTargetSlotIndex,
                        $enemyTargetClass,
                        $damageAmount,
                        $enemyHpBefore,
                        $enemyHpAfter,
                        $allyTargetTeamSide,
                        $allyTargetSlotIndex,
                        $allyTargetClass,
                        $healAmount,
                        $allyHpBefore,
                        $allyHpAfter,
                        $winnerAfterEvent
                    );
                    ";

                AddParameter(command, "$battleRunId", battleRunId);
                AddParameter(command, "$sequenceNo", battleEvent.SequenceNo);
                AddParameter(command, "$roundNo", battleEvent.RoundNo);
                AddParameter(command, "$actingTeamSide", battleEvent.ActingTeamSide.ToString());
                AddParameter(command, "$actorSlotIndex", battleEvent.ActorSlotIndex);
                AddParameter(command, "$actorClass", battleEvent.ActorClass.ToString());
                AddParameter(command, "$wasSkipped", battleEvent.WasSkipped ? 1 : 0);

                AddParameter(command, "$enemyTargetTeamSide", battleEvent.EnemyTargetTeamSide.HasValue ? battleEvent.EnemyTargetTeamSide.Value.ToString() : DBNull.Value);
                AddParameter(command, "$enemyTargetSlotIndex", battleEvent.EnemyTargetSlotIndex.HasValue ? battleEvent.EnemyTargetSlotIndex.Value : DBNull.Value);
                AddParameter(command, "$enemyTargetClass", battleEvent.EnemyTargetClass.HasValue ? battleEvent.EnemyTargetClass.Value.ToString() : DBNull.Value);
                AddParameter(command, "$damageAmount", battleEvent.DamageAmount);
                AddParameter(command, "$enemyHpBefore", battleEvent.EnemyHpBefore.HasValue ? battleEvent.EnemyHpBefore.Value : DBNull.Value);
                AddParameter(command, "$enemyHpAfter", battleEvent.EnemyHpAfter.HasValue ? battleEvent.EnemyHpAfter.Value : DBNull.Value);

                AddParameter(command, "$allyTargetTeamSide", battleEvent.AllyTargetTeamSide.HasValue ? battleEvent.AllyTargetTeamSide.Value.ToString() : DBNull.Value);
                AddParameter(command, "$allyTargetSlotIndex", battleEvent.AllyTargetSlotIndex.HasValue ? battleEvent.AllyTargetSlotIndex.Value : DBNull.Value);
                AddParameter(command, "$allyTargetClass", battleEvent.AllyTargetClass.HasValue ? battleEvent.AllyTargetClass.Value.ToString() : DBNull.Value);
                AddParameter(command, "$healAmount", battleEvent.HealAmount);
                AddParameter(command, "$allyHpBefore", battleEvent.AllyHpBefore.HasValue ? battleEvent.AllyHpBefore.Value : DBNull.Value);
                AddParameter(command, "$allyHpAfter", battleEvent.AllyHpAfter.HasValue ? battleEvent.AllyHpAfter.Value : DBNull.Value);

                AddParameter(command, "$winnerAfterEvent", battleEvent.WinnerAfterEvent == BattleResult.None
                    ? DBNull.Value
                    : battleEvent.WinnerAfterEvent.ToString());

                command.ExecuteNonQuery();
            }
        }

        private static string GetWinnerTeamSideText(BattleResult result)
        {
            return result switch
            {
                BattleResult.LeftWin => TeamSide.Left.ToString(),
                BattleResult.RightWin => TeamSide.Right.ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Battle result does not contain a winner.")
            };
        }

        private static void AddParameter(SqliteCommand command, string name, object value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }
    }
}