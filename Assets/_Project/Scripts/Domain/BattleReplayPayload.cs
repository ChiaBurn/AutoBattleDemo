using System;
using System.Collections.Generic;
using System.Linq;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Full replay payload loaded from SQLite.
    /// Contains initial team setup and event stream.
    /// </summary>
    [Serializable]
    public sealed class BattleReplayPayload
    {
        public long BattleRunId { get; }
        public string CreatedAtText { get; }
        public TeamSide WinnerTeamSide { get; }
        public int TotalRounds { get; }
        public string BattleRulesVersion { get; }
        public int InitialRandomSeed { get; }

        public IReadOnlyList<BattleReplayInitialCharacter> InitialCharacters { get; }
        public IReadOnlyList<BattleEvent> Events { get; }

        public BattleReplayPayload(
            long battleRunId,
            string createdAtText,
            TeamSide winnerTeamSide,
            int totalRounds,
            string battleRulesVersion,
            int initialRandomSeed,
            IEnumerable<BattleReplayInitialCharacter> initialCharacters,
            IEnumerable<BattleEvent> events)
        {
            if (battleRunId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleRunId), "Battle run id must be greater than 0.");
            }

            BattleRunId = battleRunId;
            CreatedAtText = createdAtText ?? throw new ArgumentNullException(nameof(createdAtText));
            WinnerTeamSide = winnerTeamSide;
            TotalRounds = totalRounds;
            BattleRulesVersion = battleRulesVersion ?? throw new ArgumentNullException(nameof(battleRulesVersion));
            InitialRandomSeed = initialRandomSeed;

            InitialCharacters = (initialCharacters ?? throw new ArgumentNullException(nameof(initialCharacters)))
                .OrderBy(character => character.TeamSide)
                .ThenBy(character => character.SlotIndex)
                .ToList();

            Events = (events ?? throw new ArgumentNullException(nameof(events)))
                .OrderBy(battleEvent => battleEvent.SequenceNo)
                .ToList();
        }
    }
}