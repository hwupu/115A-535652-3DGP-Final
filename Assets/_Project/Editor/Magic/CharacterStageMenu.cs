using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Characters acting in the UI (#141): an off-screen stage (characters, key light, camera) 500 m below the scene,
    /// rendered into a RenderTexture (UI/Stages) and shown by a RawImage with CharacterStageView, which activates the
    /// stage and fires the Animator triggers while the RawImage is visible.
    ///   Magic > Scenes > Add Ending Victory Stage   Ending scene: hamster + princess play Victory, in the empty band
    ///                                               between the results text and the buttons.
    ///   Magic > Level > Add Time-Up Defeat Stage    the open level's time-up dialog: the hamster plays Defeat above
    ///                                               the dialog box (Add HUD and Menus runs it too).
    /// Needs the clips and states from Magic > Prefabs > Use Character Models. Only what is missing; one Undo step;
    /// the scene is marked dirty, not saved. The stage objects are prefab instances: move, turn or swap them freely.
    /// </summary>
    public static class CharacterStageMenu
    {
        const string Folder = "Assets/_Project/UI/Stages";
        static readonly Vector3 StageOrigin = new(0f, -500f, 0f);   // far below any course; active only while shown

        [MenuItem(MagicMenuUtil.Root + "Scenes/Add Ending Victory Stage", priority = MagicMenuUtil.ScenesPriority + 5)]
        static void AddEndingStage()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != GameScenes.Ending)
            {
                MagicMenuUtil.Log($"Open Scenes/{GameScenes.Ending} first (the open scene is {scene.name}).");
                return;
            }
            var view = Object.FindAnyObjectByType<EndingView>(FindObjectsInactive.Include);
            if (view == null)
            {
                MagicMenuUtil.Log("No Ending screen: run Magic > Scenes > Add Ending UI first.");
                return;
            }
            if (view.GetComponentInChildren<CharacterStageView>(true) != null)
            {
                MagicMenuUtil.Log("Skipped (exists) the Ending victory stage");
                return;
            }

            Undo.SetCurrentGroupName("Add Ending Victory Stage");
            var undoGroup = Undo.GetCurrentGroup();
            var texture = EnsureTexture("EndingStage", 1152, 480);
            // Two 1.3 m hamsters side by side, room above for the jump: about 4.3 x 1.8 m in view.
            var stage = NewStage(scene, null, "EndingStage", texture, new Vector3(0f, 1.0f, 4.1f), new Vector3(0f, 0.72f, 0f));
            var hamster = AddActor(stage.transform, "Hamster", new Vector3(-0.6f, 0f, 0f), 15f);
            var princess = AddActor(stage.transform, "Princess", new Vector3(0.6f, 0f, 0f), -15f);

            // Bottom centre, from just above the buttons (120 + 64) up to below the results text.
            var image = NewImage(view.transform, texture, new Vector2(0.5f, 0f), new Vector2(0f, 200f), new Vector2(960f, 400f));
            var summary = view.transform.Find("Summary");
            if (summary != null)
                image.SetSiblingIndex(summary.GetSiblingIndex() + 1);   // over the background, under the buttons
            Wire(image.gameObject, stage, (hamster, "Victory"), (princess, "Victory"));

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log("Added EndingStage (500 m below, hamster + princess) and Ending/CharacterStage (shows it). Save the scene, then press Play.");
        }

        [MenuItem(MagicMenuUtil.Root + "Level/Add Time-Up Defeat Stage", priority = MagicMenuUtil.LevelPriority + 6)]
        static void AddDefeatStageMenu()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Undo.SetCurrentGroupName("Add Time-Up Defeat Stage");
            var undoGroup = Undo.GetCurrentGroup();
            var added = AddDefeatStage(scene);
            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                MagicMenuUtil.Log("Save the scene. To try it: Play, then let the time run out (or use the UI Test Area's Fail).");
            }
        }

        /// <summary>The hamster's Defeat above the open level's time-up dialog, if missing. Returns how many were added (0 or 1).</summary>
        public static int AddDefeatStage(Scene scene)
        {
            var menus = Object.FindAnyObjectByType<LevelMenus>(FindObjectsInactive.Include);
            if (menus == null)
            {
                MagicMenuUtil.Log("No level dialogs: run Magic > Level > Add HUD and Menus first.");
                return 0;
            }
            var timeUp = new SerializedObject(menus).FindProperty("timeUpPanel").objectReferenceValue as GameObject;
            if (timeUp == null)
            {
                MagicMenuUtil.Log("LevelMenus has no time-up panel assigned: nothing to add the defeat stage to.");
                return 0;
            }
            if (timeUp.GetComponentInChildren<CharacterStageView>(true) != null)
            {
                MagicMenuUtil.Log("Skipped (exists) the time-up defeat stage");
                return 0;
            }

            var texture = EnsureTexture("DefeatStage", 560, 400);
            var ui = LevelSkeleton.Find(scene, "UI");
            // One hamster, standing then sitting: about 2.2 x 1.6 m in view.
            var stage = NewStage(scene, ui != null ? ui.transform : null, "DefeatStage", texture, new Vector3(0f, 1.0f, 3.6f), new Vector3(0f, 0.62f, 0f));
            var hamster = AddActor(stage.transform, "Hamster", Vector3.zero, 20f);

            // Centred just above the dialog box.
            var box = timeUp.transform.Find("Box") as RectTransform;
            var top = box != null ? box.anchoredPosition.y + box.sizeDelta.y * (1f - box.pivot.y) : 200f;
            var image = NewImage(timeUp.transform, texture, new Vector2(0.5f, 0.5f), new Vector2(0f, top + 10f), new Vector2(420f, 300f));
            image.pivot = new Vector2(0.5f, 0f);
            image.SetAsLastSibling();
            Wire(image.gameObject, stage, (hamster, "Defeat"));
            MagicMenuUtil.Log($"Added {(ui != null ? "UI/" : "")}DefeatStage (500 m below, the hamster) and {timeUp.name}/CharacterStage (shows it while the dialog is open)");
            return 1;
        }

        // ---------- helpers ----------

        static RenderTexture EnsureTexture(string name, int width, int height)
        {
            var path = $"{Folder}/{name}.renderTexture";
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (texture != null)
                return texture;
            MagicMenuUtil.EnsureFolder(Folder);
            // sRGB with alpha: the camera clears to transparent, so only the characters cover the UI behind.
            texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = name, antiAliasing = 4 };
            AssetDatabase.CreateAsset(texture, path);
            MagicMenuUtil.Log("Created " + path);
            return texture;
        }

        // Inactive stage root with a camera (transparent background, no shadows / post: the scene's sun and the course
        // above must not reach it) and a point key light in front, so the faces are lit whatever the scene's sun does.
        static GameObject NewStage(Scene scene, Transform parent, string name, RenderTexture texture, Vector3 cameraAt, Vector3 lookAt)
        {
            var stage = new GameObject(name);
            SceneManager.MoveGameObjectToScene(stage, scene);
            if (parent != null)
                stage.transform.SetParent(parent, false);
            stage.transform.SetPositionAndRotation(StageOrigin, Quaternion.identity);

            var cameraGo = new GameObject("Stage Camera");
            cameraGo.transform.SetParent(stage.transform, false);
            cameraGo.transform.localPosition = cameraAt;
            cameraGo.transform.LookAt(stage.transform.TransformPoint(lookAt));
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.fieldOfView = 25f;
            camera.nearClipPlane = 0.5f;
            camera.farClipPlane = 12f;
            camera.depth = -10f;
            camera.allowHDR = false;
            camera.targetTexture = texture;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderShadows = false;
            data.renderPostProcessing = false;

            var lightGo = new GameObject("Key Light");
            lightGo.transform.SetParent(stage.transform, false);
            lightGo.transform.localPosition = new Vector3(1.2f, 2.0f, 2.2f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 6f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.None;

            stage.SetActive(false);   // CharacterStageView turns it on while its RawImage shows
            Undo.RegisterCreatedObjectUndo(stage, "Add " + name);
            return stage;
        }

        static Animator AddActor(Transform stage, string character, Vector3 position, float yaw)
        {
            var path = $"{PlaceholderPrefabsMenu.Folder}/Characters/{character}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                MagicMenuUtil.Log($"No {path}: run Magic > Prefabs > Create Placeholder Prefabs and Use Character Models first.");
                return null;
            }
            var actor = (GameObject)PrefabUtility.InstantiatePrefab(prefab, stage);
            actor.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var animator = actor.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                MagicMenuUtil.Log($"{character}: no Animator (placeholder look): run Magic > Prefabs > Use Character Models, then wire the stage's animator by hand.");
                return null;
            }
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;   // also plays if the game is paused
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return animator;
        }

        static RectTransform NewImage(Transform parent, Texture texture, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject("CharacterStage", typeof(RectTransform), typeof(RawImage));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            Undo.RegisterCreatedObjectUndo(go, "Add CharacterStage");
            return rect;
        }

        static void Wire(GameObject image, GameObject stage, params (Animator Animator, string Trigger)[] actors)
        {
            var view = image.AddComponent<CharacterStageView>();
            var so = new SerializedObject(view);
            so.FindProperty("stage").objectReferenceValue = stage;
            var list = so.FindProperty("actors");
            list.arraySize = actors.Length;
            for (var i = 0; i < actors.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("animator").objectReferenceValue = actors[i].Animator;
                element.FindPropertyRelative("trigger").stringValue = actors[i].Trigger;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
