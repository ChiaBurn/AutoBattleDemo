using System;

namespace TurnBasedBattle.Infrastructure
{
    /// <summary>
    /// Result returned after a completed battle has been persisted.
    /// </summary>
    public sealed class BattleSaveResult
    {
        public long BattleRunId { get; }
        public DateTime SavedAt { get; }
        public string DatabasePath { get; }

        public BattleSaveResult(long battleRunId, DateTime savedAt, string databasePath)
        {
            if (battleRunId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleRunId), "Battle run id must be greater than 0.");
            }

            BattleRunId = battleRunId;
            SavedAt = savedAt;
            DatabasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        }
    }
}