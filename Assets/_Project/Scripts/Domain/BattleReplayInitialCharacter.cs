using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Initial character data loaded from SQLite for replay reconstruction.
    /// </summary>
    [Serializable]
    public sealed class BattleReplayInitialCharacter
    {
        public TeamSide TeamSide { get; }
        public int SlotIndex { get; }
        public CharacterClass CharacterClass { get; }
        public int InitialHp { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int HealPower { get; }
        public int ActionCount { get; }

        public BattleReplayInitialCharacter(
            TeamSide teamSide,
            int slotIndex,
            CharacterClass characterClass,
            int initialHp,
            int attack,
            int defense,
            int healPower,
            int actionCount)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index cannot be negative.");
            }

            if (initialHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialHp), "Initial HP must be greater than 0.");
            }

            TeamSide = teamSide;
            SlotIndex = slotIndex;
            CharacterClass = characterClass;
            InitialHp = initialHp;
            Attack = attack;
            Defense = defense;
            HealPower = healPower;
            ActionCount = actionCount;
        }

        public CharacterRuntime ToRuntimeCharacter()
        {
            CharacterAttributes attributes = new CharacterAttributes(
                CharacterClass,
                maxHp: InitialHp,
                attack: Attack,
                defense: Defense,
                healPower: HealPower,
                actionCount: ActionCount);

            return new CharacterRuntime(
                TeamSide,
                SlotIndex,
                attributes,
                currentHp: InitialHp);
        }
    }
}