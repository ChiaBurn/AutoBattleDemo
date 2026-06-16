using System;
using System.Collections.Generic;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Runtime wrapper for the current battle.
    ///
    /// BattleRuntime contains combat data.
    /// BattleSession contains application-level runtime data:
    /// event buffer, save status, AI metadata, and initial seed.
    /// </summary>
    [Serializable]
    public sealed class BattleSession
    {
        public const string CurrentBattleRulesVersion = "1.0";

        private readonly List<BattleEvent> _eventBuffer;

        public BattleRuntime Runtime { get; private set; }

        public IReadOnlyList<BattleEvent> EventBuffer => _eventBuffer;

        public int InitialRandomSeed { get; }

        public string BattleRulesVersion { get; }

        public bool IsSaved { get; private set; }

        public long? SavedBattleRunId { get; private set; }

        public bool AiApplied { get; private set; }

        public double? AiWinRate { get; private set; }

        public int? AiSimulationCount { get; private set; }

        public BattleSession(BattleRuntime runtime, int initialRandomSeed)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            InitialRandomSeed = initialRandomSeed;
            BattleRulesVersion = CurrentBattleRulesVersion;
            _eventBuffer = new List<BattleEvent>();
            IsSaved = false;
            SavedBattleRunId = null;
            AiApplied = false;
            AiWinRate = null;
            AiSimulationCount = null;
        }

        private BattleSession(
            BattleRuntime runtime,
            int initialRandomSeed,
            string battleRulesVersion,
            IEnumerable<BattleEvent> eventBuffer,
            bool isSaved,
            long? savedBattleRunId,
            bool aiApplied,
            double? aiWinRate,
            int? aiSimulationCount)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            InitialRandomSeed = initialRandomSeed;
            BattleRulesVersion = battleRulesVersion ?? throw new ArgumentNullException(nameof(battleRulesVersion));
            _eventBuffer = new List<BattleEvent>(eventBuffer ?? throw new ArgumentNullException(nameof(eventBuffer)));
            IsSaved = isSaved;
            SavedBattleRunId = savedBattleRunId;
            AiApplied = aiApplied;
            AiWinRate = aiWinRate;
            AiSimulationCount = aiSimulationCount;
        }

        public int NextSequenceNo => _eventBuffer.Count + 1;

        public void AddEvent(BattleEvent battleEvent)
        {
            if (battleEvent == null)
            {
                throw new ArgumentNullException(nameof(battleEvent));
            }

            _eventBuffer.Add(battleEvent);
        }

        public void MarkAiApplied(double winRate, int simulationCount)
        {
            if (winRate < 0 || winRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(winRate), "Win rate must be between 0 and 1.");
            }

            if (simulationCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(simulationCount), "Simulation count must be greater than 0.");
            }

            AiApplied = true;
            AiWinRate = winRate;
            AiSimulationCount = simulationCount;
        }

        public void MarkSaved(long battleRunId)
        {
            if (battleRunId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleRunId), "Battle run id must be greater than 0.");
            }

            IsSaved = true;
            SavedBattleRunId = battleRunId;
        }

        public BattleSession CloneForBackgroundSimulation()
        {
            return new BattleSession(
                Runtime.Clone(),
                InitialRandomSeed,
                BattleRulesVersion,
                _eventBuffer,
                IsSaved,
                SavedBattleRunId,
                AiApplied,
                AiWinRate,
                AiSimulationCount);
        }
    }
}