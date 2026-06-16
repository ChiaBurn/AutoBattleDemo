namespace TurnBasedBattle.Domain
{
    /// <summary>
    /// The four supported character classes.
    /// </summary>
    public enum CharacterClass
    {
        Warrior = 0,
        Elf = 1,
        Mage = 2,
        Priest = 3
    }

    /// <summary>
    /// Indicates which team a character or event belongs to.
    /// </summary>
    public enum TeamSide
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Result of a battle.
    /// None means the battle has not finished yet.
    /// </summary>
    public enum BattleResult
    {
        None = 0,
        LeftWin = 1,
        RightWin = 2
    }

    /// <summary>
    /// High-level application/UI flow phase.
    /// This controls button visibility, modal visibility, and allowed operations.
    /// </summary>
    public enum BattlePhase
    {
        NotStarted = 0,

        BattleReady = 10,
        AiRunning = 20,
        AiResult = 30,

        BattleResolvingRound = 40,
        BattleInProgress = 50,
        BattleAutoResolving = 60,

        Saving = 70,
        FinishedSaved = 80,

        LoadList = 90,

        ReplayReady = 100,
        ReplayPlaying = 110,
        ReplayFinished = 120
    }
}