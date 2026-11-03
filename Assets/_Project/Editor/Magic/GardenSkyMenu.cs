using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Sky, Fog and Sun (#74): the sunny look of L1.
    ///   Sky   BD Studios "Day_Clear" hand-painted skybox (Asset Store, not committed: docs/asset-store-packs.md);
    ///         ambient light from the sky
    ///   Fog   light-blue linear fog from 35 to 110 m: softens the forest edge and the end of the terrain
    ///   Sun   Environment/Lighting/Directional Light: warm, soft shadows, set as the sun
    /// Only what is still at Unity's defaults (default sky, fog off, sun without soft shadows); anything set by hand
    /// is kept. The scene is marked dirty, not saved. (Lighting settings are not in the Undo history.)
    /// </summary>
    public static class GardenSkyMenu
    {
        const string SceneName = "L1_Garden";
        static readonly Color FogColor = new(0.72f, 0.85f, 0.95f);
        static readonly Color SunColor = new(1f, 0.94f, 0.8f);

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Sky, Fog and Sun", priority = MagicMenuUtil.LevelPriority + 26)]
        static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (this look is for the Garden only).");
                return;
            }
            var changed = 0;

            var sky = AssetStorePacks.Load<Material>(AssetStorePacks.Skybox.DayClear);
            if (sky == null)
                MagicMenuUtil.Log($"Skybox pack not imported: kept the default sky (see {AssetStorePacks.Guide}).");
            else if (RenderSettings.skybox == null || RenderSettings.skybox.name == "Default-Skybox")
            {
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1f;
                MagicMenuUtil.Log("Sky: Day_Clear (hand-painted), ambient light from the sky");
                changed++;
            }
            else
                MagicMenuUtil.Log($"Skipped (set) the sky ({RenderSettings.skybox.name})");

            if (!RenderSettings.fog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 35f;
                RenderSettings.fogEndDistance = 110f;
                RenderSettings.fogColor = FogColor;
                MagicMenuUtil.Log("Fog: linear, 35 to 110 m, light blue");
                changed++;
            }
            else
                MagicMenuUtil.Log("Skipped (on) the fog");

            var sunObject = LevelSkeleton.Find(scene, "Environment/Lighting/Directional Light");
            if (sunObject != null && sunObject.TryGetComponent(out Light sun) && sun.shadows != LightShadows.Soft)
            {
                Undo.RecordObject(sun, "Garden Sun");
                Undo.RecordObject(sun.transform, "Garden Sun");
                sun.shadows = LightShadows.Soft;
                sun.color = SunColor;
                sun.intensity = 1.25f;
                sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
                RenderSettings.sun = sun;
                MagicMenuUtil.Log("Sun: warm, 1.25, soft shadows, 45 degrees high");
                changed++;
            }
            else if (sunObject == null)
                MagicMenuUtil.Log("No Environment/Lighting/Directional Light: run Magic > Level > Create Level Scene Skeleton.");

            if (changed > 0)
            {
                DynamicGI.UpdateEnvironment();
                EditorSceneManager.MarkSceneDirty(scene);
            }
            MagicMenuUtil.Log($"Garden sky, fog and sun: {changed} set. Save the scene.");
        }
    }
}
