# Capybrawlers — Project Guide

## Game Concept

Capybrawlers is a 2D simultaneous-turn tactical creature battler for iOS and Android. Players called **Overlords** collect capybara creatures called **Capybrawlers**, each with a permanent build of one Nature and three equipment pieces fixed at summoning. Teams of three battle in simultaneous turns — both players secretly commit actions, then all actions resolve in SPD order. The skill ceiling is built on prediction, composition, and resource management, not grinding or stat inflation.

## Tech Stack

| Concern | Choice |
|---------|--------|
| Engine | Unity 2D, Universal Render Pipeline (URP) |
| Language | C# (.NET Standard 2.1, Unity flavor) |
| Platforms | iOS 15+ and Android API 23+ (Android 6.0) |
| Rendering | URP 2D Renderer |
| Version Control | Git + Git LFS |
| Package Manager | Unity Package Manager (UPM) |
| IDE | Rider or Visual Studio 2022 (Unity workload) |

## UPM Packages (install after project creation)

| Package ID | Purpose |
|---|---|
| `com.unity.render-pipelines.universal` | URP 2D Renderer (select 2D Renderer asset) |
| `com.unity.inputsystem` | Touch input for mobile |
| `com.unity.addressables` | Async asset streaming (card art, creature sprites) |
| `com.unity.textmeshpro` | All UI text |
| `com.unity.cinemachine` | Battle camera animations |
| `com.unity.test-framework` | EditMode + PlayMode unit tests |

## Folder Structure

All game content lives under `Assets/_Game/`. Never place game scripts directly under `Assets/`.

```
Assets/
  _Game/
    Scripts/
      Core/          GameManager, ServiceLocator, EventBus, SceneLoader, ISaveProvider
      Battle/        BattleManager, TurnStateMachine, SPDResolver, TerminalRallyHandler,
                     EffectProcessor, BattleTeam, StaminaPool, CardPool, Hand,
                     CapybrawlerInstance, SimpleAIController, BattleArenaTeamGenerator
      Cards/         CardData, CardEffect (abstract + 18 subclasses)
      Creatures/     NatureData, EquipmentData, BuildRegistry, CapybrawlerBuildRecord
      Collection/    CollectionManager, CollectionController, FormationController
      Economy/       (stub — summoning cadence TBD per open design question in GDD)
      UI/            All MonoBehaviour UI controllers and views
      Networking/    (stub) Matchmaking, sync interfaces
      Utils/         Extension methods, helpers
    Prefabs/
      Battle/
      Cards/
      Creatures/
      UI/
    Scenes/
      Bootstrap.unity    Entry point — instantiates GameManager, loads MainMenu additively
      MainMenu.unity
      Battle.unity       Handles all battle modes including Battle Arena
      Collection.unity
    Art/
      Sprites/
        Creatures/
        Cards/
        UI/
        Backgrounds/
      Animations/
      Fonts/
    Audio/
      SFX/
      Music/
    ScriptableObjects/
      Natures/           6 NatureData assets (one per Nature)
      Equipment/         54 EquipmentData assets (6 elements × 9 pieces)
      Cards/             54 CardData assets (one per equipment piece)
      Libraries/         NatureLibrary, EquipmentLibrary (lookup SOs)
    Resources/           Keep minimal — only for Resources.Load() calls
    StreamingAssets/     Platform-specific data files
  Plugins/               Third-party native plugins
  Editor/                Editor-only scripts (build tools, asset processors)
  Tests/
    EditMode/
    PlayMode/
```

## C# Conventions

**Namespaces** — `Capybrawlers.<System>`:
```csharp
namespace Capybrawlers.Battle   { ... }
namespace Capybrawlers.Cards    { ... }
namespace Capybrawlers.Core     { ... }
namespace Capybrawlers.Creatures { ... }
```

**Naming**:
- Classes: `PascalCase`
- `ScriptableObject` data: suffix `Data` or `Library` — `NatureData`, `EquipmentData`, `NatureLibrary`
- MonoBehaviours: suffix `Controller`, `View`, or `Handler` by role
- Interfaces: `I` prefix — `ICardEffect`, `ISaveProvider`
- Private fields: `_camelCase`
- Events: `OnEventName` using `System.Action` or `UnityEvent`
- Coroutines: `Co_` prefix — `Co_PlayCard()`, `Co_ResolutionPhase()`

**Rules**:
- No `GameObject.Find()` at runtime — use inspector references or `ServiceLocator`
- No `Update()` polling where events/callbacks work instead
- `ScriptableObject` assets are read-only at runtime — never mutate SO data; wrap in a runtime class (`CapybrawlerInstance` wraps `CapybrawlerBuildRecord`)
- One `.asmdef` per major subsystem to enforce dependency boundaries and reduce recompile times

## Key Game Systems

### Capybrawler Build System
A Capybrawler's build is fixed at summoning and never changes. It consists of one `NatureData` and three `EquipmentData` (Helm, Armor, Weapon). Final stats are computed by summing Nature base stats with all three equipment modifiers.

