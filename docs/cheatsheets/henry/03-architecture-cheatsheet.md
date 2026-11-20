# Cheat Sheet 3: Architecture of Henry's mini-exercises

How the code is split, how the pieces talk, which patterns to name, and the honest trade-offs.

---

## 1. The 30-second pitch
"Each exercise is a **self-contained folder with its own scene**. The **reusable technique** (pushing, vanishing, meters, joints, portals, cameras) lives in a generic **Core** library with no golf code in it. The exercise folder holds only the **demo glue**: layout, HUD and golf meaning. Scenes are **authored in the Editor** through a setup menu (Magic > Mini-Exercises), not built at runtime. The one exception is the loop that spawns the 100 dominoes. Everything talks through **C# events**, so the logic never knows about the UI."

---

## 2. Assemblies and the dependency rule

```
Project3DGP.Core            Assets/_Project/Scripts/Core/          generic, no game types
   ▲              ▲
   │              │
Project3DGP.Game   Project3DGP.MiniEx.Henry     Assets/_Project/MiniExercises/Henry/ (one asmdef for all 4)
(the golf game)    (the exercises)
```
- An **asmdef** (assembly definition) makes Unity compile a folder into its own DLL with **declared references**. If the exercise code tried to use a `Project3DGP.Game` type, it would **fail to compile**. The rule is enforced, not just a convention.
- The game and the exercises both use Core, but **never each other** (`docs/mini-exercises.md` rule 4). This is how the exercises feed the game: the game's HUD, power meter and garden gate reuse the same Core classes.
- Editor setup tools are in `Assets/_Project/Editor/Magic/Henry*SetupMenu.cs` (Editor-only assembly), because the exercise asmdef is runtime-only.

## 3. Folder → namespace → what's in it

| Folder | Namespace | Henry's classes |
|---|---|---|
| `Core/Interaction/` | `Project3DGP.Core.Interaction` | `RigidbodyPusher`, `IPushReceiver`, `VanishAfterHit` |
| `Core/Spawning/` | `...Core.Spawning` | `NonOverlapPlacement` (commented-out use) |
| `Core/Movement/` | `...Core.Movement` | `ThirdPersonWalker` (4.C.4, 4.E.3, 4.A.3) |
| `Core/Cameras/` | `...Core.Cameras` | `OrbitFollowCamera`, `MinimapCamera`, `MinimapIcon` |
| `Core/Gauges/` | `...Core.Gauges` | `Meter`, `PingPongGauge` |
| `Core/UI/` | `...Core.UI` | `BarView`, `SelectableCard`, `SelectionPanel`, `ScreenFader` |
| `Core/Motion/` | `...Core.Motion` | `AxisJoint`, `SineMotion` |
| `Core/Portals/` | `...Core.Portals` | `Portal`, `PortalTraveler` |
| `MiniExercises/Henry/<exercise>/Scripts/` | `Project3DGP.MiniEx.Henry` | demo glue: `DominoArena`, `ArenaHud`, `GolfSwing`, `GolfBall`, `ShotTimer`, `ClubSelector`, `ClubDefinition`, `GameUiHud`, `SwingAnimator`, `FlyingBall`, `ToolSwitcher`, `HierarchyDemoHud`, `PortalUnlockSwitch`, `PortalDemoHud` |

Other Core files (`CharacterWalker`, `DialogBar`, `RestDetector`, ...) belong to teammates or the game. You don't need to explain them.

---

## 4. Component maps (what sits on which object)

### 4.C.4 Object interaction
```
Hamster   CharacterController + ThirdPersonWalker (move) + RigidbodyPusher (push)
Arena     DominoArena ──Instantiate×100──► Domino (prefab: BoxCollider 0.5×1×0.15, Rigidbody 0.5 kg, VanishAfterHit)
          ArenaHud (reads arena.Dominoes, CollidedCount, RespawnCount)
Camera    OrbitFollowCamera → Hamster

RigidbodyPusher ──IPushReceiver.OnPushed──► VanishAfterHit ──Hit/BodyContact/Vanished──► DominoArena
```

