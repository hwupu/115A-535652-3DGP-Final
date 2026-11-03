using System.Collections.Generic;
using Project3DGP.Core.UI;
using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes > Apply Farm UI Theme (#74): restyles the placeholder uGUI of the open scene (built by GameUiBuilder
    /// and the power meter tool) with the "Farm Game UI - Starter 2D" sprites and the Lilita One font. Only looks
    /// change: sprites, colours, font, text offsets. No object is created or removed except a Shadow on button labels,
    /// and every reference, listener and layout stays. Safe to run again (same result).
    ///   Dialog boxes, the dialog bar, the minimap frame  cream panel, dark brown text
    ///   Buttons                                           coloured 3D button with a pressed sprite (colour by label, Styles)
    ///   Time bar, XP bar, power meter, sliders            brown track with a green fill
    ///   Club cards                                        cream frame (tinted by SelectableCard) on a brown card
    ///   Other text (HUD over the 3D view)                 white with a dark shadow
    /// MainMenu / Ending also get the Kenney sky (day / morning, CC0) behind the menu: the flat Background image is
    /// switched off (not deleted), so the camera's skybox shows. One Undo step; the scene is marked dirty, not saved.
    /// The font is the committed OFL copy in Assets/ThirdParty/LilitaOne, so text still shows without the pack.
    /// </summary>
    public static class FarmUiThemeMenu
    {
        const string FontPath = "Assets/ThirdParty/LilitaOne/LilitaOne-Regular.ttf";   // SIL OFL 1.1, docs/assets.md

        static readonly Color Brown = new(0.4f, 0.25f, 0.13f);
        static readonly Color CardBrown = new(0.52f, 0.36f, 0.24f, 0.96f);
        static readonly Color TextShadow = new(0.2f, 0.12f, 0.05f, 0.85f);
        static readonly Color ButtonTextShadow = new(0f, 0f, 0f, 0.3f);
        const float ButtonLip = 24f;   // px of the 3D bottom edge in the 256 px button sprites

        // Button colour by its label. Anything else is yellow.
        static readonly Dictionary<string, string> Styles = new()
        {
            { "Start", "green" }, { "Resume", "green" }, { "Next", "green" }, { "Retry", "green" }, { "Retry level", "green" },
            { "Quit", "red" }, { "Close", "red" }, { "Overwrite", "red" },
            { "Back", "gray" }, { "Cancel", "gray" }, { "Skip", "gray" },
            { "Menu", "blue" },
        };

        static readonly Dictionary<string, (string Normal, string Pressed)> ButtonSprites = new()
        {
            { "green", (AssetStorePacks.FarmUi.ButtonGreen, AssetStorePacks.FarmUi.ButtonGreenPressed) },
            { "yellow", (AssetStorePacks.FarmUi.ButtonYellow, AssetStorePacks.FarmUi.ButtonYellowPressed) },
            { "red", (AssetStorePacks.FarmUi.ButtonRed, AssetStorePacks.FarmUi.ButtonRedPressed) },
            { "gray", (AssetStorePacks.FarmUi.ButtonGray, AssetStorePacks.FarmUi.ButtonGrayPressed) },
            { "blue", (AssetStorePacks.FarmUi.ButtonBlue, AssetStorePacks.FarmUi.ButtonBluePressed) },
        };

        static Sprite S(string guid) => AssetStorePacks.Load<Sprite>(guid);

        [MenuItem(MagicMenuUtil.Root + "Scenes/Apply Farm UI Theme", priority = MagicMenuUtil.ScenesPriority + 5)]
        static void Apply()
        {
            if (!AssetStorePacks.Require(AssetStorePacks.FarmUi.Name, AssetStorePacks.FarmUi.Panel))
                return;
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                MagicMenuUtil.Log($"Missing {FontPath} (pull main), then run this again.");
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            var canvases = new List<Canvas>();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                    if (canvas.transform.parent == null || canvas.transform.parent.GetComponentInParent<Canvas>(true) == null)
                        canvases.Add(canvas);
            if (canvases.Count == 0)
            {
                MagicMenuUtil.Log($"No UI in {scene.name}: nothing to theme.");
                return;
            }

            Undo.SetCurrentGroupName("Apply Farm UI Theme");
            var undoGroup = Undo.GetCurrentGroup();
            var light = new HashSet<Transform>();   // cream panels: their text turns brown
            int panels = 0, buttons = 0, bars = 0, texts = 0;

            foreach (var canvas in canvases)
            {
                // Dialog boxes: a "Box" on a full-screen dim layer (GameUiBuilder.Dialog).
                foreach (var image in canvas.GetComponentsInChildren<Image>(true))
                {
                    var parent = image.transform.parent;
                    if (image.name == "Box" && parent != null && parent.GetComponent<Image>() != null)
                    {
                        Sliced(image, S(AssetStorePacks.FarmUi.Panel), Color.white, Size(image));
                        light.Add(image.transform);
                        panels++;
                    }
                }

                foreach (var bar in canvas.GetComponentsInChildren<DialogBar>(true))
                {
                    var panel = (GameObject)new SerializedObject(bar).FindProperty("panel").objectReferenceValue;
                    if (panel != null && panel.TryGetComponent(out Image image))
                    {
                        Sliced(image, S(AssetStorePacks.FarmUi.Panel), new Color(1f, 1f, 1f, 0.96f), Size(image));
                        light.Add(panel.transform);
                        panels++;
                    }
                }

                var minimap = canvas.transform.Find("Minimap");
                if (minimap != null && minimap.TryGetComponent(out Image frame))
                {
                    Sliced(frame, S(AssetStorePacks.FarmUi.Panel), Color.white, Size(frame), 3f);
                    panels++;
                }

                foreach (var card in canvas.GetComponentsInChildren<SelectableCard>(true))
                {
                    var so = new SerializedObject(card);
                    if (so.FindProperty("frame").objectReferenceValue is Image cardFrame)
                        Sliced(cardFrame, S(AssetStorePacks.FarmUi.Panel), cardFrame.color, Size(cardFrame), 2f);
                    var background = card.transform.Find("Background");
                    if (background != null && background.TryGetComponent(out Image cardBackground))
                        Sliced(cardBackground, S(AssetStorePacks.FarmUi.Panel), CardBrown, Size(cardBackground), 2f);
                    so.FindProperty("normalFrameColor").colorValue = new Color(1f, 1f, 1f, 0.55f);
                    so.ApplyModifiedProperties();
                    panels++;
                }

                foreach (var bar in canvas.GetComponentsInChildren<BarView>(true))
                {
                    if (bar.TryGetComponent(out Image track))
                        Sliced(track, S(AssetStorePacks.FarmUi.SliderBackground), Color.white, Size(track));
                    var fill = (RectTransform)new SerializedObject(bar).FindProperty("fill").objectReferenceValue;
                    if (fill != null && fill.TryGetComponent(out Image fillImage))
                        Sliced(fillImage, S(AssetStorePacks.FarmUi.SliderFill), Color.white, FullSize(fill, track));
                    if (bar.TryGetComponent(out PowerMeterView meter))
                    {
                        var so = new SerializedObject(meter);
                        so.FindProperty("aimingColor").colorValue = Color.white;               // the sprite is already green
                        so.FindProperty("frozenColor").colorValue = new Color(1f, 0.75f, 0.45f);
                        so.ApplyModifiedProperties();
                    }
                    bars++;
                }

                foreach (var slider in canvas.GetComponentsInChildren<Slider>(true))
                {
                    var background = slider.transform.Find("Background");
                    if (background != null && background.TryGetComponent(out Image track))
                        Sliced(track, S(AssetStorePacks.FarmUi.SliderBackground), Color.white, Size(track));
                    if (slider.fillRect != null && slider.fillRect.TryGetComponent(out Image fillImage))
                        Sliced(fillImage, S(AssetStorePacks.FarmUi.SliderFill), Color.white, Size((RectTransform)slider.transform));
                    if (slider.handleRect != null && slider.handleRect.TryGetComponent(out Image handle))
                    {
                        Undo.RecordObject(slider.handleRect, "Theme Slider Handle");
                        slider.handleRect.sizeDelta = new Vector2(36f, 16f);
                        Sliced(handle, S(AssetStorePacks.FarmUi.ButtonYellow), Color.white, new Vector2(36f, 46f));
                    }
                    bars++;
                }

                foreach (var button in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button.GetComponent<SelectableCard>() != null || !(button.targetGraphic is Image image))
                        continue;
                    ThemeButton(button, image);
                    buttons++;
                }

                foreach (var text in canvas.GetComponentsInChildren<Text>(true))
                {
                    ThemeText(text, font, light);
                    texts++;
                }
            }

            var sky = MenuSky(scene.name, canvases);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Farm UI theme in {scene.name}: {panels} panels, {buttons} buttons, {bars} bars and sliders, {texts} texts{sky}. Save the scene.");
        }

        static void ThemeButton(Button button, Image image)
        {
            var label = button.GetComponentInChildren<Text>(true);
            var key = label != null && !string.IsNullOrEmpty(label.text) ? label.text : button.name;
            var style = Styles.TryGetValue(key, out var s) ? s : "yellow";
            var sprites = ButtonSprites[style];

            var multiplier = Sliced(image, S(sprites.Normal), Color.white, Size(image));
            Undo.RecordObject(button, "Theme Button");
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                pressedSprite = S(sprites.Pressed),
                disabledSprite = S(AssetStorePacks.FarmUi.ButtonGray),
            };

            // Centre the label on the top face, above the 3D bottom edge.
            if (label != null && label.transform.parent == button.transform)
            {
                var rect = label.rectTransform;
                Undo.RecordObject(rect, "Theme Button Label");
                rect.offsetMin = new Vector2(rect.offsetMin.x, ButtonLip / multiplier);
                if (!label.TryGetComponent(out Shadow _))
                    Undo.AddComponent<Shadow>(label.gameObject);
            }
        }

        static void ThemeText(Text text, Font font, HashSet<Transform> light)
        {
            Undo.RecordObject(text, "Theme Text");
            text.font = font;
            var inButton = text.GetComponentInParent<Button>(true) != null;
            var onCream = !inButton && IsUnder(text.transform, light);
            text.color = onCream ? new Color(Brown.r, Brown.g, Brown.b, text.color.a) : new Color(1f, 1f, 1f, text.color.a);

            if (text.TryGetComponent(out Shadow shadow))
            {
                Undo.RecordObject(shadow, "Theme Text Shadow");
                shadow.enabled = !onCream;
                shadow.effectColor = inButton ? ButtonTextShadow : TextShadow;
                shadow.effectDistance = new Vector2(2f, -2f);
            }
        }

        // MainMenu / Ending: the Kenney sky behind the menu, if the scene still has Unity's default sky.
        static string MenuSky(string sceneName, List<Canvas> canvases)
        {
            var name = sceneName == GameScenes.MainMenu ? "day" : sceneName == GameScenes.Ending ? "morning" : null;
            if (name == null)
                return "";
            var material = KenneySky.Get(name);
            if (material == null)
                return "";
            var result = "";
            if (RenderSettings.skybox == null || RenderSettings.skybox.name == "Default-Skybox")
            {
                RenderSettings.skybox = material;
                result = ", sky " + material.name;
            }
            foreach (var canvas in canvases)
            {
                var background = canvas.transform.Find("Background");
                if (background != null && background.TryGetComponent(out Image image) && image.enabled)
                {
                    Undo.RecordObject(image, "Hide Flat Background");
                    image.enabled = false;
                    result += ", flat background hidden";
                }
            }
            return result;
        }

        /// <summary>
        /// Sliced sprite whose corners fit the rect: the pixels-per-unit multiplier grows until the borders fit
        /// (times <paramref name="extra"/> for thinner frames). Returns the multiplier.
        /// </summary>
        static float Sliced(Image image, Sprite sprite, Color color, Vector2 size, float extra = 1f)
        {
            Undo.RecordObject(image, "Theme Image");
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.color = color;
            var multiplier = 1f;
            if (sprite != null)
            {
                var border = sprite.border;   // x left, y bottom, z right, w top (px)
                if (size.x > 1f) multiplier = Mathf.Max(multiplier, (border.x + border.z) / size.x);
                if (size.y > 1f) multiplier = Mathf.Max(multiplier, (border.y + border.w) / size.y);
            }
            image.pixelsPerUnitMultiplier = multiplier * extra;
            return multiplier * extra;
        }

        static Vector2 Size(Graphic graphic) => Size(graphic.rectTransform);
        static Vector2 Size(RectTransform rect) => rect.rect.size;

        // A bar fill's full size (its anchors move with the value): the track's size minus the fill's insets.
        static Vector2 FullSize(RectTransform fill, Image track)
        {
            var full = track != null ? track.rectTransform.rect.size : fill.rect.size;
            return full - (fill.offsetMin - fill.offsetMax);
        }

        static bool IsUnder(Transform t, HashSet<Transform> set)
        {
            for (var c = t; c != null; c = c.parent)
                if (set.Contains(c))
                    return true;
            return false;
        }
    }
}
