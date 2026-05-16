# Capybrawlers — Art Direction Guide

## Target Display

| Property | Value |
|---|---|
| Reference resolution | 1080 × 1920 (portrait) |
| Canvas Scaler mode | Scale with Screen Size, Match Width or Height = 0.5 |
| Safe zone | 80px top (notch/Dynamic Island), 60px bottom (home bar) |
| Pixels Per Unit (PPU) | 100 (Unity default) |
| Texture compression | ASTC (iOS + Android) |

## Visual Style

Capybrawlers uses a **clean 2D cartoon style** — bold outlines, saturated colors, soft shading. Think mobile-friendly: legible at small sizes, high contrast, minimal fine detail that compresses poorly.

- Outlines: 3–5px at full resolution
- Color palette: saturated primaries with desaturated backgrounds so creatures pop
- Capybara proportions: chunky/chibi — large head, short limbs, expressive eyes

## Sprite Specifications

### Creatures (Battle Sprites)

Base HP ranges from ~40 (Flame/Storm without Plant/Water equipment) to ~140 (Plant Nature with Plant equipment). Keep sprite silhouettes visually distinct per Nature regardless of equipment.

| Property | Value |
|---|---|
| Canvas size | 512 × 512 px |
| Art area | 400 × 400 px centered (leave 56px border for VFX/outline room) |
| Pivot | Bottom-center (0.5, 0.0) |
| Format | PNG (transparent background) |
| PPU | 100 |

### Cards

| Property | Value |
|---|---|
| Full card size | 420 × 600 px |
| Artwork area | 380 × 280 px (top portion of card) |
| Card frame | Provided as separate sprite layer per rarity |
| Card icon (hand/small) | 140 × 200 px |
| Pivot | Center (0.5, 0.5) |
| Format | PNG |

### UI Elements

| Element | Size | Notes |
|---|---|---|
| Buttons (primary) | 420 × 120 px | 9-slice sprite |
| BrawlPoints / rank badge | 120 × 120 px | |
| HP bar fill | 1 × 20 px (tiling) | |
| Stamina pip | 32 × 32 px | One pip per Stamina point |
| Element icons | 80 × 80 px | One per element (6 total: Flame, Storm, Plant, Rock, Water, Moon) |
| Nature icon | 80 × 80 px | One per Nature (same 6) |
| Cooldown overlay | full card size | Semi-transparent overlay on cooldown cards |

## Sprite Atlas Strategy

Create one **Sprite Atlas** per major usage context. Never mix UI and world sprites in the same atlas.

| Atlas Name | Contents |
|---|---|
| `Atlas_BattleUI` | HP bars, Stamina pips, cooldown overlays, turn indicators, battle HUD |
| `Atlas_Cards` | All 54 card artworks |
| `Atlas_CardFrames` | Card frame overlays per element (6 frames) |
| `Atlas_CollectionUI` | Collection screen, formation selection UI |
| `Atlas_MainMenuUI` | Main menu, rank badges, lobby UI |
| `Atlas_Creatures_<NatureName>` | All creature sprites grouped by Nature (6 atlases) |
| `Atlas_Elements` | 6 element icons + 6 Nature icons (shared) |

All atlases use **ASTC 6x6** compression for the build. Keep each atlas under 2048 × 2048 px.

## Animation States (Per Creature)

Each creature requires these animator states. Use Unity's Animator with blend trees where appropriate.

| State | Trigger | Notes |
|---|---|---|
| `Idle` | Default | Looping gentle bob animation |
| `Attack` | `triggerAttack` | Plays once, returns to Idle |
| `Hit` | `triggerHit` | Brief flinch, returns to Idle |
| `Death` | `triggerDeath` | Plays once; creature fades out at end |
| `KnockOut` | `triggerKnockOut` | Creature slumps, stays in final frame |
| `Victory` | `triggerVictory` | Plays at battle win screen |
| `CardEffect` | `triggerCardEffect` | Optional glow/charge-up for Power cards |

Animator states should transition back to `Idle` using **Exit Time = false** with immediate transitions for Hit and Attack.

## Card Frame Templates

Provide one card frame variant per element (6 frames — no rarity tiers, all cards are equal rarity). Frames are composited in UI via Image layers:

```
[Background gradient]  ← element-tinted color fill
[Artwork]              ← card effect illustration
[Frame overlay]        ← element-specific border PNG
[Text area]            ← card name, Stamina cost, description (TextMeshPro)
[Element icon]         ← element symbol in corner
```

Card frame PNG must be **9-sliced** with 20px corner insets to allow scaling.

## Color Palette

### Elements / Natures
Both element and Nature share the same color identity — equipment and creature share a visual language.

| Element / Nature | Primary | Accent |
|---|---|---|
| Flame | `#E84A1A` | `#FFAA44` |
| Storm | `#F5C518` | `#FFF176` |
| Plant | `#38A832` | `#AAFFCC` |
| Rock | `#707070` | `#B0B0B0` |
| Water | `#1A6FE8` | `#55CCFF` |
| Moon | `#9B3FC8` | `#DDAAFF` |

### Rank Tiers
| Rank | Color |
|---|---|
| Pup | `#9E9E9E` |
| Sprout | `#38A832` |
| Brawler | `#2979FF` |
| Stone Fang | `#8B6914` |
| Tempest | `#4A90E2` |
| Grand Capy | `#FFD600` |

### UI Neutrals
- Background dark: `#1A1A2E`
- Panel: `#16213E`
- Text primary: `#FFFFFF`
- Text secondary: `#B0BEC5`
- Positive: `#00E676`
- Negative: `#FF1744`
- Stamina pip active: `#FFE066`
- Stamina pip empty: `#444444`

### Wood / Forest UI Theme
All battle UI panels use a warm wood family. Element-tinted cards and brawler panels sit on top of this base.

| Role | Hex |
|---|---|
| Panel bg (dark wood) | `#3B2310` |
| Panel bg (mid wood)  | `#5C3A1E` |
| Button (bark)        | `#7B4A20` |
| Border / frame       | `#C8902A` |
| Scene background     | `#1A3A1A` |
| Info panel parchment | `#F5E8C8` |
| Parchment text (ink) | `#2A1A0A` |

## VFX Guidelines

- Attack hit effects: sprite sheet animation, ~12 frames at 24fps
- Card play sparkle: particle system, lifetime < 0.5s
- Creature death: dissolve shader + particle burst
- Stamina spend: small floating number (yellow, fades up over 0.5s)

All VFX should complete in **≤ 1.5 seconds** to keep battle pace snappy on mobile.

## Font Usage

All text uses **TextMeshPro** (no legacy Unity Text components).

| Use Case | Font | Style |
|---|---|---|
| Card names | (TBD — rounded display font) | Bold |
| Card descriptions | (TBD — clean sans-serif) | Regular |
| HUD numbers (HP, Stamina) | (TBD — numeric-optimized) | Bold |
| Buttons | Same as card names | Bold |
| Flavor text | Same as card descriptions | Italic |

Confirm font licenses allow mobile distribution before importing.
