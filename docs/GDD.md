# Capybrawlers — Game Design Document

**Version 1.0 | Status: Design Phase**

---

## 1. Product Overview

Capybrawlers is a mobile tactical creature battler where players called **Overlords** collect permanently generated creatures called **Capybrawlers** and compete in simultaneous-turn-based PvP combat. Victory is determined by prediction, team composition, timing, and resource management — not grinding, rarity, or stat inflation.

Every Capybrawler is defined entirely by its Nature and three equipment pieces locked at the moment of summoning. Nothing about a Capybrawler changes after it is created. Power comes from strategic combination, not progression.

---

## 2. Core Design Pillars

| Pillar | Description |
|---|---|
| **Strategy over grinding** | Combat advantage comes from composition and play, not from account level or upgrade investment. |
| **Permanent identity** | Every Capybrawler's build is fixed forever at summoning. No upgrades, gear swaps, or evolution. |
| **Readable builds** | All Capybrawler stats and card pools are visible to both players from the start of a match. |
| **Tactical prediction** | Simultaneous turns mean both players commit blindly to actions. Reading the opponent is the primary skill ceiling. |
| **Fair exchanges** | Terminal Rally ensures no brawler's turn is simply deleted by a faster kill. Every queued action completes. |
| **No rarity power scaling** | All cards are equal in rarity. Balance comes from synergy and timing, not from which cards are harder to obtain. |

---

## 3. Overlord System

Each player is an Overlord. Account levels exist for access and cosmetics only — they provide zero combat advantage.

**Account level unlocks:**
- Ranked mode access
- Cosmetic skins and animations
- Additional game modes
- Trading
- Crafting
- Tournament entry
- UI features

A level 1 Overlord and a level 100 Overlord enter a ranked match on identical statistical footing.

---

## 4. Capybrawler System

### 4.1 Structure

Each Capybrawler consists of four permanent components:

| Component | Purpose |
|---|---|
| **Nature** | Determines base stats and combat identity |
| **Helm** | Applies stat modifier, grants 1 ability card |
| **Armor** | Applies stat modifier, grants 1 ability card |
| **Weapon** | Applies stat modifier, grants 1 ability card |

There is no leveling, upgrading, evolving, or gear swapping after summoning.

---

### 4.2 Natures

Six Natures define each Capybrawler's base stats and combat role.

| Nature | HP | ATK | DEF | SPD | Role |
|---|---|---|---|---|---|
| Flame | 40 | 30 | 10 | 20 | Burst DPS |
| Storm | 40 | 20 | 10 | 30 | Speed Assassin |
| Plant | 100 | 10 | 20 | 20 | Sustain Tank |
| Rock | 80 | 20 | 30 | 10 | Control Tank |
| Water | 60 | 20 | 15 | 15 | Debuff Support |
| Moon | 60 | 15 | 20 | 15 | Defensive Support |

**Nature combat identities:**

- **Flame** — Hits hardest early. Low HP. Lives and dies by landing kills before being outlasted.
- **Storm** — Fastest Nature. Fragile. Controls tempo by acting before everything else on the field.
- **Plant** — Highest HP in the game. Lowest ATK. Built to absorb damage and sustain through attrition.
- **Rock** — Highest DEF. Slowest Nature. Designed to lock down the field and absorb hits that would destroy lighter brawlers.
- **Water** — Utility support. Brings SPD reduction and debuffs rather than raw damage.
- **Moon** — Defensive backbone. Built around healing, Stamina economy, and buffing teammates.

---

### 4.3 Equipment

Every Capybrawler carries three equipment pieces. Each belongs to one of six elements, applies a fixed standardized stat modifier, and grants exactly one ability card.

**Equipment stat modifiers:**

| Element | Stat Change |
|---|---|
| Flame | +10 ATK / −10 HP |
| Storm | +10 SPD / −10 HP |
| Plant | +20 HP / −5 ATK |
| Rock | +10 DEF / −5 SPD |
| Water | +20 HP / −5 SPD |
| Moon | +10 DEF / −5 ATK |

Modifiers stack directly onto Nature base stats. Gaining ATK costs HP. Gaining DEF costs SPD. No free upgrades exist.

