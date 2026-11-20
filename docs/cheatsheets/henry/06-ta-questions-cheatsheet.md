# Cheat Sheet 6: Likely TA questions and live modifications (with snippets)

Adds to sheet 5 (§5 Inspector-only changes, §6 rapid-fire Q&A). Code is located by **file + method name**.
**The snippets were written against the current code but not compiled.** Try the starred (★) ones once before the demo, then `git restore .`

**Rule of thumb for live edits:** Inspector first. Edit C# only when the request needs new behaviour. **Stop Play Mode before editing.** Keep it to a few lines. Changing a **Core** file affects other exercises and the game, so for a demo that's fine, but restore it afterwards.

IDs: **C** = 4.C.4 dominoes · **E** = 4.E.3 game UI · **A** = 4.A.1 hierarchy · **P** = 4.A.3 portals.

---

## Part A: "Explain this" questions

### A-C1. "How does the hamster push the dominoes?" (`RigidbodyPusher.OnControllerColliderHit`)
```csharp
var body = hit.rigidbody;
if (body == null || body.isKinematic || body.mass > maxMass) return;   // only light, dynamic bodies
if (hit.moveDirection.y < -0.3f) return;                               // standing on it, not walking into it
var direction = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
var speedFactor = Mathf.Clamp01(speed / referenceSpeed);
var impulse = direction * (pushForce * speedFactor * Time.deltaTime);
body.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);
if (body.TryGetComponent(out IPushReceiver receiver)) receiver.OnPushed(this, impulse);
```
*Why at the contact point?* The force is off the centre of mass, so it adds torque: τ = r × F. The contact is at the hamster's height, low on a 1 m tile, so the tile tips over instead of sliding.
*Why not give the hamster a Rigidbody?* The tiles would push back and walking would feel floaty. The CharacterController gives precise movement, and we decide how hard it pushes.

### A-C2. "Where do 10 obstacles collide with each other?"
Each line is 10 tiles, 0.6 m apart, each 1 m tall, so a falling tile always reaches the next (spacing < height). `VanishAfterHit.OnCollisionEnter` filters `collision.rigidbody == null` (ground, walls), so only **tile-tile** contacts count. `DominoArena.OnBodyContact` adds the tile to a `HashSet`, so each tile counts once. HUD: "Collided with another obstacle".

### A-C3. "How do you guarantee no overlap on respawn?" (`DominoArena.TryRespawn`)
```csharp
var checkExtents = _halfExtents + new Vector3(respawnPadding, 0f, respawnPadding);
if (Physics.CheckBox(position + Vector3.up * 0.05f, checkExtents, rotation,
        Physics.AllLayers, QueryTriggerInteraction.Ignore))
    return false;                      // blocked → caller puts it in _waitingForSpot, retried every frame
Place(domino, position, rotation);
Physics.SyncTransforms();              // the next CheckBox this frame must see this tile
```
*Why +0.05 up?* So the box doesn't touch the ground collider. *Why rotated?* The test box must match the tile's real orientation.

### A-C4. "Why `SetActive(false)` and not `Destroy`?" Object pooling: no allocation and no GC spike, and the same 100 objects keep their event subscriptions. *Why `MaterialPropertyBlock`?* It changes one renderer's colour without creating a material copy, so all tiles share one material.

### A-C5. "How is the layout computed?" (`DominoArena.BuildLines`)
Polar coordinates: line angle = `line * 360 / lineCount`. `rotation * Vector3.forward` = the line direction. Position = centre + direction × (2.5 + k × 0.6) + half height up. The same rotation makes the tile's thin side face along the line.

### A-E1. "How do the bars update with events?" (`Meter.Set` → `GameUiHud.OnTimeChanged`)
```csharp
public void Set(float value)
{
    var previous = current;
    current = Mathf.Clamp(value, 0f, max);
    if (Mathf.Approximately(previous, current)) return;   // no change → no event
    Changed?.Invoke(this);
    if (current <= 0f && previous > 0f) Emptied?.Invoke(this);
}
```
`ShotTimer.ChargeShot()` → `Subtract(10)` → `Set` → `Changed` → HUD: `timeBar.SetValue(time.Normalized)` + colour + label. The HUD never reads the timer in `Update`. *Note:* the real-time countdown also goes through `Set`, so `Changed` fires every frame while time runs. That's correct (the value really changed), but be ready to say so.

