# Character animation handoff (#36, #37)

The three characters are Tripo3D models (CC BY 4.0, ADR-0013), rigged and animated **by script** in Blender 5.2 so they can be rebuilt any time. Unity setup is one click: *Magic > Prefabs > Use Character Models*.

## Files
| What | Where |
|---|---|
| Unity model (mesh + rig + all clips) | `Assets/_Project/Art/Characters/<Name>/<Name>.fbx` |
| Base colour texture (2048 JPG) | `.../<Name>/<Name>_BaseColor.jpg` |
| Material, Animator controller (made by the Magic tool) | `.../<Name>/<Name>.mat`, `<Name>.controller` |
| Blender source | `.../<Name>/Source~/<Name>.blend` |
| Pipeline scripts | `.../Characters/BlenderScripts~/rig.py`, `anim.py`, `strip.py` |

## Rebuilding (after changing a pose or a landmark)
Tripo zips unpacked to `src/<zip name>/`; `B` = `~/Applications/Blender.app/Contents/MacOS/Blender`:
```sh
$B -b --factory-startup --python rig.py  -- Hamster src/prince_hamster/<file>.fbx work   # decimate, scale, rig, skin
$B -b --factory-startup --python anim.py -- Hamster work out sheet                       # clips, FBX, texture, contact sheet
```
Same for `Princess princess_hamster` and `Cat sir_grumblepaws`. Copy `out/<Name>.fbx` and `<Name>.blend` (to `Source~`) back. If you change clips, update the clip table in `Editor/Magic/CharacterModelsMenu.cs` and, for an FBX that is already set up, clear its clip list in the Import Settings (or re-run on a fresh FBX) so the tool configures it again.

`rig.py`: imports the Tripo FBX, decimates to 15 000 triangles (flat shaded), scales to the game height with the feet at the origin, places the bones from landmarks measured on the mesh (`CONFIGS`: hip, neck, head top, shoulder, foot, tail and ear regions, in height units), skins with Blender's automatic (heat) weights, fills any unweighted vertex from the nearest bone, limits to 4 influences. `anim.py`: keys each clip as poses (rotations in the armature's rest axes), exports.

## Conventions
- 1 unit = 1 m. Heights: hamster 1.3 m, princess 1.3 m, cat 1.8 m. Feet at the origin.
- Blender: faces −Y. FBX: Forward −Z, Up Y, FBX scale all, no leaf bones. Unity imports with *Bake Axis Conversion*; the model then faces −Z, so the Magic tool turns `Visual` 180° (it checks the `Hand_L` side).
- **Generic** rig (not Humanoid), root motion off, 24 fps. Default pose = T-pose (an extra `_Rest` take keeps it; unused).
- Bones: `Root`, `Hips`, `Spine`, `Chest`, `Head`, `UpperArm_L/R`, `LowerArm_L/R`, `Hand_L/R`, `Leg_L/R`, `Foot_L/R`; princess and cat add `Tail_1..3`, the cat `Ear_L/R`.
- `ToolSocket` (hamster): made by the Magic tool under `Hand_R`, the tool pointing along the arm.

## Clips (frames at 24 fps, Blender frame 1 = Unity frame 0)
**Hamster** (Animator: triggers `Swing` + int `Tool`, `Equip`, `Celebrate`, `Fail`, `Flinch`; float `Speed`; bools `Aiming`, `Walking`)

| Clip | Frames | Loop | Notes |
|---|---|---|---|
| Idle | 1–49 | yes | arms down, breathing |
| Walk | 1–17 | yes | two steps, arms swing; Speed > 0.15 |
| AimIdle | 1–49 | yes | bent over the ball (front right), small waggle; while `Aiming` |
| Swing_Club | 1–36 | no | backswing 7, **impact 10** (`OnSwingImpact`), follow-through 15–24 |
| Swing_Hammer | 1–40 | no | overhead 11–13, **impact 16** |
| Swing_Mysterious | 1–22 | no | small tap, **impact 9** |
| Equip | 1–13 | no | club switched |
| Celebrate | 1–37 | yes | jump, arms up |
| Fail | 1–24 | no | scared lean back, hands up; holds the last pose |
| Flinch | 1–12 | no | optional (wind / cat reaction), trigger `Flinch` |

The ball leaves on the click (ShotController); the clip's impact frame is 0.33–0.63 s later. `GolferAvatar.SwingImpact` fires on that frame for hit sounds / effects.

**Princess** (triggers `Wave`, `Cheer`; bool `Scared`; float `Speed`): Idle 1–73 loop (sway, tail) · Walk 1–17 loop · Wave 1–56 once · Cheer 1–37 loop · Scared 1–25 loop (trembling, hands to face). No game code drives her yet.

**Cat** (int `State` = `CatState`, set by `GardenCat`; 0.4 s blends): Asleep 1–97 (sitting, head drooped, slow breathing) · Twitching 1–49 (sitting, ear flicks, tail swish) · Suspicious 1–97 (sitting up, looks left / right) · Searching 1–97 (standing, crouched, scanning) · Awake 1–49 (standing, paws raised and shaking, tail up). All loop. `GardenCat` adds a head turn toward the ball while searching / awake.

## Known limits
- The eyes are painted in the texture, so the sleeping cat's eyes stay open (head drooped + Zzz). Eyelid meshes or a closed-eyes texture would fix it.
- The hamster's arms are short and partly under the cape; the swing reads mostly from the body twist and the tool.
- Automatic weights on AI meshes: skirts and capes follow the hips / legs, so the princess's dress bends a little when she jumps.
