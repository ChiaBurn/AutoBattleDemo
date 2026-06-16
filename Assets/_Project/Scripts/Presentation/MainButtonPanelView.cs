using TurnBasedBattle.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the left-bottom main button panel.
    ///
    /// It controls visibility and interactability only.
    /// Button click handlers will be connected by BattleSceneController later.
    /// </summary>
    public sealed class MainButtonPanelView : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button autoResolveButton;
        [SerializeField] private Button newBattleButton;
        [SerializeField] private Button replayHistoryButton;
        [SerializeField] private Button replayToEndButton;

        public Button StartButton => startButton;
        public Button RestartButton => restartButton;
        public Button AutoResolveButton => autoResolveButton;
        public Button NewBattleButton => newBattleButton;
        public Button ReplayHistoryButton => replayHistoryButton;
        public Button ReplayToEndButton => replayToEndButton;

        public void ApplyPhase(BattlePhase phase)
        {
            HideAll();

            switch (phase)
            {
                case BattlePhase.NotStarted:
                    Show(startButton, true);
                    Show(replayHistoryButton, true);
                    break;

                case BattlePhase.BattleReady:
                case BattlePhase.BattleInProgress:
                    Show(autoResolveButton, true);
                    Show(newBattleButton, true);
                    break;

                case BattlePhase.ReplayReady:
                    Show(replayToEndButton, true);
                    Show(newBattleButton, true);
                    break;

                case BattlePhase.ReplayPlaying:
                    Show(replayToEndButton, false);
                    Show(newBattleButton, false);
                    break;

                case BattlePhase.FinishedSaved:
                case BattlePhase.ReplayFinished:
                    Show(restartButton, true);
                    Show(replayHistoryButton, true);
                    break;

                case BattlePhase.AiRunning:
                case BattlePhase.AiResult:
                case BattlePhase.BattleResolvingRound:
                case BattlePhase.BattleAutoResolving:
                case BattlePhase.Saving:
                case BattlePhase.LoadList:
                    // All buttons hidden / disabled during blocking operations.
                    break;
            }
        }

        private void HideAll()
        {
            Hide(startButton);
            Hide(restartButton);
            Hide(autoResolveButton);
            Hide(newBattleButton);
            Hide(replayHistoryButton);
            Hide(replayToEndButton);
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
    }
}