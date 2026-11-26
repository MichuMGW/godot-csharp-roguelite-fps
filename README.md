# Godot C# Roguelite FPS

A 3D first-person roguelite spellcaster and arena survival game built with **Godot 4.4 (.NET / C#)** and **Jolt Physics** as an engineering thesis project.

Fight escalating waves of enemies in a 3D arena using elemental spell kits, combine elements mid-run to unlock evolved loadouts, collect stat-modifying items and level-up upgrades, and survive until the final boss encounter.

---

## Key Features

- **Data-Driven Elemental Spell System**
  - Four base elemental kits (**Fire**, **Water**, **Air**, **Nature**) and mid-run element combination (**Earth** = Air + Nature, **Magma** = Fire + Nature, with an extensible `ElementCombiner` matrix).
  - Behaviour-based spell architecture supporting **Projectile**, **Beam**, **Area**, **Explosion**, and **Dash** behaviours across five cast modes (`Instant`, `HoldRepeatCooldown`, `Channel`, `ChargeRelease`, `ChargeAuto`).
  - Dual-hand casting (Left Hand / Right Hand), mana management (`PlayerManaComponent`), cooldown reduction scaling, and elemental status effects (`ElementStatusProfile`, `StatusComponent`).
- **Hierarchical & Multi-Region Finite State Machines (FSM)**
  - **Player FSM** split into concurrent state regions (`Super`, `Move`, `PrimaryAction`, `SecondaryAction`) enabling simultaneous movement, dashing, and dual-hand spellcasting.
  - **Enemy AI Archetypes** with dedicated FSMs and progressive time/difficulty unlocks: **Skeleton**, **Troll Archer**, **Orc**, and **Skeleton Summoner**.
  - **Final Boss Encounter** against the **Mech** featuring a multi-region AI (`Super`, `Move`, `Attack` — Gun Burst & Rocket Volley) and a dedicated Boss HP bar.
- **Component-Based Combat**
  - Modular combat components (`HealthComponent`, `PlayerHealthComponent`, `BossHealthComponent`, `HitboxComponent`, `HurtboxComponent`, `KnockbackComponent`).
  - Localized hit detection via `SkeletalHurtboxComponent` supporting `Body`, `Head`, `Limb`, and `WeakSpot` hurtbox zones, plus floating damage numbers.
- **Roguelite Progression & Run Loop**
  - Timed survival match orchestrated by `GameDirector` with dynamic difficulty scaling (`DifficultyManager`, `RunDifficulty`).
  - Experience & gold drops, interactive treasure chests (`ChestManager`) awarding stackable stat-modifying items (`ItemInventory`), and rarity-weighted level-up upgrade offers (`LevelUpManager`).
  - Complete UI flow: main menu, difficulty selection, loading screen, HUD with boss bar and item stacks, element selection overlay, pause menu, and end-game score summary (`EndGameOverlay`, `GameScore`).

---

## Tech Stack

| Category | Technology |
| --- | --- |
| Engine | Godot 4.4 (.NET / C#, Forward+ Renderer) |
| Language & Runtime | C# / .NET 8.0 (`Godot.NET.Sdk/4.4.0`) |
| 3D Physics | Jolt Physics |
| Testing | gdUnit4 (`gdUnit4.api` & `gdUnit4.test.adapter` 5.0.0) |

---

## Architecture Overview

The codebase emphasizes modularity, separation of concerns, and Godot Resource-driven configuration:

- **Event Bus (`GameEvents` autoload)** — Decouples gameplay managers, combat entities, and UI overlays via typed Godot signals.
- **Resource-Driven Content (`resources/`)** — Spells, elemental kits, status profiles, enemy base stats, items, and level-up upgrades are authored as `.tres` Godot Resources, making balancing and content expansion independent of code changes.
- **State Machine Framework (`scripts/states/`)** — Lightweight `IState`, `IUpdateState`, `IPhysicsUpdateState`, and `StateSlot` primitives shared across the player and enemy AI controllers.
- **Spell Controller & Behaviours (`scripts/player/PlayerSpellController.cs`, `scripts/spells/`)** — Factory-registered `ISpellBehaviour` implementations combined with cast-lifecycle interfaces (`IPressSpellBehaviour`, `IHoldSpellBehaviour`, `IReleaseSpellBehaviour`, `ICancelableSpellBehaviour`).

### Project Structure

```text
├── addons/         # Third-party Godot plugins (gdUnit4)
├── Assets/         # 3D models (.glb), animations, textures, VFX, and shaders
├── resources/      # Data-driven Godot Resources (.tres) & C# Resource definitions
│   ├── items/      # Stackable chest item definitions & stat modifiers
│   ├── levelups/   # Level-up upgrade offers and stat definitions
│   ├── spells/     # Spell definitions, elemental kits, and status profiles
│   └── stats/      # Player and enemy base stat resources
├── scenes/         # Godot scenes (.tscn) for entities, spells, environment, and UI
├── scripts/        # C# gameplay, AI, FSM, spell, manager, and UI scripts
│   ├── components/ # Reusable combat, movement, health, hurtbox, and drop components
│   ├── entity/     # Player, Chest, and Enemy archetypes (Mech, Orc, Skeleton, TrollArcher)
│   ├── game/       # GameDirector, GameEvents, GameScore, RunConfig
│   ├── managers/   # Difficulty, Spawn, Experience, Gold, Chest, LevelUp, Stats, UI
│   ├── player/     # Multi-region Player FSM, animations, and PlayerSpellController
│   ├── spells/     # Spell instances, cast sessions, and pluggable spell behaviours
│   └── states/     # Generic FSM interfaces and StateSlot infrastructure
└── shaders/        # Custom Godot shaders
```

---

## Controls

| Action | Input |
| --- | --- |
| Move | `W` `A` `S` `D` |
| Jump | `Space` |
| Cast Left-Hand Spell | `Left Mouse Button` |
| Cast Right-Hand Spell | `Right Mouse Button` |
| Dash / Mobility Skill | `Q` |
| Interact (Open Chests) | `E` |
| Pause / Menu | `Esc` |

---

## Getting Started

### Prerequisites

- **Godot Engine 4.4 (.NET edition)**
- **.NET 8.0 SDK**

### Running the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/MichuMGW/godot-csharp-roguelite-fps.git
   cd godot-csharp-roguelite-fps
   ```
2. Open `project.godot` in **Godot 4.4 (.NET)**.
3. Build the C# solution inside the Godot editor (or via CLI with `dotnet build "3D Shooter - prototyp.sln"`).
4. Press **F5** (Play Project) to launch the main menu.

---

## License

This project was developed as an engineering thesis project. All rights reserved unless stated otherwise.
