using System;
using System.Collections.Generic;
using DotNetRandom = System.Random;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.Application
{
    /// <summary>
    /// Creates BattleSession instances.
    ///
    /// This factory is responsible for:
    /// 1. Creating left / right teams.
    /// 2. Randomizing team order.
    /// 3. Producing an initial BattleSession.
    ///
    /// It does not run battle simulation.
    /// </summary>
    public sealed class BattleSessionFactory
    {
        private static readonly CharacterClass[] DefaultClasses =
        {
            CharacterClass.Warrior,
            CharacterClass.Elf,
            CharacterClass.Mage,
            CharacterClass.Priest
        };

        public BattleSession CreateRandomBattle(int seed)
        {
            DotNetRandom random = new DotNetRandom(seed);

            IReadOnlyList<CharacterClass> leftOrder = CreateShuffledClassOrder(random);
            IReadOnlyList<CharacterClass> rightOrder = CreateShuffledClassOrder(random);

            return CreateBattleFromOrders(leftOrder, rightOrder, seed);
        }

        public BattleSession CreateBattleFromOrders(
            IReadOnlyList<CharacterClass> leftOrder,
            IReadOnlyList<CharacterClass> rightOrder,
            int seed)
        {
            if (leftOrder == null)
            {
                throw new ArgumentNullException(nameof(leftOrder));
            }

            if (rightOrder == null)
            {
                throw new ArgumentNullException(nameof(rightOrder));
            }

            TeamRuntime leftTeam = TeamRuntime.CreateDefault(TeamSide.Left, leftOrder);
            TeamRuntime rightTeam = TeamRuntime.CreateDefault(TeamSide.Right, rightOrder);

            BattleRuntime runtime = new BattleRuntime(leftTeam, rightTeam);
            return new BattleSession(runtime, seed);
        }

        private static IReadOnlyList<CharacterClass> CreateShuffledClassOrder(DotNetRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<CharacterClass> result = new List<CharacterClass>(DefaultClasses);

            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }
    }
}