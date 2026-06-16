using TMPro;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for one character card.
    ///
    /// It only renders runtime data into UI.
    /// It should not decide battle rules.
    /// </summary>
    public sealed class CharacterCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardInfoText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color aliveColor = new Color(0.85f, 0.92f, 1f, 0.65f);
        [SerializeField] private Color defeatedColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);

        public void Render(CharacterRuntime character)
        {
            if (character == null)
            {
                Clear();
                return;
            }

            string className = BattleTextFormatter.ToDisplayName(character.Class);
            string statusText = character.IsAlive ? "存活" : "倒下";

            if (cardInfoText != null)
            {
                cardInfoText.text =
                    $"{character.SlotIndex + 1}. {className}\n" +
                    $"HP {character.CurrentHp} / {character.MaxHp}\n" +
                    $"攻擊 {character.Attack}　防禦 {character.Defense}\n" +
                    $"治療 {character.HealPower}　行動次數 {character.ActionCount}\n" +
                    $"狀態：{statusText}";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = character.IsAlive ? aliveColor : defeatedColor;
            }
        }

        public void Clear()
        {
            if (cardInfoText != null)
            {
                cardInfoText.text =
                    "-\n" +
                    "HP - / -\n" +
                    "攻擊 -　防禦 -\n" +
                    "治療 -　行動次數 -\n" +
                    "狀態：-";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = defeatedColor;
            }
        }
    }
}