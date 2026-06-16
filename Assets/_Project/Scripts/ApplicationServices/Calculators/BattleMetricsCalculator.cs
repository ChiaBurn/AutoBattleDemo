using System;
using TurnBasedBattle.ApplicationServices.Formatters;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.ApplicationServices.Calculators
{
    /// <summary>
    /// Calculates aggregated values for the Metrics UI panel.
    /// No Unity dependency.
    /// </summary>
    public sealed class BattleMetricsCalculator
    {
        public BattleMetrics Calculate(
            BattleSession session,
            BattlePhase phase,
            bool isReplayMode)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            BattleRuntime runtime = session.Runtime;

            string modeText = isReplayMode ? "回放紀錄" : "新戰鬥";
            string phaseText = BattleTextFormatter.ToDisplayName(phase);
            string saveStatusText = GetSaveStatusText(session, phase);

            return new BattleMetrics(
                modeText,
                phaseText,
                runtime.CurrentRound,
                runtime.LeftTeam.AliveCount,
                runtime.RightTeam.AliveCount,
                runtime.LeftTeam.TotalCurrentHp,
                runtime.RightTeam.TotalCurrentHp,
                runtime.LeftTeam.TotalMaxHp,
                runtime.RightTeam.TotalMaxHp,
                runtime.Result,
                saveStatusText);
        }

        private static string GetSaveStatusText(BattleSession session, BattlePhase phase)
        {
            if (phase == BattlePhase.Saving)
            {
                return "儲存中";
            }

            if (session.IsSaved)
            {
                return session.SavedBattleRunId.HasValue
                    ? $"已儲存：Run #{session.SavedBattleRunId.Value}"
                    : "已儲存";
            }

            if (session.Runtime.IsFinished)
            {
                return "尚未儲存";
            }

            return "暫存於記憶體";
        }
    }
}