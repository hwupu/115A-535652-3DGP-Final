using Project3DGP.Core.UI;
using Project3DGP.Game.Golf;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Open Shot Test Area (#15): the ball test green plus the real shot sequence: ShotController
    /// (Standard club), the AimCone at the ball and a vertical power meter on the left of a uGUI canvas.
    /// New, UNSAVED scene; close it without saving.
    /// </summary>
    public static class ShotTestSceneMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Game/Open Shot Test Area", priority = MagicMenuUtil.GamePriority + 12)]
        static void Open()
        {
            ClubProfilesMenu.CreateAll();
            if (!GolfBallTestSceneMenu.OpenArea(out var input, out var ball, out _))
                return;
            var club = AssetDatabase.LoadAssetAtPath<ClubProfile>(ClubProfilesMenu.StandardPath);   // after NewScene

            var shotObject = new GameObject("ShotController");
            shotObject.SetActive(false);   // wire first, so OnEnable sees the references
            var shot = shotObject.AddComponent<ShotController>();
            var shotSo = new SerializedObject(shot);
            shotSo.FindProperty("input").objectReferenceValue = input;
            shotSo.FindProperty("ball").objectReferenceValue = ball;
            var clubs = shotSo.FindProperty("clubs");
            clubs.arraySize = 1;
            clubs.GetArrayElementAtIndex(0).objectReferenceValue = club;
            shotSo.ApplyModifiedPropertiesWithoutUndo();
            shotObject.SetActive(true);

            var coneObject = new GameObject("AimCone");
            var line = coneObject.AddComponent<LineRenderer>();
            line.widthMultiplier = 0.04f;
            line.sharedMaterial = PlaceholderMaterials.White;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var cone = coneObject.AddComponent<AimCone>();
            var coneSo = new SerializedObject(cone);
            coneSo.FindProperty("shot").objectReferenceValue = shot;
            coneSo.FindProperty("ball").objectReferenceValue = ball;
            coneSo.ApplyModifiedPropertiesWithoutUndo();

            BuildPowerMeter(shot);
            MagicMenuUtil.Log("Opened an unsaved shot test area. Press Play; close it without saving.");
        }

        // Canvas > PowerMeter (left, vertical) > Fill + Label, read by PowerMeterView through a Core BarView.
        static void BuildPowerMeter(ShotController shot)
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var meter = new GameObject("PowerMeter", typeof(RectTransform), typeof(Image));
            var meterRect = meter.GetComponent<RectTransform>();
            meterRect.SetParent(canvasObject.transform, false);
            meterRect.anchorMin = meterRect.anchorMax = new Vector2(0f, 0.5f);   // left edge, middle
            meterRect.pivot = new Vector2(0f, 0.5f);
            meterRect.anchoredPosition = new Vector2(60f, 0f);
            meterRect.sizeDelta = new Vector2(48f, 420f);
            meter.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.SetParent(meterRect, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1f, 0f);   // BarView moves anchorMax.y to the value
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.SetParent(meterRect, false);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 0f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -8f);
            labelRect.sizeDelta = new Vector2(220f, 60f);
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = Color.white;

            var bar = meter.AddComponent<BarView>();
            var barSo = new SerializedObject(bar);
            barSo.FindProperty("fill").objectReferenceValue = fillRect;
            barSo.FindProperty("vertical").boolValue = true;
            barSo.FindProperty("smoothTime").floatValue = 0f;
            barSo.FindProperty("label").objectReferenceValue = text;
            barSo.ApplyModifiedPropertiesWithoutUndo();

            var view = meter.AddComponent<PowerMeterView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("shot").objectReferenceValue = shot;
            viewSo.FindProperty("bar").objectReferenceValue = bar;
            viewSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