### A-E2. "How does the bar animate?" (`BarView`)
The fill's `anchorMax.x` = value (0..1): the fill RectTransform stretches from the left edge, with no sprite or Image.fillAmount needed. `SmoothDamp` slides `_shown` towards `_target` (spring-like, no overshoot) using unscaled time.

### A-E3. "How is the item highlighted?" (`SelectionPanel.Select`, `SelectableCard.SetSelected`)
Mouse (card `Clicked` event) and keys 1–9 both call `Select(index)`. It sets every card's `SetSelected(i == index)` (exactly one on: yellow frame, target scale 1.08, animated with `MoveTowards`) and plays `PlayOneShot(selectSound, 0.5f)`. The first selection at `Start` is silent.

### A-E4. "How does the power meter work?" (`PingPongGauge.Tick`)
`_phase += dt * cyclesPerSecond; Value01 = Mathf.PingPong(_phase * 2f, 1f);`. One cycle = up + down = 2 units, hence ×2. The club's `meterSpeed` sets `CyclesPerSecond`. `Stop()` returns the frozen value as the power.

### A-E5. "Explain the shot." (`GolfSwing.Swing`)
`maxError = (1 - accuracy) × 20°` → direction = `Quaternion.Euler(0, Random.Range(-maxError, maxError), 0) * forward`; distance = `maxDistance × power`. Then particles `Emit`, `ball.Hit`, `timer.ChargeShot`, sound, `Shot` event.
`GolfBall.Update`: `travelled = 1 - (1-t)²` (ease-out), `Lerp(start, end, travelled)`, hop `4t(1-t)` (a parabola that is 0 at both ends). `SphereCast` shortens the shot in front of walls.

### A-E6. "How does the minimap work?" (`MinimapCamera.LateUpdate`)
An orthographic camera at `target + 30 m up`, rotation `(90, yaw, 0)` = looking straight down. Its Target Texture is `Minimap.renderTexture`, which a `RawImage` in the corner shows. The culling masks: the map camera renders the "Minimap" layer (icons), the main camera doesn't. `MinimapIcon` keeps icons flat, north-facing and a fixed size, so the ball's icon doesn't spin. `-`/`=` change `orthographicSize`.

### A-A1. "What makes it hierarchical?" (`AxisJoint.LateUpdate`)
```csharp
transform.localRotation = _restRotation * Quaternion.AngleAxis(Angle * direction, axis);
```
Each joint sets only its **local** rotation. Unity computes world = parent.world × local, so B inherits A's turn, and C inherits A's and B's. *Why `rest * delta` and not `delta * rest`?* Right-multiplying turns about the joint's **own** (local) axis. Left-multiplying would turn about the parent's axis.

### A-A2. "Explain the swing timing." (`SwingAnimator.Update`)
k = phase time / phase duration (0→1). Backswing: `SmoothStep` (slow start, slow end) over 1.2 s. Downswing: `k²` (accelerating) over 0.2 s, `Lerp(120°, −110°, k²)`. **Impact** = first frame with `shoulder.Angle <= 0` → `LaunchBall()` (the `_ballHit` flag makes it fire once).

### A-A3. "What does reversing do to the ball?" (`SwingAnimator.LaunchBall`)
`target = -transform.right * shoulder.Direction` (reversing A mirrors the swing, so the ball flies the other way). `side = transform.forward * (curve * wrist.Direction)` (reversing B flips the curve). Tool power multiplies the launch speed (sledgehammer 1.6×).

### A-A4. "How does the ball fly?" (`FlyingBall.Update`)
In the air: `v += (g·up + side)·dt`. Then `p += v·dt` (semi-implicit Euler). On the ground: `v *= 1 - friction·dt`. Bounce: `v.y = -v.y × 0.45`, below 0.5 m/s it stops bouncing, and the horizontal speed ×0.8. After 5 s: `ResetToTee`.

### A-A5. "Where is C's sin motion?" (`SineMotion.Update`)
`phase = 2π·f·t`; `CurrentOffset = amplitude × sin(phase)`; `localRotation = rest × AngleAxis(offset, axis)`. Period = 1/f. Enabled only at address (`SwingAnimator.Enter`), and `OnDisable` restores the rest pose.

### A-P1. "How does a teleport work?" (`Portal.OnTriggerEnter` → `PortalTraveler.TravelRoutine`)
Trigger → locked/destination check → `TryGetComponent(out PortalTraveler)` → `Travel` (ignored while traveling or in cooldown) → coroutine: fade out, controller off, `SetPositionAndRotation(destination)`, `SyncTransforms`, controller on, burst, cooldown, `Teleported`, fade in.