**Example build — Storm Assassin:**

| Component | Equipment | Modifier | Card |
|---|---|---|---|
| Nature | Storm | HP 40 / ATK 20 / DEF 10 / SPD 30 | — |
| Helm | Seed Helm (Plant) | +20 HP / −5 ATK | Overbite — 1 Stamina, 20 damage |
| Armor | Zeus Cloak (Storm) | +10 SPD / −10 HP | Thunderclap — 2 Stamina, 30 damage |
| Weapon | Dagger of Maelstrom (Storm) | +10 SPD / −10 HP | Eye of the Storm — 2 Stamina, 30 damage, +10 SPD next turn |
| **Final** | — | **HP 40 / ATK 15 / DEF 10 / SPD 50** | — |

---

## 5. Battlefield Formation

Each team has three formation slots:

| Slot | Count | Typical Role |
|---|---|---|
| **Frontline** | 1 | Tank — absorbs early pressure |
| **Backline** | 2 | Damage and support — protected until needed |

Standard composition places Plant or Rock in frontline, Flame or Storm and a support in backline. Mismatching roles is a legitimate strategic error with real consequences.

---

## 6. Card System

### 6.1 Pool Structure

Each Capybrawler has three equipment pieces, each granting one unique ability card. Three brawlers yield nine unique cards. Every card exists as **two copies** in the pool, making the total pool **18 cards** per team.

The full card pool is public information. Both players can see every card available to the opponent from the moment brawlers are revealed.

### 6.2 Drawing

At the start of each turn, **3 cards are drawn** from the available pool.

**Card eligibility states:**

| State | Eligible to Draw |
|---|---|
| In pool | ✅ Yes |
| Currently in hand | ❌ No |
| On cooldown | ❌ No |

Cards in hand are ineligible to prevent drawing duplicates of already-held cards. Both copies of the same card can exist in hand simultaneously if drawn across different turns.

### 6.3 Playing Cards

Cards are played during the queue phase by spending Stamina. All cards cost **0, 1, or 2 Stamina**. Unplayed hand cards return to the available pool at end of turn.

### 6.4 Cooldown

Once played, a card enters a **2-turn cooldown** — it cannot be drawn or played for the next two turns, returning to the pool at the start of the third turn after use.

**Example:**
- Card played on Turn 3
- Unavailable Turn 4 and Turn 5
- Returns to pool at Turn 6

### 6.5 Double-Copy Plays

Both copies of a card are independent. Three play patterns exist:

| Pattern | Description | Trade-off |
|---|---|---|
| **Single play** | Use one copy, keep the other cycling | Consistent pressure, no drought |
| **Staggered play** | Play copy A this turn, copy B next turn | Sustained pressure, offset cooldowns |
| **Double play** | Both copies played in the same turn | Maximum burst, both copies on cooldown simultaneously for 2 turns |

Playing both copies in the same turn is a legitimate and powerful strategy. The opponent knows the card is completely unavailable for two full turns after seeing both copies resolve.

---

## 7. Hidden Information

| Information | Visibility |
|---|---|
| Both teams' full Capybrawler builds | ✅ Always public |
| Full card pool for each team | ✅ Always public |
| Which cards have been played previously | ✅ Trackable |
| SPD values of all brawlers | ✅ Always public |
| Current hand contents | ❌ Hidden until played |
| Current Stamina pool | ❌ Hidden until actions resolve |
| Queued actions | ❌ Hidden until resolution |

**Deducibility:** A sharp player can narrow down the opponent's possible hand by tracking cooldowns and eliminating ineligible cards. Stamina can be estimated by watching how many cards are played each turn and at what costs. Neither is ever fully known — only approximated.

**Terminal Rally reveals:** When a Capybrawler dies and fires its Terminal Rally, its queued cards are revealed for the first time. Cards held but never played die with the brawler and are never seen.

---

## 8. Stamina System

Stamina is a single **shared team resource**. All three Capybrawlers draw from the same pool when playing cards.

