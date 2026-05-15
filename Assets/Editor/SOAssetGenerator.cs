#if UNITY_EDITOR
using System.Collections.Generic;
using Capybrawlers.Battle;
using Capybrawlers.Cards;
using Capybrawlers.Cards.Effects;
using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Capybrawlers.UI;

public static class SOAssetGenerator
{
    private const string NaturesDir  = "Assets/_Game/ScriptableObjects/Natures";
    private const string EquipDir    = "Assets/_Game/ScriptableObjects/Equipment";
    private const string CardsDir    = "Assets/_Game/ScriptableObjects/Cards";
    private const string LibsDir     = "Assets/_Game/ScriptableObjects/Libraries";
    private const string EffectsDir  = "Assets/_Game/ScriptableObjects/Effects";

    // ── Nature base stats from GDD ────────────────────────────────────────────
    private static readonly (NatureType type, int hp, int atk, int def, int spd, string role)[] NatureStats =
    {
        (NatureType.Flame, 40,  30, 10, 20, "Aggressive attacker — high ATK, fragile."),
        (NatureType.Storm, 40,  20, 10, 30, "Speed controller — fastest, hits first."),
        (NatureType.Plant, 100, 10, 20, 20, "Sustain tank — massive HP, supports team."),
        (NatureType.Rock,  80,  20, 30, 10, "Immovable wall — extreme DEF, moves last."),
        (NatureType.Water, 60,  20, 15, 15, "Balanced bruiser — no weaknesses."),
        (NatureType.Moon,  60,  15, 20, 15, "Utility support — effects over raw damage."),
    };

