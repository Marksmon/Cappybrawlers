# Capybrawlers — Technical Systems Reference

## System Dependency Map

```
GameManager (bootstrap singleton)
    │
    ├── ServiceLocator
    │       ├── EventBus
    │       ├── SaveProvider (ISaveProvider)
    │       ├── BPManager               (BrawlPoints + rank)
    │       ├── CollectionManager
    │       │       └── BuildRegistry   (owned CapybrawlerBuilds)
    │       └── SceneLoader
    │
    ├── BattleManager  (created per battle, destroyed after)
    │       ├── TurnStateMachine
    │       ├── BattleTeam[2]           (player + opponent)
    │       │       ├── CapybrawlerInstance[3]
    │       │       ├── StaminaPool
    │       │       ├── CardPool
    │       │       └── Hand
    │       ├── SPDResolver             (sorts actions by SPD + tiebreaker)
    │       ├── TerminalRallyHandler
    │       └── EffectProcessor         (applies mechanics: Burn, Shield, etc.)
    │
    └── UIManager  (always-alive canvas stack)
```

---

## Enumerations

```csharp
namespace Capybrawlers.Core
{
    public enum NatureType  { Flame, Storm, Plant, Rock, Water, Moon }
    public enum ElementType { Flame, Storm, Plant, Rock, Water, Moon }
    public enum EquipSlot   { Helm, Armor, Weapon }
    public enum MechanicType
    {
        Burn, Rage, Execute, Pierce, Berserker,
        Haste, Slow, Stun, FullStun,
        Reflect, Shield, StaminaSteal,
        Poison, Lifesteal,
        Heal, Purge, StaminaRegen, Cleanse
    }
    public enum RankTier    { Pup, Sprout, Brawler, StoneFang, Tempest, GrandCapy }
    public enum FormationSlot { Frontline, BacklineLeft, BacklineRight }
    public enum BattleResult  { Win, Loss }
}
```

---

## Data Models

### NatureData (ScriptableObject)
```csharp
namespace Capybrawlers.Creatures
{
    public class NatureData : ScriptableObject
    {
        public NatureType natureType;
        public int baseHP;
        public int baseATK;
        public int baseDEF;
        public int baseSPD;
        [TextArea] public string roleDescription;
        public Sprite idleSprite;
        public RuntimeAnimatorController animatorController;
    }
}
```

### EquipmentData (ScriptableObject)
```csharp
namespace Capybrawlers.Creatures
{
    public class EquipmentData : ScriptableObject
    {
        public string id;
        public string equipmentName;
        public EquipSlot slot;
        public ElementType element;
        // Signed deltas applied to Nature base stats
        public int hpMod;
        public int atkMod;
        public int defMod;
        public int spdMod;
        public CardData grantedCard;
        public Sprite icon;
    }
}
```

### CardData (ScriptableObject)
```csharp
namespace Capybrawlers.Cards
{
    public class CardData : ScriptableObject
    {
        public string id;
        public string cardName;
        public int staminaCost;       // 0, 1, or 2 — never outside this range
        public Sprite artwork;
        [TextArea] public string effectDescription;
        public List<CardEffect> effects;  // Ordered list of effects to execute
    }
}
```

### CardEffect (abstract ScriptableObject)
```csharp
namespace Capybrawlers.Cards
{
    public abstract class CardEffect : ScriptableObject
    {
        public MechanicType mechanic;
        public abstract void Execute(EffectContext context);
    }
    // Concrete subclasses: DamageEffect, BurnEffect, HealEffect, ShieldEffect,
    // HasteEffect, SlowEffect, StunEffect, PoisonEffect, LifestealEffect,
    // PurgeEffect, CleanseEffect, ReflectEffect, StaminaStealEffect,
    // StaminaRegenEffect, RageEffect, ExecuteEffect, BerserkerEffect, PierceEffect
}
```

### CapybrawlerBuild (ScriptableObject)
Fixed at summoning — never mutated at runtime.
```csharp
namespace Capybrawlers.Creatures
{
    public class CapybrawlerBuild : ScriptableObject
    {
        public string buildId;
        public NatureData nature;
        public EquipmentData helm;
        public EquipmentData armor;
        public EquipmentData weapon;

        // Computed — not serialized, derived from nature + equipment modifiers
        public int FinalHP  => nature.baseHP  + helm.hpMod  + armor.hpMod  + weapon.hpMod;
        public int FinalATK => nature.baseATK + helm.atkMod + armor.atkMod + weapon.atkMod;
        public int FinalDEF => nature.baseDEF + helm.defMod + armor.defMod + weapon.defMod;
        public int FinalSPD => nature.baseSPD + helm.spdMod + armor.spdMod + weapon.spdMod;

        // The 3 cards this build contributes to the team pool
        public CardData[] Cards => new[] { helm.grantedCard, armor.grantedCard, weapon.grantedCard };
    }
}
```

