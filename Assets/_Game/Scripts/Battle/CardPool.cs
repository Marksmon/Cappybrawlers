using System.Collections.Generic;
using Capybrawlers.Cards;

namespace Capybrawlers.Battle
{
    public class CardPoolEntry
    {
        public CardData             Card;
        public CapybrawlerInstance  Owner;        // which brawler this card belongs to
        public int                  CopyIndex;    // 0 or 1 (two copies of each card)
        public int                  CooldownTurns; // 0 = available
        public bool                 IsInHand;
    }

    public class CardPool
    {
        private readonly List<CardPoolEntry> _entries = new(18);

        public IReadOnlyList<CardPoolEntry> Entries => _entries;

        // Build the 18-entry pool from a team's three brawlers (3 cards × 2 copies × 3 brawlers).
        public CardPool(CapybrawlerInstance[] brawlers)
        {
            foreach (var brawler in brawlers)
            {
                foreach (var card in brawler.Build.Cards)
                {
                    for (int copy = 0; copy < 2; copy++)
                        _entries.Add(new CardPoolEntry { Card = card, CopyIndex = copy, Owner = brawler });
                }
            }
        }

        // Returns entries eligible to be drawn: not in hand and cooldown expired.
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
            entry.IsInHand      = false;
            entry.CooldownTurns = 2;
        }

        // Called at start of turn before draw.
        public void TickCooldowns()
        {
            foreach (var e in _entries)
                if (e.CooldownTurns > 0)
                    e.CooldownTurns--;
        }

        // Permanently remove all cards belonging to a knocked-out brawler.
        public void RemoveBrawlerCards(CapybrawlerInstance brawler)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
                if (_entries[i].Owner == brawler)
                    _entries.RemoveAt(i);
        }

        // Remove all cooldowns — called when only one brawler remains on a team.
        public void LiftAllCooldowns()
        {
            foreach (var e in _entries)
                e.CooldownTurns = 0;
        }

        // Return all hand cards to pool without playing them (battle reset only).
        public void ReturnHand()
        {
            foreach (var e in _entries)
                e.IsInHand = false;
        }
    }
}
