using System;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.Formatters
{
    /// <summary>
    /// Formats BattleEvent and battle lifecycle messages into Traditional Chinese log text.
    /// No Unity dependency.
    /// </summary>
    public sealed class BattleLogFormatter
    {
        public string FormatBattleStarted(BattleSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            string leftOrder = BattleTextFormatter.FormatClassOrder(session.Runtime.LeftTeam.Characters);
            string rightOrder = BattleTextFormatter.FormatClassOrder(session.Runtime.RightTeam.Characters);

            return $"新一輪戰鬥開始！左方：{leftOrder}；右方：{rightOrder}。";
        }

        public string FormatEvent(BattleEvent battleEvent)
        {
            if (battleEvent == null)
            {
                throw new ArgumentNullException(nameof(battleEvent));
            }

            string message = battleEvent.WasSkipped
                ? FormatSkippedEvent(battleEvent)
                : FormatActionEvent(battleEvent);

            if (battleEvent.WinnerAfterEvent != BattleResult.None)
            {
                message += FormatWinnerLine(battleEvent.WinnerAfterEvent);
            }

            return message;
        }

        public string FormatSaving()
        {
            return "正在儲存此次戰鬥數據...";
        }

        public string FormatSaved(DateTime savedAt)
        {
            return $"已將此次戰鬥數據儲存為 {savedAt:yyyy-MM-dd HH:mm:ss}。";
        }

        public string FormatSaveFailed()
        {
            return "儲存失敗：此次戰鬥數據尚未寫入資料庫，請重新嘗試。";
        }

        public string FormatReplayLoaded(DateTime createdAt)
        {
            return $"已載入紀錄 {createdAt:yyyy-MM-dd HH:mm:ss}。";
        }

        public string FormatBackgroundBattleSaved(DateTime savedAt)
        {
            return $"上一場未完成戰鬥已於背景執行至結束，並儲存為 {savedAt:yyyy-MM-dd HH:mm:ss}。";
        }

        private static string FormatSkippedEvent(BattleEvent battleEvent)
        {
            string actingTeam = BattleTextFormatter.ToDisplayName(battleEvent.ActingTeamSide);
            string actorClass = BattleTextFormatter.ToDisplayName(battleEvent.ActorClass);
            int actorDisplaySlot = battleEvent.ActorSlotIndex + 1;

            return $"第 {battleEvent.RoundNo} 回合 / {actingTeam} {actorDisplaySlot} 號{actorClass}生命值為 0，跳過行動。";
        }

        private static string FormatActionEvent(BattleEvent battleEvent)
        {
            string actingTeam = BattleTextFormatter.ToDisplayName(battleEvent.ActingTeamSide);
            string actorClass = BattleTextFormatter.ToDisplayName(battleEvent.ActorClass);
            int actorDisplaySlot = battleEvent.ActorSlotIndex + 1;

            string enemyTeam = FormatNullableTeam(battleEvent.EnemyTargetTeamSide);
            string enemyClass = FormatNullableClass(battleEvent.EnemyTargetClass);
            string enemySlot = FormatNullableSlot(battleEvent.EnemyTargetSlotIndex);

            string allyTeam = FormatNullableTeam(battleEvent.AllyTargetTeamSide);
            string allyClass = FormatNullableClass(battleEvent.AllyTargetClass);
            string allySlot = FormatNullableSlot(battleEvent.AllyTargetSlotIndex);

            return
                $"第 {battleEvent.RoundNo} 回合 / {actingTeam} {actorDisplaySlot} 號{actorClass}行動：" +
                $"攻擊{enemyTeam} {enemySlot} 號{enemyClass}，造成 {battleEvent.DamageAmount} 傷害，" +
                $"HP {FormatNullableInt(battleEvent.EnemyHpBefore)} → {FormatNullableInt(battleEvent.EnemyHpAfter)}；" +
                $"治療{allyTeam} {allySlot} 號{allyClass}，恢復 {battleEvent.HealAmount}，" +
                $"HP {FormatNullableInt(battleEvent.AllyHpBefore)} → {FormatNullableInt(battleEvent.AllyHpAfter)}。";
        }

        private static string FormatWinnerLine(BattleResult result)
        {
            return result switch
            {
                BattleResult.LeftWin => "右方隊伍全員倒下，左方隊伍獲勝。",
                BattleResult.RightWin => "左方隊伍全員倒下，右方隊伍獲勝。",
                BattleResult.None => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported battle result.")
            };
        }

        private static string FormatNullableTeam(TeamSide? teamSide)
        {
            return teamSide.HasValue
                ? BattleTextFormatter.ToDisplayName(teamSide.Value)
                : "-";
        }

        private static string FormatNullableClass(CharacterClass? characterClass)
        {
            return characterClass.HasValue
                ? BattleTextFormatter.ToDisplayName(characterClass.Value)
                : "-";
        }

        private static string FormatNullableSlot(int? slotIndex)
        {
            return slotIndex.HasValue
                ? (slotIndex.Value + 1).ToString()
                : "-";
        }

        private static string FormatNullableInt(int? value)
        {
            return value.HasValue
                ? value.Value.ToString()
                : "-";
        }
    }
}