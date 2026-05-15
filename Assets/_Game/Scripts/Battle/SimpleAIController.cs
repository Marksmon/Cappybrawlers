using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Greedy random AI: spends stamina on the highest-cost cards first,
    // assigns them to random alive brawlers targeting random opponents.
    public static class SimpleAIController
    {
        public static void QueueActions(BattleTeam aiTeam, BattleTeam opponentTeam)
        {
            var alive    = new List<CapybrawlerInstance>(aiTeam.ActiveBrawlers);
            var targets  = new List<CapybrawlerInstance>(opponentTeam.ActiveBrawlers);
            var hand     = new List<CardPoolEntry>(aiTeam.Hand.Cards);

            // Sort hand by stamina cost descending — spend greedily.
            hand.Sort((a, b) => b.Card.staminaCost.CompareTo(a.Card.staminaCost));

            // AI always targets the opponent's frontline (first alive brawler).
            CapybrawlerInstance frontTarget = null;
            foreach (var b in opponentTeam.Brawlers)
                if (!b.IsKnockedOut) { frontTarget = b; break; }

            foreach (var entry in hand)
            {
                if (alive.Count == 0 || frontTarget == null) break;
                if (!aiTeam.StaminaPool.TrySpend(entry.Card.staminaCost)) continue;

                var actor  = alive[Random.Range(0, alive.Count)];
                var target = frontTarget;

                actor.QueuedActions.Add(new QueuedAction
                {
                    Source = actor,
                    Card   = entry.Card,
                    Target = target,
                });

                aiTeam.CardPool.StartCooldown(entry);
            }
        }
    }
}
