# Sound list (event → sound)

All sounds come from **Kenney Impact Sounds** (CC0), committed in `Assets/ThirdParty/Kenney/ImpactSounds`. Each group has 5 variants (`_000`–`_004`), and one is picked at random with a small pitch change. *Magic > Game > Add Sounds* creates the sets and wires them into the open scene. To change a sound, drag other clips into the SoundSet asset or component in the Inspector; no code changes.

Use this list in the report and slides (Form B: ≥ 4 sound effects per member).

## Shared events (every level, `GameplaySounds`, Henry)
Each level has its own `Settings/Audio/Sounds_<Level>.asset`; the cells show what differs per level.

| Event | Code | L1 Garden | L2 Desert | L3 Lake |
|---|---|---|---|---|
| Shot, club | `ShotController.ShotTaken` (`ClubProfile.Swing` = Club) | impactWood_medium | same | same |
| Shot, sledgehammer | (Swing = Hammer) | impactMetal_heavy | same | same |
| Shot, mysterious tool | (Swing = Tap) | impactPlate_light | same | same |
| Ball lands on the ground | `GolfBall.Collided`, contact facing up | impactSoft_medium (grass) | impactSoft_heavy (sand) | impactGlass_light (ice) |
| Ball hits something hard | `GolfBall.Collided`, other contacts | impactWood_light | impactMining (rock) | impactGlass_medium |
| Out of bounds | `GolfBall.WentOutOfBounds` | impactGeneric_light | same | same |
| Hamster footsteps | avatar walking (every 0.7 m) | footstep_grass | footstep_carpet (soft, sand-like) | footstep_snow |
| Goal met (cup, target) | `LevelGoal.Reached` | impactTin_medium | same | same |
| Gate opens | `LevelExit.Opened` | impactWood_heavy | impactWood_heavy | impactMetal_medium |
| XP seed collected | `XpCollectible.Collected` | impactGlass_light | same | same |
| Level up | `GameSession.LeveledUp` | impactBell_heavy | same | same |
| Time up | `ILevelFlow.LevelFailed` | impactPunch_heavy | same | same |

The bounce volume follows the impact speed, so rolling onto the next ground block is silent.

## Level extras (`SoundSet.Extras`, played with `GameplaySounds.PlayExtra(name, position)`)
| Level | Name | Sound | Event | Wired by |
|---|---|---|---|---|
| L1 | `CatSuspicious` | impactPlank_medium | cat turns Suspicious (`GardenCat.StateChanged`) | `GardenSounds` (Henry) |
| L1 | `CatWake` | impactMining | cat wakes up (`GardenCat.WokeUp`) | `GardenSounds` (Henry) |
| L1 | `PondSplash` | impactSoft_heavy | ball enters the pond (`BallTriggerEffect` on the pond) | Add Sounds (Henry) |
| L2 | `ScarabHit` | impactPunch_medium | scarab hits the ball / hamster | Bahadir (#42) |
| L2 | `SandLanding` | impactSoft_heavy | e.g. ball lands in a sand trap | Bahadir (#42) |
| L3 | `IceCrack` | impactGlass_heavy | ice cracks | Pongpat (#73) |
| L3 | `GuardianHit` | impactMetal_medium | guardian hits / blocks | Pongpat (#73) |
| L3 | `SnowLanding` | impactSoft_medium | ball lands in snow | Pongpat (#73) |

To use an extra in L2/L3 level code:
```csharp
GameplaySounds.PlayExtra("ScarabHit", transform.position);
```
The pack has no wind or ambience loops. Wind (L2) and birds (L1) still need another source (#42).

## GUI (`UiSounds` on every canvas, Henry)
| Event | Sound |
|---|---|
| Button click | impactWood_light |
| Back / Cancel / Close / Skip / Resume | impactSoft_medium |
| Volume slider drag | impactPlate_light (quiet, at most every 0.08 s) |
| Club card select | `ClubPanelView` keeps its own sound (Freesound "invalid1") |

Sounds play at the **SFX volume** from Settings; GUI sounds also play while the game is paused.
