using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedBattle.Domain;

namespace TurnBasedBattle.Application
{
    /// <summary>
    /// Converts domain enum / runtime values into Traditional Chinese display text.
    /// No Unity dependency.
    /// </summary>
    public static class BattleTextFormatter
    {
        public static string ToDisplayName(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => "戰士",
                CharacterClass.Elf => "精靈",
                CharacterClass.Mage => "法師",
                CharacterClass.Priest => "牧師",
                _ => throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, "Unsupported character class.")
            };
        }

        public static string ToDisplayName(TeamSide teamSide)
        {
            return teamSide switch
            {
                TeamSide.Left => "左方",
                TeamSide.Right => "右方",
                _ => throw new ArgumentOutOfRangeException(nameof(teamSide), teamSide, "Unsupported team side.")
            };
        }

        public static string ToTeamDisplayName(TeamSide teamSide)
        {
            return teamSide switch
            {
                TeamSide.Left => "左隊",
                TeamSide.Right => "右隊",
                _ => throw new ArgumentOutOfRangeException(nameof(teamSide), teamSide, "Unsupported team side.")
            };
        }

        public static string ToDisplayName(BattleResult result)
        {
            return result switch
            {
                BattleResult.None => "未分勝負",
                BattleResult.LeftWin => "左方",
                BattleResult.RightWin => "右方",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported battle result.")
            };
        }

        public static string ToDisplayName(BattlePhase phase)
        {
            return phase switch
            {
                BattlePhase.NotStarted => "尚未開始",
                BattlePhase.BattleReady => "準備中",
                BattlePhase.AiRunning => "AI 計算中",
                BattlePhase.AiResult => "AI 結果確認",
                BattlePhase.BattleResolvingRound => "回合執行中",
                BattlePhase.BattleInProgress => "戰鬥中",
                BattlePhase.BattleAutoResolving => "自動執行中",
                BattlePhase.Saving => "儲存中",
                BattlePhase.FinishedSaved => "已結束",
                BattlePhase.LoadList => "選擇回放紀錄",
                BattlePhase.ReplayReady => "回放準備中",
                BattlePhase.ReplayPlaying => "播放中",
                BattlePhase.ReplayFinished => "回放結束",
                _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unsupported battle phase.")
            };
        }

        public static string FormatClassOrder(IEnumerable<CharacterRuntime> characters)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            return string.Join(" → ", characters.Select(character => ToDisplayName(character.Class)));
        }
    }
}