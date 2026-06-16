using System;
using System.Collections.Generic;
using DotNetRandom = System.Random;
using TurnBasedBattle.Application;
using TurnBasedBattle.Domain;
using UnityEngine;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// Main scene controller for BattleScene.
    ///
    /// Current implementation scope:
    /// 1. Start a new battle.
    /// 2. Resolve next round.
    /// 3. Resolve until finished.
    /// 4. Render character cards, log, metrics, and button visibility.
    ///
    /// Not implemented yet:
    /// 1. AI suggestion.
    /// 2. SQLite persistence.
    /// 3. Replay / history playback.
    /// </summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        [Header("Team Cards")]
        [SerializeField] private CharacterCardView[] leftCharacterCards;
        [SerializeField] private CharacterCardView[] rightCharacterCards;

        [Header("Left Panel Views")]
        [SerializeField] private BattleLogView battleLogView;
        [SerializeField] private BattleMetricsView battleMetricsView;
        [SerializeField] private MainButtonPanelView mainButtonPanelView;

        [Header("Center View")]
        [SerializeField] private CenterControlView centerControlView;

        private readonly BattleSessionFactory _sessionFactory = new BattleSessionFactory();
        private readonly BattleSimulator _battleSimulator = new BattleSimulator();
        private readonly BattleLogFormatter _logFormatter = new BattleLogFormatter();
        private readonly BattleMetricsCalculator _metricsCalculator = new BattleMetricsCalculator();

        private BattleSession _currentSession;
        private DotNetRandom _battleRandom;
        private BattlePhase _currentPhase = BattlePhase.NotStarted;

        private void Awake()
        {
            BindButtonEvents();
            ApplyPhase(BattlePhase.NotStarted);
            ClearBattleViews();
        }

        private void OnDestroy()
        {
            UnbindButtonEvents();
        }

        private void BindButtonEvents()
        {
            if (mainButtonPanelView != null)
            {
                if (mainButtonPanelView.StartButton != null)
                {
                    mainButtonPanelView.StartButton.onClick.AddListener(StartNewBattle);
                }

                if (mainButtonPanelView.RestartButton != null)
                {
                    mainButtonPanelView.RestartButton.onClick.AddListener(StartNewBattle);
                }

                if (mainButtonPanelView.AutoResolveButton != null)
                {
                    mainButtonPanelView.AutoResolveButton.onClick.AddListener(ResolveUntilFinished);
                }

                if (mainButtonPanelView.NewBattleButton != null)
                {
                    mainButtonPanelView.NewBattleButton.onClick.AddListener(StartNewBattleFromNewBattleButton);
                }

                if (mainButtonPanelView.ReplayHistoryButton != null)
                {
                    mainButtonPanelView.ReplayHistoryButton.onClick.AddListener(ShowReplayNotImplementedLog);
                }

                if (mainButtonPanelView.ReplayToEndButton != null)
                {
                    mainButtonPanelView.ReplayToEndButton.onClick.AddListener(ShowReplayNotImplementedLog);
                }
            }

            if (centerControlView != null)
            {
                if (centerControlView.NextRoundButton != null)
                {
                    centerControlView.NextRoundButton.onClick.AddListener(ResolveNextRound);
                }

                if (centerControlView.AiSuggestButton != null)
                {
                    centerControlView.AiSuggestButton.onClick.AddListener(ShowAiNotImplementedLog);
                }

                if (centerControlView.ReplayCurrentButton != null)
                {
                    centerControlView.ReplayCurrentButton.onClick.AddListener(ShowReplayNotImplementedLog);
                }
            }
        }

        private void UnbindButtonEvents()
        {
            if (mainButtonPanelView != null)
            {
                if (mainButtonPanelView.StartButton != null)
                {
                    mainButtonPanelView.StartButton.onClick.RemoveListener(StartNewBattle);
                }

                if (mainButtonPanelView.RestartButton != null)
                {
                    mainButtonPanelView.RestartButton.onClick.RemoveListener(StartNewBattle);
                }

                if (mainButtonPanelView.AutoResolveButton != null)
                {
                    mainButtonPanelView.AutoResolveButton.onClick.RemoveListener(ResolveUntilFinished);
                }

                if (mainButtonPanelView.NewBattleButton != null)
                {
                    mainButtonPanelView.NewBattleButton.onClick.RemoveListener(StartNewBattleFromNewBattleButton);
                }

                if (mainButtonPanelView.ReplayHistoryButton != null)
                {
                    mainButtonPanelView.ReplayHistoryButton.onClick.RemoveListener(ShowReplayNotImplementedLog);
                }

                if (mainButtonPanelView.ReplayToEndButton != null)
                {
                    mainButtonPanelView.ReplayToEndButton.onClick.RemoveListener(ShowReplayNotImplementedLog);
                }
            }

            if (centerControlView != null)
            {
                if (centerControlView.NextRoundButton != null)
                {
                    centerControlView.NextRoundButton.onClick.RemoveListener(ResolveNextRound);
                }

                if (centerControlView.AiSuggestButton != null)
                {
                    centerControlView.AiSuggestButton.onClick.RemoveListener(ShowAiNotImplementedLog);
                }

                if (centerControlView.ReplayCurrentButton != null)
                {
                    centerControlView.ReplayCurrentButton.onClick.RemoveListener(ShowReplayNotImplementedLog);
                }
            }
        }

        private void StartNewBattle()
        {
            int seed = Environment.TickCount;

            _currentSession = _sessionFactory.CreateRandomBattle(seed);
            _battleRandom = new DotNetRandom(seed);

            if (battleLogView != null)
            {
                battleLogView.Clear();
                battleLogView.AddLine(_logFormatter.FormatBattleStarted(_currentSession));
            }

            ApplyPhase(BattlePhase.BattleReady);
            RenderAll();
        }

        private void StartNewBattleFromNewBattleButton()
        {
            if (_currentSession != null && !_currentSession.Runtime.IsFinished)
            {
                // Temporary behavior until official background persistence is implemented.
                // This prevents the button from doing nothing during development,
                // but the final implementation will finish and save the old battle via SQLite.
                if (battleLogView != null)
                {
                    battleLogView.AddLine("目前版本尚未接上背景完成與存檔流程；先建立新戰鬥。");
                }
            }

            StartNewBattle();
        }

        private void ResolveNextRound()
        {
            if (!CanResolveBattle())
            {
                return;
            }

            ApplyPhase(BattlePhase.BattleResolvingRound);

            IReadOnlyList<BattleEvent> events = _battleSimulator.ResolveNextRound(
                _currentSession,
                _battleRandom);

            AddEventLogs(events);

            BattlePhase nextPhase = _currentSession.Runtime.IsFinished
                ? BattlePhase.FinishedSaved
                : BattlePhase.BattleInProgress;

            ApplyPhase(nextPhase);
            RenderAll();
        }

        private void ResolveUntilFinished()
        {
            if (!CanResolveBattle())
            {
                return;
            }

            ApplyPhase(BattlePhase.BattleAutoResolving);

            IReadOnlyList<BattleEvent> events = _battleSimulator.ResolveUntilFinished(
                _currentSession,
                _battleRandom);

            AddEventLogs(events);

            ApplyPhase(BattlePhase.FinishedSaved);
            RenderAll();
        }

        private bool CanResolveBattle()
        {
            if (_currentSession == null)
            {
                Debug.LogWarning("[BattleSceneController] Cannot resolve battle because current session is null.");
                return false;
            }

            if (_battleRandom == null)
            {
                Debug.LogWarning("[BattleSceneController] Cannot resolve battle because random generator is null.");
                return false;
            }

            if (_currentSession.Runtime.IsFinished)
            {
                return false;
            }

            return true;
        }

        private void AddEventLogs(IEnumerable<BattleEvent> events)
        {
            if (battleLogView == null || events == null)
            {
                return;
            }

            foreach (BattleEvent battleEvent in events)
            {
                battleLogView.AddLine(_logFormatter.FormatEvent(battleEvent));
            }
        }

        private void ShowAiNotImplementedLog()
        {
            if (battleLogView != null)
            {
                battleLogView.AddLine("AI 建議左隊配置功能尚未實作。");
            }
        }

        private void ShowReplayNotImplementedLog()
        {
            if (battleLogView != null)
            {
                battleLogView.AddLine("回放功能尚未實作。");
            }
        }

        private void ApplyPhase(BattlePhase phase)
        {
            _currentPhase = phase;

            if (mainButtonPanelView != null)
            {
                mainButtonPanelView.ApplyPhase(phase);
            }

            BattleResult result = _currentSession != null
                ? _currentSession.Runtime.Result
                : BattleResult.None;

            if (centerControlView != null)
            {
                centerControlView.ApplyPhase(phase, result);
            }

            RenderMetrics();
        }

        private void RenderAll()
        {
            RenderTeams();
            RenderMetrics();
        }

        private void RenderTeams()
        {
            if (_currentSession == null)
            {
                ClearCharacterCards(leftCharacterCards);
                ClearCharacterCards(rightCharacterCards);
                return;
            }

            RenderTeamCards(leftCharacterCards, _currentSession.Runtime.LeftTeam);
            RenderTeamCards(rightCharacterCards, _currentSession.Runtime.RightTeam);
        }

        private static void RenderTeamCards(CharacterCardView[] cardViews, TeamRuntime team)
        {
            if (cardViews == null || team == null)
            {
                return;
            }

            for (int i = 0; i < cardViews.Length; i++)
            {
                if (cardViews[i] == null)
                {
                    continue;
                }

                if (i < team.Characters.Count)
                {
                    cardViews[i].Render(team.Characters[i]);
                }
                else
                {
                    cardViews[i].Clear();
                }
            }
        }

        private void RenderMetrics()
        {
            if (battleMetricsView == null)
            {
                return;
            }

            if (_currentSession == null)
            {
                battleMetricsView.Clear();
                return;
            }

            bool isReplayMode =
                _currentPhase == BattlePhase.ReplayReady ||
                _currentPhase == BattlePhase.ReplayPlaying ||
                _currentPhase == BattlePhase.ReplayFinished;

            BattleMetrics metrics = _metricsCalculator.Calculate(
                _currentSession,
                _currentPhase,
                isReplayMode);

            battleMetricsView.Render(metrics);
        }

        private void ClearBattleViews()
        {
            ClearCharacterCards(leftCharacterCards);
            ClearCharacterCards(rightCharacterCards);

            if (battleMetricsView != null)
            {
                battleMetricsView.Clear();
            }

            if (battleLogView != null)
            {
                battleLogView.Clear();
            }
        }

        private static void ClearCharacterCards(CharacterCardView[] cardViews)
        {
            if (cardViews == null)
            {
                return;
            }

            for (int i = 0; i < cardViews.Length; i++)
            {
                if (cardViews[i] != null)
                {
                    cardViews[i].Clear();
                }
            }
        }
    }
}