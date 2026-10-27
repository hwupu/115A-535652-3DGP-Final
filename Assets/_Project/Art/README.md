# Art: our own models (#80)

Models we make ourselves (Blender). Each counts for Form B "Art work" (>= 1 per member, >= 3 total) and goes in the report.

| Model | Folder | Replaces the placeholder in | Owner |
|---|---|---|---|
| Hamster hero | `Characters/Hamster/` | `Prefabs/Characters/Hamster.prefab` | Henry |
| Hamster princess | `Characters/Princess/` | `Prefabs/Characters/Princess.prefab` | Henry |
| Standard club, Sledgehammer, Mysterious | `Tools/` | `Prefabs/Tools/*.prefab` | Henry |

## Files
- **Unity files** (in the model's folder): `Hamster.fbx`, textures as `.png`. Both go through Git LFS automatically.
- **Blender source** (`.blend`) in the model's `Source~/` folder. Unity ignores folders ending in `~`, so it never tries to import the `.blend` (that would need Blender on every machine), but git still keeps it (LFS).
- Keep files small (1 GB LFS quota for the whole team): no 4K textures, no high-poly sculpts. Low-poly + flat colours fits our style.

## Conventions (so the model drops into the prefab without code changes)
- **Scale:** 1 Blender unit = 1 m. Hamster about 1.3 m tall, princess the same; tools about 0.45 m (they sit in the hand).
- **Facing:** the character looks along Unity +Z. In Blender, model it facing the front view (-Y).
- **Pivot:** characters: between the feet at ground level. Tools: at the grip, the tool hanging down.
- **Hamster right hand:** add an empty (or bone) named `ToolSocket` in the right hand, where the grip goes. The avatar parents the tool there (#88).
- **FBX export** (File > Export > FBX): Selected Objects; Forward **-Z**, Up **Y**; Apply Scalings **FBX All**; **Apply Transform** on. Check in Unity that it faces +Z and is the right size.
- Rig and animation clips come later in #36 (`docs/art/hamster-animation-handoff.md`); a static model is fine for #80.

## Swap the placeholder for your model
1. Drag the `.fbx` into the model's folder. In its Import Settings: Scale Factor 1, Materials > Location: Use External Materials if you want to edit them in Unity.
2. Open the prefab (double-click), drag the FBX in as a child of the root, and line it up with the old `Visual`.
3. Delete the old `Visual` child and rename your model to `Visual`. For the hamster, make sure `ToolSocket` is inside the right hand.
4. Save the prefab. Every scene using it updates.
5. Add a row to `CREDITS.md` and `docs/assets.md` as **own work** (name, author = you, "made for this project").
