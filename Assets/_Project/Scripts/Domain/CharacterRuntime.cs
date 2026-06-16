using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Runtime data for a character during a battle.
    ///
    /// CharacterAttributes stores immutable class-level values.
    /// CharacterRuntime stores mutable battle values such as current HP.
    /// </summary>
    [Serializable]
    public sealed class CharacterRuntime
    {
        public TeamSide TeamSide { get; }
        public int SlotIndex { get; private set; }
        public CharacterAttributes Attributes { get; }

        public int CurrentHp { get; private set; }

        public CharacterClass Class => Attributes.Class;
        public int MaxHp => Attributes.MaxHp;
        public int Attack => Attributes.Attack;
        public int Defense => Attributes.Defense;
        public int HealPower => Attributes.HealPower;
        public int ActionCount => Attributes.ActionCount;

        public bool IsAlive => CurrentHp > 0;
        public bool IsDefeated => CurrentHp <= 0;

        public CharacterRuntime(
            TeamSide teamSide,
            int slotIndex,
            CharacterAttributes attributes)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index cannot be negative.");
            }

            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            TeamSide = teamSide;
            SlotIndex = slotIndex;
            CurrentHp = attributes.MaxHp;
        }

        /// <summary>
        /// Used when reconstructing a runtime character from replay or cloned battle data.
        /// </summary>
        public CharacterRuntime(
            TeamSide teamSide,
            int slotIndex,
            CharacterAttributes attributes,
            int currentHp)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index cannot be negative.");
            }

            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            TeamSide = teamSide;
            SlotIndex = slotIndex;
            CurrentHp = ClampHp(currentHp);
        }

        /// <summary>
        /// Applies damage and returns the HP value before and after the change.
        /// </summary>
        public HpChange ApplyDamage(int damageAmount)
        {
            if (damageAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damageAmount), "Damage amount cannot be negative.");
            }

            int hpBefore = CurrentHp;
            CurrentHp = ClampHp(CurrentHp - damageAmount);

            return new HpChange(hpBefore, CurrentHp);
        }

        /// <summary>
        /// Applies healing and returns the HP value before and after the change.
        /// </summary>
        public HpChange ApplyHeal(int healAmount)
        {
            if (healAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healAmount), "Heal amount cannot be negative.");
            }

            int hpBefore = CurrentHp;
            CurrentHp = ClampHp(CurrentHp + healAmount);

            return new HpChange(hpBefore, CurrentHp);
        }

        /// <summary>
        /// Updates the slot index when a team order is changed.
        /// This should be used only before battle starts or when rebuilding runtime data.
        /// </summary>
        public void SetSlotIndex(int slotIndex)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index cannot be negative.");
            }

            SlotIndex = slotIndex;
        }

        /// <summary>
        /// Creates a deep-enough copy for simulation, AI evaluation, background finishing, or replay initialization.
        /// CharacterAttributes is immutable, so it can be safely shared.
        /// </summary>
        public CharacterRuntime Clone()
        {
            return new CharacterRuntime(
                TeamSide,
                SlotIndex,
                Attributes,
                CurrentHp);
        }

        private int ClampHp(int hp)
        {
            if (hp < 0)
            {
                return 0;
            }

            if (hp > MaxHp)
            {
                return MaxHp;
            }

            return hp;
        }
    }

    /// <summary>
    /// Represents a before/after HP delta.
    /// This is useful when creating BattleEvent records and Log text.
    /// </summary>
    public readonly struct HpChange
    {
        public int Before { get; }
        public int After { get; }

        public HpChange(int before, int after)
        {
            Before = before;
            After = after;
        }
    }
}