### A-P2. "Why does `OnTriggerEnter` fire at all?" A trigger message needs a Rigidbody (or CharacterController) on one side. The portal and the switch also get a **kinematic Rigidbody** (setup tool: "makes trigger events reliable with a CharacterController").

### A-P3. "How are the particles done?" Swirl: looping system, ring-shaped emitter, Velocity over Lifetime with **Y** = rise and **Orbital Y** = spin. Locked → `Stop(true, StopEmittingAndClear)`, open → `Play()`, colour via `main.startColor`. Arrival: **one** world-space system moved to the hamster and `Emit(40)`. World space means old particles stay where they were born.

### A-P4. "Why the cooldown?" The arrival point is next to the destination area's return portal. Without the cooldown, a slightly misplaced arrival would send the hamster straight back.

---

## Part B: "Change X and show me" (snippets)
Each snippet shows only what changes. "…" = existing code stays.

### 4.C.4 dominoes

**B-C1. "150 / 200 obstacles"**: Arena → `Line Count` (or `Dominoes Per Line`) **before Play**. ⚠ Changing it during Play and pressing R breaks things (the arrays are sized in `Start`) → index out of range.

**B-C2. "Vanish after 2 s"**: Arena → `Vanish Delay` = 2 (during Play: press R so all tiles take the new delay).

