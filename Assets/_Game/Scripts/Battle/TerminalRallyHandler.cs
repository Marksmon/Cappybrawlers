using System.Collections.Generic;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public static class TerminalRallyHandler
    {
        // After each action resolves, check both teams for newly knocked-out brawlers.
        // A knocked-out brawler with remaining queued actions enters Terminal Rally:
        // its actions are prepended to the front of the remaining queue so they fire next.
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
                if (brawler.QueuedActions.Count == 0) continue;

                brawler.IsInTerminalRally = true;

                bool isPlayer = IsOnTeam(brawler, playerTeam);
                events.Publish(new BrawlerKnockedOutEvent(brawler, isPlayer ? 0 : 1));

                // Prepend rally actions so they fire before any remaining queue entries.
                remainingQueue.InsertRange(0, brawler.QueuedActions);
                brawler.QueuedActions.Clear();
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
