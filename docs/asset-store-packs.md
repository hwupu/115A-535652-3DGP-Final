# Asset Store packs: install them yourself

Some of our UI and level art comes from free **Unity Asset Store** packs. Their license doesn't allow sharing them, so **they are never in git** (CLAUDE.md rule 6). Our scenes and prefabs still use them: they point to the pack files by an ID (GUID) that is the same on every computer. **Each of us imports the packs once.** Without them you'll see white boxes instead of buttons, an empty sky, plain terrain and invisible trees. Nothing breaks, and the text still shows because the font is committed.

## The packs

| Pack | Publisher | Version | Store page | Used for | What to import |
|---|---|---|---|---|---|
| Farm Game UI - Starter 2D | maanetorn | 1.0 | https://assetstore.unity.com/packages/slug/318607 | Buttons, panels, bars of every menu and the HUD | Everything |
| Free Stylized Hand-Painted Skybox | BD Studios | 1.0 | https://assetstore.unity.com/packages/slug/265475 | Sky in L1, MainMenu, Ending | Everything (you may skip `Demo.unity`) |
| Handpainted Grass & Ground Textures | Chromisu | 1.4 | https://assetstore.unity.com/packages/slug/187634 | L1 terrain (grass, fairway, rough, dirt) | Only the **`Textures`** folder (Grass, Dirt, Snow). The rest is big and not needed |
| Pandazole - Nature Environment Low poly Pack | Pandazole | 1.0.0 | https://assetstore.unity.com/packages/slug/212621 | Trees, bushes, rocks, flowers, mushrooms, grass in L1 | Everything |

All four are free. Total on disk: about 210 MB.

## How to install (about 10 minutes, once)
1. In a browser, open each store page above, sign in with your Unity ID and click **Add to My Assets**. It's free, but every member has to claim it on their own account.
2. In Unity: **Window > Package Manager**. At the top left choose **My Assets**. Sign in with the same Unity ID if asked.
3. For each pack: select it, click **Download**, then **Import**.
   - The import window lists the files. Leave everything ticked, **except for the Chromisu textures**: untick everything, then tick only the `Textures` folder.
   - If Unity asks to install **package dependencies** (Post Processing, Shader Graph), click **Install/Import**. They are already in `Packages/manifest.json`, so nothing changes.
4. Unity imports each pack into a folder at the top of `Assets/`. Move them under `Assets/AssetStore/` so they are clearly local-only. In the **Project** window, create the folder `Assets/AssetStore` if it's missing and drag these four folders into it:
   - `Assets/maanetorn`
   - `Assets/Free Stylized Hand-Painted Skybox`
   - `Assets/Handpainted_Grass_and_Ground_Textures`
   - `Assets/Pandazole_Ultimate_Pack`

   Always move them **inside Unity**, never in Finder or Explorer. Unity keeps their IDs, so all references still work. `.gitignore` ignores these folders in both places, so they can't be committed by accident.
5. Check: open `Scenes/L1_Garden`. You should see the cream and green buttons, the painted sky, the textured terrain and the trees. In **Source Control** (or `git status`) none of the pack files may show up.

## Rules
- **Never commit** a pack file, including any `.meta` file inside a pack folder. If `git status` lists one, stop and ask.
- **Don't edit pack files.** Copy what you need into `Assets/_Project/`, or reference it from our own asset: for example `Materials/Nature/PandaNature.mat` uses the Pandazole texture with a URP shader, because the pack's own material is for the Built-in pipeline.
- **Adding a new pack:** add a row here, in [`assets.md`](assets.md) and in [`CREDITS.md`](../CREDITS.md) in the same PR, with the version, and tell the team to import it.
- The Magic tools that use a pack (*Apply Farm UI Theme*, *Use Pandazole Nature Models*, *Garden: Sculpt and Paint Terrain*, *Garden: Sky, Fog and Sun*) stop with a `[Magic]` message that points here if the pack is missing.
