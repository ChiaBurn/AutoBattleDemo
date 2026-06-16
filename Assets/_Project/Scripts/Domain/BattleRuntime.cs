using System;

namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// Runtime data for one battle.
    ///
    /// This object contains both teams, current round number, and battle result.
    /// It does not contain UI state, DB state, or replay state.
    /// </summary>
    [Serializable]
    public sealed class BattleRuntime
    {
        public TeamRuntime LeftTeam { get; }
        public TeamRuntime RightTeam { get; }

        public int CurrentRound { get; private set; }

        public BattleResult Result { get; private set; }

        public bool IsFinished => Result != BattleResult.None;

        public BattleRuntime(TeamRuntime leftTeam, TeamRuntime rightTeam)
        {
            LeftTeam = leftTeam ?? throw new ArgumentNullException(nameof(leftTeam));
            RightTeam = rightTeam ?? throw new ArgumentNullException(nameof(rightTeam));

            if (LeftTeam.Side != TeamSide.Left)
            {
                throw new ArgumentException("Left team must have TeamSide.Left.", nameof(leftTeam));
            }

            if (RightTeam.Side != TeamSide.Right)
            {
                throw new ArgumentException("Right team must have TeamSide.Right.", nameof(rightTeam));
            }

            CurrentRound = 0;
            Result = BattleResult.None;
        }

        public TeamRuntime GetTeam(TeamSide side)
        {
            return side == TeamSide.Left ? LeftTeam : RightTeam;
        }

        public TeamRuntime GetOpponentTeam(TeamSide side)
        {
            return side == TeamSide.Left ? RightTeam : LeftTeam;
        }

        public void BeginNextRound()
        {
            if (IsFinished)
            {
                throw new InvalidOperationException("Cannot begin a new round after the battle is finished.");
            }

            CurrentRound++;
        }

        public BattleResult EvaluateResult()
        {
            bool leftDefeated = LeftTeam.IsAllDefeated;
            bool rightDefeated = RightTeam.IsAllDefeated;

            if (leftDefeated && rightDefeated)
            {
                // The original exercise does not define draw.
                // This project resolves simultaneous defeat as RightWin because the left team acts first,
                // but under the current sequential action rules this case should be extremely unlikely.
                Result = BattleResult.RightWin;
            }
            else if (leftDefeated)
            {
                Result = BattleResult.RightWin;
            }
            else if (rightDefeated)
            {
                Result = BattleResult.LeftWin;
            }
            else
            {
                Result = BattleResult.None;
            }

            return Result;
        }

        public BattleRuntime Clone()
        {
            BattleRuntime clone = new BattleRuntime(
                LeftTeam.Clone(),
                RightTeam.Clone());

            clone.CurrentRound = CurrentRound;
            clone.Result = Result;

            return clone;
        }
    }
}