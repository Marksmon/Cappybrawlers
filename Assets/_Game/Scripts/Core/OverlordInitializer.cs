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
            var existing = _save.Load();
            if (existing != null)
            {
                PatchActiveTeamIfEmpty(existing);
                return;
            }

            var starters = StarterBuilds();
            var profile = new PlayerProfile
            {
                playerId          = Guid.NewGuid().ToString(),
                displayName       = GenerateDisplayName(),
                bpPoints          = 0,
                currentRank       = RankTier.Pup,
                accountLevel      = 1,
                ownedBuildRecords = starters,
                activeTeamBuildIds = new[]
                {
                    starters[0].buildId,
                    starters[1].buildId,
                    starters[2].buildId,
                },
            };
            _save.Save(profile);
        }

        private void PatchActiveTeamIfEmpty(PlayerProfile profile)
        {
            if (profile.activeTeamBuildIds != null &&
                profile.activeTeamBuildIds.Length == 3 &&
                !string.IsNullOrEmpty(profile.activeTeamBuildIds[0]))
                return;

            if (profile.ownedBuildRecords == null || profile.ownedBuildRecords.Count < 3)
                return;

            profile.activeTeamBuildIds = new[]
            {
                profile.ownedBuildRecords[0].buildId,
                profile.ownedBuildRecords[1].buildId,
                profile.ownedBuildRecords[2].buildId,
            };
            _save.Save(profile);
        }

        // Slot 0 = Rock (frontline), Slot 1 = Moon, Slot 2 = Flame (backline).
        // Mirrors the demo/test layout: Flame | Moon | Rock vs Plant | Water | Storm.
        private static List<CapybrawlerBuildRecord> StarterBuilds() => new()
        {
            new CapybrawlerBuildRecord
            {
                buildId  = Guid.NewGuid().ToString(),
                nature   = NatureType.Rock,
                helmId   = "rock_helm_a",
                armorId  = "rock_armor_a",
                weaponId = "rock_weapon_a",
            },
            new CapybrawlerBuildRecord
            {
                buildId  = Guid.NewGuid().ToString(),
                nature   = NatureType.Moon,
                helmId   = "moon_helm_a",
                armorId  = "moon_armor_a",
                weaponId = "moon_weapon_a",
            },
            new CapybrawlerBuildRecord
            {
                buildId  = Guid.NewGuid().ToString(),
                nature   = NatureType.Flame,
                helmId   = "flame_helm_a",
                armorId  = "flame_armor_a",
                weaponId = "flame_weapon_a",
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
