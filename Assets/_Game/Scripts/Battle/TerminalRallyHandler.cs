using System.Collections.Generic;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public static class TerminalRallyHandler
    {
        // After each action resolves, check both teams for newly knocked-out brawlers.
        // Every KO: publishes BrawlerKnockedOutEvent and removes the brawler's cards.
        // If the KO'd brawler had queued actions, those fire next (Terminal Rally).
        public static void CheckAndInsertRallyActions(
            CapybrawlerInstance justActed,
            List<QueuedAction>  remainingQueue,
            BattleTeam          playerTeam,
            BattleTeam          opponentTeam,
            IEventBus           events)
        {
            var allBrawlers = new List<CapybrawlerInstance>();
            allBrawlers.AddRange(playerTeam.Brawlers);
            allBrawlers.AddRange(opponentTeam.Brawlers);

            foreach (var brawler in allBrawlers)
            {
                if (!brawler.IsKnockedOut || brawler.IsInTerminalRally) continue;

                brawler.IsInTerminalRally = true;
                bool isPlayer = IsOnTeam(brawler, playerTeam);
                events.Publish(new BrawlerKnockedOutEvent(brawler, isPlayer ? 0 : 1));

                // Remove the brawler's cards and lift cooldowns for last survivor.
                (isPlayer ? playerTeam : opponentTeam).OnBrawlerKnockedOut(brawler);

                // Terminal Rally: prepend any queued actions so they fire before the rest.
                if (brawler.QueuedActions.Count > 0)
                {
                    remainingQueue.InsertRange(0, brawler.QueuedActions);
                    brawler.QueuedActions.Clear();
                }
            }
        }

        private static bool IsOnTeam(CapybrawlerInstance brawler, BattleTeam team)
        {
            foreach (var b in team.Brawlers)
                if (b == brawler) return true;
            return false;
        }
    }
}
