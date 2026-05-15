using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public readonly struct BrawlerKnockedOutEvent
    {
        public readonly CapybrawlerInstance Brawler;
        public readonly int TeamIndex;
        public BrawlerKnockedOutEvent(CapybrawlerInstance brawler, int teamIndex)
        { Brawler = brawler; TeamIndex = teamIndex; }
    }

    public readonly struct ActionResolvedEvent
    {
        public readonly QueuedAction Action;
        public readonly int DamageDealt;
        public ActionResolvedEvent(QueuedAction action, int damageDealt)
        { Action = action; DamageDealt = damageDealt; }
    }

    public readonly struct TurnPhaseChangedEvent
    {
        public readonly TurnPhase Phase;
        public TurnPhaseChangedEvent(TurnPhase phase) { Phase = phase; }
    }

    public readonly struct TurnChangedEvent
    {
        public readonly int Turn;
        public TurnChangedEvent(int turn) { Turn = turn; }
    }

    // Published every frame during DecisionPhase with remaining seconds.
    public readonly struct DecisionTimerEvent
    {
        public readonly float TimeLeft;
        public DecisionTimerEvent(float t) { TimeLeft = t; }
    }

    // Published when the 15-second timer expires before the player confirms.
    // BattleUIController handles this by auto-locking staged cards.
    public readonly struct DecisionTimerExpiredEvent { }

    // Published when Drawing Phase requires the player to discard a card.
    public readonly struct DiscardRequiredEvent { }

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
        { NewTotal = newTotal; Delta = delta; }
    }

    public readonly struct PenaltyDamageEvent
    {
        public readonly int Turn;
        public readonly int Damage;
        public PenaltyDamageEvent(int turn, int damage) { Turn = turn; Damage = damage; }
    }

    // Published every frame during the Drawing Phase discard window (5-second timer).
    public readonly struct DrawPhaseTimerEvent
    {
        public readonly float TimeLeft;
        public DrawPhaseTimerEvent(float t) { TimeLeft = t; }
    }

    // Published when the 5-second discard timer expires — auto-discard fires.
    public readonly struct DrawPhaseTimerExpiredEvent { }
}
