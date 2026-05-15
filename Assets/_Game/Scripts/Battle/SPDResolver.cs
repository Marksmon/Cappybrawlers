using System.Collections.Generic;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public static class SPDResolver
    {
        // Merges queued actions from both teams and sorts by CurrentSPD descending.
        // Tiebreaker: NatureType ordinal ascending (Storm=0 wins, Rock=5 loses).
        public static List<QueuedAction> Resolve(BattleTeam playerTeam, BattleTeam opponentTeam)
        {
            var all = new List<QueuedAction>();

            foreach (var b in playerTeam.Brawlers)
                all.AddRange(b.QueuedActions);

            foreach (var b in opponentTeam.Brawlers)
                all.AddRange(b.QueuedActions);

            all.Sort((a, b) =>
            {
                int spdCmp = b.Source.CurrentSPD.CompareTo(a.Source.CurrentSPD);
                if (spdCmp != 0) return spdCmp;
                // Tiebreaker: lower NatureType ordinal wins (Storm=0 beats Rock=5).
                return a.Source.Build.Nature.natureType.CompareTo(b.Source.Build.Nature.natureType);
            });

            return all;
        }
    }
}
