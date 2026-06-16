using TMPro;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the center control area between both teams.
    /// </summary>
    public sealed class CenterControlView : MonoBehaviour
    {
        [SerializeField] private Button aiSuggestButton;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private Button replayCurrentButton;

        public Button AiSuggestButton => aiSuggestButton;
        public Button NextRoundButton => nextRoundButton;
        public Button ReplayCurrentButton => replayCurrentButton;

        public void ApplyPhase(BattlePhase phase, BattleResult result)
        {
            HideAll();

            switch (phase)
            {
                case BattlePhase.BattleReady:
                    Show(aiSuggestButton, true);
                    Show(nextRoundButton, true);
                    break;

                case BattlePhase.BattleInProgress:
                    Show(nextRoundButton, true);
                    break;

                case BattlePhase.ReplayReady:
                    Show(nextRoundButton, true);
                    SetButtonText(nextRoundButton, "播放下一回合");
                    break;

                case BattlePhase.FinishedSaved:
                    ShowWinner(result);
                    Show(replayCurrentButton, true);
                    break;

                case BattlePhase.ReplayFinished:
                    ShowReplayFinished();
                    Show(replayCurrentButton, true);
                    SetButtonText(replayCurrentButton, "重播本紀錄");
                    break;

                case BattlePhase.AiRunning:
                case BattlePhase.AiResult:
                case BattlePhase.BattleResolvingRound:
                case BattlePhase.BattleAutoResolving:
                case BattlePhase.Saving:
                case BattlePhase.LoadList:
                case BattlePhase.NotStarted:
                case BattlePhase.ReplayPlaying:
                    break;
            }

            if (phase != BattlePhase.ReplayReady)
            {
                SetButtonText(nextRoundButton, "下一回合");
            }

            if (phase != BattlePhase.ReplayFinished)
            {
                SetButtonText(replayCurrentButton, "重播本場");
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