**B-C3 ★. "Respawn at a random free spot (the spec's wording)"** (`DominoArena`): uncomment the two fields, then make `TryRespawn` look like this:
```csharp
[SerializeField] Vector2 respawnArea = new Vector2(21f, 21f);   // uncommented
[SerializeField] int maxTries = 30;                             // uncommented

bool TryRespawn(VanishAfterHit domino)
{
    var checkExtents = _halfExtents + new Vector3(respawnPadding, 0f, respawnPadding);

    // home version, now commented out:
    // var index = _dominoes.IndexOf(domino);
    // var position = _homePositions[index];
    // var rotation = _homeRotations[index];
    // if (Physics.CheckBox(position + Vector3.up * 0.05f, checkExtents, rotation,
    //         Physics.AllLayers, QueryTriggerInteraction.Ignore))
    //     return false;

    var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    var areaCenter = transform.position + Vector3.up * (_halfExtents.y + 0.05f);
    if (!NonOverlapPlacement.TryFindSpot(areaCenter, respawnArea, checkExtents, rotation,
            Physics.AllLayers, maxTries, out var position))
        return false;

    Place(domino, position, rotation);
    Physics.SyncTransforms();
    RespawnCount++;
    return true;
}
```
**Say:** rejection sampling: up to 30 random points, each tested with `CheckBox`, and the first free one wins. Respawned tiles now stand randomly, and R puts them back in lines. (The tile's centre is `_halfExtents.y + 0.05` up, so it appears 5 cm above the ground and drops.)

**B-C4. "Put all dominoes in one big circle"** (replace the double loop in `BuildLines`):
```csharp
var count = lineCount * dominoesPerLine;
var radius = count * spacing / (2f * Mathf.PI);       // circumference = count × spacing → 100 × 0.6 m ≈ 9.5 m radius
for (var i = 0; i < count; i++)
{
    var angle = i * 360f / count;
    var outward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
    var position = transform.position + outward * radius + Vector3.up * _halfExtents.y;
    var rotation = Quaternion.Euler(0f, angle + 90f, 0f);   // thin side along the circle → falls onto the next
    _homePositions[i] = position;
    _homeRotations[i] = rotation;
    Place(_dominoes[i], position, rotation);
}
```
One push → **100** obstacles collide in a chain. Check it stays inside the walls (lower `Spacing` if not).

**B-C5. "Show how many are lying down"** (`ArenaHud.Update`):
```csharp
int active = 0, hit = 0, collidingNow = 0, fallen = 0;
…
if (Vector3.Angle(domino.transform.up, Vector3.up) > 45f)   // tilted more than 45° = fallen
    fallen++;
…
$"Fallen: {fallen}\n" +
```

**B-C6. "Play a click when tiles collide"** (`DominoArena`; add an AudioSource on Arena and assign a clip, e.g. `Assets/ThirdParty/Freesound/sfx_throw_199823.mp3`):
```csharp
[SerializeField] AudioSource audioSource;
[SerializeField] AudioClip clickSound;
float _nextClick;

void OnBodyContact(VanishAfterHit domino)
{
    _collided.Add(domino);
    if (audioSource != null && clickSound != null && Time.time >= _nextClick)
    {
        audioSource.PlayOneShot(clickSound, 0.3f);
        _nextClick = Time.time + 0.05f;   // at most 20 clicks per second, or a falling line is just noise
    }
}
```

**B-C7. "Kick objects up into the air"** (`RigidbodyPusher`, Core):
```csharp
var direction = (new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized + Vector3.up * 0.5f).normalized;
```
Raise `Push Force` (e.g. 40) to see it.

**B-C8. "Don't push when walking slowly"** (`RigidbodyPusher`, after `speedFactor`): `if (speedFactor < 0.5f) return;` → only running (Shift) knocks tiles over.

**B-C9. "Each line a different colour"** (`DominoArena.BuildLines`, after `Place(...)`):
```csharp
SetColor(_dominoes[index], Color.HSVToRGB((float)line / lineCount, 0.5f, 1f));   // hue around the wheel
```
(A respawned tile comes back white because `Place` sets `idleColor`. Say so, or store the colour per index.)

### 4.E.3 game UI

**B-E1. "Shot costs 5 s / start with 60 s / no real-time countdown"**: Game → ShotTimer → `Shot Cost`, `Time → Max/Current`, `Count Down`.

**B-E2. "Add a 4th club"**: Project → `Create > Project3DGP > Henry 4.E.3 > Club Definition` → set stats → ClubPanel: duplicate a card → add it to `SelectionPanel.Cards` and the asset to `ClubSelector.Clubs` (same order). Key 4 works automatically (`Key.Digit1 + i`).

**B-E3 ★. "Add a stamina bar: running drains it, resting heals it"** (new file `4E3_GameUI/Scripts/Stamina.cs`, put it on Hamster; duplicate TimeBar in the Canvas and assign it):
```csharp
using Project3DGP.Core.Gauges;
using Project3DGP.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] Meter stamina = new Meter(100f);
        [SerializeField] float drainPerSecond = 25f;
        [SerializeField] float regenPerSecond = 10f;
        [SerializeField] BarView bar;

        void OnEnable() => stamina.Changed += OnChanged;     // event-driven, same as the time bar
        void OnDisable() => stamina.Changed -= OnChanged;
        void Start() => bar.SetValue(stamina.Normalized, instant: true);

        void Update()
        {
            var running = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            if (running) stamina.Subtract(drainPerSecond * Time.deltaTime);   // "damage"
            else stamina.Add(regenPerSecond * Time.deltaTime);                // "healing"
        }

        void OnChanged(Meter meter)
        {
            bar.SetValue(meter.Normalized);
            bar.SetLabel($"Stamina {meter.Current:0}");
        }
    }
}
```
**Say:** `Meter` is generic, so a new attribute is ~20 lines. `Meter.Set` raises no event when the value is already full, so there's no wasted redraw.

**B-E4. "Particles when selecting too"** (`ClubSelector`; drag the HitParticles object and the Hamster in):
```csharp
[SerializeField] ParticleSystem selectParticles;
[SerializeField] Transform hamster;

void OnEnable() => panel.Selected += OnSelected;
void OnDisable() => panel.Selected -= OnSelected;

void OnSelected(int index)
{
    if (selectParticles == null || hamster == null) return;
    selectParticles.transform.position = hamster.position + Vector3.up;
    selectParticles.Emit(20);
}
```
(The silent first selection at `Start` also raises `Selected`, so you get one burst at scene start. Mention it.)

**B-E5. "Perfect timing gives a bonus"** (`GolfSwing.Swing`, after `distance`): `if (power >= 0.95f) distance *= 1.2f;`

**B-E6. "Do something when time runs out"** (`GameUiHud`; event exists already):
```csharp
void OnEnable()  { …; timer.Remaining.Emptied += OnTimeUp; }
void OnDisable() { …; timer.Remaining.Emptied -= OnTimeUp; }
void OnTimeUp(Meter time) => Debug.Log("Time's up!");   // or play a sound / show a panel
```

**B-E7. "Bar changes colour at different levels" / "slower bar animation"**: Canvas → GameUiHud → `Warning At`, `Critical At`, colours; TimeBar → BarView → `Smooth Time`.

**B-E8. "Show shots left"** (`GameUiHud.Update`): `var shotsLeft = Mathf.FloorToInt(timer.Remaining.Current / timer.ShotCost);` and add `$"\nShots left: {shotsLeft}"` to `infoText.text`.

### 4.A.1 hierarchy

**B-A1. "A about X / B about Z / reverse with other keys"**: Inspector on A_Shoulder / B_Wrist → AxisJoint → `Axis`, `Reverse Key`.

**B-A2 ★. "C must slide along B's local y-axis (handout)"** (`SineMotion.cs`, Core):
```csharp
// uncomment the two fields:
[SerializeField] Vector3 slideAxis = Vector3.up;
[SerializeField] float slideAmplitude = 0.02f;
…
// in Update: uncomment SLIDE, comment out TURN
CurrentOffset = slideAmplitude * wave;
transform.localPosition = _restPosition + slideAxis.normalized * CurrentOffset;
// CurrentOffset = amplitude * wave;
// transform.localRotation = _restRotation * Quaternion.AngleAxis(CurrentOffset, axis);
```
**Say:** `localPosition` is in the parent's (B's) frame, so `(0,1,0)` is B's y-axis wherever B is turned. Raise `Slide Amplitude` to ~0.1 m in the Inspector to make it obvious. (The HUD then shows metres with a ° sign. Just say so.)

