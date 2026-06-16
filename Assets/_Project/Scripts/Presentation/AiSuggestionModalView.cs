using System;
using System.Linq;
using TMPro;
using TurnBasedBattle.ApplicationServices.AI;
using TurnBasedBattle.ApplicationServices.Formatters;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// Controls AI running / AI result modal UI.
    /// This component should be attached to an always-active object, such as ModalRoot.
    /// </summary>
    public sealed class AiSuggestionModalView : MonoBehaviour
    {
        [Header("Modal Objects")]
        [SerializeField] private GameObject screenBlockerOverlay;
        [SerializeField] private GameObject aiRunningModal;
        [SerializeField] private GameObject aiResultModal;

        [Header("Running")]
        [SerializeField] private TMP_Text aiRunningText;

        [Header("Result")]
        [SerializeField] private TMP_Text aiResultTitleText;
        [SerializeField] private TMP_Text aiResultDetailText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        public event Action ApplyClicked;

        public event Action CancelClicked;

        private void Awake()
        {
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(HandleApplyClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HandleCancelClicked);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(HandleApplyClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(HandleCancelClicked);
            }
        }

        public void ShowRunning()
        {
            SetOverlay(true);

            if (aiRunningModal != null)
            {
                aiRunningModal.SetActive(true);
            }

            if (aiResultModal != null)
            {
                aiResultModal.SetActive(false);
            }

            if (aiRunningText != null)
            {
                aiRunningText.text = "正在計算左方勝率最高的排列方式...";
            }
        }

        public void ShowResult(AiEvaluationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            SetOverlay(true);

            if (aiRunningModal != null)
            {
                aiRunningModal.SetActive(false);
            }

            if (aiResultModal != null)
            {
                aiResultModal.SetActive(true);
            }

            if (aiResultTitleText != null)
            {
                aiResultTitleText.text = "AI 建議左隊配置";
            }

            if (aiResultDetailText != null)
            {
                string rightOrder = FormatClassOrder(result.FixedRightOrder);
                string suggestedLeftOrder = FormatClassOrder(result.SuggestedLeftOrder);

                aiResultDetailText.text =
                    $"模擬預估左方勝率：{result.WinRate:P1}\n" +
                    $"已測試 {result.TestedOrderCount} 種左隊排列\n" +
                    $"每種排列 {result.SimulationCountPerOrder} 場，共 {result.TotalSimulationCount} 場\n" +
                    $"耗時 {result.ElapsedTime.TotalSeconds:0.00} 秒\n" +
                    $"右方：{rightOrder}\n" +
                    $"建議左方：{suggestedLeftOrder}";
            }
        }

        public void Hide()
        {
            SetOverlay(false);

            if (aiRunningModal != null)
            {
                aiRunningModal.SetActive(false);
            }

            if (aiResultModal != null)
            {
                aiResultModal.SetActive(false);
            }
        }

        private void HandleApplyClicked()
        {
            ApplyClicked?.Invoke();
        }

        private void HandleCancelClicked()
        {
            CancelClicked?.Invoke();
        }

        private void SetOverlay(bool isActive)
        {
            if (screenBlockerOverlay != null)
            {
                screenBlockerOverlay.SetActive(isActive);
            }
        }

        private static string FormatClassOrder(System.Collections.Generic.IEnumerable<Domain.CharacterClass> order)
        {
            return string.Join(" → ", order.Select(BattleTextFormatter.ToDisplayName));
        }
    }
}