### CapybrawlerInstance (Runtime Class — not a ScriptableObject)
Mutable runtime state wrapping a fixed `CapybrawlerBuild`.
```csharp
namespace Capybrawlers.Creatures
{
    public class CapybrawlerInstance
    {
        public CapybrawlerBuild Build { get; }
        public int CurrentHP { get; private set; }
        public int CurrentATK { get; private set; }   // includes Rage/buff deltas
        public int CurrentDEF { get; private set; }
        public int CurrentSPD { get; private set; }   // includes Haste/Slow deltas
        public bool IsKnockedOut => CurrentHP <= 0;
        public bool IsInTerminalRally { get; private set; }
        public FormationSlot Slot { get; set; }

        public List<ActiveEffect> ActiveEffects { get; }     // Burn, Poison, etc.
        public List<CardData> QueuedActions { get; }         // actions committed this turn
        public bool IsFullStunned { get; private set; }

        public void ApplyDamage(int raw, bool pierce = false) { ... }
        public void ApplyHeal(int amount) { ... }
        public void AddEffect(ActiveEffect effect) { ... }
        public void TickEffects() { ... }   // called at Start of Turn
        public void EnterTerminalRally() { IsInTerminalRally = true; }
        public void QueueAction(CardData card) { QueuedActions.Add(card); }
    }
}
```

### BattleTeam
```csharp
namespace Capybrawlers.Battle
{
    public class BattleTeam
    {
        public CapybrawlerInstance[] Brawlers { get; }   // length 3
        public StaminaPool Stamina { get; }
        public CardPool Pool { get; }
        public Hand Hand { get; }
        public bool IsDefeated => Brawlers.All(b => b.IsKnockedOut);
    }
}
```

### StaminaPool
```csharp
namespace Capybrawlers.Battle
{
    public class StaminaPool
    {
        public int Current { get; private set; }
        public int Max { get; }         // TBD — see Open Design Questions in GDD
        public int RegenPerTurn { get; } // TBD

        public bool TrySpend(int amount) { ... }  // returns false if insufficient
        public void Regen() { ... }
        public void Steal(int amount, StaminaPool source) { ... }
        public void Add(int amount) { ... }
    }
}
```

### CardPool
```csharp
namespace Capybrawlers.Battle
{
    public class CardPool
    {
        // Initialized from 3 brawlers × 3 cards × 2 copies = 18 slots
        public List<CardPoolEntry> Entries { get; }    // 18 entries

        public List<CardData> GetEligible(Hand hand) { ... }  // excludes in-hand + on-cooldown
        public void StartCooldown(CardData card) { ... }      // 2-turn cooldown
        public void TickCooldowns() { ... }                   // called at Start of Turn
    }

    public class CardPoolEntry
    {
        public CardData Card;
        public int CopyIndex;       // 0 or 1 (two copies exist)
        public int CooldownTurns;   // 0 = available, >0 = on cooldown
        public bool IsInHand;
    }
}
```

### PlayerProfile (Serializable — saved to disk)
```csharp
[Serializable]
public class PlayerProfile
{
    public string playerId;
    public string displayName;
    public int bpPoints;
    public RankTier currentRank;
    public int accountLevel;
    public string[] ownedBuildIds;   // references CapybrawlerBuild asset IDs
}
```

---

## Battle State Machine

Six phases execute in order each turn:

```
        ┌──────────────┐
        │     Idle     │  No battle active
        └──────┬───────┘
               │ StartBattle()
        ┌──────▼───────┐
        │    Setup     │  Assign brawlers, init pools, reveal builds
        └──────┬───────┘
               │
        ┌──────▼────────────┐◄──────────────────────────────────────────────┐
        │  StartOfTurn      │  Stamina regen both teams. Tick all cooldowns. │
        │                   │  Tick all active effects (Burn/Poison/buffs).  │
        └──────┬────────────┘                                                │
               │                                                             │
        ┌──────▼────────────┐                                                │
        │    DrawPhase      │  Each player draws 3 eligible cards.           │
        └──────┬────────────┘                                                │
               │                                                             │
        ┌──────▼────────────┐                                                │
        │    QueuePhase     │  Both players secretly assign cards + Stamina  │
        │  (simultaneous)   │  to brawlers. Neither sees the other's queue.  │
        └──────┬────────────┘                                                │
               │ Both players confirm                                        │
        ┌──────▼────────────┐                                                │
        │  ResolutionPhase  │  Actions fire in SPD order (high→low).        │
        │                   │  Tiebreaker: Storm>Flame>Water>Moon>Plant>Rock  │
        └──────┬────────────┘                                                │
               │                                                             │
        ┌──────▼────────────┐                                                │
        │  TerminalRally    │  Any brawler at 0 HP fires all queued          │
        │                   │  actions before removal.                       │
        └──────┬────────────┘                                                │
               │                                                             │
        ┌──────▼────────────┐                                                │
        │    Cleanup        │  Remove fallen brawlers. Return unplayed       │
        │                   │  hand cards to pool.                           │
        └──────┬────────────┘                                                │
               │                                                             │
       ┌───────┴──────────┐                                                  │
       │  Win condition?  │                                                  │
       ├─ Yes ────────────┤                                                  │
       │  ┌───────────┐   │                                                  │
       │  │ BattleEnd │   │  Award/deduct BP. Show results.                  │
       │  └───────────┘   │                                                  │
       └─ No ─────────────┴───────────────────────────────────────────────► │
                                                                             │
                                (loop back to StartOfTurn)                  │
```

