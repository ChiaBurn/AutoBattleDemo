using TMPro;
using TurnBasedBattle.Application;
using TurnBasedBattle.Domain;
using UnityEngine;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the left-side metrics panel.
    /// </summary>
    public sealed class BattleMetricsView : MonoBehaviour
    {
        [SerializeField] private TMP_Text modeMetricText;
        [SerializeField] private TMP_Text phaseMetricText;
        [SerializeField] private TMP_Text roundMetricText;
        [SerializeField] private TMP_Text leftAliveMetricText;
        [SerializeField] private TMP_Text rightAliveMetricText;
        [SerializeField] private TMP_Text leftHpMetricText;
        [SerializeField] private TMP_Text rightHpMetricText;
        [SerializeField] private TMP_Text winnerMetricText;
        [SerializeField] private TMP_Text saveMetricText;

        public void Render(BattleMetrics metrics)
        {
            if (metrics == null)
            {
                Clear();
                return;
            }

            SetText(modeMetricText, $"模式：{metrics.ModeText}");
            SetText(phaseMetricText, $"狀態：{metrics.PhaseText}");
            SetText(roundMetricText, $"目前回合：第 {metrics.CurrentRound} 回合");
            SetText(leftAliveMetricText, $"左隊存活：{metrics.LeftAliveCount} / 4");
            SetText(rightAliveMetricText, $"右隊存活：{metrics.RightAliveCount} / 4");
            SetText(leftHpMetricText, $"左隊總 HP：{metrics.LeftTotalHp} / {metrics.LeftMaxHp}");
            SetText(rightHpMetricText, $"右隊總 HP：{metrics.RightTotalHp} / {metrics.RightMaxHp}");
            SetText(winnerMetricText, $"勝者：{BattleTextFormatter.ToDisplayName(metrics.Result)}");
            SetText(saveMetricText, $"儲存狀態：{metrics.SaveStatusText}");
        }

        public void Clear()
        {
            SetText(modeMetricText, "模式：-");
            SetText(phaseMetricText, "狀態：-");
            SetText(roundMetricText, "目前回合：-");
            SetText(leftAliveMetricText, "左隊存活：- / -");
            SetText(rightAliveMetricText, "右隊存活：- / -");
            SetText(leftHpMetricText, "左隊總 HP：- / -");
            SetText(rightHpMetricText, "右隊總 HP：- / -");
            SetText(winnerMetricText, "勝者：-");
            SetText(saveMetricText, "儲存狀態：-");
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}