**Properties:**
- Shared across the entire team — not per brawler
- Regenerates at the start of each turn
- Hidden from the opponent at all times
- Card costs are always 0, 1, or 2 Stamina

**Stamina economy cards** exist within two elements and can turn the resource itself into a combat weapon:

- **Stamina Regen** (Moon, Water) — Restore Stamina to own pool mid-turn outside normal regeneration
- **Stamina Steal** (Rock, Plant) — Remove Stamina from the opponent's pool and add it to yours

Since Stamina is hidden, steal cards require reading the opponent's pool state. Playing a steal card against an empty pool gains nothing.

---

## 9. Mechanics Glossary

All card effects are built from the following defined mechanics. Each mechanic belongs to specific elements only — cards never use a mechanic outside their element's assigned list.

| Mechanic | Assigned To | Description |
|---|---|---|
| **Burn** | Flame | Target takes set damage at start of their turn. Stacks up to 3. Each stack = 5 damage per turn. Duration 2–3 turns. |
| **Rage** | Flame | This brawler gains bonus ATK next turn only. |
| **Execute** | Flame | Deals bonus damage when target is below an HP threshold. |
| **Pierce** | Flame, Storm | Damage bypasses active shields completely. |
| **Berserker** | Flame, Rock | Spend own HP to deal amplified damage. |
| **Haste** | Storm, Moon | Increase own SPD temporarily. |
| **Slow** | Storm, Water | Reduce one enemy's SPD temporarily. |
| **Stun** | Storm | Target's next queued card is not played this turn. |
| **Full Stun** | Rock, Water | Target cannot play any cards next turn. |
| **Reflect** | Rock | Half of all damage received this turn bounces back to the attacker at end of turn. |
| **Shield** | Plant, Rock | Absorbs incoming damage until broken. |
| **Stamina Steal** | Plant, Rock | Take Stamina from opponent's pool. |
| **Poison** | Plant | Damage increases over time per stack. Each stack: 2 / 4 / 6 damage per turn over 3 turns. Stacks up to 3. |
| **Lifesteal** | Plant, Moon | Damage dealt also heals the attacker. |
| **Heal** | Moon, Water | Restore HP to the teammate with the most damage taken. |
| **Purge** | Moon | Remove all active buffs from one enemy. |
| **Stamina Regen** | Moon, Water | Restore Stamina to own pool. |
| **Cleanse** | Water | Remove all active debuffs from self. |

---

## 10. Battle System

### 10.1 Turn Structure

Each full turn follows six phases in order:

1. **Start of Turn** — Stamina regenerates for both teams. Cooldown timers tick down by one for all cards on cooldown.
2. **Draw Phase** — Each player draws 3 cards from their available pool.
3. **Queue Phase** — Both players simultaneously and secretly assign cards to brawlers and commit Stamina. Neither player sees the other's choices.
4. **Resolution Phase** — All queued actions fire in SPD order, highest to lowest.
5. **Terminal Rally** — Any Capybrawler that died during resolution completes all queued actions before being removed.
6. **Cleanup** — Fallen brawlers are removed. Unplayed hand cards return to the pool. Turn ends.

---

### 10.2 SPD Resolution

Actions resolve from highest SPD to lowest. A faster brawler acts before a slower one — it can land kills before heals fire, pre-empt buffs, or disrupt setups.

**Speed ties — tiebreaker hierarchy:**

When two brawlers share the same SPD value, resolution order follows:

| Priority | Nature |
|---|---|
| 1st | Storm |
| 2nd | Flame |
| 3rd | Water |
| 4th | Moon |
| 5th | Plant |
| 6th | Rock |

Storm wins every speed tie. Rock loses every speed tie. The hierarchy reflects each Nature's combat identity.

---

### 10.3 Terminal Rally

When a Capybrawler's HP reaches zero during resolution, it does not disappear immediately. All actions it queued this turn complete first. The brawler is removed only after every queued action resolves.

**Purpose:**
- Prevents high-SPD units from deleting targets and voiding their entire turn
- Guarantees every brawler contributes regardless of when it falls
- Creates dramatic final moments and enables intentional sacrifice plays

