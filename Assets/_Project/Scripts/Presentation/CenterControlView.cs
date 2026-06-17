using TMPro;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the center control area between both teams.
    ///
    /// Responsibilities:
    /// 1. Control center button visibility.
    /// 2. Control winner / replay-finished text.
    /// 3. Display the skip button only during skippable fast-forward phases.
    ///
    /// This class does not own battle rules, replay logic, AI logic, or persistence logic.
    /// </summary>
    public sealed class CenterControlView : MonoBehaviour
    {
        [SerializeField] private Button aiSuggestButton;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private Button replayCurrentButton;
        [SerializeField] private Button skipAnimationButton;

        public Button AiSuggestButton => aiSuggestButton;
        public Button NextRoundButton => nextRoundButton;
        public Button ReplayCurrentButton => replayCurrentButton;
        public Button SkipAnimationButton => skipAnimationButton;

        public void ApplyPhase(BattlePhase phase, BattleResult result)
        {
            HideAll();

            switch (phase)
            {
                case BattlePhase.BattleReady:
                    Show(aiSuggestButton, true);
                    Show(nextRoundButton, true);
                    SetButtonText(nextRoundButton, "下一回合");
                    break;

                case BattlePhase.BattleInProgress:
                    Show(nextRoundButton, true);
                    SetButtonText(nextRoundButton, "下一回合");
                    break;

                case BattlePhase.BattleResolvingRound:
                    // Normal next-round animation.
                    // No skip button; this mode is intentionally readable.
                    break;

                case BattlePhase.BattleAutoResolving:
                    // Fast-forward battle-to-end animation.
                    // Skip is allowed here.
                    Show(skipAnimationButton, true);
                    break;

                case BattlePhase.ReplayReady:
                    Show(nextRoundButton, true);
                    SetButtonText(nextRoundButton, "播放下一回合");
                    break;

                case BattlePhase.ReplayPlaying:
                    // Normal replay-next-round animation.
                    // No skip button; this mode is intentionally readable.
                    break;

                case BattlePhase.ReplayAutoPlaying:
                    // Fast-forward replay-to-end animation.
                    // Skip is allowed here.
                    Show(skipAnimationButton, true);
                    break;

                case BattlePhase.FinishedSaved:
                    ShowWinner(result);
                    Show(replayCurrentButton, true);
                    SetButtonText(replayCurrentButton, "重播本場");
                    break;

                case BattlePhase.ReplayFinished:
                    ShowReplayFinished();
                    Show(replayCurrentButton, true);
                    SetButtonText(replayCurrentButton, "重播本紀錄");
                    break;

                case BattlePhase.NotStarted:
                case BattlePhase.AiRunning:
                case BattlePhase.AiResult:
                case BattlePhase.Saving:
                case BattlePhase.LoadList:
                    break;
            }
        }

        private void ShowWinner(BattleResult result)
        {
            if (winnerText == null)
            {
                return;
            }

            winnerText.gameObject.SetActive(true);
            winnerText.text = $"勝者：{BattleTextFormatter.ToDisplayName(result)}";
        }

        private void ShowReplayFinished()
        {
            if (winnerText == null)
            {
                return;
            }

            winnerText.gameObject.SetActive(true);
            winnerText.text = "回放結束";
        }

        private void HideAll()
        {
            Hide(aiSuggestButton);
            Hide(nextRoundButton);
            Hide(replayCurrentButton);
            Hide(skipAnimationButton);

            if (winnerText != null)
            {
                winnerText.gameObject.SetActive(false);
            }
        }

        private static void Show(Button button, bool interactable)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(true);
            button.interactable = interactable;
        }

        private static void Hide(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(false);
        }

        private static void SetButtonText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(includeInactive: true);

            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
    }
}