using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.AI
{
    /// <summary>
    /// Result of AI left-team order evaluation.
    /// </summary>
    public sealed class AiEvaluationResult
    {
        public IReadOnlyList<CharacterClass> SuggestedLeftOrder { get; }
        public IReadOnlyList<CharacterClass> FixedRightOrder { get; }

        public double WinRate { get; }
        public int TestedOrderCount { get; }
        public int SimulationCountPerOrder { get; }
        public int TotalSimulationCount { get; }
        public TimeSpan ElapsedTime { get; }

        public AiEvaluationResult(
            IEnumerable<CharacterClass> suggestedLeftOrder,
            IEnumerable<CharacterClass> fixedRightOrder,
            double winRate,
            int testedOrderCount,
            int simulationCountPerOrder,
            int totalSimulationCount,
            TimeSpan elapsedTime)
        {
            if (winRate < 0 || winRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(winRate), "Win rate must be between 0 and 1.");
            }

            if (testedOrderCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(testedOrderCount), "Tested order count must be greater than 0.");
            }

            if (simulationCountPerOrder <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(simulationCountPerOrder), "Simulation count per order must be greater than 0.");
            }

            if (totalSimulationCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalSimulationCount), "Total simulation count must be greater than 0.");
            }

            SuggestedLeftOrder = (suggestedLeftOrder ?? throw new ArgumentNullException(nameof(suggestedLeftOrder))).ToList();
            FixedRightOrder = (fixedRightOrder ?? throw new ArgumentNullException(nameof(fixedRightOrder))).ToList();

            WinRate = winRate;
            TestedOrderCount = testedOrderCount;
            SimulationCountPerOrder = simulationCountPerOrder;
            TotalSimulationCount = totalSimulationCount;
            ElapsedTime = elapsedTime;
        }
    }
}