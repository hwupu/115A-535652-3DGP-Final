# Cheat Sheet 1: C# for a Python / TypeScript programmer (Henry's mini-exercises)

Every example is **real code from the four exercises or the Core scripts they use**, unless it is labelled *illustrative*.
Companion: [02-unity-lifecycle-cheatsheet.md](02-unity-lifecycle-cheatsheet.md) (what Unity calls, and when).

---

## 0. The 60-second mental model

| | Python | TypeScript | C# |
|---|---|---|---|
| Typing | dynamic | static, structural, erased at runtime | **static, nominal, kept at runtime** |
| Memory | GC | GC | GC; **structs (`Vector3`, `Quaternion`, `Color`) are copied by value** |
| Blocks | indentation | `{}` | `{}` and every statement ends with `;` |
| Free functions | yes | yes | **no**: everything is in a class (`static class NonOverlapPlacement` for helper functions) |
| Compile | none | `tsc` | Unity compiles when the Editor gets focus; errors appear as `error CS####` in the Console |
| Generators | `yield` | `function*` | `IEnumerator` + `yield return` (Unity uses these for **coroutines**, see §9) |

One file = one class, and the file name must match the class name for `MonoBehaviour`s (`DominoArena.cs` → `class DominoArena`).

---

## 1. File skeleton (`PortalUnlockSwitch.cs`, shortened)
```csharp
using Project3DGP.Core.Portals;   // like Python's import: lets us write Portal instead of Project3DGP.Core.Portals.Portal
using UnityEngine;

namespace Project3DGP.MiniEx.Henry   // namespace = package; matches the asmdef Project3DGP.MiniEx.Henry
{
    public class PortalUnlockSwitch : MonoBehaviour   // ":" = inherits from (like Python's class X(Base))
    {
        [SerializeField] Portal portal;   // private field, but shown and saved in the Inspector
        public bool IsPressed { get; private set; }   // read-only property for other scripts
        void OnTriggerEnter(Collider other) { ... }   // Unity calls this by name (no override keyword)
    }
}
```
Fields without an access modifier are **private** by default.

---

## 2. Types

### 2.1 `var` and number literals
`var` = type inferred at compile time (still static, like TS `const x = 1`). `1f` is a `float` literal. C# won't silently mix `double` into `float`, which is why Unity code writes `0.5f`, `-9.81f`.

### 2.2 Value types vs reference types (the important one)
`Vector3`, `Quaternion`, `Color` are **structs**: assigning copies them. Changing a copy does nothing to the original. That is why `BarView.Apply` does this three-step dance:
```csharp
var anchorMax = fill.anchorMax;   // 1. copy the struct out
anchorMax.x = _shown;             // 2. change the copy
fill.anchorMax = anchorMax;       // 3. write the whole struct back
// fill.anchorMax.x = _shown;     // compile error CS1612: you'd be changing a temporary copy
```
The same applies to parameters: in `GolfBall.Hit`, `direction.y = 0f; direction.Normalize();` changes only the method's own copy, never the caller's vector.
Classes (`GameObject`, `Rigidbody`, `Meter`, `List<T>`) are **references**, as in Python.

### 2.3 Enums + `switch` (`SwingAnimator`)
```csharp
public enum Phase { Address, Backswing, Downswing, FollowThrough, Return }
public Phase Current { get; private set; } = Phase.Address;

switch (Current)
{
    case Phase.Backswing:
    {   // braces give each case its own scope, so each can declare its own `var k`
        ...
        break;   // C# requires break (no silent fall-through to the next case)
    }
    case Phase.FollowThrough:
        if (_time >= holdTime) Enter(Phase.Return);
        break;
}
```
This **enum + timer** is the whole state machine of the swing.

### 2.4 String interpolation and format specifiers
```csharp
domino.name = $"Domino_{i:000}";                        // 7 -> "Domino_007"  (DominoArena.Start)
$"Last shot: {distance:0.0} m  (-{timer.ShotCost:0} s)" // one decimal / no decimals  (GameUiHud.OnShot)
$"Accuracy {club.accuracy * 100f:0}%"                   // 0.45 -> "Accuracy 45%"  (ClubSelector.Awake)
```
Same as Python f-strings, with `:format` after the value.

---

## 3. Members of a class

