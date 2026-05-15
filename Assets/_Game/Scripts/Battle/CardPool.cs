using System.Collections.Generic;
using Capybrawlers.Cards;

namespace Capybrawlers.Battle
{
    public class CardPoolEntry
    {
        public CardData Card;
        public int      CopyIndex;     // 0 or 1 (two copies of each card)
        public int      CooldownTurns; // 0 = available
        public bool     IsInHand;
    }

    public class CardPool
    {
        private readonly List<CardPoolEntry> _entries = new(18);

        public IReadOnlyList<CardPoolEntry> Entries => _entries;

        // Build the 18-entry pool from a team's three brawlers (3 cards × 2 copies each × 3 brawlers).
        public CardPool(CapybrawlerInstance[] brawlers)
        {
            foreach (var brawler in brawlers)
            {
                foreach (var card in brawler.Build.Cards)
                {
                    for (int copy = 0; copy < 2; copy++)
                        _entries.Add(new CardPoolEntry { Card = card, CopyIndex = copy });
                }
            }
        }

        // Returns entries that are eligible to be drawn: not in hand and cooldown expired.
        public List<CardPoolEntry> GetEligible()
        {
            var result = new List<CardPoolEntry>();
            foreach (var e in _entries)
                if (!e.IsInHand && e.CooldownTurns <= 0)
                    result.Add(e);
            return result;
        }

        // Mark a played card as cooling down for 2 turns.
        public void StartCooldown(CardPoolEntry entry)
        {
            entry.IsInHand     = false;
            entry.CooldownTurns = 2;
        }

        // Called at StartOfTurn before draw.
        public void TickCooldowns()
        {
            foreach (var e in _entries)
                if (e.CooldownTurns > 0)
                    e.CooldownTurns--;
        }

        // Return all hand cards to pool without playing them (Cleanup phase).
        public void ReturnHand()
        {
            foreach (var e in _entries)
                e.IsInHand = false;
        }
    }
}
