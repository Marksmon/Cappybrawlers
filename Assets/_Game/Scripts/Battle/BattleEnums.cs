namespace Capybrawlers.Battle
{
    public enum TurnPhase
    {
        Idle,
        Setup,
        DrawPhase,        // cards drawn / hand refilled
        DecisionPhase,    // 15-second player input window (was QueuePhase)
        ActionPhase,      // SPD-ordered resolution (was Resolution)
        TerminalRally,
        BattleEnd,
    }
}
