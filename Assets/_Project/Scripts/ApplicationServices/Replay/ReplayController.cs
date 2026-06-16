using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.Replay
{
    /// <summary>
    /// Applies persisted BattleEvent stream to a reconstructed BattleRuntime.
    ///
    /// This class does not recalculate damage, healing, or random targets.
    /// It only applies persisted HP-after values.
    /// </summary>
    public sealed class ReplayController
    {
        private readonly BattleReplayPayload _payload;
        private readonly IReadOnlyList<BattleEvent> _events;

        private int _nextEventIndex;

        public BattleSession Session { get; }

        public long BattleRunId => _payload.BattleRunId;

        public string CreatedAtText => _payload.CreatedAtText;

        public bool IsFinished => _nextEventIndex >= _events.Count || Session.Runtime.IsFinished;

        public ReplayController(BattleReplayPayload payload)
        {
            _payload = payload ?? throw new ArgumentNullException(nameof(payload));
            _events = _payload.Events;
            _nextEventIndex = 0;

            Session = CreateInitialSession(_payload);
        }

        public IReadOnlyList<BattleEvent> PlayNextRound()
        {
            if (IsFinished)
            {
                return Array.Empty<BattleEvent>();
            }

            int targetRound = _events[_nextEventIndex].RoundNo;

            if (Session.Runtime.CurrentRound < targetRound)
            {
                Session.Runtime.BeginNextRound();
            }

            List<BattleEvent> appliedEvents = new List<BattleEvent>();

            while (_nextEventIndex < _events.Count &&
                   _events[_nextEventIndex].RoundNo == targetRound)
            {
                BattleEvent battleEvent = _events[_nextEventIndex];
                ApplyEvent(battleEvent);
                Session.AddEvent(battleEvent);
                appliedEvents.Add(battleEvent);
                _nextEventIndex++;
            }

            return appliedEvents;
        }

        public IReadOnlyList<BattleEvent> PlayToEnd()
        {
            List<BattleEvent> appliedEvents = new List<BattleEvent>();

            while (!IsFinished)
            {
                IReadOnlyList<BattleEvent> roundEvents = PlayNextRound();
                appliedEvents.AddRange(roundEvents);
            }

            return appliedEvents;
        }

        private void ApplyEvent(BattleEvent battleEvent)
        {
            if (battleEvent == null)
            {
                throw new ArgumentNullException(nameof(battleEvent));
            }

            if (!battleEvent.WasSkipped)
            {
                ApplyEnemyHpAfter(battleEvent);
                ApplyAllyHpAfter(battleEvent);
            }

            Session.Runtime.EvaluateResult();
        }

        private void ApplyEnemyHpAfter(BattleEvent battleEvent)
        {
            if (!battleEvent.EnemyTargetTeamSide.HasValue ||
                !battleEvent.EnemyTargetSlotIndex.HasValue ||
                !battleEvent.EnemyHpAfter.HasValue)
            {
                return;
            }

            CharacterRuntime enemyTarget = Session.Runtime
                .GetTeam(battleEvent.EnemyTargetTeamSide.Value)
                .GetCharacterBySlotIndex(battleEvent.EnemyTargetSlotIndex.Value);

            enemyTarget.SetCurrentHpForReplay(battleEvent.EnemyHpAfter.Value);
        }

        private void ApplyAllyHpAfter(BattleEvent battleEvent)
        {
            if (!battleEvent.AllyTargetTeamSide.HasValue ||
                !battleEvent.AllyTargetSlotIndex.HasValue ||
                !battleEvent.AllyHpAfter.HasValue)
            {
                return;
            }

            CharacterRuntime allyTarget = Session.Runtime
                .GetTeam(battleEvent.AllyTargetTeamSide.Value)
                .GetCharacterBySlotIndex(battleEvent.AllyTargetSlotIndex.Value);

            allyTarget.SetCurrentHpForReplay(battleEvent.AllyHpAfter.Value);
        }

        private static BattleSession CreateInitialSession(BattleReplayPayload payload)
        {
            TeamRuntime leftTeam = CreateTeam(payload, TeamSide.Left);
            TeamRuntime rightTeam = CreateTeam(payload, TeamSide.Right);

            BattleRuntime runtime = new BattleRuntime(leftTeam, rightTeam);
            return new BattleSession(runtime, payload.InitialRandomSeed);
        }

        private static TeamRuntime CreateTeam(BattleReplayPayload payload, TeamSide teamSide)
        {
            List<CharacterRuntime> characters = payload.InitialCharacters
                .Where(character => character.TeamSide == teamSide)
                .OrderBy(character => character.SlotIndex)
                .Select(character => character.ToRuntimeCharacter())
                .ToList();

            if (characters.Count != 4)
            {
                throw new InvalidOperationException(
                    $"Replay team should contain exactly 4 characters. TeamSide={teamSide}, Count={characters.Count}");
            }

            return new TeamRuntime(teamSide, characters);
        }
    }
}