**B-A3. "Add another joint (elbow)"** (Editor: add an empty `D_Elbow` between A_Shoulder and B_Wrist, re-parent B under it, add AxisJoint axis Z; then `SwingAnimator`):
```csharp
[SerializeField] AxisJoint elbow;          // D (optional)
[SerializeField] float elbowBend = 40f;
// Backswing:  if (elbow != null) elbow.Angle = elbowBend * k;
// Downswing:  if (elbow != null) elbow.Angle = Mathf.Lerp(elbowBend, 0f, eased);
```
No other changes: B and C are children of D, so they follow it.

**B-A4. "A key to reverse C"** (`SineMotion`, Core; add `using UnityEngine.InputSystem;`):
```csharp
[SerializeField] Key reverseKey = Key.None;   // set to Digit3 on C_Club
int _direction = 1;
// first lines of Update:
if (reverseKey != Key.None && Keyboard.current != null && Keyboard.current[reverseKey].wasPressedThisFrame)
    _direction = -_direction;
// and:
var wave = (useCosine ? Mathf.Cos(phase) : Mathf.Sin(phase)) * _direction;
```
(Works at address only. During the swing the component is disabled.)

**B-A5. "Swing automatically, no key"** (`SwingAnimator`):
```csharp
[SerializeField] bool autoSwing;
// Address branch:
var pressed = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
if ((pressed || autoSwing) && !ball.InFlight)
    Enter(Phase.Backswing);
```

**B-A6. "Slower swing / bigger wiggle / ball resets after 3 s / bouncier"**: SwingAnimator times and angles; C_Club → SineMotion; GolfBall → FlyingBall `Flight Seconds`, `Bounciness`.

### 4.A.3 portals

**B-P1. "Add a 4th destination"**: duplicate an area's portal (Cmd+D), move it, create an empty `Arrival` where the hamster should land, drag it into `Destination`.

**B-P2. "Sound on teleport"** (`PortalTraveler`, Core; add an AudioSource on Hamster):
```csharp
[SerializeField] AudioSource audioSource;
[SerializeField] AudioClip teleportSound;
// in TravelRoutine, right after the burst:
if (audioSource != null && teleportSound != null)
    audioSource.PlayOneShot(teleportSound);
```

**B-P3. "Portal sends you to a random place"** (`Portal` + `PortalTraveler`, Core):
```csharp
// Portal.cs: it has `using System;`, so write UnityEngine.Random explicitly (System.Random would be ambiguous)
[SerializeField] Transform[] randomDestinations;
public Transform Destination => randomDestinations != null && randomDestinations.Length > 0
    ? randomDestinations[UnityEngine.Random.Range(0, randomDestinations.Length)]
    : destination;

// PortalTraveler.TravelRoutine: read it ONCE (each read picks a new random one)
var target = portal.Destination;
transform.SetPositionAndRotation(target.position, target.rotation);
```
Keep `destination` set too (`OnTriggerEnter` checks it). The HUD reads `Destination` again, so its area name may be wrong. Good to mention as a side effect.

