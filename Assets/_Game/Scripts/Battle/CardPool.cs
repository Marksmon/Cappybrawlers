using System.Collections.Generic;
using Capybrawlers.Cards;

namespace Capybrawlers.Battle
{
    public class CardPoolEntry
    {
        public CardData             Card;
        public CapybrawlerInstance  Owner;
        public int                  CopyIndex;    // 0 or 1 (two copies of each card)
        public int                  CooldownTurns; // unused — kept for compatibility
        public bool                 IsInHand;
        public bool                 IsUsed;       // in discard pile, awaiting reshuffle
    }

    public class CardPool
    {
        private readonly List<CardPoolEntry> _entries = new(18);

        public IReadOnlyList<CardPoolEntry> Entries => _entries;

        // Cards in the deck (not in hand, not in discard pile).
        public int DeckCount
        {
            get
            {
                int n = 0;
                foreach (var e in _entries)
                    if (!e.IsInHand && !e.IsUsed) n++;
                return n;
            }
        }

        // Build the 18-entry pool from a team's three brawlers (3 cards × 2 copies × 3 brawlers).
        public CardPool(CapybrawlerInstance[] brawlers)
        {
            foreach (var brawler in brawlers)
            {
                foreach (var card in brawler.Build.Cards)
                {
                    if (card == null) { UnityEngine.Debug.LogWarning($"[CardPool] {brawler.Build.BuildId} has a null grantedCard — skipping. Run Generate SO Assets."); continue; }
                    for (int copy = 0; copy < 2; copy++)
                        _entries.Add(new CardPoolEntry { Card = card, CopyIndex = copy, Owner = brawler });
                }
            }
        }

        // Returns deck entries eligible to be drawn (not in hand, not in discard pile).
        public List<CardPoolEntry> GetEligible()
        {
            var result = new List<CardPoolEntry>();
            foreach (var e in _entries)
                if (!e.IsInHand && !e.IsUsed)
                    result.Add(e);
            return result;
        }

        // Mark a played card as used (moves to discard pile).
        public void StartCooldown(CardPoolEntry entry)
        {
            entry.IsInHand = false;
            entry.IsUsed   = true;
        }

        // Move all discarded cards back into the deck (reshuffle), excluding hand cards.
        public void ReshuffleUsed()
        {
            foreach (var e in _entries)
                if (e.IsUsed) e.IsUsed = false;
        }

        // Called at start of turn — no-op since cooldowns are gone, kept for compatibility.
        public void TickCooldowns() { }

        // Permanently remove all cards belonging to a knocked-out brawler.
        public void RemoveBrawlerCards(CapybrawlerInstance brawler)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
                if (_entries[i].Owner == brawler)
                    _entries.RemoveAt(i);
        }

        // Lift all cooldowns — kept for compatibility (no-op).
        public void LiftAllCooldowns() { }

        // Return all hand cards to pool without playing them (battle reset only).
        public void ReturnHand()
        {
            foreach (var e in _entries)
                e.IsInHand = false;
        }
    }
}
