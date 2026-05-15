using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Battle
{
    public class Hand
    {
        private readonly List<CardPoolEntry> _cards = new(3);

        public IReadOnlyList<CardPoolEntry> Cards => _cards;

        // Draw up to 3 eligible cards from the pool (random selection).
        public void Draw(CardPool pool)
        {
            _cards.Clear();
            var eligible = pool.GetEligible();

            while (_cards.Count < 3 && eligible.Count > 0)
            {
                int idx   = Random.Range(0, eligible.Count);
                var entry = eligible[idx];
                entry.IsInHand = true;
                _cards.Add(entry);
                eligible.RemoveAt(idx);
            }
        }

        // Called after Resolution: return unplayed cards and clear hand.
        public void ReturnUnplayed(CardPool pool)
        {
            foreach (var entry in _cards)
                if (entry.IsInHand) // still in hand means it was not played
                    entry.IsInHand = false;
            _cards.Clear();
        }
    }
}