### 3.1 Fields, properties, attributes
```csharp
[Header("Layout")]                                     // Inspector heading
[SerializeField] VanishAfterHit dominoPrefab;          // private + Inspector
[SerializeField, Min(1)] int lineCount = 10;           // Inspector won't allow < 1
[Tooltip("Gap between tile centres along a line (m).")]
[SerializeField] float spacing = 0.6f;
[SerializeField, Range(0f, 1f)] float selectVolume = 0.5f;   // slider in the Inspector
```
**Why `[SerializeField] private` and not `public`?** The Inspector can edit it, but other scripts can't change it by accident. Other scripts get a property instead.

Properties = getters/setters (like TS `get x()` / Python `@property`):
```csharp
public bool IsHit { get; private set; }                    // anyone reads, only this class writes (VanishAfterHit)
public float Delay { get => delay; set => delay = value; } // wraps a serialized field
public bool VanishEnabled                                  // setter with logic (DominoArena)
{
    get => vanishEnabled;
    set
    {
        vanishEnabled = value;                             // `value` = the assigned value
        foreach (var domino in _dominoes) domino.VanishEnabled = value;
    }
}
```

### 3.2 `=>` on a member is **not a lambda**
```csharp
public int CollidedCount => _collided.Count;            // read-only property, computed every time
void Awake() => Body = GetComponent<Rigidbody>();       // one-line method
void OnHit(VanishAfterHit domino) => SetColor(domino, hitColor);
```
It is shorthand for `{ return ...; }`. A lambda is a value you can pass around (§8).

### 3.3 Optional and named arguments
```csharp
public void Select(int index, bool playEffects = true)   // default value (SelectionPanel)
Select(startIndex, playEffects: false);                  // named argument at the call, like Python kwargs
timeBar.SetValue(timer.Remaining.Normalized, instant: true);
```

### 3.4 `out` parameters and discards
```csharp
if (other.TryGetComponent(out PortalTraveler traveler))   // returns bool AND fills `traveler` (Portal.OnTriggerEnter)
    traveler.Travel(this);
if (!other.TryGetComponent(out CharacterController _))    // `_` = "I only care that it exists" (PortalUnlockSwitch)
    return;
NonOverlapPlacement.TryFindSpot(..., out var position);   // the static helper returns its result through `out`
```
Python would return a tuple `(found, value)`. `out` is C#'s way.

### 3.5 `static` and `readonly`
```csharp
static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");   // one per class, computed once
public static class NonOverlapPlacement { public static bool TryFindSpot(...) }   // helpers, no instance
readonly List<VanishAfterHit> _dominoes = new List<VanishAfterHit>();   // the reference can't change; the list contents can
```

### 3.6 Constructors and `[Serializable]` plain classes (`Meter`, `PingPongGauge`)
```csharp
[Serializable]
public class Meter            // NOT a MonoBehaviour: a plain C# class used as a field
{
    [SerializeField] float max = 100f;
    public Meter() { }        // Unity's serializer needs a no-argument constructor
    public Meter(float max) { this.max = max; current = max; }
}
// used as a field in ShotTimer, shows up in the Inspector as "Time" with Max / Current:
[SerializeField] Meter time = new Meter(120f);
```
`MonoBehaviour`s are **never** created with `new`; Unity creates them (`AddComponent` / scene / `Instantiate`).

### 3.7 ScriptableObject = a data file (`ClubDefinition`)
```csharp
[CreateAssetMenu(menuName = "Project3DGP/Henry 4.E.3/Club Definition", fileName = "Club")]
public class ClubDefinition : ScriptableObject
{
    public string displayName = "Club";
    public float maxDistance = 15f;
    [Range(0f, 1f)] public float accuracy = 0.8f;
}
```
The three clubs are assets in `4E3_GameUI/Data/`. Changing a number there changes the card text and the shot, with no code change.

---

## 4. Null handling (likely question)
```csharp
Hit?.Invoke(this);          // ?. = call only if not null (no subscribers -> nothing happens)
_style ??= new GUIStyle(GUI.skin.box) { fontSize = 18 };   // assign only if null (create once, in OnGUI)
if (hitParticles != null) { ... }   // optional Inspector references are checked explicitly
```
**Unity gotcha:** a destroyed Unity object is "fake null". `== null` is overloaded to handle that, but `?.` and `??` skip the overload. So the code uses `?.` / `??=` only on **plain C# objects** (events, `GUIStyle`) and uses `!= null` for Unity objects (`ParticleSystem`, `AudioSource`, `Renderer`). This is a good detail to mention if asked.