- `NatureData` (ScriptableObject): `natureType`, `baseHP/ATK/DEF/SPD`, role description
- `EquipmentData` (ScriptableObject): `id`, `slot`, `element`, `hpMod/atkMod/defMod/spdMod`, `grantedCard`
- `CapybrawlerBuildRecord` (serializable struct): `buildId`, `nature` (enum), `helmId`, `armorId`, `weaponId`
- `BuildRegistry`: resolves a `CapybrawlerBuildRecord` to computed stats + card pool by looking up SOs from `EquipmentLibrary`

### Battle System
Turn structure: **Start of Turn → Draw Phase → Queue Phase (simultaneous) → Resolution → Terminal Rally → Cleanup**

- `BattleManager`: owns `BattleTeam[2]` + `TurnStateMachine`; created per battle, destroyed after
- `TurnStateMachine`: drives the 6-phase turn loop; state transitions via `Advance()`
- `SPDResolver`: collects all queued `(CapybrawlerInstance, CardData)` pairs from both teams, sorts by `CurrentSPD` descending; tiebreaker = NatureType ordinal (Storm wins, Rock loses)
- `EffectProcessor`: dispatches each `CardEffect.Execute(EffectContext)` in resolution order
- `TerminalRallyHandler`: when a brawler reaches 0 HP, fires all its queued actions before removal

### Stamina System
Stamina is a shared team resource — all three brawlers spend from the same pool.

- `StaminaPool`: `Current`, `Max`, `RegenPerTurn` (values TBD — see Open Design Questions in GDD); `TrySpend()`, `Steal(from)`, `Add()`
- Stamina is **always hidden from the opponent** — `StaminaChangedEvent` is published to own team's UI only
- Cards that cost 0/1/2 Stamina — no card outside this range

### Card Pool / Hand System
Each team has a fixed 18-card pool (9 unique × 2 copies). Cards are never shuffled into a custom deck.

- `CardPool`: 18 `CardPoolEntry` slots; `GetEligible(hand)` filters out cards currently in hand or on cooldown; `TickCooldowns()` decrements counters at Start of Turn
- `Hand`: draws 3 eligible cards per turn; unplayed cards return to pool at Cleanup
- Cooldown: after a card is played it enters a **2-turn cooldown** before returning to the pool

### Collection / Summoning System
Players own `CapybrawlerBuildRecord` entries stored in `PlayerProfile`. The summoning method (gacha, crafting, earned) is an open design question — see GDD section 15. For now, a dev-time seeder provides starter builds on first run.

- `BuildRegistry`: resolves build records to computed stats using the `EquipmentLibrary` SO
- `CollectionController`: loads owned builds, displays them, enables formation selection
- `FormationController`: three slots (Frontline, BacklineLeft, BacklineRight); validates team, saves to profile

## Scene Architecture

- `Bootstrap.unity` is always Build Settings index 0. It instantiates `GameManager` (DontDestroyOnLoad), registers all core services, then additively loads `MainMenu.unity`.
- Never reference scenes by string literal — use `SceneId` enum + `SceneLoader` utility.
- `Battle.unity` handles all battle modes, including Battle Arena random-team assignment.
- All scenes except Bootstrap may be loaded/unloaded additively.

## Build Instructions

Replace `<version>` with the installed Unity Editor version (e.g., `6000.0.26f1`).

**Android (from Windows):**
```powershell
& "C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "c:\Source\repos\Cappybrawlers\Cappybrawlers" `
  -buildTarget Android `
  -executeMethod Capybrawlers.Editor.BuildPipeline.BuildAndroid `
  -logFile build_android.log
```

**iOS (requires macOS + Xcode):**
```bash
/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath "/path/to/Cappybrawlers" \
  -buildTarget iOS \
  -executeMethod Capybrawlers.Editor.BuildPipeline.BuildIOS \
  -logFile build_ios.log
```

The `BuildPipeline` editor script lives at `Assets/Editor/BuildPipeline.cs` and uses `UnityEditor.BuildPipeline.BuildPlayer()`.

## First-Open Unity Checklist

After creating the project in Unity Hub (2D URP template), do these **before writing any code**:

1. Edit > Project Settings > Editor > Asset Serialization Mode → **Force Text**
2. Edit > Project Settings > Player → set Company Name (`Capybrawlers`) and Product Name (`Capybrawlers`)
3. Edit > Project Settings > Player > iOS → set Bundle Identifier (`com.capybrawlers.game`)
4. Edit > Project Settings > Player > Android → set same Bundle Identifier
5. Install all UPM packages listed above
6. Create the `Assets/_Game/` folder hierarchy
7. Commit everything as the initial Unity baseline commit

## See Also

- [docs/GDD.md](docs/GDD.md) — Full PRD: Natures, equipment, card library, battle rules, ranking
- [docs/SYSTEMS.md](docs/SYSTEMS.md) — Data models, state machine, EventBus catalog, assembly definitions
- [docs/ART_GUIDE.md](docs/ART_GUIDE.md) — Sprite specs, animation states, atlas strategy, palette
- [docs/BUILD_PROFILES.md](docs/BUILD_PROFILES.md) — iOS/Android build config, keystore, CI