### 4.E.3 Game UI
```
Hamster   ThirdPersonWalker + GolfSwing (PingPongGauge field, hit sound, HitParticles ref)
Balls×5   GolfBall
Game      ShotTimer (Meter "Time" field)
Canvas    GameUiHud → TimeBar (BarView), PowerBar (BarView), info Text
          ClubPanel: SelectionPanel + ClubSelector → 3 × SelectableCard  ◄── Data/*.asset (ClubDefinition)
          Map: RawImage ◄── Minimap.renderTexture ◄── Minimap Camera (MinimapCamera, culling = Minimap layer)
Icons     MinimapIcon children on layer "Minimap" (main camera doesn't render that layer)

ShotTimer.Meter ──Changed──► GameUiHud ──SetValue──► BarView      (logic → event → view)
GolfSwing ──Shot──► GameUiHud
SelectableCard ──Clicked──► SelectionPanel ◄── number keys;  GolfSwing reads ClubSelector.Current
```

### 4.A.1 Hierarchical model
```
Hamster  SwingAnimator, ToolSwitcher, (HUD refs)
├── A_Shoulder  AxisJoint (axis Z, key 1)
│   └── B_Wrist AxisJoint (axis Y, key 2)
│       └── C_Club SineMotion (axis Z, 8°, 1.2 Hz)
│           ├── GolfClub      (ToolSwitcher shows one)
│           └── Sledgehammer
GolfBall  FlyingBall

SwingAnimator sets shoulder.Angle / wrist.Angle (Update) → AxisJoint applies localRotation (LateUpdate)
SwingAnimator.LaunchBall → FlyingBall.Launch(velocity, sideAcceleration)
```

### 4.A.3 Portals
```
Hub, Pond Island, High Platform, Hedge Maze   (each area has an "Arrival" empty = a destination)
Portal ×6  (prefab: trigger collider + kinematic Rigidbody + Portal + Swirl particle child + pad renderer)
Switch     PortalUnlockSwitch → highPlatformPortal.Unlock()
Hamster    CharacterController + ThirdPersonWalker + PortalTraveler (fader, ArrivalParticles)
Fader      Canvas + CanvasGroup + ScreenFader
HUD        PortalDemoHud ◄──Teleported── PortalTraveler

Portal decides WHO may travel and WHERE (trigger, lock, destination); PortalTraveler decides HOW (fade, move, burst, cooldown)
```

---

## 5. Design patterns (name them for the TA)

