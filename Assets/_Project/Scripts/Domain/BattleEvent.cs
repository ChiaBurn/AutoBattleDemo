using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Immutable record of one character action or one skipped action.
    ///
    /// BattleEvent is the source for:
    /// 1. Log text.
    /// 2. Replay playback.
    /// 3. SQLite BattleEventRecord persistence.
    /// </summary>
    [Serializable]
    public sealed class BattleEvent
    {
        public int SequenceNo { get; }
        public int RoundNo { get; }

        public TeamSide ActingTeamSide { get; }
        public int ActorSlotIndex { get; }
        public CharacterClass ActorClass { get; }

        public bool WasSkipped { get; }

        public TeamSide? EnemyTargetTeamSide { get; }
        public int? EnemyTargetSlotIndex { get; }
        public CharacterClass? EnemyTargetClass { get; }
        public int DamageAmount { get; }
        public int? EnemyHpBefore { get; }
        public int? EnemyHpAfter { get; }

        public TeamSide? AllyTargetTeamSide { get; }
        public int? AllyTargetSlotIndex { get; }
        public CharacterClass? AllyTargetClass { get; }
        public int HealAmount { get; }
        public int? AllyHpBefore { get; }
        public int? AllyHpAfter { get; }

        public BattleResult WinnerAfterEvent { get; }

        private BattleEvent(
            int sequenceNo,
            int roundNo,
            TeamSide actingTeamSide,
            int actorSlotIndex,
            CharacterClass actorClass,
            bool wasSkipped,
            TeamSide? enemyTargetTeamSide,
            int? enemyTargetSlotIndex,
            CharacterClass? enemyTargetClass,
            int damageAmount,
            int? enemyHpBefore,
            int? enemyHpAfter,
            TeamSide? allyTargetTeamSide,
            int? allyTargetSlotIndex,
            CharacterClass? allyTargetClass,
            int healAmount,
            int? allyHpBefore,
            int? allyHpAfter,
            BattleResult winnerAfterEvent)
        {
            if (sequenceNo <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequenceNo), "Sequence number must be greater than 0.");
            }

            if (roundNo <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundNo), "Round number must be greater than 0.");
            }

            if (actorSlotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actorSlotIndex), "Actor slot index cannot be negative.");
            }

            if (damageAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damageAmount), "Damage amount cannot be negative.");
            }

            if (healAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healAmount), "Heal amount cannot be negative.");
            }

            SequenceNo = sequenceNo;
            RoundNo = roundNo;
            ActingTeamSide = actingTeamSide;
            ActorSlotIndex = actorSlotIndex;
            ActorClass = actorClass;
            WasSkipped = wasSkipped;

            EnemyTargetTeamSide = enemyTargetTeamSide;
            EnemyTargetSlotIndex = enemyTargetSlotIndex;
            EnemyTargetClass = enemyTargetClass;
            DamageAmount = damageAmount;
            EnemyHpBefore = enemyHpBefore;
            EnemyHpAfter = enemyHpAfter;

            AllyTargetTeamSide = allyTargetTeamSide;
            AllyTargetSlotIndex = allyTargetSlotIndex;
            AllyTargetClass = allyTargetClass;
            HealAmount = healAmount;
            AllyHpBefore = allyHpBefore;
            AllyHpAfter = allyHpAfter;

            WinnerAfterEvent = winnerAfterEvent;
        }

        public static BattleEvent CreateSkipped(
            int sequenceNo,
            int roundNo,
            CharacterRuntime actor,
            BattleResult winnerAfterEvent)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            return new BattleEvent(
                sequenceNo,
                roundNo,
                actor.TeamSide,
                actor.SlotIndex,
                actor.Class,
                wasSkipped: true,
                enemyTargetTeamSide: null,
                enemyTargetSlotIndex: null,
                enemyTargetClass: null,
                damageAmount: 0,
                enemyHpBefore: null,
                enemyHpAfter: null,
                allyTargetTeamSide: null,
                allyTargetSlotIndex: null,
                allyTargetClass: null,
                healAmount: 0,
                allyHpBefore: null,
                allyHpAfter: null,
                winnerAfterEvent);
        }

        public static BattleEvent CreateAction(
            int sequenceNo,
            int roundNo,
            CharacterRuntime actor,
            CharacterRuntime enemyTarget,
            int damageAmount,
            HpChange enemyHpChange,
            CharacterRuntime allyTarget,
            int healAmount,
            HpChange allyHpChange,
            BattleResult winnerAfterEvent)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (enemyTarget == null)
            {
                throw new ArgumentNullException(nameof(enemyTarget));
            }

            if (allyTarget == null)
            {
                throw new ArgumentNullException(nameof(allyTarget));
            }

            return new BattleEvent(
                sequenceNo,
                roundNo,
                actor.TeamSide,
                actor.SlotIndex,
                actor.Class,
                wasSkipped: false,
                enemyTargetTeamSide: enemyTarget.TeamSide,
                enemyTargetSlotIndex: enemyTarget.SlotIndex,
                enemyTargetClass: enemyTarget.Class,
                damageAmount: damageAmount,
                enemyHpBefore: enemyHpChange.Before,
                enemyHpAfter: enemyHpChange.After,
                allyTargetTeamSide: allyTarget.TeamSide,
                allyTargetSlotIndex: allyTarget.SlotIndex,
                allyTargetClass: allyTarget.Class,
                healAmount: healAmount,
                allyHpBefore: allyHpChange.Before,
                allyHpAfter: allyHpChange.After,
                winnerAfterEvent);
        }
    }
}