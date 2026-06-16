using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Aggregated values for the left-side Metrics UI panel.
    ///
    /// This class is intentionally separate from CharacterAttributes.
    /// "Attributes" means character configuration.
    /// "Metrics" means aggregated runtime display data.
    /// </summary>
    [Serializable]
    public sealed class BattleMetrics
    {
        public string ModeText { get; }
        public string PhaseText { get; }

        public int CurrentRound { get; }

        public int LeftAliveCount { get; }
        public int RightAliveCount { get; }

        public int LeftTotalHp { get; }
        public int RightTotalHp { get; }

        public int LeftMaxHp { get; }
        public int RightMaxHp { get; }

        public BattleResult Result { get; }

        public string SaveStatusText { get; }

        public BattleMetrics(
            string modeText,
            string phaseText,
            int currentRound,
            int leftAliveCount,
            int rightAliveCount,
            int leftTotalHp,
            int rightTotalHp,
            int leftMaxHp,
            int rightMaxHp,
            BattleResult result,
            string saveStatusText)
        {
            ModeText = modeText ?? throw new ArgumentNullException(nameof(modeText));
            PhaseText = phaseText ?? throw new ArgumentNullException(nameof(phaseText));
            SaveStatusText = saveStatusText ?? throw new ArgumentNullException(nameof(saveStatusText));

            if (currentRound < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentRound), "Current round cannot be negative.");
            }

            if (leftAliveCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(leftAliveCount), "Left alive count cannot be negative.");
            }

            if (rightAliveCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rightAliveCount), "Right alive count cannot be negative.");
            }

            if (leftTotalHp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(leftTotalHp), "Left total HP cannot be negative.");
            }

            if (rightTotalHp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rightTotalHp), "Right total HP cannot be negative.");
            }

            if (leftMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(leftMaxHp), "Left max HP must be greater than 0.");
            }

            if (rightMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rightMaxHp), "Right max HP must be greater than 0.");
            }

            CurrentRound = currentRound;
            LeftAliveCount = leftAliveCount;
            RightAliveCount = rightAliveCount;
            LeftTotalHp = leftTotalHp;
            RightTotalHp = rightTotalHp;
            LeftMaxHp = leftMaxHp;
            RightMaxHp = rightMaxHp;
            Result = result;
        }
    }
}