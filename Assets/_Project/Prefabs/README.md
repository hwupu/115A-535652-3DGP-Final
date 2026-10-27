# Prefabs

| Folder | What | Made by |
|---|---|---|
| `Shared/` | Game prefabs every level uses: `GolfBall`, `GolferAvatar` | Magic > Game > ... |
| `Characters/`, `Tools/`, `Course/`, `Nature/` | Placeholder models (primitives) | Magic > Prefabs > Create Placeholder Prefabs |

Every prefab keeps its look in a child named **`Visual`**; colliders and scripts sit on the root.

## Replace a placeholder with a real model
1. Import the model (`.fbx`) into `Assets/_Project/Art/...` (own models, #80) or `Assets/ThirdParty/<Pack>/` (credited).
2. Open the placeholder prefab, drag the model in as a child of the root and line it up with the old `Visual` (feet / grip at the origin, facing +Z).
3. Delete the old `Visual` and rename the model to `Visual`. Save. Every scene using the prefab updates.

**Hamster (golfer avatar):** `Shared/GolferAvatar` is a *variant* of `Characters/Hamster`, so only the Hamster needs the swap. Keep an empty named **`ToolSocket`** in the model's right hand (the held tool is placed there), and a pivot named `ShoulderR` if you want the code swing fallback before the Animator exists. With an Animator (#36), the avatar uses its triggers `Swing`, `Celebrate`, `Fail` instead. No script changes are needed.

**Tools:** the grip is the prefab's origin and the tool hangs down (-Y). `Settings/Clubs/*.asset` point at them (`ToolPrefab`).