**Key ruling:** If a brawler dies with no Stamina remaining, it queues no actions and fires no Terminal Rally regardless of cards in hand.

---

## 11. Equipment & Card Library

### 11.1 Card Design Rules

- Every card at the same Stamina tier carries equivalent total value — but what that value does is entirely different across elements
- No card is a numerical upgrade of another within the same element
- All damage cards target a single enemy only — no multi-target damage
- Each element's cards use only mechanics assigned to that element

---

### 11.2 Flame — Full Card Set
**Stat Modifier: +10 ATK / −10 HP**
**Available mechanics: Burn, Rage, Execute, Pierce, Berserker**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Ashen Visor | Helm | 0 | **Kindle** | Deal 8 damage. Apply 1 Burn stack — 5 dmg/turn for 2 turns. |
| Ember Crown | Helm | 1 | **Backdraft** | Deal 10 damage per active Burn stack on target (10 / 20 / 30). |
| Inferno Crest | Helm | 2 | **Scorch Mark** | Apply 2 Burn stacks — 10 dmg/turn for 3 turns. No direct damage. |
| Char Wrap | Armor | 0 | **Ignite** | Rage — this brawler gains +8 ATK next turn. |
| Blaze Mantle | Armor | 1 | **Berserker** | Lose 15 HP. Deal 35 damage. |
| Pyroclast Plate | Armor | 2 | **Frenzy** | Rage — +20 ATK next turn. Lose 10 HP. |
| Cinder Shard | Weapon | 0 | **Flicker** | Deal 12 damage. |
| Scorch Blade | Weapon | 1 | **Execute** | Deal 15 damage. If target has ≤30 HP, deal 40 instead. |
| Blazing Greatsword | Weapon | 2 | **Inferno** | Deal 40 damage. Pierce — ignores all shields. |

---

### 11.3 Storm — Full Card Set
**Stat Modifier: +10 SPD / −10 HP**
**Available mechanics: Haste, Slow, Stun, Pierce**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Gust Hood | Helm | 0 | **Tailwind** | Haste — +8 SPD this turn only. |
| Cyclone Visor | Helm | 1 | **Slipstream** | Haste — +12 SPD for 2 turns. If faster than target, also deal 15 damage. |
| Tempest Crown | Helm | 2 | **Eye of the Gale** | Haste — +20 SPD for 3 turns. |
| Static Wrap | Armor | 0 | **Discharge** | Slow — target −6 SPD for 2 turns. |
| Thunder Plate | Armor | 1 | **Voltage Drop** | Deal 14 damage. Slow — target −10 SPD for 2 turns. |
| Stormbreaker Mantle | Armor | 2 | **Grounded** | Slow — target −18 SPD for 3 turns. Cancels all active Haste on target. |
| Spark Dagger | Weapon | 0 | **Static Jolt** | Deal 12 damage. |
| Lightning Blade | Weapon | 1 | **Stun Bolt** | Deal 16 damage. Stun — target's next queued card is not played. |
| Maelstrom Edge | Weapon | 2 | **Thunderclap** | Deal 35 damage. Pierce — ignores all shields. |

---

### 11.4 Plant — Full Card Set
**Stat Modifier: +20 HP / −5 ATK**
**Available mechanics: Poison, Shield, Lifesteal, Stamina Steal**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Seed Visor | Helm | 0 | **Spore** | Apply 1 Poison stack — 2/4/6 dmg per turn over 3 turns. |
| Petal Crown | Helm | 1 | **Toxic Bloom** | Deal 12 damage. Apply 1 Poison stack. |
| Canopy Crest | Helm | 2 | **Venom Crown** | Apply 2 Poison stacks. Existing Poison stacks tick once immediately. |
| Bark Wrap | Armor | 0 | **Bark** | Shield — absorbs 15 damage. |
| Thorn Plate | Armor | 1 | **Living Bark** | Shield — absorbs 20 damage. Stamina Steal — take 1 Stamina from opponent. |
| Ancient Bark Mantle | Armor | 2 | **Bark Fortress** | Shield — absorbs 35 damage. If broken, attacker receives 1 Poison stack. |
| Root Spike | Weapon | 0 | **Vine Lash** | Deal 10 damage. Lifesteal — heal self 5 HP. |
| Briar Whip | Weapon | 1 | **Thorn Whip** | Deal 14 damage. Stamina Steal — take 1 Stamina from opponent. |
| Thornwall Staff | Weapon | 2 | **Entangle** | Deal 20 damage. Lifesteal — heal self 10 HP. Stamina Steal — take 1 Stamina from opponent. |