`{ fontSize = 18 }` after `new` is an **object initializer**: it sets properties right after construction.

---

## 5. Collections and loops
```csharp
readonly List<VanishAfterHit> _dominoes = new List<VanishAfterHit>();     // Python list
readonly HashSet<VanishAfterHit> _collided = new HashSet<VanishAfterHit>(); // Python set: Add twice = counted once
Vector3[] _homePositions;                                                   // fixed-size array
public IReadOnlyList<VanishAfterHit> Dominoes => _dominoes;                 // others can read, not Add/Remove

foreach (var domino in arena.Dominoes) { ... }                              // ArenaHud.Update
for (var i = _waitingForSpot.Count - 1; i >= 0; i--)                         // backwards: RemoveAt(i) can't skip one
    if (TryRespawn(_waitingForSpot[i])) _waitingForSpot.RemoveAt(i);
var index = line * dominoesPerLine + k;                                     // 2D (line, k) -> 1D index
Show((_current + 1) % tools.Length);                                        // % wraps 0,1,0,1 (ToolSwitcher)
```

---

## 6. Interfaces (`IPushReceiver`)
```csharp
public interface IPushReceiver { void OnPushed(RigidbodyPusher pusher, Vector3 impulse); }

public class VanishAfterHit : MonoBehaviour, IPushReceiver   // one base class + any number of interfaces
{
    void IPushReceiver.OnPushed(RigidbodyPusher pusher, Vector3 impulse) => MarkHit();   // explicit implementation
}
// RigidbodyPusher doesn't know about dominoes; it asks "does this body want to be told?"
if (body.TryGetComponent(out IPushReceiver receiver)) receiver.OnPushed(this, impulse);
```
Like a TS `interface`, but **nominal**: the class must declare `: IPushReceiver`. The *explicit* form (`void IPushReceiver.OnPushed`) hides the method from normal calls, so only code that holds the interface can call it.

---

## 7. Events and `Action` (the glue in every exercise)

### 7.1 Declare / raise / subscribe
```csharp
public event Action<VanishAfterHit> Hit;      // declare: subscribers get one VanishAfterHit argument
Hit?.Invoke(this);                            // raise (VanishAfterHit.MarkHit)
domino.Hit += OnHit;                          // subscribe with a method (DominoArena.Start)
```
`Action<T>` = a function type `(T) => void` in TS. `Action<float>` = `(distance: number) => void` (`GolfSwing.Shot`).
`event` means outsiders can only `+=` / `-=`; only the owner can `Invoke`.

### 7.2 Every `+=` in `OnEnable` has a `-=` in `OnDisable`
```csharp
void OnEnable()  { timer.Remaining.Changed += OnTimeChanged; swing.Shot += OnShot; }   // GameUiHud
void OnDisable() { timer.Remaining.Changed -= OnTimeChanged; swing.Shot -= OnShot; }
```
Otherwise a destroyed HUD would still be called. **Honest exception:** `DominoArena` subscribes in `Start` and never unsubscribes. That is safe because the arena creates and owns the tiles, so they always have the same lifetime.

