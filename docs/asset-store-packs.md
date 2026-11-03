# Asset Store packs: install them yourself

Some of our UI art comes from a free **Unity Asset Store** pack. Its license doesn't allow sharing it, so **it is never in git** (CLAUDE.md rule 6). Our scenes still use it: they point to the pack files by an ID (GUID) that is the same on every computer. **Each of us imports it once.** Without it, buttons and panels show as white boxes. Nothing breaks, and the text still shows because the font is committed.

The level art (Kenney kits, CC0) *is* committed in `Assets/ThirdParty/Kenney`, so there's nothing to install for it.

## The pack

| Pack | Publisher | Version | Store page | Used for | What to import |
|---|---|---|---|---|---|
| Farm Game UI - Starter 2D | maanetorn | 1.0 | https://assetstore.unity.com/packages/slug/318607 | Buttons, panels and bars of every menu and the HUD | Everything |

It's free, about 100 MB on disk.

## How to install (about 5 minutes, once)
1. In a browser, open the store page above, sign in with your Unity ID and click **Add to My Assets**. It's free, but each member has to claim it on their own account.
2. In Unity: **Window > Package Manager**. At the top left choose **My Assets**, and sign in with the same Unity ID if asked.
3. Select the pack, click **Download**, then **Import**, and leave everything ticked.
4. Unity imports it into `Assets/maanetorn`. Move it under `Assets/AssetStore/` so it's clearly local-only: in the **Project** window, create `Assets/AssetStore` if it's missing and drag `maanetorn` into it. Always move it **inside Unity**, never in Finder or Explorer, so its IDs stay the same. `.gitignore` ignores it in both places.
5. Check: open `Scenes/MainMenu`. The buttons should be green, yellow and red. In **Source Control** (or `git status`), no pack files may show up.

## Rules
- **Never commit** a pack file, including any `.meta` file inside a pack folder. If `git status` lists one, stop and ask.
- **Don't edit pack files.** Copy what you need into `Assets/_Project/`, or reference it from our own asset.
- **Adding a new pack:** in the same PR, add a row here, in [`assets.md`](assets.md) and in [`CREDITS.md`](../CREDITS.md) with its version, and tell the team to import it.
- *Magic > Scenes > Apply Farm UI Theme* stops with a `[Magic]` message pointing here if the pack is missing.
