using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Builds a random opponent team per PRD section 13:
    //   Slot 0 (Frontline):      Plant or Rock
    //   Slot 1 (BacklineLeft):   Moon  or Water
    //   Slot 2 (BacklineRight):  Flame or Storm
    public static class BattleArenaTeamGenerator
    {
        private static readonly NatureType[][] SlotNatures =
        {
            new[] { NatureType.Plant, NatureType.Rock  },
            new[] { NatureType.Moon,  NatureType.Water },
            new[] { NatureType.Flame, NatureType.Storm },
        };

        public static ResolvedBuild[] Generate(NatureLibrary natures, EquipmentLibrary equipment)
        {
            var builds = new ResolvedBuild[3];

            for (int slot = 0; slot < 3; slot++)
            {
                var nature    = SlotNatures[slot][Random.Range(0, 2)];
                var element   = (ElementType)nature; // Nature and Element enums are aligned
                var record    = new CapybrawlerBuildRecord
                {
                    buildId  = System.Guid.NewGuid().ToString(),
                    nature   = nature,
                    helmId   = RandomEquipId(element, EquipSlot.Helm),
                    armorId  = RandomEquipId(element, EquipSlot.Armor),
                    weaponId = RandomEquipId(element, EquipSlot.Weapon),
                };
                builds[slot] = BuildRegistry.Resolve(record, natures, equipment);
            }

            return builds;
        }

        // IDs follow the naming convention "<element>_<slot>_<a|b|c>" (3 options per slot).
        private static string RandomEquipId(ElementType element, EquipSlot slot)
        {
            var elementName = element.ToString().ToLower();
            var slotName    = slot.ToString().ToLower();
            var suffix      = new[] { "a", "b", "c" }[Random.Range(0, 3)];
            return $"{elementName}_{slotName}_{suffix}";
        }
    }
}
