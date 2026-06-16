using System;
using System.Linq;
using TMPro;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Infrastructure.Records;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// One selectable row in the battle history list.
    /// </summary>
    public sealed class BattleHistoryListItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color selectedColor = new Color(0.35f, 0.65f, 1f, 0.45f);

        private BattleRunSummaryRecord _record;
        private Action<BattleHistoryListItemView> _onSelected;

        public BattleRunSummaryRecord Record => _record;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Render(
            BattleRunSummaryRecord record,
            Action<BattleHistoryListItemView> onSelected)
        {
            _record = record ?? throw new ArgumentNullException(nameof(record));
            _onSelected = onSelected;

            if (summaryText != null)
            {
                string leftOrder = FormatClassOrder(record.LeftOrder);
                string rightOrder = FormatClassOrder(record.RightOrder);
                string winner = BattleTextFormatter.ToDisplayName(record.WinnerTeamSide);

                summaryText.text =
                    $"Run #{record.BattleRunId}｜{record.CreatedAtText}｜勝者：{winner}｜回合：{record.TotalRounds}\n" +
                    $"左方：{leftOrder}\n" +
                    $"右方：{rightOrder}";
            }

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedColor : normalColor;
            }
        }

        private void HandleClicked()
        {
            _onSelected?.Invoke(this);
        }

        private static string FormatClassOrder(System.Collections.Generic.IEnumerable<TurnBasedBattle.Domain.CharacterClass> order)
        {
            return string.Join(" → ", order.Select(BattleTextFormatter.ToDisplayName));
        }
    }
}