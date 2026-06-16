using System;
using System.Collections.Generic;
using System.IO;
using DotNetRandom = System.Random;
using TurnBasedBattle.Domain;
using TurnBasedBattle.Infrastructure;
using UnityEngine;
using TurnBasedBattle.ApplicationServices.Factories;
using TurnBasedBattle.ApplicationServices.Simulation;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.ApplicationServices.Calculators;
using TurnBasedBattle.Infrastructure.Queries;
using TurnBasedBattle.Infrastructure.Records;

namespace TurnBasedBattle.Presentation
{
    /// <summary>
    /// Main scene controller for BattleScene.
    ///
    /// Current implementation scope:
    /// 1. Start a new battle.
    /// 2. Resolve next round.
    /// 3. Resolve until finished.
    /// 4. Persist completed battles into SQLite.
    /// 5. Render character cards, log, metrics, and button visibility.
    ///
    /// Not implemented yet:
    /// 1. AI suggestion.
    /// 2. Replay / history playback.
    /// </summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        private const string DatabaseFileName = "battle_runs.db";

        [Header("Team Cards")]
        [SerializeField] private CharacterCardView[] leftCharacterCards;
        [SerializeField] private CharacterCardView[] rightCharacterCards;

        [Header("Left Panel Views")]
        [SerializeField] private BattleLogView battleLogView;
        [SerializeField] private BattleMetricsView battleMetricsView;
        [SerializeField] private MainButtonPanelView mainButtonPanelView;

        [Header("Center View")]
        [SerializeField] private CenterControlView centerControlView;

        [Header("Modal Views")]
        [SerializeField] private LoadListModalView loadListModalView;

        private readonly BattleSessionFactory _sessionFactory = new BattleSessionFactory();
        private readonly BattleSimulator _battleSimulator = new BattleSimulator();
        private readonly BattleLogFormatter _logFormatter = new BattleLogFormatter();
        private readonly BattleMetricsCalculator _metricsCalculator = new BattleMetricsCalculator();

        private BattlePersistenceService _persistenceService;
        private BattleHistoryQueryService _historyQueryService;

        private BattleSession _currentSession;
        private DotNetRandom _battleRandom;
        private BattlePhase _currentPhase = BattlePhase.NotStarted;
        private BattlePhase _phaseBeforeLoadList = BattlePhase.NotStarted;

