using System;
using System.Collections.Generic;

namespace Capybrawlers.Core
{
    public class OverlordInitializer
    {
        private readonly ISaveProvider _save;

        public OverlordInitializer(ISaveProvider save) => _save = save;

        public void EnsureProfileExists()
        {
            if (_save.Load() != null)
                return;

            var profile = new PlayerProfile
            {
                playerId          = Guid.NewGuid().ToString(),
                displayName       = GenerateDisplayName(),
                bpPoints          = 0,
                currentRank       = RankTier.Pup,
                accountLevel      = 1,
                ownedBuildRecords = StarterBuilds(),
            };
            _save.Save(profile);
        }

        // Three predefined starter builds — one per formation slot — so Phase 3 testing is unblocked
        // before the summoning system is designed.
        private static List<CapybrawlerBuildRecord> StarterBuilds() => new()
        {
            new CapybrawlerBuildRecord
            {
                buildId = Guid.NewGuid().ToString(),
                nature  = NatureType.Flame,
                helmId  = "flame_helm_a",
                armorId = "flame_armor_a",
                weaponId = "flame_weapon_a",
            },
            new CapybrawlerBuildRecord
            {
                buildId  = Guid.NewGuid().ToString(),
                nature   = NatureType.Plant,
                helmId   = "plant_helm_a",
                armorId  = "plant_armor_a",
                weaponId = "plant_weapon_a",
            },
            new CapybrawlerBuildRecord
            {
                buildId  = Guid.NewGuid().ToString(),
                nature   = NatureType.Rock,
                helmId   = "rock_helm_a",
                armorId  = "rock_armor_a",
                weaponId = "rock_weapon_a",
            },
        };

        private static string GenerateDisplayName()
        {
            var adj  = new[] { "Swift", "Bold", "Calm", "Wild", "Fierce", "Lazy", "Sneaky", "Mighty" };
            var noun = new[] { "Capy", "Brawler", "Tank", "Striker", "Guardian", "Shadow", "Storm", "Rock" };
            var rng  = new Random();
            return $"{adj[rng.Next(adj.Length)]}{noun[rng.Next(noun.Length)]}{rng.Next(100, 999)}";
        }
    }
}