---

### 11.5 Rock — Full Card Set
**Stat Modifier: +10 DEF / −5 SPD**
**Available mechanics: Shield, Reflect, Full Stun, Stamina Steal, Berserker**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Pebble Visor | Helm | 0 | **Stone Hide** | Reflect — half of all damage received this turn bounces back to attacker at end of turn. |
| Granite Crown | Helm | 1 | **Grit** | Berserker — lose 12 HP. Deal 30 damage. |
| Fortress Crest | Helm | 2 | **Bulwark** | Reflect — half damage reflects back. +10 DEF for 2 turns. |
| Rubble Wrap | Armor | 0 | **Rubble** | Shield — absorbs 15 damage. |
| Boulder Plate | Armor | 1 | **Fortify** | Shield — absorbs 20 damage. +5 DEF for 2 turns. |
| Bastion Mantle | Armor | 2 | **Seismic Crush** | Deal 25 damage. Full Stun — target cannot play any cards next turn. |
| Gravel Spike | Weapon | 0 | **Rock Throw** | Deal 12 damage. |
| Hammer Fist | Weapon | 1 | **Crusher** | Deal 20 damage. Stamina Steal — take 1 Stamina from opponent. |
| Earthbreaker | Weapon | 2 | **Demolish** | Berserker — lose 15 HP. Deal 45 damage. |

---

### 11.6 Water — Full Card Set
**Stat Modifier: +20 HP / −5 SPD**
**Available mechanics: Heal, Slow, Full Stun, Cleanse, Stamina Regen**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Mist Visor | Helm | 0 | **Douse** | Slow — target −8 SPD for 2 turns. |
| Current Crown | Helm | 1 | **Undertow** | Deal 14 damage. Slow — target −12 SPD for 2 turns. |
| Vortex Crest | Helm | 2 | **Whirlpool** | Slow — target −22 SPD for 3 turns. Cancels active Haste on target. |
| Ripple Wrap | Armor | 0 | **Cool Stream** | Stamina Regen — regenerate 1 Stamina. |
| Tide Plate | Armor | 1 | **Healing Stream** | Heal — restore 20 HP to the most damaged teammate. |
| Whirlpool Mantle | Armor | 2 | **Cleansing Wave** | Cleanse — remove all own debuffs. Heal — restore 15 HP to most damaged teammate. |
| Drip Blade | Weapon | 0 | **Splash** | Deal 10 damage. |
| Current Blade | Weapon | 1 | **Torrent** | Deal 20 damage. Stamina Regen — regenerate 1 Stamina. |
| Maelstrom Trident | Weapon | 2 | **Tidal Crash** | Deal 30 damage. Full Stun — target cannot play any cards next turn. |

---

### 11.7 Moon — Full Card Set
**Stat Modifier: +10 DEF / −5 ATK**
**Available mechanics: Heal, Haste, Purge, Lifesteal, Stamina Regen**

| Equipment | Type | Cost | Card | Effect |
|---|---|---|---|---|
| Crescent Visor | Helm | 0 | **Lunar Draw** | Stamina Regen — regenerate 1 Stamina. |
| Lunar Crown | Helm | 1 | **Moonrise** | Stamina Regen — regenerate 2 Stamina. Haste — +5 SPD this turn. |
| Eclipse Crest | Helm | 2 | **Full Moon** | Stamina Regen — regenerate 3 Stamina. Heal — restore 15 HP to most damaged teammate. |
| Starlight Wrap | Armor | 0 | **Star Veil** | Heal — restore 12 HP to the most damaged teammate. |
| Moonveil Plate | Armor | 1 | **Purging Light** | Purge — remove all active buffs from one enemy. Deal 10 damage. |
| Celestial Mantle | Armor | 2 | **Lunar Restoration** | Heal — restore 30 HP to most damaged teammate. Haste — that teammate gains +8 SPD next turn. |
| Moonbeam Rod | Weapon | 0 | **Crescent Strike** | Deal 10 damage. Lifesteal — heal self 5 HP. |
| Stellar Lance | Weapon | 1 | **Lunar Lance** | Deal 20 damage. Haste — +8 SPD this turn. |
| Astral Spear | Weapon | 2 | **Eclipse** | Deal 28 damage. Purge — remove all buffs from target. Lifesteal — heal self 14 HP. |