**B-P4. "Press E to use a portal instead of walking in"** (`Portal`, Core; add `using UnityEngine.InputSystem;`):
```csharp
PortalTraveler _inside;   // the traveler standing in the trigger, if any

void OnTriggerEnter(Collider other)
{
    if (locked || destination == null) return;
    if (other.TryGetComponent(out PortalTraveler traveler)) _inside = traveler;   // was: traveler.Travel(this)
}

void OnTriggerExit(Collider other)
{
    if (other.TryGetComponent(out PortalTraveler traveler) && traveler == _inside) _inside = null;
}

void Update()
{
    if (_inside != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
    {
        _inside.Travel(this);
        _inside = null;   // it's leaving; don't rely on OnTriggerExit after a teleport
    }
}
```
Input is read in `Update`, not `OnTriggerStay` (which runs on physics steps and can miss or repeat a key press).

**B-P5. "Unlock the portal by collecting 3 coins"** (new `4A3_GamePortals/Scripts/PortalCoin.cs`; make 3 coins by copying the Switch object, replacing its script):
```csharp
using Project3DGP.Core.Portals;
using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    public class PortalCoin : MonoBehaviour
    {
        [SerializeField] Portal portal;
        [SerializeField] int needed = 3;
        static int _collected;   // shared by all coins

        // Reset at every Play (static fields survive between Play sessions when Domain Reload is off).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCount() => _collected = 0;

        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterController _)) return;
            _collected++;
            gameObject.SetActive(false);
            if (_collected >= needed) portal.Unlock();
        }
    }
}
```

**B-P6. "Keep the hamster's facing direction after teleport"** (`PortalTraveler.TravelRoutine`):
`transform.SetPositionAndRotation(portal.Destination.position, transform.rotation);`

**B-P7. "Portal loads another scene"**: uncomment the `destinationScene` field + `DestinationScene` property (`Portal`), the `using UnityEngine.SceneManagement;` line and the block in `TravelRoutine` (`PortalTraveler`), set the scene name on one portal, and add the scene to **File > Build Profiles > Scene List**.

---

## Part C: Tricky "what if" questions (short answers)
- **"What if the hamster runs very fast into a tile?"** `speedFactor` is clamped to 1, so the push is capped. Very high speeds could tunnel through thin colliders. Fix with continuous collision detection on the tile Rigidbody.
- **"What if two tiles respawn in the same frame on the same spot?"** They can't overlap: `Physics.SyncTransforms()` after each placement makes the next `CheckBox` see the first tile.
- **"Is the obstacle count really constant?"** The pool is always 100. A tile can be *waiting* (inactive) while its spot is blocked, so the HUD can briefly show fewer than 100 *active*.
- **"Why does `OnCollisionEnter` fire on both tiles?"** Unity sends collision messages to both bodies. Each tile counts itself, and the `HashSet` prevents double counting.
- **"Frame-rate independence?"** Every rate is multiplied by `Time.deltaTime`. The push impulse is per-frame × dt, so the total is the same at any fps.
- **"What if the time meter reaches 0 mid-swing?"** `Swing()` still finishes that shot (the meter was already running). New swings are blocked by `!timer.IsUp`.
- **"Why is the power bar polled but the time bar event-driven?"** The marker changes every frame anyway (an event would fire every frame). The time bar changes on game events, which is what the spec asks.
- **"Can the minimap camera see the main scene objects?"** Yes. Its culling mask includes the world plus the Minimap layer. The main camera excludes Minimap.
- **"What if A's axis equals B's axis?"** Then both rotate in the same plane and the spec's "different axis" fails. That's why A = Z and B = Y.
- **"Gimbal lock?"** Joints use `Quaternion.AngleAxis` about a single axis and compose quaternions, so there's no Euler-angle chain to lock.
- **"What if the portal destination is inside a wall?"** `CharacterController` would be stuck. Arrivals are placed by hand in open space. A runtime check would be `Physics.CheckCapsule` before moving.
- **"Why not move the hamster with `transform.position` without disabling the controller?"** The controller would overwrite it on the next `Move()`, or the hamster would snap back.
- **"Coroutine vs `Invoke` vs async?"** A coroutine is tied to the GameObject's life (it stops when the object is disabled), runs on the main thread, and reads like a timeline. `async` needs care with Unity objects after destroy.
- **"Which parts did you write vs reuse?"** README "What's new" + sheet 4 §5. No third-party code. The minimap idea is reused from PA1 and not counted.
