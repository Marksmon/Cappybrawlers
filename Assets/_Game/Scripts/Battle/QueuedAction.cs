using Capybrawlers.Cards;

namespace Capybrawlers.Battle
{
    public class QueuedAction
    {
        public CapybrawlerInstance Source;
        public CardData            Card;
        public CapybrawlerInstance Target; // null for self/team-wide effects
    }
}
