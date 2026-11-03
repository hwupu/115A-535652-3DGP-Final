using Project3DGP.Core.Cameras;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// The level minimap (#78) for Add HUD and Menus and Add Minimap Icon: a top-down orthographic camera that
    /// follows the ball (north up, - / = zoom) and renders into UI/Minimap/Minimap.renderTexture, plus flat icons
    /// on the "Minimap" layer that only this camera sees: ball (white), avatar (yellow), cup (green), exit (cyan),
    /// hazards (red). Same pipeline as mini-exercise 4.E.3 (Core MinimapCamera). Callers own the Undo group.
    /// </summary>
    internal static class LevelMinimap
    {
        public const string Folder = "Assets/_Project/UI/Minimap";
        public const string TexturePath = Folder + "/Minimap.renderTexture";
        public const string LayerName = "Minimap";
        public const string IconName = "MinimapIcon";

        public static readonly Color BallColor = Color.white;
        public static readonly Color AvatarColor = new(1f, 0.85f, 0.1f);
        public static readonly Color GoalColor = new(0.3f, 0.9f, 0.35f);
        public static readonly Color ExitColor = new(0.3f, 0.85f, 1f);
        public static readonly Color HazardColor = new(1f, 0.25f, 0.2f);

        /// <summary>The "Minimap" layer, or -1 (logged) if Project Settings > Tags and Layers doesn't have it.</summary>
        public static int Layer
        {
            get
            {
                var layer = LayerMask.NameToLayer(LayerName);
                if (layer < 0)
                    MagicMenuUtil.Log($"No \"{LayerName}\" layer: add it in Project Settings > Tags and Layers, then run this again.");
                return layer;
            }
        }

        public static RenderTexture EnsureTexture()
        {
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(TexturePath);
            if (texture != null)
                return texture;
            MagicMenuUtil.EnsureFolder(Folder);
            texture = new RenderTexture(512, 512, 24) { name = "Minimap" };
            AssetDatabase.CreateAsset(texture, TexturePath);
            MagicMenuUtil.Log("Created " + TexturePath);
            return texture;
        }

        /// <summary>Cameras/Minimap Camera, 30 m above the ball, looking down. Returns the camera.</summary>
        public static Camera AddCamera(Scene scene, Transform ball, RenderTexture texture)
        {
            var go = new GameObject("Minimap Camera");
            SceneManager.MoveGameObjectToScene(go, scene);
            var parent = LevelSkeleton.Find(scene, "Cameras");
            if (parent != null)
                go.transform.SetParent(parent.transform, false);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            camera.targetTexture = texture;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.15f, 0.1f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 80f;
            go.transform.SetPositionAndRotation(ball.position + Vector3.up * 30f, Quaternion.Euler(90f, 0f, 0f));
            var minimap = go.AddComponent<MinimapCamera>();
            var so = new SerializedObject(minimap);
            so.FindProperty("target").objectReferenceValue = ball;
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(go, "Add Minimap Camera");
            MagicMenuUtil.Log("Added Cameras/Minimap Camera (follows the ball, - / = zoom)");
            return camera;
        }

        /// <summary>Takes the Minimap layer out of every other camera, so the icons show only on the map.</summary>
        public static void HideLayerFromOtherCameras(int layer, Camera minimap)
        {
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (camera == minimap || camera.GetComponent<MinimapCamera>() != null || (camera.cullingMask & (1 << layer)) == 0)
                    continue;
                Undo.RecordObject(camera, "Hide Minimap Layer");
                camera.cullingMask &= ~(1 << layer);
                MagicMenuUtil.Log($"{camera.name}: no longer renders the {LayerName} layer");
            }
        }

        /// <summary>
        /// Icons for the ball, avatar, cup, exits and every object under Gameplay/Hazards (each only if it has none).
        /// Returns how many were added.
        /// </summary>
        public static int AddLevelIcons(Scene scene, int layer)
        {
            var added = 0;
            foreach (var ball in Object.FindObjectsByType<GolfBall>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (AddIcon(ball.transform, layer, "Ball", BallColor, 1.2f)) added++;
            foreach (var avatar in Object.FindObjectsByType<GolferAvatar>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (AddIcon(avatar.transform, layer, "Avatar", AvatarColor, 1.6f)) added++;
            var cup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            if (cup != null && AddIcon(cup.transform, layer, "Goal", GoalColor, 1.8f)) added++;
            foreach (var exit in Object.FindObjectsByType<LevelExit>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (AddIcon(exit.transform, layer, "Exit", ExitColor, 2.2f)) added++;
            var hazards = LevelSkeleton.Find(scene, "Gameplay/Hazards");
            if (hazards != null)
                foreach (Transform hazard in hazards.transform)
                    if (AddIcon(hazard, layer, "Hazard", HazardColor, 1.8f)) added++;
            return added;
        }

        /// <summary>A flat MinimapIcon quad as a child of the target. False if the target already has an icon.</summary>
        public static bool AddIcon(Transform target, int layer, string kind, Color color, float size)
        {
            if (target.GetComponentInChildren<MinimapIcon>(true) != null)
                return false;
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = IconName;
            Object.DestroyImmediate(go.GetComponent<Collider>());   // a map marker must not stop the ball
            go.layer = layer;
            go.transform.SetParent(target, false);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = PlaceholderMaterials.Get("Minimap" + kind, color, "Universal Render Pipeline/Unlit");
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var icon = go.AddComponent<MinimapIcon>();
            var so = new SerializedObject(icon);
            so.FindProperty("size").floatValue = size;
            so.ApplyModifiedPropertiesWithoutUndo();
            icon.Apply();
            Undo.RegisterCreatedObjectUndo(go, "Add Minimap Icon");
            MagicMenuUtil.Log($"Added the minimap icon ({kind.ToLower()}) to {target.name}");
            return true;
        }
    }
}
