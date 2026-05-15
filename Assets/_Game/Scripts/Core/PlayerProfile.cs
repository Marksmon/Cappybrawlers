using System;
using System.Collections.Generic;

namespace Capybrawlers.Core
{
    [Serializable]
    public class PlayerProfile
    {
        public string playerId;
        public string displayName;
        public int bpPoints;
        public RankTier currentRank;
        public int accountLevel;
        public List<CapybrawlerBuildRecord> ownedBuildRecords = new();
        public string[] activeTeamBuildIds = new string[3];
    }

    // Stored in PlayerProfile; resolved to full stats + cards by BuildRegistry (Creatures assembly).
    [Serializable]
    public class CapybrawlerBuildRecord
    {
        public string buildId;
        public NatureType nature;
        public string helmId;
        public string armorId;
        public string weaponId;
    }
}
