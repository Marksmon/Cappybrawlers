using Capybrawlers.Cards;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Creatures
{
    // Resolves a serialized CapybrawlerBuildRecord into computed stats and card list
    // by looking up ScriptableObject assets from the EquipmentLibrary.
    public static class BuildRegistry
    {
        public static ResolvedBuild Resolve(CapybrawlerBuildRecord record, NatureLibrary natures, EquipmentLibrary equipment)
        {
            var nature = natures.Get(record.nature);
            var helm   = equipment.Get(record.helmId);
            var armor  = equipment.Get(record.armorId);
            var weapon = equipment.Get(record.weaponId);

            return new ResolvedBuild(record.buildId, nature, helm, armor, weapon);
        }
    }

    public class ResolvedBuild
    {
        public string BuildId   { get; }
        public NatureData Nature { get; }
        public EquipmentData Helm   { get; }
        public EquipmentData Armor  { get; }
        public EquipmentData Weapon { get; }

        public int FinalHP  => Nature.baseHP  + Helm.hpMod  + Armor.hpMod  + Weapon.hpMod;
        public int FinalATK => Nature.baseATK + Helm.atkMod + Armor.atkMod + Weapon.atkMod;
        public int FinalDEF => Nature.baseDEF + Helm.defMod + Armor.defMod + Weapon.defMod;
        public int FinalSPD => Nature.baseSPD + Helm.spdMod + Armor.spdMod + Weapon.spdMod;

        public CardData[] Cards => new[] { Helm.grantedCard, Armor.grantedCard, Weapon.grantedCard };

        public ResolvedBuild(string buildId, NatureData nature, EquipmentData helm, EquipmentData armor, EquipmentData weapon)
        {
            BuildId = buildId;
            Nature  = nature;
            Helm    = helm;
            Armor   = armor;
            Weapon  = weapon;
        }
    }
}
