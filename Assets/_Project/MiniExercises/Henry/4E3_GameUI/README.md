# 4.E.3 Game UI — Henry

Issue: #58 · Demo: Nov 16
Scene: `Scenes/4E3_GameUI.unity` · Guide: `docs/mini-exercises.md`
Setup: **Magic > Mini-Exercises > Henry 4.E.3: Set Up Scene** (creates the hamster, balls, arena, minimap camera, Canvas, club data and EventSystem if missing).

The hamster walks to a golf ball, picks a club on the right, and swings: press F to start the power meter, press F again to stop the marker. The ball slides forward **club max distance × power**. Every shot costs **10 s** on the time meter, which also counts down in real time. The minimap in the top-right corner shows the hamster and the balls from above.

## What's new (developed for this exercise)
- Club selection panel (`SelectionPanel`, `SelectableCard`): mouse or number keys; the selected card lights up, grows, and plays a **sound** (half volume).
- Hit effect: a confetti **particle burst** and a hit sound at the ball on each shot (`GolfSwing`).
- Event-driven attribute bar: `Meter` raises `Changed`, and the time bar (`BarView`) redraws and changes colour only when that event fires. Shots subtract time through the same event path.
- Power meter (`PingPongGauge`): a marker running 0 → 1 → 0 that decides the shot distance.
- Minimap (`MinimapCamera`): an orthographic top-down camera rendering to a RenderTexture, with icons on a "Minimap" layer that only the minimap camera sees.
- Simple ball movement (`GolfBall`): ease-out slide with a small hop, stopping in front of walls (SphereCast).

## Controls
| Key | Action |
|---|---|
| WASD / arrows | Walk (relative to the camera) |
| Shift / Space | Run / jump |
| F | Start the power meter, then F again to hit (near a ball) |
| 1, 2, 3 or click a card | Select a club |
| R | Refill time, reset the balls |
| - / = | Zoom the minimap out / in |
| Right mouse drag, scroll | Orbit / zoom the camera |

## Requirement checklist
- [x] Interactive UI for gear selection: 3 club cards (Standard Club, Sledgehammer, Putter) on the right, mouse + keys
- [x] Selected item highlighted with a sound effect (the spec says particle **or** sound): select sound (CC0) + frame colour + scale. Particles play on the ball hit instead.
- [x] Attribute bars update with events: time meter (-10 s per shot event, colour green → yellow → red) and power meter
- [x] Minimap with a top-down view: top-right corner, follows the hamster

## Reusable code in Core
- `Core.Gauges.Meter`, `Core.UI.BarView`: the game's time bar and later the XP bar (#20, #78).
- `Core.Gauges.PingPongGauge`: the game's power meter (`ShotController.PowerMarker01`, architecture §4.8).
- `Core.UI.SelectionPanel`, `Core.UI.SelectableCard`: club selection in the HUD (#78).
- `Core.Cameras.MinimapCamera`: minimap in the HUD (#78).
- `Core.Movement.ThirdPersonWalker`, `Core.Cameras.OrbitFollowCamera`: shared with 4.C.4.

## TA modification points
| What the TA may ask | Where |
|---|---|
| Time per shot / start time | `ShotTimer` on **Game** → `Shot Cost`, `Time` → `Max` / `Current` |
| Club stats (distance, accuracy, meter speed) | `Data/*.asset` (Inspector); add a club = new asset + a 4th card |
| Shot formula | `GolfSwing.Swing()` (distance = max distance × power; random angle from accuracy) |
| Ball speed / hop | `GolfBall` → `Launch Speed`, `Hop Per 10m`; easing in `GolfBall.Update()` |
| Bar colours / thresholds | `GameUiHud` on **Canvas** → colours, `Warning At`, `Critical At` |
| Bar animation speed | `BarView` → `Smooth Time` (TimeBar) |
| Highlight effect | `SelectionPanel` on **ClubPanel** → `Select Sound`, `Select Volume`; `SelectableCard` → colours, `Selected Scale` |
| Hit particles | `GolfSwing` on **Hamster** → `Hit Particle Count`; the **HitParticles** object (speed, size, colours) |
| Minimap zoom / rotation | `MinimapCamera` → orthographic size, `Rotate With Target` |

## References
- Course spec: `docs/references/course/mini-exercises.md`
- Sounds (CC0, Freesound): hit = "Egg Shaker - 1 Throw" by connermusician, https://freesound.org/s/199823/ ; select = "invalid1.wav" by mmaal, https://freesound.org/s/663859/ (also in `docs/assets.md`, `CREDITS.md`)
- Third-party code: none
