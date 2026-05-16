using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Greedy random AI: spends stamina on the highest-cost cards first,
    // assigns them round-robin to alive brawlers, capped at 10 actions per turn.
    public static class SimpleAIController
    {
        private const int MaxActionsPerTurn = 10;

        public static void QueueActions(BattleTeam aiTeam, BattleTeam opponentTeam)
        {
            var alive = new List<CapybrawlerInstance>(aiTeam.ActiveBrawlers);
            var hand  = new List<CardPoolEntry>(aiTeam.Hand.Cards);

            if (alive.Count == 0) return;

            // Sort hand by stamina cost descending — spend greedily.
            hand.Sort((a, b) => b.Card.staminaCost.CompareTo(a.Card.staminaCost));

            // AI always targets the opponent's frontline (first alive brawler).
            CapybrawlerInstance frontTarget = null;
            foreach (var b in opponentTeam.Brawlers)
                if (!b.IsKnockedOut) { frontTarget = b; break; }
            if (frontTarget == null) return;

            int queued = 0;
            int cycle  = 0;
            foreach (var entry in hand)
            {
                if (queued >= MaxActionsPerTurn) break;
                if (!aiTeam.StaminaPool.TrySpend(entry.Card.staminaCost)) continue;

                var actor = alive[cycle % alive.Count];
                actor.QueuedActions.Add(new QueuedAction
                {
                    Source = actor,
                    Card   = entry.Card,
                    Target = frontTarget,
                });

                aiTeam.CardPool.StartCooldown(entry);
                aiTeam.Hand.RemoveCard(entry);
                cycle++;
                queued++;
            }
        }
    }
}
