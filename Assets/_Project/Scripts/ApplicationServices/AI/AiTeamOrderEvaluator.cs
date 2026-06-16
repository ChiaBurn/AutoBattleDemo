using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetRandom = System.Random;
using TurnBasedBattle.ApplicationServices.Factories;
using TurnBasedBattle.ApplicationServices.Simulation;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.AI
{
    /// <summary>
    /// Evaluates all 24 possible left-team class orders against a fixed right-team order.
    ///
    /// This is pure C# and does not depend on UnityEngine.
    /// </summary>
    public sealed class AiTeamOrderEvaluator
    {
        private static readonly CharacterClass[] AllClasses =
        {
            CharacterClass.Warrior,
            CharacterClass.Elf,
            CharacterClass.Mage,
            CharacterClass.Priest
        };

        private readonly BattleSessionFactory _sessionFactory;
        private readonly BattleSimulator _battleSimulator;

        public AiTeamOrderEvaluator()
            : this(new BattleSessionFactory(), new BattleSimulator())
        {
        }

        public AiTeamOrderEvaluator(
            BattleSessionFactory sessionFactory,
            BattleSimulator battleSimulator)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            _battleSimulator = battleSimulator ?? throw new ArgumentNullException(nameof(battleSimulator));
        }

        public AiEvaluationResult EvaluateBestLeftOrder(
            IReadOnlyList<CharacterClass> fixedRightOrder,
            int simulationCountPerOrder,
            int baseSeed)
        {
            if (fixedRightOrder == null)
            {
                throw new ArgumentNullException(nameof(fixedRightOrder));
            }

            if (fixedRightOrder.Count != 4)
            {
                throw new ArgumentException("Right order must contain exactly 4 classes.", nameof(fixedRightOrder));
            }

            if (simulationCountPerOrder <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(simulationCountPerOrder), "Simulation count per order must be greater than 0.");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            List<IReadOnlyList<CharacterClass>> candidateOrders = GeneratePermutations(AllClasses).ToList();

            IReadOnlyList<CharacterClass> bestOrder = candidateOrders[0];
            int bestWinCount = -1;

            for (int orderIndex = 0; orderIndex < candidateOrders.Count; orderIndex++)
            {
                IReadOnlyList<CharacterClass> candidateOrder = candidateOrders[orderIndex];
                int winCount = EvaluateOneOrder(
                    candidateOrder,
                    fixedRightOrder,
                    simulationCountPerOrder,
                    baseSeed);

                if (winCount > bestWinCount)
                {
                    bestWinCount = winCount;
                    bestOrder = candidateOrder;
                }
            }

            stopwatch.Stop();

            int totalSimulationCount = candidateOrders.Count * simulationCountPerOrder;
            double winRate = (double)bestWinCount / simulationCountPerOrder;

            return new AiEvaluationResult(
                bestOrder,
                fixedRightOrder,
                winRate,
                candidateOrders.Count,
                simulationCountPerOrder,
                totalSimulationCount,
                stopwatch.Elapsed);
        }

        private int EvaluateOneOrder(
            IReadOnlyList<CharacterClass> leftOrder,
            IReadOnlyList<CharacterClass> rightOrder,
            int simulationCount,
            int baseSeed)
        {
            int leftWinCount = 0;

            for (int simulationIndex = 0; simulationIndex < simulationCount; simulationIndex++)
            {
                int simulationSeed = CreateSimulationSeed(baseSeed, simulationIndex);

                BattleSession session = _sessionFactory.CreateBattleFromOrders(
                    leftOrder,
                    rightOrder,
                    simulationSeed);

                DotNetRandom random = new DotNetRandom(simulationSeed);
                _battleSimulator.ResolveUntilFinished(session, random);

                if (session.Runtime.Result == BattleResult.LeftWin)
                {
                    leftWinCount++;
                }
            }

            return leftWinCount;
        }

        private static int CreateSimulationSeed(int baseSeed, int simulationIndex)
        {
            unchecked
            {
                int hash = baseSeed;
                hash = (hash * 397) ^ simulationIndex;
                hash = (hash * 397) ^ 0x51A17E;
                return hash;
            }
        }

        private static IEnumerable<IReadOnlyList<CharacterClass>> GeneratePermutations(
            IReadOnlyList<CharacterClass> source)
        {
            CharacterClass[] buffer = source.ToArray();
            return GeneratePermutationsRecursive(buffer, 0);
        }

        private static IEnumerable<IReadOnlyList<CharacterClass>> GeneratePermutationsRecursive(
            CharacterClass[] buffer,
            int startIndex)
        {
            if (startIndex == buffer.Length - 1)
            {
                yield return buffer.ToArray();
                yield break;
            }

            for (int i = startIndex; i < buffer.Length; i++)
            {
                Swap(buffer, startIndex, i);

                foreach (IReadOnlyList<CharacterClass> permutation in GeneratePermutationsRecursive(buffer, startIndex + 1))
                {
                    yield return permutation;
                }

                Swap(buffer, startIndex, i);
            }
        }

        private static void Swap(CharacterClass[] array, int a, int b)
        {
            if (a == b)
            {
                return;
            }

            (array[a], array[b]) = (array[b], array[a]);
        }
    }
}