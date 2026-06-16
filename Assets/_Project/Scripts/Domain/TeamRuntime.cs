using System;
using System.Collections.Generic;
using System.Linq;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Runtime data for one team during a battle.
    ///
    /// This class owns the ordered character list.
    /// SlotIndex must match the character's position in the list.
    /// </summary>
    [Serializable]
    public sealed class TeamRuntime
    {
        private readonly List<CharacterRuntime> _characters;

        public TeamSide Side { get; }

        public IReadOnlyList<CharacterRuntime> Characters => _characters;

        public int AliveCount => _characters.Count(character => character.IsAlive);

        public int TotalCurrentHp => _characters.Sum(character => character.CurrentHp);

        public int TotalMaxHp => _characters.Sum(character => character.MaxHp);

        public bool IsAllDefeated => _characters.All(character => character.IsDefeated);

        public TeamRuntime(TeamSide side, IEnumerable<CharacterRuntime> characters)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            Side = side;
            _characters = characters.ToList();

            if (_characters.Count == 0)
            {
                throw new ArgumentException("Team must contain at least one character.", nameof(characters));
            }

            ValidateAndNormalizeSlots();
        }

        public static TeamRuntime CreateDefault(TeamSide side, IReadOnlyList<CharacterClass> classOrder)
        {
            if (classOrder == null)
            {
                throw new ArgumentNullException(nameof(classOrder));
            }

            if (classOrder.Count != 4)
            {
                throw new ArgumentException("Default team must contain exactly 4 character classes.", nameof(classOrder));
            }

            List<CharacterRuntime> characters = new List<CharacterRuntime>();

            for (int i = 0; i < classOrder.Count; i++)
            {
                CharacterAttributes attributes = CharacterAttributes.CreateDefault(classOrder[i]);
                characters.Add(new CharacterRuntime(side, i, attributes));
            }

            return new TeamRuntime(side, characters);
        }

        public CharacterRuntime GetCharacterBySlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _characters.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index is outside of the team range.");
            }

            return _characters[slotIndex];
        }

        public List<CharacterRuntime> GetAliveCharacters()
        {
            return _characters
                .Where(character => character.IsAlive)
                .ToList();
        }

        public CharacterRuntime GetLowestHpAlly()
        {
            CharacterRuntime? target = _characters
                .OrderBy(character => character.CurrentHp)
                .ThenBy(character => character.SlotIndex)
                .FirstOrDefault();

            if (target == null)
            {
                throw new InvalidOperationException("Team has no characters.");
            }

            return target;
        }

        public void ReplaceOrder(IReadOnlyList<CharacterRuntime> orderedCharacters)
        {
            if (orderedCharacters == null)
            {
                throw new ArgumentNullException(nameof(orderedCharacters));
            }

            if (orderedCharacters.Count != _characters.Count)
            {
                throw new ArgumentException("New order must contain the same number of characters.", nameof(orderedCharacters));
            }

            _characters.Clear();
            _characters.AddRange(orderedCharacters);

            ValidateAndNormalizeSlots();
        }

        public TeamRuntime Clone()
        {
            List<CharacterRuntime> clonedCharacters = _characters
                .Select(character => character.Clone())
                .ToList();

            return new TeamRuntime(Side, clonedCharacters);
        }

        private void ValidateAndNormalizeSlots()
        {
            for (int i = 0; i < _characters.Count; i++)
            {
                CharacterRuntime character = _characters[i];

                if (character.TeamSide != Side)
                {
                    throw new InvalidOperationException(
                        $"Character team side mismatch. Expected={Side}, Actual={character.TeamSide}.");
                }

                character.SetSlotIndex(i);
            }
        }
    }
}