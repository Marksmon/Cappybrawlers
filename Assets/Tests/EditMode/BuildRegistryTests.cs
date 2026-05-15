using Capybrawlers.Cards;
using Capybrawlers.Core;
using Capybrawlers.Creatures;
using NUnit.Framework;
using UnityEngine;

namespace Capybrawlers.Tests
{
    // Verifies that BuildRegistry correctly combines Nature base stats with
    // equipment modifiers for all six Nature types.
    public class BuildRegistryTests
    {
        // ── Fixtures ─────────────────────────────────────────────────────────────

        private NatureLibrary    _natures;
        private EquipmentLibrary _equipment;

        [SetUp]
        public void SetUp()
        {
            _natures   = ScriptableObject.CreateInstance<NatureLibrary>();
            _equipment = ScriptableObject.CreateInstance<EquipmentLibrary>();

            // Inject test data via reflection (SerializeField backing fields).
            SetPrivateField(_natures,   "_natures",   MakeNatures());
            SetPrivateField(_equipment, "_equipment", MakeEquipment());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_natures);
            Object.DestroyImmediate(_equipment);
        }

        // ── Final stat tests ─────────────────────────────────────────────────────

        [TestCase(NatureType.Flame, 70, 20, 10, 15, ExpectedResult = new[] { 70+2, 20+3, 10+0, 15+1 })]
        [TestCase(NatureType.Storm, 50, 15, 8,  20, ExpectedResult = new[] { 50+2, 15+3, 8+0,  20+1 })]
        [TestCase(NatureType.Plant, 90, 10, 15, 12, ExpectedResult = new[] { 90+2, 10+3, 15+0, 12+1 })]
        [TestCase(NatureType.Rock,  100, 8, 20, 8,  ExpectedResult = new[] { 100+2, 8+3, 20+0, 8+1 })]
        [TestCase(NatureType.Water, 60, 12, 12, 16, ExpectedResult = new[] { 60+2, 12+3, 12+0, 16+1 })]
        [TestCase(NatureType.Moon,  55, 18, 9,  18, ExpectedResult = new[] { 55+2, 18+3, 9+0,  18+1 })]
        public int[] FinalStats_AreNaturePlusEquipmentMods(
            NatureType nature, int hp, int atk, int def, int spd)
        {
            OverrideNatureStats(nature, hp, atk, def, spd);
            var record   = MakeRecord(nature);
            var resolved = BuildRegistry.Resolve(record, _natures, _equipment);
            return new[] { resolved.FinalHP, resolved.FinalATK, resolved.FinalDEF, resolved.FinalSPD };
        }

        [Test]
        public void Cards_ReturnsThreeCards_OnePerEquipmentSlot()
        {
            var record   = MakeRecord(NatureType.Flame);
            var resolved = BuildRegistry.Resolve(record, _natures, _equipment);
            Assert.AreEqual(3, resolved.Cards.Length);
        }

        [Test]
        public void Resolve_ThrowsKeyNotFoundException_ForUnknownEquipmentId()
        {
            var record = new CapybrawlerBuildRecord
            {
                buildId  = "test",
                nature   = NatureType.Flame,
                helmId   = "flame_helm_a",
                armorId  = "flame_armor_a",
                weaponId = "MISSING_ID",
            };
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => BuildRegistry.Resolve(record, _natures, _equipment));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static CapybrawlerBuildRecord MakeRecord(NatureType nature) =>
            new() { buildId = "test", nature = nature, helmId = "flame_helm_a", armorId = "flame_armor_a", weaponId = "flame_weapon_a" };

        // Equipment mods shared across all test builds: HP+2, ATK+3, DEF+0, SPD+1 total.
        private static EquipmentData[] MakeEquipment()
        {
            var helm   = MakeEquipData("flame_helm_a",   EquipSlot.Helm,   ElementType.Flame, hpMod: 1, atkMod: 1);
            var armor  = MakeEquipData("flame_armor_a",  EquipSlot.Armor,  ElementType.Flame, hpMod: 1, atkMod: 1);
            var weapon = MakeEquipData("flame_weapon_a", EquipSlot.Weapon, ElementType.Flame, atkMod: 1, spdMod: 1);
            return new[] { helm, armor, weapon };
        }

        private static EquipmentData MakeEquipData(
            string id, EquipSlot slot, ElementType element,
            int hpMod = 0, int atkMod = 0, int defMod = 0, int spdMod = 0)
        {
            var e = ScriptableObject.CreateInstance<EquipmentData>();
            e.id      = id;
            e.slot    = slot;
            e.element = element;
            e.hpMod   = hpMod;
            e.atkMod  = atkMod;
            e.defMod  = defMod;
            e.spdMod  = spdMod;
            e.grantedCard = ScriptableObject.CreateInstance<CardData>();
            return e;
        }

        private NatureData[] MakeNatures()
        {
            var natures = new NatureData[(int)NatureType.Moon + 1];
            foreach (NatureType n in System.Enum.GetValues(typeof(NatureType)))
                natures[(int)n] = MakeNatureData(n, 50, 10, 10, 10);
            return natures;
        }

        private static NatureData MakeNatureData(NatureType type, int hp, int atk, int def, int spd)
        {
            var n = ScriptableObject.CreateInstance<NatureData>();
            n.natureType = type;
            n.baseHP     = hp;
            n.baseATK    = atk;
            n.baseDEF    = def;
            n.baseSPD    = spd;
            return n;
        }

        // Override stats for a single nature in the library's backing array.
        private void OverrideNatureStats(NatureType type, int hp, int atk, int def, int spd)
        {
            var arr = (NatureData[])GetPrivateField(_natures, "_natures");
            var nd  = arr[(int)type];
            nd.baseHP  = hp;
            nd.baseATK = atk;
            nd.baseDEF = def;
            nd.baseSPD = spd;
            // Bust the lazy lookup cache so it rebuilds with the updated values.
            SetPrivateField(_natures, "_lookup", null);
        }

        private static void SetPrivateField(object obj, string field, object value)
        {
            var f = obj.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            f?.SetValue(obj, value);
        }

        private static object GetPrivateField(object obj, string field)
        {
            var f = obj.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f?.GetValue(obj);
        }
    }
}
