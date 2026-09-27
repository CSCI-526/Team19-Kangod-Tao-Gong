# Team contribution record

Updated: 2026-09-26

## Version boundary

- Team baseline: `jassu75/Money-Heist` main, commit `e6775599da3e3c1360b4e1b43a393cfb9ee330f5`.
- Local baseline snapshot: `b5001c8`.
- Current local branch: `codex/pickup-effects-on-team-main`.
- The team baseline remains recoverable as the parent commit; no remote branch was changed.

## Team baseline work

The team version supplies the endless road, automatic forward driving, steering input, obstacle generation, chase cars, proximity meter, distance score, scene structure, and the original immediate restart behavior after a fatal crash.

## Our work

### New gameplay systems

- Added `MysteryPickup` and `PickupSpawner`.
- Added a hidden 50/50 pickup outcome: full fuel or reversed steering for five seconds.
- Added the local `FuelState` model and fuel drain display.
- Added `RiskRunState` and `RiskRunController` so a crash slows the car, pursuit distance recovers after clean driving, capture stops the run, and **R** restarts it.
- Kept the team `ScoreMeter` unchanged; pickups do not add score in this slice.

### Main-scene integration

- Added `CarPickupEffects` and `RiskRunController` to the player car.
- Updated `AutoDriveCar`, `HandleCrash`, and `ChaseMeter` to use the shared recovery clock and to avoid immediate scene reload on ordinary collisions.
- Added a `PickupSystem` root to `GetawayChase.unity` with the pickup spawner and HUD.

### Player feedback update

- Added a dark panel behind the compact HUD so text stays readable.
- Replaced long status sentences with short English prompts.
- Added a two-second center popup after each pickup:
  - `FUEL FULL!` / `Fuel restored to 100%`
  - `CONTROLS REVERSED!` / `A / Left: move right     D / Right: move left`
- Added `PickupCount` so repeated pickups with the same result can still trigger a new popup.

## Current player-facing prompts

- `Fuel 97/100`
- `Police gap: 45.7 m`
- `Avoid crashes to escape`
- `SLOWED 1.6s - STEER!`
- `RECOVERING - KEEP DRIVING`
- `Controls: normal`
- `REVERSED 5.0s`
- `Last pickup: full fuel`
- `Last pickup: reverse`
- `Pickup: mystery`
- `A/D or Left/Right: steer. Avoid obstacles.`
- `Pickup: 50% FUEL FULL or 50% REVERSE for 5s.`
- Capture popup: `CAUGHT!` / `Press R to restart`

## Verification boundary

- Unity `6000.3.23f1` compiled the changed scripts and entered Play Mode.
- Live smoke check confirmed the readable HUD panel and center capture popup.
- The pickup spawn and crash slowdown were previously confirmed in the same team snapshot.
- Direct collection of both random pickup outcomes and WebGL build remain open; player balance and readability still need team playtest feedback.

## Files owned by our slice

- `Assets/Scripts/Pickups/`
- `Assets/Scripts/PickupIntegration/`
- `Assets/Prefabs/Pickups/MysteryPickup.prefab`
- `Assets/Editor/Pickups/`
- `Assets/Tests/Editor/`
- `Assets/Scenes/PickupPlayground.unity`
- `Assets/Scenes/GetawayChase.unity` integration objects
- `Assets/Scripts/Car/AutoDriveCar.cs`
- `Assets/Scripts/Car/HandleCrash.cs`
- `Assets/Scripts/HUD/Proximity/ChaseMeter.cs`
