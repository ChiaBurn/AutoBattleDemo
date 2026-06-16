using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Initial character attributes.
    ///
    /// Runtime values such as current HP should be stored in CharacterRuntime,
    /// not in this class.
    /// </summary>
    [Serializable]
    public sealed class CharacterAttributes
    {
        public CharacterClass Class { get; }
        public int MaxHp { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int HealPower { get; }
        public int ActionCount { get; }

        public CharacterAttributes(
            CharacterClass characterClass,
            int maxHp,
            int attack,
            int defense,
            int healPower,
            int actionCount)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be greater than 0.");
            }

            if (attack < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attack), "Attack cannot be negative.");
            }

            if (defense < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defense), "Defense cannot be negative.");
            }

            if (healPower < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healPower), "Heal power cannot be negative.");
            }

            if (actionCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionCount), "Action count must be greater than 0.");
            }

            Class = characterClass;
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            HealPower = healPower;
            ActionCount = actionCount;
        }

        /// <summary>
        /// Creates the attributes according to the class.
        /// </summary>
        public static CharacterAttributes CreateDefault(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => new CharacterAttributes(
                    CharacterClass.Warrior,
                    maxHp: 1000,
                    attack: 100,
                    defense: 50,
                    healPower: 0,
                    actionCount: 1),

                CharacterClass.Elf => new CharacterAttributes(
                    CharacterClass.Elf,
                    maxHp: 1000,
                    attack: 90,
                    defense: 10,
                    healPower: 0,
                    actionCount: 3),

                CharacterClass.Mage => new CharacterAttributes(
                    CharacterClass.Mage,
                    maxHp: 1000,
                    attack: 70,
                    defense: 15,
                    healPower: 10,
                    actionCount: 1),

                CharacterClass.Priest => new CharacterAttributes(
                    CharacterClass.Priest,
                    maxHp: 1000,
                    attack: 20,
                    defense: 20,
                    healPower: 20,
                    actionCount: 2),

                _ => throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, "Unsupported character class.")
            };
        }
    }
}