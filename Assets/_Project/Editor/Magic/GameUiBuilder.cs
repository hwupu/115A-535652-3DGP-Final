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