**Win condition:** A team wins when all 3 opponent brawlers are knocked out. Both teams losing their last brawler in the same Terminal Rally is an edge case — see Open Design Questions in GDD.

---

## EventBus Event Catalog

All events live in `Capybrawlers.Core.Events`.

| Event Class | Published By | Subscribed By |
|---|---|---|
| `BattleStartedEvent` | BattleManager | UIManager, AudioManager |
| `BattleEndedEvent(result, bpDelta)` | BattleManager | UIManager, BPManager |
| `TurnStartedEvent(turnNumber)` | TurnStateMachine | UIManager, AudioManager |
| `QueuePhaseStartedEvent` | TurnStateMachine | UIManager (show ready button) |
| `QueueConfirmedEvent(team)` | BattleTeam | UIManager (show waiting indicator) |
| `ResolutionStartedEvent` | TurnStateMachine | UIManager (hide hands) |
| `ActionResolvedEvent(brawler, card)` | SPDResolver | UIManager, AudioManager |
| `BrawlerDamagedEvent(brawler, amount, wasPierced)` | CapybrawlerInstance | BrawlerView, UIManager |
| `BrawlerHealedEvent(brawler, amount)` | CapybrawlerInstance | BrawlerView, UIManager |
| `BrawlerKnockedOutEvent(brawler)` | CapybrawlerInstance | BattleManager, BrawlerView |
| `TerminalRallyStartedEvent(brawler)` | TerminalRallyHandler | UIManager, AudioManager |
| `TerminalRallyEndedEvent(brawler)` | TerminalRallyHandler | BattleManager |
| `StaminaChangedEvent(team, newValue)` | StaminaPool | UIManager (own team only) |
| `StaminaStolenEvent(victim, amount)` | StaminaPool | UIManager, AudioManager |
| `CardDrawnEvent(team, card)` | Hand | UIManager (own hand only) |
| `CardPlayedEvent(brawler, card)` | QueuePhase | UIManager |
| `CardCooldownStartedEvent(card, turns)` | CardPool | UIManager |
| `CardCooldownEndedEvent(card)` | CardPool | UIManager |
| `EffectAppliedEvent(brawler, mechanic)` | EffectProcessor | BrawlerView, UIManager |
| `EffectTickedEvent(brawler, mechanic, damage)` | EffectProcessor | BrawlerView, UIManager |
| `EffectExpiredEvent(brawler, mechanic)` | EffectProcessor | BrawlerView |
| `BPChangedEvent(newTotal, delta)` | BPManager | UIManager |
| `RankChangedEvent(oldRank, newRank)` | BPManager | UIManager |

---

## Assembly Definition Structure

| .asmdef Name | Folder | References |
|---|---|---|
| `Capybrawlers.Core` | Scripts/Core/ | (none — no game deps) |
| `Capybrawlers.Cards` | Scripts/Cards/ | Core |
| `Capybrawlers.Creatures` | Scripts/Creatures/ | Core, Cards |
| `Capybrawlers.Battle` | Scripts/Battle/ | Core, Cards, Creatures |
| `Capybrawlers.Collection` | Scripts/Collection/ | Core, Creatures |
| `Capybrawlers.UI` | Scripts/UI/ | Core, Cards, Creatures, Battle, Collection |
| `Capybrawlers.Tests.EditMode` | Tests/EditMode/ | All above (test only) |
| `Capybrawlers.Tests.PlayMode` | Tests/PlayMode/ | All above (test only) |

---

## ServiceLocator Usage

```csharp
// Register (done in GameManager.Awake, in dependency order)
ServiceLocator.Register<IEventBus>(new EventBus());
ServiceLocator.Register<ISaveProvider>(new LocalJsonSaveProvider());
ServiceLocator.Register<IBPManager>(new BPManager());
ServiceLocator.Register<ICollectionManager>(new CollectionManager());

// Resolve (anywhere at runtime after bootstrap)
var bp = ServiceLocator.Get<IBPManager>();
bp.ApplyResult(BattleResult.Win);
```