### 7.3 Who raises what
| Exercise | Event | Raised by | Listened to by |
|---|---|---|---|
| 4.C.4 | `Hit`, `BodyContact`, `Vanished` | `VanishAfterHit` (each tile) | `DominoArena` (colour, count, respawn) |
| 4.E.3 | `Meter.Changed` | `ShotTimer` via `Meter.Set` | `GameUiHud` (bar, colour, label) |
| 4.E.3 | `GolfSwing.Shot` | `GolfSwing.Swing` | `GameUiHud` ("Last shot") |
| 4.E.3 | `SelectableCard.Clicked` | uGUI Button click | `SelectionPanel` |
| 4.A.3 | `PortalTraveler.Teleported` | `TravelRoutine` | `PortalDemoHud` |
| 4.A.3 | `Portal.Unlocked` | `Portal.Unlock` | nobody yet (ready for the game, #38) |

---

## 8. Lambdas and closures

### 8.1 The lambdas in this code
```csharp
GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke());   // SelectableCard.Awake: forward the click
```
`() => ...` is exactly TS's arrow function.

### 8.2 The closure trap (`SelectionPanel.Start`), a good thing to explain
```csharp
for (var i = 0; i < cards.Count; i++)
{
    var index = i;                          // copy for the lambda: each card remembers its own index
    cards[i].Clicked += () => Select(index);
}
```
A lambda captures the **variable**, not its value. A `for` loop has one `i` shared by all iterations, so `() => Select(i)` would make every card select `cards.Count` (out of range, so nothing happens). The fresh `index` per iteration fixes it. (`foreach` gives a fresh variable per iteration, but `for` doesn't.) Python has the same trap with `lambda: f(i)` in a loop.

---

## 9. Coroutines (used for real in 4.A.3)
A coroutine is a method that runs **across several frames**. `yield return` pauses it; Unity resumes it later. It is built on C# iterators (`IEnumerator`), like Python generators.
```csharp
public void Travel(Portal portal)
{
    if (IsTraveling || Time.time < _readyTime) return;
    StartCoroutine(TravelRoutine(portal));       // starts it; returns at the first yield
}

IEnumerator TravelRoutine(Portal portal)          // PortalTraveler
{
    IsTraveling = true;
    if (fader != null)
        yield return fader.FadeTo(1f, fadeTime); // wait until the nested coroutine finishes (screen black)
    ...move the hamster, burst particles, raise Teleported...
    if (fader != null)
        yield return fader.FadeTo(0f, fadeTime); // fade back in
    IsTraveling = false;
}

public IEnumerator FadeTo(float target, float duration)   // ScreenFader
{
    var start = _group.alpha;
    for (var time = 0f; time < duration; time += Time.unscaledDeltaTime)
    {
        _group.alpha = Mathf.Lerp(start, target, time / duration);
        yield return null;                       // wait one frame
    }
    _group.alpha = target;                       // land exactly on the target
}
```
| `yield return ...` | waits for |
|---|---|
| `null` | the next frame |
| another `IEnumerator` | that one to finish (nested) |
| `new WaitForSeconds(2f)` | 2 s of scaled time (*illustrative, not used here*) |
| `yield break` | ends the coroutine (the commented-out scene-switch block uses it) |

The other exercises use **timers in `Update`** instead (`VanishAfterHit._timeLeft`, `SwingAnimator._time`, `PingPongGauge._phase`). Both are fine. A timer is easier to pause, reset or inspect, while a coroutine reads better for a fixed sequence like fade → move → fade.

---

## 10. Misc syntax you will see
| Code | Meaning |
|---|---|
| `using Random = UnityEngine.Random;` (GolfSwing) | alias: `System` also has a `Random`, so this picks Unity's |
| `a ? b : c ? d : e` (GameUiHud) | nested ternary = if / else if / else |
| `Key.Digit1 + i` (SelectionPanel) | enum arithmetic: Digit1..Digit9 are consecutive values |
| `keyboard[Key.Digit1 + i]`, `Keyboard.current[switchKey]` | indexer: a key looked up by an enum value |
| `float.NegativeInfinity` (VanishAfterHit) | "never happened" timestamp: `Time.time - it` is always huge |
| `Mathf.Approximately(a, b)` | float equality with a tolerance (never use `==` on floats) |
| `GetComponentInChildren<Renderer>(true)` | `true` = also search inactive children |
| `FindObjectsByType<GolfBall>(FindObjectsSortMode.None)` | find all in the scene; slow, so it's called once in `Start` |
| `#if` / `[RuntimeInitializeOnLoadMethod]` | not used in these exercises |

---

## 11. Likely TA questions → short answers
- **"What's the difference between a field and a property?"** A field stores data. A property is a getter/setter pair that can add logic or restrict writing (`{ get; private set; }`).
- **"Why `[SerializeField]` and not `public`?"** It keeps encapsulation and still lets you edit the value in the Inspector.
- **"Why `struct` matters here?"** `Vector3`/`Quaternion` are copied, so you must write the struct back (`BarView.Apply`), and parameters can be changed safely inside a method (`GolfBall.Hit`).
- **"What is `event Action<float>`?"** A list of callbacks with one float parameter. Only the owner can raise it; others subscribe.
- **"What's a coroutine? Is it a thread?"** **No.** It runs on the main thread. Unity resumes it once per frame after `Update`. It splits work across frames, it is not parallel.
- **"Why `Meter` isn't a MonoBehaviour?"** It's pure data + logic with no transform. A `[Serializable]` class can be a field of any component and still be edited in the Inspector. One class covers time, health, stamina and XP.
- **"Why an interface for push receivers?"** The pusher (Core) mustn't depend on domino code. Any component that implements `IPushReceiver` gets notified.
