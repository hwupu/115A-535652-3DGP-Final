# 4.A.3 Game portals — Henry

Issue: #60 · Demo: Dec 14
Scene: `Scenes/4A3_GamePortals.unity` · Guide: `docs/mini-exercises.md`
Setup: **Magic > Mini-Exercises > Henry 4.A.3: Set Up Scene** (creates the hub, 3 areas, 6 portals, the switch, the hamster, particles, fader and HUD if missing).

The hamster starts in a walled **Hub** with **3 portals**, one to each area: **Pond Island**, **High Platform** (4 m up, unreachable on foot) and **Hedge Maze**. Each area has a **return portal** to the hub, so there are 6 portals in all. Walls are higher than the jump, so portals are the only way between areas. The High Platform portal starts **locked** (grey, no particles) until the hamster steps on the red **switch** in the hub.

Walking into a portal fades the screen to black, moves the hamster, plays a **particle burst around it** and fades back in. Every open portal has a looping **swirl particle system**.

## What's new (developed for this exercise)
- `Portal`: trigger zone with a destination, a locked state (grey, particles off) and `Unlock()`, plus an `Unlocked` event.
- `PortalTraveler`: the teleport sequence as a coroutine (fade out → move → burst → fade in), with a safe CharacterController move and a cooldown.
- `ScreenFader`: full-screen fade with a CanvasGroup, usable from any coroutine.
- Swirl particles (ring emitter + rising + orbital velocity) and the arrival burst.
- `PortalUnlockSwitch`: a floor switch that unlocks a portal (extra feature).
- Scene (level) switching through a portal: written but **commented out** (`Portal.destinationScene`, the block in `PortalTraveler.TravelRoutine`), to show how the game's garden gate (#38) will load the next level.

## Controls
| Key | Action |
|---|---|
| WASD / arrows | Walk (relative to the camera) |
| Shift / Space | Run / jump |
| Walk into a portal | Teleport |
| Step on the red plate | Unlock the High Platform portal |
| Right mouse drag, scroll | Orbit / zoom the camera |

## Requirement checklist
- [x] Player-controlled character moves in an environment: the hamster (`ThirdPersonWalker`)
- [x] >= 3 portals transfer the character to another location: 3 in the hub + 3 return portals
- [x] >= 1 particle system at each portal: the swirl (`Swirl` child of every portal)
- [x] Particle system around the character after transfer: `ArrivalParticles` burst

## Reusable code in Core
- `Core.Portals.Portal`, `Core.Portals.PortalTraveler`: the garden gate (#38). The locked state + `Unlock()` match "unlock on cup completion"; the commented scene switch matches "transition to the next scene".
- `Core.UI.ScreenFader`: the fade for #38 and scene changes.
- `Core.Movement.ThirdPersonWalker`, `Core.Cameras.OrbitFollowCamera`: shared with 4.C.4 and 4.E.3.

## TA modification points
| What the TA may ask | Where |
|---|---|
| Where a portal leads | Portal → `Destination` (drag another area's **Arrival**), or move an **Arrival** object |
| Add a portal | Duplicate a portal (Cmd+D / Ctrl+D), set its `Destination` |
| Lock / unlock a portal | Portal → `Locked`; unlocking in code: `Portal.Unlock()` (see `PortalUnlockSwitch`) |
| Portal colour | Portal → `Open Color`, `Locked Color` |
| Fade speed, instant teleport | Hamster → PortalTraveler → `Fade Time` (or clear `Fader` = instant) |
| Cooldown | PortalTraveler → `Cooldown` |
| Arrival burst | PortalTraveler → `Arrival Particle Count`; the **ArrivalParticles** object |
| Swirl look | Portal prefab → **Swirl** (emission rate, Velocity over Lifetime: Y = rise, Orbital Y = spin) |
| Load another scene | Uncomment the scene-switch lines in `Portal` and `PortalTraveler`, add the scene to Build Settings |

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Third-party code: none