---

## 12. BrawlPoints and Ranking

The ranked ladder uses **BrawlPoints (BP)** as the sole progression metric.

**Point change per match:**

| Result | BP Change |
|---|---|
| Win | +25 BP |
| Loss | −15 BP |

**Rank tiers:**

| Rank | BP Range |
|---|---|
| Pup | 0 – 499 |
| Sprout | 500 – 999 |
| Brawler | 1000 – 1999 |
| Stone Fang | 2000 – 2999 |
| Tempest | 3000 – 3999 |
| Grand Capy | 4000+ |

Seasons reset the ladder periodically. End-of-season rank determines cosmetic rewards. BrawlPoints cannot be purchased — they are earned exclusively through match results.

---

## 13. Battle Arena — Mode Specification

### 13.1 Team Composition Rule

Each player is randomly assigned one Capybrawler from each of the following role pairs at the start of every battle:

| Slot | Pool | Role |
|---|---|---|
| Slot 1 | Plant or Rock | Tank |
| Slot 2 | Moon or Water | Support |
| Slot 3 | Flame or Storm | Striker |

Which specific Nature fills each slot is random. Both copies of each card in the team's pool are assigned random skills from the available equipment card library. Players may re-roll their team before confirming the match.

### 13.2 No Leveling

Capybrawlers in Battle Arena mode do not level up. Stats are entirely determined by Nature and equipment. Every player enters on equal footing regardless of account age or session history.

### 13.3 Match Flow

1. Both players roll team compositions
2. Brawler builds are revealed — full card pools become public
3. Formation is set — one frontline, two backline
4. Battle begins — simultaneous turns until one team has all three brawlers eliminated
5. BrawlPoints are awarded or deducted
6. Post-match result screen displays current rank and tier progress

---

## 14. Design Constraints Summary

| Constraint | Rule |
|---|---|
| No multi-target damage | All damage cards target one enemy only |
| No rarity power scaling | All cards are equal rarity — balance through synergy |
| No leveling Capybrawlers | Stats never change after summoning |
| No equipment swapping | Build is permanent from summoning |
| No P2W mechanics | BrawlPoints cannot be purchased |
| Card costs | Always 0, 1, or 2 Stamina — nothing outside this range |
| Mechanics per element | Each mechanic belongs to specific elements and cannot appear outside them |
| Pool size | Always 18 cards (9 unique × 2 copies) |
| Hand size | Always 3 cards drawn per turn |
| Cooldown duration | Always 2 turns after a card is played |
| Stamina visibility | Always hidden from opponent |
| Hand visibility | Hidden until played during resolution |

---

## 15. Open Design Questions

The following items require further specification before implementation:

| Item | Question |
|---|---|
| Stamina pool size | What is the starting value and per-turn regen amount? |
| Stamina cap | Is there a maximum Stamina the pool can hold? |
| Burn and Poison interaction | If a brawler dies during a DoT tick, does the tick still resolve? |
| Full Stun + Terminal Rally | If a brawler is Full Stunned and then dies, does Terminal Rally still fire? |
| Summoning cadence | How are new Capybrawlers acquired — gacha, crafting, direct purchase, or earned through play? |
| Win condition edge cases | What happens if both teams lose their last brawler in the same Terminal Rally? |
| Minimum SPD floor | Is there a minimum SPD value a brawler can be reduced to, or can it reach zero or below? |
| Formation switching | Can formation be changed mid-match or only before the battle begins? |
| Ranked season length | How long does each ranked season run before the ladder resets? |
