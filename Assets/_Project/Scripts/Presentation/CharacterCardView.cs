using TMPro;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    public enum CharacterCardHighlightRole
    {
        None = 0,
        Actor = 1,
        EnemyTarget = 2,
        AllyTarget = 3
    }

    /// <summary>
    /// View component for one character card.
    ///
    /// Visual rule:
    /// - Actor is represented by yellow outline.
    /// - Enemy target is represented by red background.
    /// - Ally target is represented by green background.
    ///
    /// This allows Actor + AllyTarget to be displayed at the same time.
    /// </summary>
    public sealed class CharacterCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardInfoText;
        [SerializeField] private Image backgroundImage;

        [Header("Base Colors")]
        [SerializeField] private Color aliveColor = new Color(0.85f, 0.92f, 1f, 0.65f);
        [SerializeField] private Color defeatedColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);

        [Header("Target Highlight Colors")]
        [SerializeField] private Color enemyTargetColor = new Color(1f, 0.30f, 0.30f, 0.90f);
        [SerializeField] private Color allyTargetColor = new Color(0.30f, 1f, 0.45f, 0.90f);

        [Header("Actor Outline")]
        [SerializeField] private Color actorOutlineColor = new Color(1f, 0.85f, 0.10f, 1f);
        [SerializeField] private Vector2 actorOutlineDistance = new Vector2(5f, -5f);

        private CharacterRuntime _lastRenderedCharacter;
        private Outline _actorOutline;

        private void Awake()
        {
            EnsureActorOutline();
            SetActorOutlineVisible(false);
        }

        public void Render(CharacterRuntime character)
        {
            _lastRenderedCharacter = character;

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

            SetHighlight(CharacterCardHighlightRole.None);
        }

        public void Clear()
        {
            _lastRenderedCharacter = null;

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

            SetActorOutlineVisible(false);
        }

        public void SetHighlight(CharacterCardHighlightRole role)
        {
            switch (role)
            {
                case CharacterCardHighlightRole.Actor:
                    SetActorOutlineVisible(true);
                    break;

                case CharacterCardHighlightRole.EnemyTarget:
                    SetBackgroundColor(enemyTargetColor);
                    break;

                case CharacterCardHighlightRole.AllyTarget:
                    SetBackgroundColor(allyTargetColor);
                    break;

                case CharacterCardHighlightRole.None:
                default:
                    SetActorOutlineVisible(false);
                    SetBackgroundColor(GetBaseColor());
                    break;
            }
        }

        private void EnsureActorOutline()
        {
            if (backgroundImage == null)
            {
                return;
            }

            _actorOutline = backgroundImage.GetComponent<Outline>();

            if (_actorOutline == null)
            {
                _actorOutline = backgroundImage.gameObject.AddComponent<Outline>();
            }

            _actorOutline.effectColor = actorOutlineColor;
            _actorOutline.effectDistance = actorOutlineDistance;
            _actorOutline.useGraphicAlpha = false;
        }

        private void SetActorOutlineVisible(bool isVisible)
        {
            if (_actorOutline == null)
            {
                EnsureActorOutline();
            }

            if (_actorOutline != null)
            {
                _actorOutline.effectColor = actorOutlineColor;
                _actorOutline.effectDistance = actorOutlineDistance;
                _actorOutline.useGraphicAlpha = false;
                _actorOutline.enabled = isVisible;
            }
        }

        private void SetBackgroundColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }

        private Color GetBaseColor()
        {
            if (_lastRenderedCharacter == null)
            {
                return defeatedColor;
            }

            return _lastRenderedCharacter.IsAlive ? aliveColor : defeatedColor;
        }
    }
}