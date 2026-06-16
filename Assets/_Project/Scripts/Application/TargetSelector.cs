using System;
using System.Collections.Generic;
using TurnBasedBattle.Domain;
using DotNetRandom = System.Random;

namespace TurnBasedBattle.Application
{
    /// <summary>
    /// Selects enemy and ally targets according to the exercise rules.
    /// No Unity dependency.
    /// </summary>
    public sealed class TargetSelector
    {
        public CharacterRuntime SelectEnemyTarget(TeamRuntime enemyTeam, DotNetRandom random)
        {
            if (enemyTeam == null)
            {
                throw new ArgumentNullException(nameof(enemyTeam));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<CharacterRuntime> aliveEnemies = enemyTeam.GetAliveCharacters();

            if (aliveEnemies.Count == 0)
            {
                throw new InvalidOperationException("Cannot select an enemy target because no enemy is alive.");
            }

            int selectedIndex = random.Next(0, aliveEnemies.Count);
            return aliveEnemies[selectedIndex];
        }

        public CharacterRuntime SelectAllyTarget(TeamRuntime allyTeam)
        {
            if (allyTeam == null)
            {
                throw new ArgumentNullException(nameof(allyTeam));
            }

            // The exercise says the ally target is the teammate with the lowest HP,
            // and it can be the actor itself.
            // It does not explicitly require HP > 0 for ally target selection,
            // so defeated allies are also eligible to be healed if the team has not fully lost yet.
            return allyTeam.GetLowestHpAlly();
        }
    }
}