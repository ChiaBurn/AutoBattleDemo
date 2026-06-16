using System;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.Calculators
{
    /// <summary>
    /// Pure damage / healing formula calculator.
    /// No Unity dependency.
    /// </summary>
    public sealed class DamageCalculator
    {
        public int CalculateDamage(CharacterRuntime attacker, CharacterRuntime defender)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender == null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            int baseDamage = attacker.Attack - defender.Defense;
            int clampedBaseDamage = Math.Max(0, baseDamage);

            return clampedBaseDamage * attacker.ActionCount;
        }

        public int CalculateHeal(CharacterRuntime actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            return actor.HealPower * actor.ActionCount;
        }
    }
}