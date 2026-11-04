using System.IO;
using Project3DGP.Core.UI;
using Project3DGP.MiniEx.Pongpat.FunctionSelection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>Authors #65 in the Editor. Re-running adds missing parts and preserves authored values.</summary>
    public static class PongpatFunctionSelectionSetupMenu
    {
        const string Folder = "Assets/_Project/MiniExercises/Pongpat/4E2_FunctionSelection";
        const string ScenePath = Folder + "/Scenes/4E2_FunctionSelection.unity";
        static readonly Color PanelColor = new Color(0.07f, 0.11f, 0.18f, 1f);
        static readonly Color Amber = new Color(1f, 0.65f, 0.2f);
        static readonly Color Frost = new Color(0.3f, 0.8f, 1f);

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Pongpat 4.E.2: Set Up Scene",
            priority = MagicMenuUtil.MiniExercisesPriority + 31)]
        public static void SetUpScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MagicMenuUtil.Log("Stop Play mode before setting up Pongpat 4.E.2.");
                return;
            }
            if (!File.Exists(ScenePath))
            {
                MagicMenuUtil.Log("Missing 4.E.2 scene. Run Magic > Mini-Exercises > Create All Planned first.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                MagicMenuUtil.Log("Cancelled: open scenes were not saved.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set up Pongpat 4.E.2");
            try
            {
                var root = Root(scene, "Pongpat Function Selection");
                var router = Component<PageRouter>(root);
                var demo = Component<FunctionSelectionDemo>(root);
                var audio = Component<AudioSource>(root, source =>
                {
                    source.playOnAwake = false;
                    source.spatialBlend = 0f;
                });
                EnsureCamera(root.transform, scene);
                EnsureEventSystem(scene);

                // Page roots are siblings. The router and audio stay active when a page is hidden.
                var menu = Child(root.transform, "Menu Page");
                var level = Child(root.transform, "Level Page");
                var settings = Child(root.transform, "Settings Page");
                var characters = Child(root.transform, "Characters Page");
                var menuCanvas = Canvas(menu.transform, true);
                var levelCanvas = Canvas(level.transform, false);
                var settingsCanvas = Canvas(settings.transform, true);
                var characterCanvas = Canvas(characters.transform, false);

                Label(menuCanvas, "Title", "FUNCTION SELECTION", new Vector2(0, 230), new Vector2(1060, 80), 42);
                Label(menuCanvas, "Subtitle", "Pongpat | 4.E.2 | Standalone demo", new Vector2(0, 155), new Vector2(1000, 45), 24);
                var play = Button(menuCanvas, "Game Level", "Game Level", new Vector2(0, 45), demo.OpenLevel);
                Button(menuCanvas, "Settings", "Settings", new Vector2(0, -40), demo.OpenSettings);
                Button(menuCanvas, "Characters", "Character Selection", new Vector2(0, -125), demo.OpenCharacters);
                Label(menuCanvas, "Help", "Click a function, or use arrows and Enter. Escape returns here.",
                    new Vector2(0, -260), new Vector2(1100, 50), 22);

                var backFromLevel = Button(levelCanvas, "Back", "Back to Menu", new Vector2(-475, 300), demo.ReturnToMenu,
                    new Vector2(260, 60), keyboardNavigation: false);
                Button(levelCanvas, "Restart", "Restart Level", new Vector2(475, 300), demo.RestartLevel,
                    new Vector2(260, 60), keyboardNavigation: false);
                var status = Label(levelCanvas, "Status", "Collect all 3 golden seeds. Avoid the red monsters.",
                    new Vector2(0, 220), new Vector2(1150, 75), 25);
                Label(levelCanvas, "Controls", "WASD / arrows: move  |  Escape: menu  |  Monster contact: return to spawn",
                    new Vector2(0, -315), new Vector2(1200, 50), 22);

                Label(settingsCanvas, "Title", "SETTINGS", new Vector2(0, 265), new Vector2(1000, 70), 40);
                var volumeLabel = Label(settingsCanvas, "Volume Label", "Demo volume: 50%", new Vector2(0, 155), new Vector2(900, 45), 26);
                var volume = Slider(settingsCanvas, "Volume", new Vector2(0, 95));
                Button(settingsCanvas, "Preview Sound", "Preview Sound", new Vector2(0, 10), demo.PlayPreviewTone);
                var brightnessLabel = Label(settingsCanvas, "Brightness Label", "Brightness: 50%", new Vector2(0, -85), new Vector2(900, 45), 26);
                var brightness = Slider(settingsCanvas, "Brightness", new Vector2(0, -145));
                Label(settingsCanvas, "Brightness Help", "View brightness changes in Game Level or Character Selection.",
                    new Vector2(0, -205), new Vector2(1100, 45), 22);
                Button(settingsCanvas, "Back", "Back to Menu", new Vector2(0, -265), demo.ReturnToMenu);

                Label(characterCanvas, "Title", "CHARACTER SELECTION", new Vector2(0, 265), new Vector2(1100, 70), 40);
                Label(characterCanvas, "Help", "Choose an avatar, then open Game Level to use it.", new Vector2(0, 190), new Vector2(1100, 50), 25);
                var amberButton = Button(characterCanvas, "Amber", "Choose Amber", new Vector2(-250, -80), demo.SelectAmber);
                Button(characterCanvas, "Frost", "Choose Frost", new Vector2(250, -80), demo.SelectFrost);
                var characterLabel = Label(characterCanvas, "Selection", "Selected: Amber", new Vector2(0, -170), new Vector2(1000, 60), 30);
                Button(characterCanvas, "Back", "Back to Menu", new Vector2(0, -270), demo.ReturnToMenu);

                var world = Child(level.transform, "World");
                var floor = Material("Arena", new Color(0.24f, 0.35f, 0.3f));
                var wall = Material("Walls", new Color(0.23f, 0.29f, 0.38f));
                var amberMaterial = Material("Amber", Amber);
                var frostMaterial = Material("Frost", Frost);
                var previews = Child(characters.transform, "Character Previews");
                Primitive(previews.transform, "Amber Preview", PrimitiveType.Capsule,
                    new Vector3(-9.7f, 3.5f, 0), Vector3.one * 3f, amberMaterial, false);
                Primitive(previews.transform, "Frost Preview", PrimitiveType.Capsule,
                    new Vector3(9.7f, 3.5f, 0), Vector3.one * 3f, frostMaterial, false);
                var monsterMaterial = Material("Monsters", new Color(0.9f, 0.2f, 0.25f));
                var seedMaterial = Material("Seeds", new Color(1f, 0.88f, 0.15f));
                Primitive(world.transform, "Floor", PrimitiveType.Cube, new Vector3(0, -0.2f, 0), new Vector3(22, 0.5f, 22), floor);
                Primitive(world.transform, "North Wall", PrimitiveType.Cube, new Vector3(0, 0.7f, 11), new Vector3(22.5f, 1.4f, 0.5f), wall);
                Primitive(world.transform, "South Wall", PrimitiveType.Cube, new Vector3(0, 0.7f, -11), new Vector3(22.5f, 1.4f, 0.5f), wall);
                Primitive(world.transform, "West Wall", PrimitiveType.Cube, new Vector3(-11, 0.7f, 0), new Vector3(0.5f, 1.4f, 22), wall);
                Primitive(world.transform, "East Wall", PrimitiveType.Cube, new Vector3(11, 0.7f, 0), new Vector3(0.5f, 1.4f, 22), wall);
                var spawn = Child(world.transform, "Spawn", go => go.transform.localPosition = new Vector3(0, 0.1f, -7));
                var playerObject = Child(world.transform, "Player", go => go.transform.localPosition = spawn.transform.localPosition);
                var player = Component<CharacterController>(playerObject, controller =>
                {
                    controller.height = 1.8f;
                    controller.radius = 0.4f;
                    controller.center = new Vector3(0, 0.9f, 0);
                    controller.stepOffset = 0.25f;
                });
                var amberAvatar = Primitive(playerObject.transform, "Amber Avatar", PrimitiveType.Capsule,
                    new Vector3(0, 0.9f, 0), new Vector3(0.8f, 0.9f, 0.8f), amberMaterial, false);
                var frostAvatar = Primitive(playerObject.transform, "Frost Avatar", PrimitiveType.Capsule,
                    new Vector3(0, 0.9f, 0), new Vector3(0.8f, 0.9f, 0.8f), frostMaterial, false);
                var monsterA = Primitive(world.transform, "Monster 1", PrimitiveType.Sphere,
                    new Vector3(-4, 0.8f, 0), Vector3.one * 1.5f, monsterMaterial, false);
                var monsterB = Primitive(world.transform, "Monster 2", PrimitiveType.Sphere,
                    new Vector3(4, 0.8f, 4), Vector3.one * 1.5f, monsterMaterial, false);
                var seedA = Primitive(world.transform, "Golden Seed 1", PrimitiveType.Sphere,
                    new Vector3(-6, 0.65f, 3), Vector3.one * 0.7f, seedMaterial, false);
                var seedB = Primitive(world.transform, "Golden Seed 2", PrimitiveType.Sphere,
                    new Vector3(0, 0.65f, 7), Vector3.one * 0.7f, seedMaterial, false);
                var seedC = Primitive(world.transform, "Golden Seed 3", PrimitiveType.Sphere,
                    new Vector3(6, 0.65f, 3), Vector3.one * 0.7f, seedMaterial, false);
                var light = Component<Light>(Child(root.transform, "Demo Light", go => go.transform.rotation = Quaternion.Euler(50, -30, 0)), source =>
                {
                    source.type = LightType.Directional;
                    source.intensity = 1f;
                });

                References(router, "pages", menu, level, settings, characters);
                References(router, "firstSelections", play, backFromLevel, volume, amberButton);
                Reference(demo, "router", router);
                Reference(demo, "player", player);
                Reference(demo, "spawnPoint", spawn.transform);
                References(demo, "avatarVisuals", amberAvatar, frostAvatar);
                References(demo, "monsters", monsterA.transform, monsterB.transform);
                References(demo, "collectibles", seedA, seedB, seedC);
                Reference(demo, "demoLight", light);
                Reference(demo, "demoAudio", audio);
                Reference(demo, "volumeSlider", volume);
                Reference(demo, "brightnessSlider", brightness);
                Reference(demo, "volumeLabel", volumeLabel);
                Reference(demo, "brightnessLabel", brightnessLabel);
                Reference(demo, "characterLabel", characterLabel);
                Reference(demo, "levelStatus", status);

                EditorSceneManager.MarkSceneDirty(scene);
                AssetDatabase.SaveAssets();
                Selection.activeGameObject = root;
                MagicMenuUtil.Log("Pongpat 4.E.2 ready: three functions, two settings, two avatars, three seeds and two monsters. " +
                    "Save the scene, then press Play. Build Settings were preserved; this scene runs directly.");
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        static GameObject Root(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                {
                    MagicMenuUtil.Log("Skipped existing " + name + ".");
                    return root;
                }
            var created = new GameObject(name);
            SceneManager.MoveGameObjectToScene(created, scene);
            Undo.RegisterCreatedObjectUndo(created, "Create " + name);
            MagicMenuUtil.Log("Created " + name + ".");
            return created;
        }

        static GameObject Child(Transform parent, string name, System.Action<GameObject> initialize = null, bool ui = false)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                MagicMenuUtil.Log("Skipped existing " + name + ".");
                return existing.gameObject;
            }
            var created = ui ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);
            created.transform.SetParent(parent, false);
            initialize?.Invoke(created);
            Undo.RegisterCreatedObjectUndo(created, "Create " + name);
            MagicMenuUtil.Log("Created " + name + ".");
            return created;
        }

        static T Component<T>(GameObject go, System.Action<T> initialize = null) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component != null)
                return component;
            component = Undo.AddComponent<T>(go);
            initialize?.Invoke(component);
            return component;
        }

        static void EnsureCamera(Transform parent, Scene scene)
        {
            var go = Child(parent, "Demo Camera", camera =>
            {
                camera.transform.position = new Vector3(0, 19, -16);
                camera.transform.rotation = Quaternion.Euler(50, 0, 0);
            });
            Component<Camera>(go, camera =>
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = PanelColor;
                camera.orthographic = true;
                camera.orthographicSize = 14;
                camera.depth = 10;
                camera.farClipPlane = 100;
            });
            foreach (var sceneRoot in scene.GetRootGameObjects())
                if (sceneRoot.GetComponentInChildren<AudioListener>(true) != null)
                    return;
            Component<AudioListener>(go);
        }

        static void EnsureEventSystem(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<EventSystem>(true) != null)
                {
                    MagicMenuUtil.Log("Skipped existing EventSystem; check it uses InputSystemUIInputModule.");
                    return;
                }
            var events = Root(scene, "EventSystem");
            Component<EventSystem>(events);
            Component<InputSystemUIInputModule>(events, module => module.AssignDefaultActions());
        }

        static Transform Canvas(Transform parent, bool opaque)
        {
            var go = Child(parent, "Canvas", ui: true);
            Component<UnityEngine.Canvas>(go, canvas =>
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 20;
            });
            Component<CanvasScaler>(go, scaler =>
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
            });
            Component<GraphicRaycaster>(go);
            if (opaque)
            {
                var background = Child(go.transform, "Background", item => Stretch((RectTransform)item.transform), true);
                Component<Image>(background, image => { image.color = PanelColor; image.raycastTarget = false; });
            }
            return go.transform;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static void Position(GameObject go, Vector2 position, Vector2 size)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static Text Label(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
        {
            var go = Child(parent, name, item => Position(item, position, size), true);
            return Component<Text>(go, label =>
            {
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.text = text;
                label.fontSize = fontSize;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.raycastTarget = false;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
            });
        }

        static Button Button(Transform parent, string name, string text, Vector2 position, UnityAction action,
            Vector2? size = null, bool keyboardNavigation = true)
        {
            var actualSize = size ?? new Vector2(380, 64);
            var go = Child(parent, name, item => Position(item, position, actualSize), true);
            var image = Component<Image>(go, graphic => graphic.color = new Color(0.19f, 0.32f, 0.47f));
            var button = Component<UnityEngine.UI.Button>(go, control =>
            {
                control.targetGraphic = image;
                var colors = control.colors;
                colors.highlightedColor = new Color(0.65f, 0.85f, 1f);
                colors.selectedColor = new Color(0.65f, 0.85f, 1f);
                control.colors = colors;
                if (!keyboardNavigation)
                    control.navigation = new Navigation { mode = Navigation.Mode.None };
            });
            Label(go.transform, "Label", text, Vector2.zero, actualSize - new Vector2(12, 8), 25);
            // Preserve all existing callbacks. Repair only this missing persistent listener.
            var hasListener = false;
            for (var i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentTarget(i) == action.Target as Object &&
                    button.onClick.GetPersistentMethodName(i) == action.Method.Name)
                    hasListener = true;
            if (!hasListener)
            {
                Undo.RecordObject(button, "Connect " + name);
                UnityEventTools.AddPersistentListener(button.onClick, action);
            }
            return button;
        }

        static Slider Slider(Transform parent, string name, Vector2 position)
        {
            var go = Child(parent, name, item => Position(item, position, new Vector2(600, 44)), true);
            Component<Image>(go, image => image.color = new Color(0.19f, 0.25f, 0.33f));
            var handleArea = Child(go.transform, "Handle Area", item =>
            {
                Stretch((RectTransform)item.transform);
                ((RectTransform)item.transform).offsetMin = new Vector2(18, 0);
                ((RectTransform)item.transform).offsetMax = new Vector2(-18, 0);
            }, true);
            var handle = Child(handleArea.transform, "Handle", item =>
                ((RectTransform)item.transform).sizeDelta = new Vector2(30, 48), true);
            var handleImage = Component<Image>(handle, image => image.color = Amber);
            var slider = Component<UnityEngine.UI.Slider>(go, control =>
            {
                control.minValue = 0;
                control.maxValue = 1;
                control.value = 0.5f;
            });
            // Null fields are repaired without changing a human's custom slider configuration.
            if (slider.handleRect == null)
            {
                Undo.RecordObject(slider, "Connect slider handle");
                slider.handleRect = (RectTransform)handle.transform;
            }
            if (slider.targetGraphic == null)
            {
                Undo.RecordObject(slider, "Connect slider graphic");
                slider.targetGraphic = handleImage;
            }
            return slider;
        }

        static Material Material(string name, Color color)
        {
            var path = Folder + "/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
            if (existing != null)
                return existing;
            MagicMenuUtil.EnsureFolder(Folder + "/Materials");
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path + ".");
            return material;
        }

        static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position,
            Vector3 scale, Material material, bool solid = true)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                MagicMenuUtil.Log("Skipped existing " + name + ".");
                return existing.gameObject;
            }
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            MagicMenuUtil.Log("Created " + name + ".");
            return go;
        }

        static void Reference(Object target, string field, Object value) => References(target, field, value);

        static void References(Object target, string field, params Object[] values)
        {
            var settings = new SerializedObject(target);
            var property = settings.FindProperty(field);
            if (property.isArray)
            {
                var oldSize = property.arraySize;
                if (oldSize < values.Length)
                    property.arraySize = values.Length;
                for (var i = 0; i < values.Length; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    if (i >= oldSize || element.objectReferenceValue == null)
                        element.objectReferenceValue = values[i];
                }
            }
            else if (property.objectReferenceValue == null)
                property.objectReferenceValue = values[0];
            if (settings.hasModifiedProperties)
            {
                settings.ApplyModifiedProperties();
                MagicMenuUtil.Log("Connected missing " + target.name + "." + field + " references.");
            }
        }
    }
}