        private void Awake()
        {
            string databasePath = Path.Combine(UnityEngine.Application.persistentDataPath, DatabaseFileName);
            _persistenceService = new BattlePersistenceService(databasePath);
            _historyQueryService = new BattleHistoryQueryService(databasePath);

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
                    mainButtonPanelView.ReplayHistoryButton.onClick.AddListener(OpenReplayHistoryList);
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

            if (loadListModalView != null)
            {
                loadListModalView.ConfirmSelected += HandleReplayHistoryConfirmed;
                loadListModalView.CancelClicked += HandleReplayHistoryCancelled;
                loadListModalView.Hide();
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
                    mainButtonPanelView.ReplayHistoryButton.onClick.RemoveListener(OpenReplayHistoryList);
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

            if (loadListModalView != null)
            {
                loadListModalView.ConfirmSelected -= HandleReplayHistoryConfirmed;
                loadListModalView.CancelClicked -= HandleReplayHistoryCancelled;
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
            BattleSaveResult backgroundSaveResult = null;
            Exception backgroundSaveException = null;

            if (_currentSession != null && !_currentSession.Runtime.IsFinished)
            {
                try
                {
                    EnsureCurrentBattleFinishedWithoutPresentation();
                    backgroundSaveResult = SaveCompletedCurrentBattleWithoutUiLog();
                }
                catch (Exception exception)
                {
                    backgroundSaveException = exception;
                    Debug.LogError($"[BattleSceneController] Failed to finish and save previous battle.\n{exception}");
                }
            }

            StartNewBattle();

            if (battleLogView == null)
            {
                return;
            }

            if (backgroundSaveResult != null)
            {
                battleLogView.AddLine(_logFormatter.FormatBackgroundBattleSaved(backgroundSaveResult.SavedAt));
            }
            else if (backgroundSaveException != null)
            {
                battleLogView.AddLine("上一場未完成戰鬥背景儲存失敗；詳細錯誤請查看 Console。");
            }
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

            if (_currentSession.Runtime.IsFinished)
            {
                SaveFinishedBattleWithUiFeedback();
                ApplyPhase(BattlePhase.FinishedSaved);
            }
            else
            {
                ApplyPhase(BattlePhase.BattleInProgress);
            }

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

            SaveFinishedBattleWithUiFeedback();
            ApplyPhase(BattlePhase.FinishedSaved);
            RenderAll();
        }

        private void EnsureCurrentBattleFinishedWithoutPresentation()
        {
            if (_currentSession == null || _currentSession.Runtime.IsFinished)
            {
                return;
            }

            if (_battleRandom == null)
            {
                _battleRandom = new DotNetRandom(_currentSession.InitialRandomSeed);
            }

            _battleSimulator.ResolveUntilFinished(_currentSession, _battleRandom);
        }

        private BattleSaveResult SaveCompletedCurrentBattleWithoutUiLog()
        {
            if (_currentSession == null)
            {
                throw new InvalidOperationException("Cannot save because current session is null.");
            }

            if (_currentSession.IsSaved && _currentSession.SavedBattleRunId.HasValue)
            {
                return new BattleSaveResult(
                    _currentSession.SavedBattleRunId.Value,
                    DateTime.Now,
                    Path.Combine(UnityEngine.Application.persistentDataPath, DatabaseFileName));
            }

            BattleSaveResult result = _persistenceService.SaveCompletedBattle(_currentSession);
            _currentSession.MarkSaved(result.BattleRunId);

            Debug.Log(
                $"[BattleSceneController] Battle saved. " +
                $"BattleRunId={result.BattleRunId}, DatabasePath={result.DatabasePath}"
            );

            return result;
        }

        private void SaveFinishedBattleWithUiFeedback()
        {
            if (_currentSession == null)
            {
                return;
            }

            if (_currentSession.IsSaved)
            {
                return;
            }

            ApplyPhase(BattlePhase.Saving);

            if (battleLogView != null)
            {
                battleLogView.AddLine(_logFormatter.FormatSaving());
            }

            try
            {
                BattleSaveResult result = SaveCompletedCurrentBattleWithoutUiLog();

                if (battleLogView != null)
                {
                    battleLogView.AddLine(_logFormatter.FormatSaved(result.SavedAt));
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BattleSceneController] Failed to save completed battle.\n{exception}");

                if (battleLogView != null)
                {
                    battleLogView.AddLine(_logFormatter.FormatSaveFailed());
                }
            }
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

        private void OpenReplayHistoryList()
        {
            if (_historyQueryService == null)
            {
                Debug.LogWarning("[BattleSceneController] History query service is not initialized.");
                return;
            }

            if (loadListModalView == null)
            {
                if (battleLogView != null)
                {
                    battleLogView.AddLine("回放紀錄列表尚未綁定 UI。");
                }

                return;
            }

            try
            {
                _phaseBeforeLoadList = _currentPhase;

                IReadOnlyList<BattleRunSummaryRecord> records =
                    _historyQueryService.GetLatestBattleRuns(limit: 50);

                loadListModalView.Show(records);
                ApplyPhase(BattlePhase.LoadList);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BattleSceneController] Failed to open replay history list.\n{exception}");

                if (battleLogView != null)
                {
                    battleLogView.AddLine("讀取回放紀錄失敗；詳細錯誤請查看 Console。");
                }
            }
        }

        private void HandleReplayHistoryConfirmed(BattleRunSummaryRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (loadListModalView != null)
            {
                loadListModalView.Hide();
            }

            if (battleLogView != null)
            {
                battleLogView.Clear();
                battleLogView.AddLine($"已選擇紀錄 {record.CreatedAtText}；Replay 載入功能尚未實作。");
            }

            ApplyPhase(_phaseBeforeLoadList);
            RenderAll();
        }

        private void HandleReplayHistoryCancelled()
        {
            if (loadListModalView != null)
            {
                loadListModalView.Hide();
            }

            ApplyPhase(_phaseBeforeLoadList);
            RenderAll();
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