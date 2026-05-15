using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public readonly struct BrawlerKnockedOutEvent
    {
        public readonly CapybrawlerInstance Brawler;
        public readonly int TeamIndex; // 0 = player, 1 = opponent

        public BrawlerKnockedOutEvent(CapybrawlerInstance brawler, int teamIndex)
        {
            Brawler   = brawler;
            TeamIndex = teamIndex;
        }
    }

    public readonly struct ActionResolvedEvent
    {
        public readonly QueuedAction Action;
        public readonly int DamageDealt;

        public ActionResolvedEvent(QueuedAction action, int damageDealt)
        {
            Action      = action;
            DamageDealt = damageDealt;
        }
    }

    public readonly struct TurnPhaseChangedEvent
    {
        public readonly TurnPhase Phase;
        public TurnPhaseChangedEvent(TurnPhase phase) { Phase = phase; }
    }

    public readonly struct BattleEndEvent
    {
        public readonly bool PlayerWon;
        public BattleEndEvent(bool playerWon) { PlayerWon = playerWon; }
    }

    public readonly struct StaminaChangedEvent
    {
        public readonly int NewTotal;
        public readonly int Delta;

        public StaminaChangedEvent(int newTotal, int delta)
        {
            NewTotal = newTotal;
            Delta    = delta;
        }
    }
}