| Pattern | Where | One-line justification |
|---|---|---|
| **Observer** (C# events) | `VanishAfterHit` → `DominoArena`; `Meter.Changed` → `GameUiHud`; `Teleported` → HUD | the sender doesn't know the receivers; adding a listener changes nothing in the sender |
| **Object pool** | `DominoArena` (100 tiles created once; vanish = `SetActive(false)`, respawn = reuse) | no `Instantiate`/`Destroy` during play, so no garbage and no frame spikes |
| **State machine** | `SwingAnimator` (`Phase` enum + timer); `GolfSwing` F key (meter running or not) | each phase has clear entry (`Enter`) and exit conditions |
| **Model / view split** | `Meter` (data) → `GameUiHud` (presenter) → `BarView` (view) | logic is testable; the UI can change without touching rules |
| **Interface for decoupling** | `IPushReceiver` | Core pusher works with any pushable object |
| **Data-driven design** | `ClubDefinition` ScriptableObjects | add/tune clubs without code |
| **Composition over inheritance** | small components stacked on one object (walker + pusher + traveler) | each does one job; reuse by adding a component |
| **Rejection sampling** | `NonOverlapPlacement.TryFindSpot` (commented out) | random point, test, retry up to `maxTries` |
| **Coroutine sequence** | `PortalTraveler.TravelRoutine` | a timed sequence reads top to bottom |

---

## 6. Key decisions (ADRs and PO choices)

| Decision | Where recorded | Why |
|---|---|---|
| Exercises are standalone folders/scenes; Core is generic | ADR-0010, `docs/mini-exercises.md` | must run alone for E3 and the demo; reuse in the game |
| Loop-spawned objects allowed in exercise scenes | ADR-0010 | the domino loop; everything else authored in the Editor |
| Scenes built by **Magic** Editor menu tools that create only what's missing | ADR-0005, ADR-0011 | repeatable setup, never overwrites your manual edits |
| Tiles respawn **at home**, not random (random kept commented) | 4.C.4 README | lines can be knocked down again; random version ready to show |
| Vanish delay 5 s (spec: 2 s) | 4.C.4 README | time to see the chain; set `Vanish Delay` = 2 to match the spec exactly |
| Golf ball with scripted motion, no Rigidbody (4.E.3, 4.A.1) | PO choice in READMEs | the distance is known at the hit, predictable and explainable |
| C **turns** with sin instead of sliding along B's y | 4.A.1 README / `SineMotion` | B's y is the arm, so sliding made the club pump; the handout version is commented out |
| Scene switching through a portal written but commented out | 4.A.3 README | prepares the game's garden gate (#38) |

---

## 7. Cross-cutting conventions
- **Input System only** (`Keyboard.current`, `Mouse.current`), no old `Input.GetKey`.
- **Every tunable is a `[SerializeField]` with a tooltip**, so the TA's "change X" is usually an Inspector edit.
- **No per-frame allocations** in hot loops: the HUD text is rebuilt every 0.1 s, the label only when the second changes, `Shader.PropertyToID` is cached, and `MaterialPropertyBlock` recolours without new materials.
- **Colours via `MaterialPropertyBlock`**: 100 tiles still share one material (batching), and the per-tile colour is a renderer override.
- **URP colour property is `_BaseColor`** (not `_Color`, which was the built-in pipeline's name).

---

## 8. Honest trade-offs (say them before the TA finds them)
- **`DominoArena.TryRespawn` uses `_dominoes.IndexOf(domino)`**, which is O(n). Fine for 100 tiles. For thousands, store the index on the tile.
- **`SetColor` calls `GetComponentInChildren` each time.** It only runs on hit/respawn (not per frame), so it's fine. It could be cached.
- **Three exercises use `OnGUI` for the info box.** It's a quick debug HUD. 4.E.3 (the UI exercise) uses a proper uGUI Canvas.
- **`GolfSwing` finds the balls once in `Start`.** A ball added at runtime wouldn't be found. Not needed in the demo.
- **R in 4.E.3 is read by two scripts** (`ShotTimer` refills, `GolfSwing` resets balls). Each script does its own part, but there's no central input map.
- **`FlyingBall` integrates in `Update` with a variable `dt`**, so the flight differs very slightly between frame rates. It's semi-implicit Euler (velocity is updated first, then position uses the new velocity), the more stable variant. Fine for a 5 s demo flight. For exact results you'd step it in `FixedUpdate` or use the closed-form parabola.
- **Respawned tiles can wait forever** if something lies on their spot. That's intentional (never spawn overlapping). The HUD "Obstacles: n / 100" shows it, and R rebuilds.
- **Event subscriptions in `DominoArena.Start` are never removed.** That's safe because the arena owns the tiles (same lifetime).

---

## 9. Likely TA questions → short answers
- **"Why put code in Core instead of the exercise folder?"** It's generic and reused by the game (HUD bars, power meter, garden gate). The asmdef rule keeps golf code out of it.
- **"Does your exercise depend on the game?"** No. The asmdef references only Core, so it can't. Export = the exercise folder + Core.
- **"Why not build the scene in code?"** It's a team rule (ADR-0005/0011): scenes are authored and saved, and the Magic tool only fills in what's missing. Only the 100 dominoes are loop-spawned (allowed by ADR-0010).
- **"How would you add a new obstacle type / club / portal?"** A prefab plus Inspector settings / a new ScriptableObject asset plus a card / duplicate a portal and set its destination. No new code needed for any of them.