    // ── Equipment per element: (suffix, slot, hpMod, atkMod, defMod, spdMod, cardName, cost, mechanic, description)
    private static readonly (string element, ElementType elem, EquipSlot slot, string suffix,
        int hpMod, int atkMod, int defMod, int spdMod,
        string cardName, int cost, string mechanic, string desc)[] EquipmentTable =
    {
        // ── Flame ──────────────────────────────────────────────────────────────
        ("flame", ElementType.Flame, EquipSlot.Helm,   "a",  -4,  4, 0, 0, "Blazing Crest",   1, "Burn",      "Deal ATK damage and inflict Burn (2 damage/turn for 3 turns)."),
        ("flame", ElementType.Flame, EquipSlot.Helm,   "b",  -3,  3, 0, 0, "Ember Visor",     0, "Rage",      "Grant +5 ATK for 2 turns."),
        ("flame", ElementType.Flame, EquipSlot.Helm,   "c",  -3,  3, 0, 0, "Inferno Crown",   2, "Execute",   "Deal ATK×1.5 damage. Bonus ATK×0.5 if target HP < 30%."),
        ("flame", ElementType.Flame, EquipSlot.Armor,  "a",  -2,  0, 0, 0, "Magma Plate",     1, "Berserker", "Grant +3 ATK for each Active Effect on self (max +15) for 1 turn."),
        ("flame", ElementType.Flame, EquipSlot.Armor,  "b",  -2,  0, 0, 0, "Ashmail",         0, "Burn",      "Deal ATK damage and inflict Burn (2 damage/turn for 3 turns)."),
        ("flame", ElementType.Flame, EquipSlot.Armor,  "c",  -1,  0, 0, 0, "Scorch Vest",     1, "Pierce",    "Deal ATK damage ignoring target DEF."),
        ("flame", ElementType.Flame, EquipSlot.Weapon, "a",   0,  3, 0, 0, "Lava Fang",       1, "Pierce",    "Deal ATK damage ignoring target DEF."),
        ("flame", ElementType.Flame, EquipSlot.Weapon, "b",   0,  2, 0, 0, "Cinder Claws",    0, "Burn",      "Deal ATK damage and inflict Burn (2 damage/turn for 3 turns)."),
        ("flame", ElementType.Flame, EquipSlot.Weapon, "c",   0,  5, 0, 0, "Pyroclasm Blade", 2, "Execute",   "Deal ATK×1.5 damage. Bonus ATK×0.5 if target HP < 30%."),

        // ── Storm ──────────────────────────────────────────────────────────────
        ("storm", ElementType.Storm, EquipSlot.Helm,   "a",  -4,  0, 0,  4, "Tempest Crown",  1, "Haste",     "Grant +5 SPD for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Helm,   "b",  -3,  0, 0,  3, "Gale Visor",     0, "Slow",      "Inflict -5 SPD on target for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Helm,   "c",  -3,  0, 0,  3, "Static Crest",   2, "Stun",      "Remove the target's lowest-priority queued action this turn."),
        ("storm", ElementType.Storm, EquipSlot.Armor,  "a",  -2,  0, 0,  0, "Cyclone Mail",   1, "Haste",     "Grant +5 SPD for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Armor,  "b",  -2,  0, 0,  0, "Windweave",      0, "Slow",      "Inflict -5 SPD on target for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Armor,  "c",  -1,  0, 0,  0, "Thunder Vest",   1, "FullStun",  "Remove ALL of target's queued actions this turn."),
        ("storm", ElementType.Storm, EquipSlot.Weapon, "a",   0,  0, 0,  3, "Lightning Fang", 1, "Haste",     "Grant +5 SPD for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Weapon, "b",   0,  0, 0,  2, "Bolt Claws",     0, "Slow",      "Inflict -5 SPD on target for 2 turns."),
        ("storm", ElementType.Storm, EquipSlot.Weapon, "c",   0,  0, 0,  5, "Vortex Blade",   2, "Stun",      "Remove the target's lowest-priority queued action this turn."),

        // ── Plant ──────────────────────────────────────────────────────────────
        ("plant", ElementType.Plant, EquipSlot.Helm,   "a",   8, -2, 0, 0, "Verdant Helm",   1, "Heal",      "Restore ATK×0.5 HP to self."),
        ("plant", ElementType.Plant, EquipSlot.Helm,   "b",   6, -2, 0, 0, "Leaf Crown",     0, "Cleanse",   "Remove all negative effects from every ally."),
        ("plant", ElementType.Plant, EquipSlot.Helm,   "c",   6, -1, 0, 0, "Thornwood Crest",2, "Poison",    "Inflict Poison: 4 damage/turn for 4 turns."),
        ("plant", ElementType.Plant, EquipSlot.Armor,  "a",   4, -1, 0, 0, "Mossbark Plate", 1, "Heal",      "Restore ATK×0.5 HP to self."),
        ("plant", ElementType.Plant, EquipSlot.Armor,  "b",   4, -1, 0, 0, "Blossom Mail",   0, "Purge",     "Remove all negative effects from self."),
        ("plant", ElementType.Plant, EquipSlot.Armor,  "c",   2,  0, 0, 0, "Spore Vest",     1, "Poison",    "Inflict Poison: 4 damage/turn for 4 turns."),
        ("plant", ElementType.Plant, EquipSlot.Weapon, "a",   0, -1, 0, 0, "Thorn Lash",     1, "Poison",    "Inflict Poison: 4 damage/turn for 4 turns."),
        ("plant", ElementType.Plant, EquipSlot.Weapon, "b",   0, -1, 0, 0, "Vine Whip",      0, "Heal",      "Restore ATK×0.5 HP to self."),
        ("plant", ElementType.Plant, EquipSlot.Weapon, "c",   0,  0, 0, 0, "Bloom Blade",    2, "Lifesteal", "Deal ATK damage; restore 50% of damage dealt as HP to self."),

        // ── Rock ───────────────────────────────────────────────────────────────
        ("rock", ElementType.Rock, EquipSlot.Helm,   "a",   0, -2,  4, -2, "Stone Helm",     1, "Shield",    "Grant a shield absorbing up to 15 damage for 2 turns."),
        ("rock", ElementType.Rock, EquipSlot.Helm,   "b",   0, -2,  3, -2, "Boulder Crown",  0, "Reflect",   "Grant Reflect for 1 turn: return 50% of incoming damage to attacker."),
        ("rock", ElementType.Rock, EquipSlot.Helm,   "c",   0, -1,  3, -1, "Granite Crest",  2, "Shield",    "Grant a shield absorbing up to 15 damage for 2 turns."),
        ("rock", ElementType.Rock, EquipSlot.Armor,  "a",   0, -1,  2, -1, "Rubble Plate",   1, "Shield",    "Grant a shield absorbing up to 15 damage for 2 turns."),
        ("rock", ElementType.Rock, EquipSlot.Armor,  "b",   0, -1,  2,  0, "Obsidian Mail",  0, "Reflect",   "Grant Reflect for 1 turn: return 50% of incoming damage to attacker."),
        ("rock", ElementType.Rock, EquipSlot.Armor,  "c",   0,  0,  1,  0, "Pebble Vest",    1, "Reflect",   "Grant Reflect for 1 turn: return 50% of incoming damage to attacker."),
        ("rock", ElementType.Rock, EquipSlot.Weapon, "a",   0, -1,  1, -1, "Earth Fang",     1, "Shield",    "Grant a shield absorbing up to 15 damage for 2 turns."),
        ("rock", ElementType.Rock, EquipSlot.Weapon, "b",   0, -1,  2, -1, "Quake Claws",    0, "Reflect",   "Grant Reflect for 1 turn: return 50% of incoming damage to attacker."),
        ("rock", ElementType.Rock, EquipSlot.Weapon, "c",   0,  0,  3, -2, "Tectonic Blade", 2, "Shield",    "Grant a shield absorbing up to 15 damage for 2 turns."),

        // ── Water ──────────────────────────────────────────────────────────────
        ("water", ElementType.Water, EquipSlot.Helm,   "a",   8,  0, 0, -2, "Tide Helm",      1, "Lifesteal", "Deal ATK damage; restore 50% of damage dealt as HP to self."),
        ("water", ElementType.Water, EquipSlot.Helm,   "b",   6,  0, 0, -2, "Coral Crown",    0, "Heal",      "Restore ATK×0.5 HP to self."),
        ("water", ElementType.Water, EquipSlot.Helm,   "c",   6,  0, 0, -1, "Torrent Crest",  2, "Slow",      "Inflict -5 SPD on target for 2 turns."),
        ("water", ElementType.Water, EquipSlot.Armor,  "a",   4,  0, 0, -1, "Seafoam Plate",  1, "Heal",      "Restore ATK×0.5 HP to self."),
        ("water", ElementType.Water, EquipSlot.Armor,  "b",   4,  0, 0, -1, "Kelp Mail",      0, "Purge",     "Remove all negative effects from self."),
        ("water", ElementType.Water, EquipSlot.Armor,  "c",   2,  0, 0,  0, "Current Vest",   1, "Cleanse",   "Remove all negative effects from every ally."),
        ("water", ElementType.Water, EquipSlot.Weapon, "a",   0,  0, 0, -1, "Whirlpool Fang", 1, "Lifesteal", "Deal ATK damage; restore 50% of damage dealt as HP to self."),
        ("water", ElementType.Water, EquipSlot.Weapon, "b",   0,  0, 0, -1, "Reef Claws",     0, "Heal",      "Restore ATK×0.5 HP to self."),
        ("water", ElementType.Water, EquipSlot.Weapon, "c",   0,  0, 0,  0, "Maelstrom Blade",2, "Lifesteal", "Deal ATK damage; restore 50% of damage dealt as HP to self."),

        // ── Moon ───────────────────────────────────────────────────────────────
        ("moon", ElementType.Moon, EquipSlot.Helm,   "a",   0, -2,  4,  0, "Lunar Helm",     1, "StaminaSteal",  "Steal 1 Stamina from the opponent's pool."),
        ("moon", ElementType.Moon, EquipSlot.Helm,   "b",   0, -2,  3,  0, "Crescent Crown", 0, "StaminaRegen",  "Add 2 Stamina to your team's pool."),
        ("moon", ElementType.Moon, EquipSlot.Helm,   "c",   0, -1,  3,  0, "Eclipse Crest",  2, "StaminaSteal",  "Steal 1 Stamina from the opponent's pool."),
        ("moon", ElementType.Moon, EquipSlot.Armor,  "a",   0, -1,  2,  0, "Nightweave Plate",1,"StaminaRegen",  "Add 2 Stamina to your team's pool."),
        ("moon", ElementType.Moon, EquipSlot.Armor,  "b",   0, -1,  2,  0, "Starfield Mail", 0, "Purge",         "Remove all negative effects from self."),
        ("moon", ElementType.Moon, EquipSlot.Armor,  "c",   0,  0,  1,  0, "Void Vest",      1, "Cleanse",       "Remove all negative effects from every ally."),
        ("moon", ElementType.Moon, EquipSlot.Weapon, "a",   0, -1,  1,  0, "Shadow Fang",    1, "StaminaSteal",  "Steal 1 Stamina from the opponent's pool."),
        ("moon", ElementType.Moon, EquipSlot.Weapon, "b",   0, -1,  2,  0, "Phantom Claws",  0, "StaminaRegen",  "Add 2 Stamina to your team's pool."),
        ("moon", ElementType.Moon, EquipSlot.Weapon, "c",   0,  0,  3,  0, "Eclipse Blade",  2, "StaminaSteal",  "Steal 1 Stamina from the opponent's pool."),
    };

    private const string BattleScenePath = "Assets/_Game/Scenes/Battle.unity";

    [MenuItem("Capybrawlers/Generate SO Assets")]
    public static void Generate()
    {
        EnsureDirs();

        var effectAssets = CreateEffectAssets();
        var cardAssets   = CreateCardAssets(effectAssets);
        var equipAssets  = CreateEquipmentAssets(cardAssets);
        var natAssets    = CreateNatureAssets();
        CreateNatureLibrary(natAssets);
        CreateEquipmentLibrary(equipAssets);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        WireLibrariesToBattleScene();

        Debug.Log("[Capybrawlers] SO assets generated and wired. Press Play from Battle.unity to test.");
    }

    private static void WireLibrariesToBattleScene()
    {
        var natLib  = AssetDatabase.LoadAssetAtPath<NatureLibrary>($"{LibsDir}/NatureLibrary.asset");
        var equipLib = AssetDatabase.LoadAssetAtPath<EquipmentLibrary>($"{LibsDir}/EquipmentLibrary.asset");
        if (natLib == null || equipLib == null)
        {
            Debug.LogWarning("[Capybrawlers] Library assets not found — skipping scene wiring.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Additive);

        foreach (var root in scene.GetRootGameObjects())
        {
            var sceneCtrl = root.GetComponentInChildren<BattleSceneController>(true);
            if (sceneCtrl != null)
            {
                var so = new SerializedObject(sceneCtrl);
                so.FindProperty("_natures")  .objectReferenceValue = natLib;
                so.FindProperty("_equipment").objectReferenceValue = equipLib;
                so.ApplyModifiedProperties();
            }

            var battleMgr = root.GetComponentInChildren<BattleManager>(true);
            if (battleMgr != null)
            {
                var so = new SerializedObject(battleMgr);
                so.FindProperty("_natures")  .objectReferenceValue = natLib;
                so.FindProperty("_equipment").objectReferenceValue = equipLib;
                so.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
    }

    // ── Effect assets (one shared SO per mechanic type) ───────────────────────

    private static Dictionary<string, CardEffect> CreateEffectAssets()
    {
        var map = new Dictionary<string, CardEffect>();

        // Damage
        var burn = LoadOrCreate<BurnEffect>($"{EffectsDir}/Effect_Burn.asset");
        burn.burnDamagePerTurn = 2; burn.burnDuration = 3;
        EditorUtility.SetDirty(burn); map["Burn"] = burn;

        var pierce = LoadOrCreate<PierceEffect>($"{EffectsDir}/Effect_Pierce.asset");
        EditorUtility.SetDirty(pierce); map["Pierce"] = pierce;

        var execute = LoadOrCreate<ExecuteEffect>($"{EffectsDir}/Effect_Execute.asset");
        EditorUtility.SetDirty(execute); map["Execute"] = execute;

        var poison = LoadOrCreate<PoisonEffect>($"{EffectsDir}/Effect_Poison.asset");
        poison.poisonDamagePerTurn = 4; poison.duration = 4;
        EditorUtility.SetDirty(poison); map["Poison"] = poison;

        // Buff/Debuff
        var rage = LoadOrCreate<RageEffect>($"{EffectsDir}/Effect_Rage.asset");
        rage.atkBonus = 5; rage.duration = 2;
        EditorUtility.SetDirty(rage); map["Rage"] = rage;

        var berserker = LoadOrCreate<BerserkerEffect>($"{EffectsDir}/Effect_Berserker.asset");
        berserker.atkBonusPerMissingHpPercent = 15;
        EditorUtility.SetDirty(berserker); map["Berserker"] = berserker;

        var haste = LoadOrCreate<HasteEffect>($"{EffectsDir}/Effect_Haste.asset");
        haste.spdBonus = 5; haste.duration = 2;
        EditorUtility.SetDirty(haste); map["Haste"] = haste;

        var slow = LoadOrCreate<SlowEffect>($"{EffectsDir}/Effect_Slow.asset");
        slow.spdReduction = 5; slow.duration = 2;
        EditorUtility.SetDirty(slow); map["Slow"] = slow;

        var shield = LoadOrCreate<ShieldEffect>($"{EffectsDir}/Effect_Shield.asset");
        shield.shieldAmount = 15; shield.duration = 2;
        EditorUtility.SetDirty(shield); map["Shield"] = shield;

        var reflect = LoadOrCreate<ReflectEffect>($"{EffectsDir}/Effect_Reflect.asset");
        reflect.reflectPercent = 50; reflect.duration = 1;
        EditorUtility.SetDirty(reflect); map["Reflect"] = reflect;

        // Healing
        var heal = LoadOrCreate<HealEffect>($"{EffectsDir}/Effect_Heal.asset");
        EditorUtility.SetDirty(heal); map["Heal"] = heal;

        var lifesteal = LoadOrCreate<LifestealEffect>($"{EffectsDir}/Effect_Lifesteal.asset");
        EditorUtility.SetDirty(lifesteal); map["Lifesteal"] = lifesteal;

        var purge = LoadOrCreate<PurgeEffect>($"{EffectsDir}/Effect_Purge.asset");
        EditorUtility.SetDirty(purge); map["Purge"] = purge;

        var cleanse = LoadOrCreate<CleanseEffect>($"{EffectsDir}/Effect_Cleanse.asset");
        EditorUtility.SetDirty(cleanse); map["Cleanse"] = cleanse;

        // Control
        var stun = LoadOrCreate<StunEffect>($"{EffectsDir}/Effect_Stun.asset");
        EditorUtility.SetDirty(stun); map["Stun"] = stun;

        var fullStun = LoadOrCreate<FullStunEffect>($"{EffectsDir}/Effect_FullStun.asset");
        EditorUtility.SetDirty(fullStun); map["FullStun"] = fullStun;

        // Stamina
        var staminaSteal = LoadOrCreate<StaminaStealEffect>($"{EffectsDir}/Effect_StaminaSteal.asset");
        staminaSteal.amount = 1;
        EditorUtility.SetDirty(staminaSteal); map["StaminaSteal"] = staminaSteal;

        var staminaRegen = LoadOrCreate<StaminaRegenEffect>($"{EffectsDir}/Effect_StaminaRegen.asset");
        staminaRegen.amount = 2;
        EditorUtility.SetDirty(staminaRegen); map["StaminaRegen"] = staminaRegen;

        return map;
    }

    // ── Nature assets ─────────────────────────────────────────────────────────

    private static NatureData[] CreateNatureAssets()
    {
        var list = new List<NatureData>();
        foreach (var (type, hp, atk, def, spd, role) in NatureStats)
        {
            var path = $"{NaturesDir}/Nature_{type}.asset";
            var nd   = LoadOrCreate<NatureData>(path);
            nd.natureType     = type;
            nd.baseHP         = hp;
            nd.baseATK        = atk;
            nd.baseDEF        = def;
            nd.baseSPD        = spd;
            nd.roleDescription = role;
            EditorUtility.SetDirty(nd);
            list.Add(nd);
        }
        return list.ToArray();
    }

    // ── Card assets ───────────────────────────────────────────────────────────

    private static Dictionary<string, CardData> CreateCardAssets(Dictionary<string, CardEffect> effectMap)
    {
        var map = new Dictionary<string, CardData>();
        foreach (var row in EquipmentTable)
        {
            var cardId = $"card_{row.element}_{SlotStr(row.slot)}_{row.suffix}";
            var path   = $"{CardsDir}/Card_{row.element}_{SlotStr(row.slot)}_{row.suffix}.asset";
            var cd     = LoadOrCreate<CardData>(path);
            cd.id                = cardId;
            cd.cardName          = row.cardName;
            cd.staminaCost       = row.cost;
            cd.effectDescription = row.desc;

            // Wire the effect SO.
            cd.effects = new System.Collections.Generic.List<CardEffect>();
            if (effectMap.TryGetValue(row.mechanic, out var effect))
                cd.effects.Add(effect);
            else
                Debug.LogWarning($"[Capybrawlers] No effect SO found for mechanic '{row.mechanic}' on card '{row.cardName}'");

            EditorUtility.SetDirty(cd);
            map[cardId] = cd;
        }
        return map;
    }

    // ── Equipment assets ──────────────────────────────────────────────────────

    private static EquipmentData[] CreateEquipmentAssets(Dictionary<string, CardData> cardMap)
    {
        var list = new List<EquipmentData>();
        foreach (var row in EquipmentTable)
        {
            var equipId = $"{row.element}_{SlotStr(row.slot)}_{row.suffix}";
            var path    = $"{EquipDir}/Equip_{row.element}_{SlotStr(row.slot)}_{row.suffix}.asset";
            var ed      = LoadOrCreate<EquipmentData>(path);
            ed.id            = equipId;
            ed.equipmentName = $"{Capitalize(row.element)} {row.slot} {row.suffix.ToUpper()}";
            ed.slot          = row.slot;
            ed.element       = row.elem;
            ed.hpMod         = row.hpMod;
            ed.atkMod        = row.atkMod;
            ed.defMod        = row.defMod;
            ed.spdMod        = row.spdMod;

            var cardId = $"card_{row.element}_{SlotStr(row.slot)}_{row.suffix}";
            if (cardMap.TryGetValue(cardId, out var card))
                ed.grantedCard = card;

            EditorUtility.SetDirty(ed);
            list.Add(ed);
        }
        return list.ToArray();
    }

    // ── Library assets ────────────────────────────────────────────────────────

    private static void CreateNatureLibrary(NatureData[] natures)
    {
        var path = $"{LibsDir}/NatureLibrary.asset";
        var lib  = LoadOrCreate<NatureLibrary>(path);
        var so   = new SerializedObject(lib);
        var prop = so.FindProperty("_natures");
        prop.arraySize = natures.Length;
        for (int i = 0; i < natures.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = natures[i];
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(lib);
    }

    private static void CreateEquipmentLibrary(EquipmentData[] equipment)
    {
        var path = $"{LibsDir}/EquipmentLibrary.asset";
        var lib  = LoadOrCreate<EquipmentLibrary>(path);
        var so   = new SerializedObject(lib);
        var prop = so.FindProperty("_equipment");
        prop.arraySize = equipment.Length;
        for (int i = 0; i < equipment.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = equipment[i];
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(lib);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureDirs()
    {
        foreach (var dir in new[] { NaturesDir, EquipDir, CardsDir, LibsDir, EffectsDir })
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parts  = dir.Split('/');
                var parent = string.Join("/", parts, 0, parts.Length - 1);
                AssetDatabase.CreateFolder(parent, parts[^1]);
            }
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static string SlotStr(EquipSlot slot) => slot switch
    {
        EquipSlot.Helm   => "helm",
        EquipSlot.Armor  => "armor",
        EquipSlot.Weapon => "weapon",
        _                => slot.ToString().ToLower()
    };

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
#endif
