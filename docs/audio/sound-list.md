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

## Music (BGM)
Picked by Henry (PO) on 2026-11-04. Files are mp3 128 kbps, loudness-matched to about -18 LUFS, so one music volume fits all (except PulseBox, see below).

**Playback:** `Effects/MusicPlayer` (one per scene): plays its track from the start with a 1 s fade-in, loops, plays an optional jingle once at the same moment, keeps playing while paused, and follows the **Music** volume setting live. `Play(clip, …)` switches tracks and `Stop(fade)` fades out, for later use (e.g. the shared audio manager, #42). *Magic > Game > Add Music* adds it to **MainMenu, L1_Garden and Ending** (Henry); L2/L3 owners add a GameObject with an AudioSource + MusicPlayer and drag in their track.

| Scene | Track | File | Loop |
|---|---|---|---|
| MainMenu | Kevin MacLeod – Carefree | `ThirdParty/FreeMusicArchive/bgm_menu_carefree_kevin-macleod.mp3` | yes |
| L1 Garden | PulseBox – Comedy Background | `ThirdParty/FreeMusicArchive/bgm_l1_comedy-background_pulsebox.mp3` | yes |
| L2 Desert | ZHRØ – Desert ambient music | `ThirdParty/Freesound/bgm_l2_desert_ambient_686838.mp3` | yes |
| L3 Lake | Sirkoto51 – Ice Cavern Loop | `ThirdParty/Freesound/bgm_l3_ice_cavern_loop_368574.mp3` | yes (made as a seamless loop) |
| Ending (happy) | Alena Smirnova – Moderate Passion Tango, **plus** colorsCrimsonTears – Fanfare 2 played **once** at the start, at the same time | `ThirdParty/FreeMusicArchive/bgm_ending_happy_tango_alena-smirnova.mp3` + `ThirdParty/Freesound/sfx_fanfare_580310.mp3` | tango yes, fanfare no |
| (not used yet) | HoliznaCC0 – Game Over 4, kept for a bad ending | `ThirdParty/FreeMusicArchive/bgm_ending_bad_game-over-4_holiznacc0.mp3` | yes |

- **PulseBox is CC BY-ND (no derivatives):** the file is only re-encoded, never edited or normalized. It's about 7 dB louder than the others, so play it at an AudioSource volume of about **0.45**.
- **Tango is CC BY-NC:** fine for this non-commercial class project; it must not be used commercially.
- **Game Over 4 is kept for later** (PO 2026-11-04): the game has no bad-ending scene yet (time up shows the level's fail scene and the time-up dialog). The team decides where it plays.
- **Gapless loop:** mp3 can add a few milliseconds of silence at the start. If the L3 loop clicks, ask for a `.wav` copy.
- *Add Music* sets every `bgm_*` file to Streaming + Vorbis, so long tracks don't sit decompressed in memory.
- Originals stay outside the repo (Henry: `~/Downloads/3DGP-music-originals`).
