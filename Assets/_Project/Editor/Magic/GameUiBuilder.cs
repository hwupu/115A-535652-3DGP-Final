using Project3DGP.Core.UI;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Builds the placeholder uGUI of the HUD and the level dialogs (#20, architecture §4.8) for the Magic tools
    /// (Add HUD and Menus, Open UI Test Area). Plain coloured rects and legacy Text; Henry's art replaces the
    /// sprites later without code changes. Buttons get persistent listeners, so the Inspector shows what each does.
    /// The caller registers Undo for the returned roots.
    /// </summary>
    internal static class GameUiBuilder
    {
        static readonly Color PanelColor = new(0.12f, 0.12f, 0.16f, 0.95f);
        static readonly Color DimColor = new(0f, 0f, 0f, 0.55f);

        static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static GameObject NewCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return go;
        }

        /// <summary>Adds an EventSystem (Input System UI module) under the parent if the scene has none.</summary>
        public static GameObject EnsureEventSystem(Transform parent)
        {
            if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
                return null;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        /// <summary>
        /// A full-screen "HudView" child of the canvas: upper left the stage icon + time bar, strokes below; upper
        /// right the menu button. Returns the HudView (on that child; register Undo for its GameObject).
        /// </summary>
        public static HudView BuildHud(GameObject canvas, MonoBehaviour flow)
        {
            var root = Stretch("HudView", canvas.transform, 0f);
            var timer = NewRect("Timer", root, new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(520f, 96f));

            var icon = NewRect("StageIcon", timer, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(96f, 96f)).gameObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var barRect = NewRect("TimeBar", timer, new Vector2(0f, 0.5f), new Vector2(116f, 0f), new Vector2(400f, 32f));
            barRect.pivot = new Vector2(0f, 0.5f);
            var background = barRect.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
            background.raycastTarget = false;
            var fill = Stretch("Fill", barRect, 4f);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.95f, 0.85f, 0.4f);
            fillImage.raycastTarget = false;
            var bar = barRect.gameObject.AddComponent<BarView>();
            var barSo = new SerializedObject(bar);
            barSo.FindProperty("fill").objectReferenceValue = fill;
            barSo.FindProperty("smoothTime").floatValue = 0.25f;
            barSo.ApplyModifiedPropertiesWithoutUndo();

            var strokes = Label("Strokes", root, "Strokes: 0", 30, TextAnchor.UpperLeft);
            Place(strokes.rectTransform, new Vector2(0f, 1f), new Vector2(156f, -132f), new Vector2(360f, 44f));

            var hud = root.gameObject.AddComponent<HudView>();
            var menuButton = NewButton("MenuButton", root, "Menu", hud.OpenMenu);
            Place((RectTransform)menuButton.transform, new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(160f, 64f));

            var so = new SerializedObject(hud);
            so.FindProperty("flow").objectReferenceValue = flow;
            so.FindProperty("timerGroup").objectReferenceValue = timer.gameObject;
            so.FindProperty("timeBar").objectReferenceValue = bar;
            so.FindProperty("stageIcon").objectReferenceValue = icon;
            so.FindProperty("strokesLabel").objectReferenceValue = strokes;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        /// <summary>
        /// The XP bar (#78) under the strokes, upper left: "Lv 1" and a thin bar with "0 / 100 XP" on it.
        /// Returns the XpBarView (on an "XpBar" child of the canvas; register Undo for its GameObject).
        /// </summary>
        public static XpBarView BuildXpBar(GameObject canvas)
        {
            var root = NewRect("XpBar", canvas.transform, new Vector2(0f, 1f), new Vector2(40f, -186f), new Vector2(520f, 40f));
            var level = Label("Level", root, "Lv 1", 30, TextAnchor.MiddleCenter);
            Place(level.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(96f, 40f));

            var barRect = NewRect("Bar", root, new Vector2(0f, 0.5f), new Vector2(116f, 0f), new Vector2(400f, 26f));
            var background = barRect.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
            background.raycastTarget = false;
            var fill = Stretch("Fill", barRect, 3f);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.45f, 0.75f, 1f);
            fillImage.raycastTarget = false;
            var text = Label("Label", barRect, "", 18, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            var bar = barRect.gameObject.AddComponent<BarView>();
            var barSo = new SerializedObject(bar);
            barSo.FindProperty("fill").objectReferenceValue = fill;
            barSo.FindProperty("label").objectReferenceValue = text;
            barSo.FindProperty("smoothTime").floatValue = 0.4f;
            barSo.ApplyModifiedPropertiesWithoutUndo();

            var view = root.gameObject.AddComponent<XpBarView>();
            var so = new SerializedObject(view);
            so.FindProperty("bar").objectReferenceValue = bar;
            so.FindProperty("levelLabel").objectReferenceValue = level;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>
        /// The minimap frame (#78), upper right under the menu button: a RawImage showing the minimap camera's texture.
        /// Returns the frame (a "Minimap" child of the canvas; register Undo for it).
        /// </summary>
        public static GameObject BuildMinimapFrame(GameObject canvas, Texture texture)
        {
            var frame = NewRect("Minimap", canvas.transform, new Vector2(1f, 1f), new Vector2(-40f, -110f), new Vector2(280f, 280f));
            var border = frame.gameObject.AddComponent<Image>();
            border.color = new Color(1f, 1f, 1f, 0.8f);
            border.raycastTarget = false;
            var map = Stretch("Map", frame, 5f).gameObject.AddComponent<RawImage>();
            map.texture = texture;
            map.raycastTarget = false;
            return frame.gameObject;
        }

        /// <summary>
        /// The club cards (#78), right side, centred: one card per club slot (<paramref name="count"/>), with the
        /// select sound. Returns the ClubPanelView (on a "ClubPanel" child of the canvas; register Undo for it).
        /// </summary>
        public static ClubPanelView BuildClubPanel(GameObject canvas, int count, AudioClip selectSound)
        {
            const float cardWidth = 280f, cardHeight = 90f, gap = 12f;
            var height = count * cardHeight + (count - 1) * gap;
            var root = NewRect("ClubPanel", canvas.transform, new Vector2(1f, 0.5f), new Vector2(-40f, -50f), new Vector2(cardWidth, height));
            var audio = root.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            var view = root.gameObject.AddComponent<ClubPanelView>();

            var so = new SerializedObject(view);
            var list = so.FindProperty("cards");
            list.arraySize = count;
            for (var i = 0; i < count; i++)
            {
                var card = ClubCard(root, i, new Vector2(0f, -i * (cardHeight + gap)), new Vector2(cardWidth, cardHeight));
                list.GetArrayElementAtIndex(i).objectReferenceValue = card;
            }
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.FindProperty("selectSound").objectReferenceValue = selectSound;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        // Card = frame (tinted when selected) + dark background + colour square + title + numbers; the frame takes the click.
        static SelectableCard ClubCard(Transform parent, int index, Vector2 position, Vector2 size)
        {
            var frame = NewRect($"Card_{index + 1}", parent, new Vector2(0.5f, 1f), position, size);
            var frameImage = frame.gameObject.AddComponent<Image>();
            frameImage.color = new Color(1f, 1f, 1f, 0.25f);
            var background = Stretch("Background", frame, 4f).gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
            background.raycastTarget = false;
            var icon = NewRect("Icon", frame, new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(48f, 48f)).gameObject.AddComponent<Image>();
            icon.raycastTarget = false;
            var title = Label("Title", frame, "Club", 24, TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(76f, -10f), new Vector2(196f, 30f));
            var details = Label("Details", frame, "", 17, TextAnchor.UpperLeft);
            Place(details.rectTransform, new Vector2(0f, 1f), new Vector2(76f, -42f), new Vector2(196f, 44f));

            var button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = frameImage;
            button.transition = Selectable.Transition.None;                       // the card draws its own highlight
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            var card = frame.gameObject.AddComponent<SelectableCard>();
            var so = new SerializedObject(card);
            so.FindProperty("frame").objectReferenceValue = frameImage;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("details").objectReferenceValue = details;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        /// <summary>
        /// The HUD's dialog bar (bottom center): tutorial hints (#28) and story lines (#51). Click it to skip a line.
        /// Returns the DialogBar (on a "DialogBar" child of the canvas; register Undo for its GameObject).
        /// </summary>
        public static DialogBar BuildDialogBar(GameObject canvas)
        {
            var root = Stretch("DialogBar", canvas.transform, 0f);
            var panel = NewRect("Panel", root, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1100f, 120f));
            panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);   // catches the skip click
            var text = Label("Text", panel, "", 30, TextAnchor.MiddleCenter);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f, 24f);
            textRect.offsetMax = new Vector2(-30f, -12f);
            var hint = Label("SkipHint", panel, "click to skip", 18, TextAnchor.LowerRight);
            Place(hint.rectTransform, new Vector2(1f, 0f), new Vector2(-12f, 6f), new Vector2(200f, 24f));
            hint.color = new Color(1f, 1f, 1f, 0.6f);

            var bar = root.gameObject.AddComponent<DialogBar>();
            var so = new SerializedObject(bar);
            so.FindProperty("panel").objectReferenceValue = panel.gameObject;
            so.FindProperty("label").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            panel.gameObject.SetActive(false);
            return bar;
        }

        /// <summary>A "Menus" canvas above the HUD with the pause, confirm, settings and time-up dialogs.</summary>
        public static LevelMenus BuildMenus(MonoBehaviour flow)
        {
            var canvas = NewCanvas("Menus", 10);
            var menus = canvas.AddComponent<LevelMenus>();

            var pause = Dialog(canvas.transform, "PauseDialog", "Paused", out var pauseBox,
                ("Resume", menus.Resume), ("Retry level", menus.Retry), ("Settings", menus.OpenSettings),
                ("Save", menus.Save), ("Main menu", menus.GoToMainMenu), ("Quit", menus.Quit));
            var saveButton = pauseBox.Find("Save").GetComponent<Button>();
            var message = Label("Message", pauseBox, "", 24, TextAnchor.MiddleCenter);
            Place(message.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(440f, 36f));

            var confirm = Dialog(canvas.transform, "ConfirmDialog", "Replace the saved game?", out _,
                ("Overwrite", menus.ConfirmOverwrite), ("Cancel", menus.CancelOverwrite));
            var timeUp = Dialog(canvas.transform, "TimeUpDialog", "Time's up!", out _,
                ("Retry", menus.Retry), ("Main menu", menus.GoToMainMenu), ("Close", menus.Quit));
            var settings = BuildSettings(canvas.transform, menus.CloseSettings);

            var so = new SerializedObject(menus);
            so.FindProperty("flow").objectReferenceValue = flow;
            so.FindProperty("pausePanel").objectReferenceValue = pause;
            so.FindProperty("confirmPanel").objectReferenceValue = confirm;
            so.FindProperty("timeUpPanel").objectReferenceValue = timeUp;
            so.FindProperty("settingsPanel").objectReferenceValue = settings;
            so.FindProperty("saveButton").objectReferenceValue = saveButton;
            so.FindProperty("messageLabel").objectReferenceValue = message;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Hidden until LevelMenus shows them (it hides them in Awake too).
            pause.SetActive(false);
            confirm.SetActive(false);
            timeUp.SetActive(false);
            settings.gameObject.SetActive(false);
            return menus;
        }

        /// <summary>
        /// "MainMenu" canvas: background, game title, Start / Continue (disabled until #44) / Settings / Quit, settings panel.
        /// </summary>
        public static MainMenuView BuildMainMenu()
        {
            var canvas = NewCanvas("MainMenu", 0);
            Background(canvas.transform, new Color(0.35f, 0.6f, 0.4f));
            var title = Label("Title", canvas.transform, "Hamster Golf", 96, TextAnchor.MiddleCenter);   // the game title is TBD
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1400f, 160f));

            var view = canvas.AddComponent<MainMenuView>();
            var menu = Dialog(canvas.transform, "Menu", "", out var box,
                ("Start", view.StartGame), ("Continue", null), ("Settings", view.OpenSettings), ("Quit", view.Quit));
            menu.GetComponent<Image>().color = Color.clear;
            box.anchoredPosition = new Vector2(0f, -80f);
            var continueButton = box.Find("Continue").GetComponent<Button>();
            continueButton.interactable = false;   // Save & Load (#44) enables it
            var settings = BuildSettings(canvas.transform, view.CloseSettings);
            settings.gameObject.SetActive(false);

            var so = new SerializedObject(view);
            so.FindProperty("menuPanel").objectReferenceValue = menu;
            so.FindProperty("continueButton").objectReferenceValue = continueButton;
            so.FindProperty("settingsPanel").objectReferenceValue = settings;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>"Ending" canvas: background, title, this run's results and totals, Main menu / Close.</summary>
        public static EndingView BuildEnding()
        {
            var canvas = NewCanvas("Ending", 0);
            Background(canvas.transform, new Color(0.2f, 0.25f, 0.45f));
            var title = Label("Title", canvas.transform, "The princess is safe!", 80, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1400f, 140f));
            var summary = Label("Summary", canvas.transform, "", 36, TextAnchor.UpperCenter);
            Place(summary.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(1400f, 420f));

            var view = canvas.AddComponent<EndingView>();
            var mainMenu = NewButton("Main menu", canvas.transform, "Main menu", view.GoToMainMenu);
            Place((RectTransform)mainMenu.transform, new Vector2(0.5f, 0f), new Vector2(-220f, 120f), new Vector2(400f, 64f));
            var close = NewButton("Close", canvas.transform, "Close", view.Quit);
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(220f, 120f), new Vector2(400f, 64f));

            var so = new SerializedObject(view);
            so.FindProperty("summaryLabel").objectReferenceValue = summary;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static void Background(Transform canvas, Color color)
        {
            var background = Stretch("Background", canvas, 0f).gameObject.AddComponent<Image>();
            background.color = color;
            background.raycastTarget = false;
        }

        /// <summary>Settings panel: master, music and SFX sliders and a Back button that calls <paramref name="back"/>.</summary>
        public static SettingsPanel BuildSettings(Transform parent, UnityAction back)
        {
            var root = Dialog(parent, "SettingsPanel", "Settings", out var box, ("Back", back));
            box.sizeDelta = new Vector2(620f, 460f);
            var backRect = (RectTransform)box.Find("Back");
            backRect.anchorMin = backRect.anchorMax = backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 30f);

            var panel = root.AddComponent<SettingsPanel>();
            var so = new SerializedObject(panel);
            string[] names = { "Master", "Music", "SFX" };
            string[] fields = { "master", "music", "sfx" };
            for (var i = 0; i < names.Length; i++)
            {
                var y = -120f - i * 80f;
                var label = Label(names[i] + "Label", box, names[i], 30, TextAnchor.MiddleLeft);
                Place(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(-200f, y), new Vector2(160f, 50f));
                var slider = DefaultControls.CreateSlider(new DefaultControls.Resources()).GetComponent<Slider>();
                slider.name = names[i] + "Slider";
                Place((RectTransform)slider.transform, new Vector2(0.5f, 1f), new Vector2(90f, y), new Vector2(340f, 30f));
                slider.transform.SetParent(box, false);
                slider.value = 1f;
                so.FindProperty(fields[i]).objectReferenceValue = slider;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        // A full-screen dim layer (blocks clicks to the HUD) with a centered box: title, then one button per row.
        static GameObject Dialog(Transform parent, string name, string title, out RectTransform box,
            params (string Label, UnityAction Action)[] buttons)
        {
            var root = Stretch(name, parent, 0f);
            root.gameObject.AddComponent<Image>().color = DimColor;

            box = NewRect("Box", root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 150f + buttons.Length * 80f));
            box.pivot = new Vector2(0.5f, 0.5f);
            box.gameObject.AddComponent<Image>().color = PanelColor;

            var heading = Label("Title", box, title, 40, TextAnchor.MiddleCenter);
            Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(480f, 60f));

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = NewButton(buttons[i].Label, box, buttons[i].Label, buttons[i].Action);
                Place((RectTransform)button.transform, new Vector2(0.5f, 1f), new Vector2(0f, -100f - i * 80f), new Vector2(400f, 64f));
            }
            return root.gameObject;
        }

        static Button NewButton(string name, Transform parent, string text, UnityAction action)
        {
            var go = DefaultControls.CreateButton(new DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);
            var label = go.GetComponentInChildren<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = 30;
            label.color = new Color(0.1f, 0.1f, 0.1f);
            var button = go.GetComponent<Button>();
            if (action != null)
                UnityEventTools.AddPersistentListener(button.onClick, action);
            return button;
        }

        static Text Label(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            go.AddComponent<Shadow>();
            return label;
        }

        static RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Place(rect, anchor, position, size);
            return rect;
        }

        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static RectTransform Stretch(string name, Transform parent, float inset)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            return rect;
        }
    }
}
