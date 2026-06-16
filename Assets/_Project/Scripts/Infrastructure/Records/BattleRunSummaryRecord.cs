using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.Infrastructure.Records
{
    /// <summary>
    /// Lightweight record for displaying a battle run in the replay history list.
    /// This is not the full replay payload.
    /// </summary>
    public sealed class BattleRunSummaryRecord
    {
        public long BattleRunId { get; }
        public string CreatedAtText { get; }
        public TeamSide WinnerTeamSide { get; }
        public int TotalRounds { get; }
        public string BattleRulesVersion { get; }
        public bool AiApplied { get; }
        public double? AiWinRate { get; }
        public int? AiSimulationCount { get; }

        public IReadOnlyList<CharacterClass> LeftOrder { get; }
        public IReadOnlyList<CharacterClass> RightOrder { get; }

        public BattleRunSummaryRecord(
            long battleRunId,
            string createdAtText,
            TeamSide winnerTeamSide,
            int totalRounds,
            string battleRulesVersion,
            bool aiApplied,
            double? aiWinRate,
            int? aiSimulationCount,
            IEnumerable<CharacterClass> leftOrder,
            IEnumerable<CharacterClass> rightOrder)
        {
            if (battleRunId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleRunId), "Battle run id must be greater than 0.");
            }

            if (totalRounds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalRounds), "Total rounds must be greater than 0.");
            }

            BattleRunId = battleRunId;
            CreatedAtText = createdAtText ?? throw new ArgumentNullException(nameof(createdAtText));
            WinnerTeamSide = winnerTeamSide;
            TotalRounds = totalRounds;
            BattleRulesVersion = battleRulesVersion ?? throw new ArgumentNullException(nameof(battleRulesVersion));
            AiApplied = aiApplied;
            AiWinRate = aiWinRate;
            AiSimulationCount = aiSimulationCount;

            LeftOrder = (leftOrder ?? throw new ArgumentNullException(nameof(leftOrder))).ToList();
            RightOrder = (rightOrder ?? throw new ArgumentNullException(nameof(rightOrder))).ToList();
        }
    }
}