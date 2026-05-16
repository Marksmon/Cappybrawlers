using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Battle
{
    public class Hand
    {
        public const int MaxSize = 18;

        private readonly List<CardPoolEntry> _cards = new(MaxSize);

        public IReadOnlyList<CardPoolEntry> Cards => _cards;

        // Draw exactly N cards at random. If the deck runs out mid-draw,
        // auto-reshuffles the discard pile and continues drawing.
        public void DrawN(CardPool pool, int count)
        {
            int drawn = 0;
            while (drawn < count && _cards.Count < MaxSize)
            {
                var eligible = pool.GetEligible();
                if (eligible.Count == 0)
                {
                    pool.ReshuffleUsed();
                    eligible = pool.GetEligible();
                    if (eligible.Count == 0) break; // all cards are in hand
                }

                int idx   = Random.Range(0, eligible.Count);
                var entry = eligible[idx];
                entry.IsInHand = true;
                _cards.Add(entry);
                drawn++;
            }
        }

        // Remove a card that was played or forcibly discarded.
        public void RemoveCard(CardPoolEntry entry)
        {
            entry.IsInHand = false;
            _cards.Remove(entry);
        }

        // Remove all cards belonging to a knocked-out brawler.
        public void RemoveBrawlerCards(CapybrawlerInstance brawler)
        {
            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                if (_cards[i].Owner == brawler)
                {
                    _cards[i].IsInHand = false;
                    _cards.RemoveAt(i);
                }
            }
        }

        // Clear hand completely (used on battle reset only).
        public void Clear()
        {
            foreach (var e in _cards) e.IsInHand = false;
            _cards.Clear();
        }
    }
}
