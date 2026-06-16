using System;
using System.Collections.Generic;
using TMPro;
using TurnBasedBattle.Infrastructure.Records;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the replay history modal.
    /// It renders saved BattleRun summaries and exposes confirm / cancel events.
    /// </summary>
    public sealed class LoadListModalView : MonoBehaviour
    {
        [Header("Modal Objects")]
        [SerializeField] private GameObject screenBlockerOverlay;
        [SerializeField] private GameObject modalRoot;

        [Header("List")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private BattleHistoryListItemView rowTemplate;
        [SerializeField] private TMP_Text emptyText;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private readonly List<BattleHistoryListItemView> _rows = new List<BattleHistoryListItemView>();

        private BattleHistoryListItemView _selectedRow;

        public event Action<BattleRunSummaryRecord> ConfirmSelected;

        public event Action CancelClicked;

        private void Awake()
        {
            if (rowTemplate != null)
            {
                rowTemplate.gameObject.SetActive(false);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HandleCancelClicked);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(HandleCancelClicked);
            }
        }

        public void Show(IReadOnlyList<BattleRunSummaryRecord> records)
        {
            SetActive(true);
            ClearRows();

            _selectedRow = null;
            SetConfirmInteractable(false);

            bool hasRecords = records != null && records.Count > 0;

            if (emptyText != null)
            {
                emptyText.gameObject.SetActive(!hasRecords);
                emptyText.text = "目前沒有可回放的戰鬥紀錄。";
            }

            if (!hasRecords)
            {
                return;
            }

            for (int i = 0; i < records.Count; i++)
            {
                BattleHistoryListItemView row = Instantiate(rowTemplate, contentRoot);
                row.gameObject.SetActive(true);
                row.Render(records[i], HandleRowSelected);
                _rows.Add(row);
            }
        }

        public void Hide()
        {
            SetActive(false);
            ClearRows();
            _selectedRow = null;
            SetConfirmInteractable(false);
        }

        private void HandleRowSelected(BattleHistoryListItemView selectedRow)
        {
            _selectedRow = selectedRow;

            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].SetSelected(_rows[i] == selectedRow);
            }

            SetConfirmInteractable(_selectedRow != null);
        }

        private void HandleConfirmClicked()
        {
            if (_selectedRow == null || _selectedRow.Record == null)
            {
                return;
            }

            ConfirmSelected?.Invoke(_selectedRow.Record);
        }

        private void HandleCancelClicked()
        {
            CancelClicked?.Invoke();
        }

        private void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null)
                {
                    Destroy(_rows[i].gameObject);
                }
            }

            _rows.Clear();
        }

        private void SetActive(bool isActive)
        {
            if (screenBlockerOverlay != null)
            {
                screenBlockerOverlay.SetActive(isActive);
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(isActive);
            }
            else
            {
                gameObject.SetActive(isActive);
            }
        }

        private void SetConfirmInteractable(bool interactable)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = interactable;
            }
        }
    }
}