using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// View component for the battle log scroll content.
    ///
    /// The newest log is inserted at the top.
    /// </summary>
    public sealed class BattleLogView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private TMP_Text logRowTemplate;

        private readonly List<TMP_Text> _runtimeRows = new List<TMP_Text>();

        private void Awake()
        {
            if (logRowTemplate != null)
            {
                logRowTemplate.gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _runtimeRows.Count; i++)
            {
                if (_runtimeRows[i] != null)
                {
                    Destroy(_runtimeRows[i].gameObject);
                }
            }

            _runtimeRows.Clear();
        }

        public void AddLine(string message)
        {
            if (contentRoot == null || logRowTemplate == null)
            {
                Debug.LogWarning("[BattleLogView] Missing contentRoot or logRowTemplate reference.");
                return;
            }

            TMP_Text row = Instantiate(logRowTemplate, contentRoot);
            row.gameObject.SetActive(true);
            row.text = message ?? string.Empty;

            // Newest first.
            row.transform.SetSiblingIndex(0);

            _runtimeRows.Insert(0, row);
        }

        public void AddLines(IEnumerable<string> messages)
        {
            if (messages == null)
            {
                return;
            }

            foreach (string message in messages)
            {
                AddLine(message);
            }
        }
    }
}