# Risk and recovery prototype

## Current reporting slice (2026-09-27)

The current local slice deliberately leaves pickup score out of the gameplay path. It adds one special pickup whose result is hidden until collection and then chosen with a 50/25/25 split:

| Result | Behavior |
| --- | --- |
| Proximity recovery | Adds half of the maximum police-gap capacity, capped at the maximum. It does not add score or start a temporary effect. |
| Reversed controls | Reverses steering for 5 seconds. Fuel is unchanged and no score is added. |
| One-hit shield | Blocks the next collision that would start crash recovery. It is consumed once and cannot be stacked. |

The current main-scene slice uses the local `FuelState` owned by `CarPickupEffects`; it does not add a second score authority or a pickup score bonus. If the team later lands a separate fuel authority, reconcile that interface before merging. The latest team baseline was opened with Unity **6000.3.23f1** and the main scene was smoke-tested in Play Mode: the pickup spawned, the HUD showed the fuel and hidden-result state, and an obstacle contact showed the temporary slowdown instead of an immediate restart. A full random-result collection and WebGL build are still pending.

The `+10` pickup reward, `+75` completion reward, and Space forfeiture rules described in the historical section below are retained only for comparison with the earlier prototype. They are not part of this reporting slice.

Local development branch: `codex/pickup-effects-on-team-main`.
Base: `jassu75/Money-Heist` main at `e6775599da3e3c1360b4e1b43a393cfb9ee330f5`.
Editor: Unity **6000.3.23f1**.

## Historical behavior from the earlier reward prototype

Optional plain cube pickups give 10 points immediately and apply one random temporary effect:

| Effect | Behavior | Duration |
| --- | --- | --- |
| Reversed controls | Left/A moves right; Right/D moves left | 5 seconds |
| Speed boost | Forward speed is multiplied by 1.35 | 5 seconds |
| Slippery steering | Lateral acceleration and deceleration are multiplied by 0.3 | 5 seconds |

Completing the 5-second effect earns another 75 points. Press **Space** to cancel early and forfeit that pending 75; already earned points remain. One effect is active at a time. A new pickup discards the old pending bonus, grants its own 10 points, and starts a new effect and 75-point challenge. This applies to repeated pickups of the same type too. Timers use scaled game time.

Colliding with an obstacle slows the car to 35% speed for 1.6 seconds. Police distance changes continuously using the same forward motion as the car, instead of subtracting a fixed health amount per impact. Collision protection lasts 1 second; contacts with the same placed obstacle are counted once. After impact, gaining distance is held for 2.6 seconds, then clean driving recovers distance gradually. Police travel at 14 m/s, the base car at 15 m/s; the gap starts at 45 m and is capped at 75 m. These are initial tuning values, not playtest findings.

At 0 m the run ends, pending rewards are lost, and movement and score stop. Press **R** to restart after capture. There is no fixed lives counter. The score combines forward road distance and already earned pickup points; sideways weaving does not farm distance points in this prototype. Pursuit is represented by functional text, not a separate police AI.

## Mechanics-only scope

Use only basic geometric placeholders and functional text. Do not add artwork, custom decorative materials, textures, icons, particle effects, decorative animation, music, or sound effects. The pickup builder uses an unanimated default cube and creates no material assets. This restriction also applies to self-made and AI-generated content.

## Try the separate scene

Use **Money Heist > Pickups > Create or Refresh Test Scene**, then play `Assets/Scenes/PickupPlayground.unity`. The builder creates a pickup prefab and a separate copy of the existing game scene. It replaces the inherited skybox with a plain background and disables post-processing and volumes. Effects are random; the first pickup is off-centre so the player can choose to approach it. Obstacle rows are 24–36 m apart, with a 15% chance of a second obstacle in the row. The scene is added to Build Settings for restart.

`GetawayChase.unity` is not edited by this builder. Refreshing the test scene replaces edits made to the generated test scene and prefab.

## Main-scene integration status

The integration is now present in `Assets/Scenes/GetawayChase.unity` on the local branch:

1. `CarPickupEffects` and `RiskRunController` are on the player car beside `AutoDriveCar`.
2. `HandleCrash` and `ChaseMeter` route impacts through the recovery model, so a collision slows the run and capture requires an explicit **R** restart.
3. A separate `PickupSystem` root owns `PickupSpawner` and `PickupHUD`. It references the existing road, player, obstacle spawner, and `MysteryPickup` prefab; pickups remain outside the obstacle-spawner hierarchy.
4. `PickupSpawner` configures the special pickup as a 50% reversed-controls, 25% proximity-recovery, or 25% one-hit-shield result. The team `ScoreMeter` remains unchanged, so this slice adds no pickup score.
5. Spacing and effect duration still need player feedback before the team decides whether to merge the tuning.

`AutoDriveCar` remains the only script that moves the vehicle. `RiskRunState` supplies one integrated forward distance per frame so pursuit and actual movement agree across effect expiry and capture. Without an enabled `CarPickupEffects` component, the original scene keeps its original driving, collision restart and distance-score behavior. Road and obstacle-generation scripts are not changed.

## Validation commands

The source-level check for the current slice passed: `FuelState` consumed and clamped fuel correctly, a seeded 1,000-draw sample produced all three random outcomes, and proximity recovery adds half the maximum gap while clamping at the maximum. Unit coverage also checks that a shield blocks one new collision, is preserved during the built-in collision protection window, and resets with the run. This does not verify Unity scene wiring or actual input/collider behavior.

The risk/recovery version passed **62 state/lifecycle tests and 38 real input/physics checks**, using **6000.3.22f1 in a separate temporary copy**. Checks include cancellation, payout, collision slowdown, distance recovery, capture, stopped movement/score, and R restart. All 23 C# source files matched the tested copy by SHA-256. Those automated checks are historical; the current **6000.3.23f1** main-scene smoke result is recorded above, while a full automated rerun on the team snapshot remains pending. These checks establish behavior, not player enjoyment or balanced tuning.

A Development WebGL build of this risk/recovery version also succeeded. The local output is `Builds/LocalPreview-RiskRun-6000.3.22f1` (ignored by Git), served at `http://127.0.0.1:8766/`. Browser checks confirmed scene/text, continuing after a collision, capture, and R restart through the focused canvas. No console errors/warnings were reported during that check. The full mechanic assertions above were run in the Editor. This is a local preview, not a GitHub Pages deployment; the earlier pickup-only preview is preserved separately.

- EditMode: run assembly `MoneyHeist.Pickups.EditorTests` using Unity Test Runner.
- Scene generation: `-batchmode -executeMethod PickupDemoBuilder.CreateDemo -quit`.
- Input/physics smoke check: `-batchmode -executeMethod PickupValidation.RunSmoke -pickupSmokeReport <absolute-json-path>`; do not add `-quit`, because the runner exits when its asynchronous checks finish.
- WebGL: `-batchmode -executeMethod PickupValidation.BuildWebGL -quit`.

The smoke runner is editor-only and is attached temporarily by the validation command. It is not saved in the game scene or included in player builds. See the local course progress record for actual results; command availability alone does not indicate a pass.
