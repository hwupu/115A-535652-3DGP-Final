using System.IO;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.Movement;
using Project3DGP.Core.UI;
using Project3DGP.MiniEx.Henry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Mini-Exercises > Henry 4.E.3: Set Up Scene (#58). In the 4E3_GameUI scene, creates what is
    /// missing (found by name): the "Minimap" layer, materials, club data, the minimap RenderTexture, the
    /// Hamster (walker + swing), 5 golf balls, arena walls, the minimap camera, the Canvas (time bar, power
    /// meter, minimap, club cards with sound, info text), the hit particles, the EventSystem and the camera follow.
    /// Sounds: the two CC0 Freesound clips (docs/assets.md) are assigned if they exist; otherwise drag
    /// clips in later. One Undo step for scene objects.
    /// </summary>
    public static class HenryGameUiSetupMenu
    {
        const string ExerciseFolder = "Assets/_Project/MiniExercises/Henry/4E3_GameUI";
        const string SceneName = "4E3_GameUI";
        const string MinimapLayerName = "Minimap";
        const float ArenaHalfSize = 15f;
        const float BallRadius = 0.15f;
        const string SelectSoundPath = "Assets/ThirdParty/Freesound/sfx_platform_leave_663859.mp3";
        const string HitSoundPath = "Assets/ThirdParty/Freesound/sfx_throw_199823.mp3";

        static Font _font;

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Henry 4.E.3: Set Up Scene", priority = MagicMenuUtil.MiniExercisesPriority + 21)]
        static void SetUp()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (Henry/4E3_GameUI/Scenes).");
                return;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var minimapLayer = EnsureLayer(MinimapLayerName);
            if (minimapLayer < 0)
                return;
            var clubs = EnsureClubs();
            var minimapTexture = EnsureMinimapTexture();
            var selectSound = AssetDatabase.LoadAssetAtPath<AudioClip>(SelectSoundPath);
            var hitSound = AssetDatabase.LoadAssetAtPath<AudioClip>(HitSoundPath);

            Undo.SetCurrentGroupName("Set Up 4.E.3 Scene");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            var camera = Find(scene, "Main Camera");
            var hamster = Find(scene, "Hamster");
            if (hamster == null)
            {
                hamster = CreateHamster(minimapLayer, hitSound);
                created++;
            }

            if (Find(scene, "HitParticles") == null)
            {
                var particles = CreateHitParticles();
                SetReference(hamster.GetComponent<GolfSwing>(), "hitParticles", particles);
                created++;
            }

            if (Find(scene, "Balls") == null)
            {
                CreateBalls(minimapLayer);
                created++;
            }

            if (Find(scene, "Arena") == null)
            {
                CreateArena();
                created++;
            }

            if (Find(scene, "Minimap Camera") == null)
            {
                CreateMinimapCamera(hamster.transform, minimapTexture);
                created++;
            }

            var game = Find(scene, "Game");
            if (game == null)
            {
                game = new GameObject("Game");
                game.AddComponent<ShotTimer>();
                game.AddComponent<ClubSelector>();
                Undo.RegisterCreatedObjectUndo(game, "Create Game");
                MagicMenuUtil.Log("Created Game (ShotTimer + ClubSelector)");
                created++;
            }

            if (Find(scene, "Canvas") == null)
            {
                CreateCanvas(game, hamster, clubs, minimapTexture, selectSound);
                created++;
            }

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
                MagicMenuUtil.Log("Created EventSystem (Input System UI module)");
                created++;
            }

            if (camera != null && !camera.TryGetComponent(out OrbitFollowCamera _))
            {
                var follow = Undo.AddComponent<OrbitFollowCamera>(camera);
                SetReference(follow, "target", hamster.transform);
                SetReference(hamster.GetComponent<ThirdPersonWalker>(), "cameraTransform", camera.transform);
                // The main camera must not draw the minimap icons.
                var mainCamera = camera.GetComponent<Camera>();
                Undo.RecordObject(mainCamera, "Hide minimap layer");
                mainCamera.cullingMask &= ~(1 << minimapLayer);
                MagicMenuUtil.Log("Main Camera: added OrbitFollowCamera, hid the Minimap layer");
                created++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (created > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            if (selectSound == null || hitSound == null)
                MagicMenuUtil.Log("Sounds not found in Assets/ThirdParty/Freesound: assign 'Select Sound' (Canvas/ClubPanel) " +
                                  "and 'Hit Sound' (Hamster) by hand once they are imported.");
            MagicMenuUtil.Log($"4.E.3 scene: {created} things created. Save the scene, then press Play.");
        }

        // ---------- Project assets ----------

        /// <summary>Finds the layer, or names the first free user layer. Returns -1 if all are taken.</summary>
        static int EnsureLayer(string name)
        {
            var existing = LayerMask.NameToLayer(name);
            if (existing >= 0)
                return existing;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = 6; i < layers.arraySize; i++)   // 0-5 are Unity's built-in layers
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue))
                    continue;
                layer.stringValue = name;
                tagManager.ApplyModifiedProperties();
                MagicMenuUtil.Log($"Added layer {i} \"{name}\" (Project Settings > Tags and Layers)");
                return i;
            }

            MagicMenuUtil.Log("No free layer for the minimap icons. Free one in Project Settings > Tags and Layers.");
            return -1;
        }

        static ClubDefinition[] EnsureClubs()
        {
            return new[]
            {
                EnsureClub("StandardClub", "Standard Club", new Color(0.3f, 0.55f, 0.95f), 15f, 0.85f, 0.8f),
                EnsureClub("Sledgehammer", "Sledgehammer", new Color(0.9f, 0.3f, 0.25f), 28f, 0.45f, 1.3f),
                EnsureClub("Putter", "Putter", new Color(0.35f, 0.8f, 0.4f), 6f, 1f, 0.5f),
            };
        }

        static ClubDefinition EnsureClub(string file, string displayName, Color color, float maxDistance, float accuracy, float meterSpeed)
        {
            var path = $"{ExerciseFolder}/Data/{file}.asset";
            var club = AssetDatabase.LoadAssetAtPath<ClubDefinition>(path);
            if (club != null)
                return club;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
            club = ScriptableObject.CreateInstance<ClubDefinition>();
            club.displayName = displayName;
            club.color = color;
            club.maxDistance = maxDistance;
            club.accuracy = accuracy;
            club.meterSpeed = meterSpeed;
            AssetDatabase.CreateAsset(club, path);
            MagicMenuUtil.Log("Created " + path);
            return club;
        }

        static RenderTexture EnsureMinimapTexture()
        {
            var path = $"{ExerciseFolder}/Materials/Minimap.renderTexture";
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (texture != null)
                return texture;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
            texture = new RenderTexture(512, 512, 24) { name = "Minimap" };
            AssetDatabase.CreateAsset(texture, path);
            MagicMenuUtil.Log("Created " + path);
            return texture;
        }

        static Material GetMaterial(string name, Color color, string shader = "Universal Render Pipeline/Lit")
        {
            var path = $"{ExerciseFolder}/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
            material = new Material(Shader.Find(shader) ?? Shader.Find("Standard")) { color = color };
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }

        static Material Unlit(string name, Color color) => GetMaterial(name, color, "Universal Render Pipeline/Unlit");

        // ---------- World ----------

        static GameObject CreateHamster(int minimapLayer, AudioClip hitSound)
        {
            var hamster = new GameObject("Hamster");
            hamster.transform.position = new Vector3(0f, 0f, -4f);
            var controller = hamster.AddComponent<CharacterController>();
            controller.height = 0.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.4f, 0f);
            controller.stepOffset = 0.2f;
            hamster.AddComponent<ThirdPersonWalker>();
            var audio = hamster.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            var swing = hamster.AddComponent<GolfSwing>();
            SetReference(swing, "audioSource", audio);
            SetReference(swing, "hitSound", hitSound);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(hamster.transform, false);
            var fur = GetMaterial("Hamster", new Color(0.85f, 0.6f, 0.35f));
            var dark = GetMaterial("HamsterNose", new Color(0.2f, 0.12f, 0.1f));
            Part(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 0.6f), fur);
            Part(visual, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "Nose", new Vector3(0f, 0.5f, 0.3f), Vector3.one * 0.1f, dark);

            // Minimap icon: a yellow square plus a small "nose" square that shows which way it faces.
            var icon = MapIcon(hamster.transform, "MinimapIcon", minimapLayer, 1.6f, Unlit("IconHamster", new Color(1f, 0.85f, 0.1f)));
            var nose = MapIcon(icon.transform, "Nose", minimapLayer, 0.45f, Unlit("IconHamster", Color.yellow)).transform;
            nose.localPosition = new Vector3(0f, 0.7f, 0f);   // the icon lies flat, so its local Y points forward
            nose.localRotation = Quaternion.identity;          // already flat through the parent icon

            Undo.RegisterCreatedObjectUndo(hamster, "Create Hamster");
            MagicMenuUtil.Log("Created Hamster (ThirdPersonWalker + GolfSwing)");
            return hamster;
        }

        static void CreateBalls(int minimapLayer)
        {
            var parent = new GameObject("Balls");
            var ballMaterial = GetMaterial("Ball", Color.white);
            var iconMaterial = Unlit("IconBall", Color.white);
            for (var i = 0; i < 5; i++)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);   // keeps its SphereCollider
                ball.name = $"Ball_{i + 1:00}";
                ball.transform.SetParent(parent.transform, false);
                ball.transform.localPosition = new Vector3(-6f + i * 3f, BallRadius, -2f);
                ball.transform.localScale = Vector3.one * (BallRadius * 2f);
                ball.GetComponent<Renderer>().sharedMaterial = ballMaterial;
                ball.AddComponent<GolfBall>();
                // Icon scale is divided by the ball's own scale so it is ~0.9 m on the map.
                MapIcon(ball.transform, "MinimapIcon", minimapLayer, 0.9f / (BallRadius * 2f), iconMaterial);
            }
            Undo.RegisterCreatedObjectUndo(parent, "Create Balls");
            MagicMenuUtil.Log("Created Balls (5 x GolfBall)");
        }

        static void CreateArena()
        {
            var arena = new GameObject("Arena");
            var material = GetMaterial("Wall", new Color(0.55f, 0.55f, 0.6f));
            var length = ArenaHalfSize * 2f + 0.5f;
            Wall(arena, "Wall_North", new Vector3(0f, 0.5f, ArenaHalfSize), new Vector3(length, 1f, 0.5f), material);
            Wall(arena, "Wall_South", new Vector3(0f, 0.5f, -ArenaHalfSize), new Vector3(length, 1f, 0.5f), material);
            Wall(arena, "Wall_East", new Vector3(ArenaHalfSize, 0.5f, 0f), new Vector3(0.5f, 1f, length), material);
            Wall(arena, "Wall_West", new Vector3(-ArenaHalfSize, 0.5f, 0f), new Vector3(0.5f, 1f, length), material);
            Undo.RegisterCreatedObjectUndo(arena, "Create Arena");
            MagicMenuUtil.Log("Created Arena (4 walls)");
        }

        static void CreateMinimapCamera(Transform target, RenderTexture texture)
        {
            var go = new GameObject("Minimap Camera");
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 12f;
            camera.targetTexture = texture;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.15f, 0.1f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 60f;
            go.transform.SetPositionAndRotation(target.position + Vector3.up * 30f, Quaternion.Euler(90f, 0f, 0f));
            var minimap = go.AddComponent<MinimapCamera>();
            SetReference(minimap, "target", target);
            Undo.RegisterCreatedObjectUndo(go, "Create Minimap Camera");
            MagicMenuUtil.Log("Created Minimap Camera (orthographic, renders to Minimap.renderTexture)");
        }

        // ---------- UI ----------

        static void CreateCanvas(GameObject game, GameObject hamster, ClubDefinition[] clubs,
            RenderTexture minimapTexture, AudioClip selectSound)
        {
            // Screen Space - Overlay: the UI is drawn on top of everything, independent of the camera.
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = LayerMask.NameToLayer("UI");
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var root = canvasGo.transform;

            // Time bar, upper left (same place as the game's time bar, architecture 4.8).
            var timeBar = Bar(root, "TimeBar", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(520f, 40f), false,
                new Color(0.3f, 0.8f, 0.35f), 0.25f);

            // Power meter, left side, vertical.
            var powerBar = Bar(root, "PowerMeter", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(48f, 400f), true,
                new Color(1f, 0.55f, 0.15f), 0f);
            Label(powerBar.transform, "Title", "Power", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, 36f), new Vector2(120f, 30f));

            // Minimap, upper right: a frame with a RawImage showing the minimap camera's RenderTexture.
            var frame = Panel(root, "Minimap", new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(320f, 320f), new Color(1f, 1f, 1f, 0.8f));
            var map = new GameObject("Map", typeof(RectTransform), typeof(RawImage));
            map.transform.SetParent(frame.transform, false);
            Stretch(map.GetComponent<RectTransform>(), 6f);
            map.GetComponent<RawImage>().texture = minimapTexture;
            map.GetComponent<RawImage>().raycastTarget = false;

            // Club cards, right side, below the minimap.
            var panel = CreateClubPanel(root, clubs.Length, selectSound);

            // Info text, bottom centre.
            var info = Label(root, "InfoText", "", 28, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(1000f, 120f));
            Label(root, "Controls", "WASD move  Shift run  Space jump  F swing  1-3 / click club  R reset  - / = map zoom  Right-drag orbit",
                18, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1400f, 30f));

            // Wire everything together.
            var selector = game.GetComponent<ClubSelector>();
            SetReference(selector, "panel", panel);
            var clubsProperty = new SerializedObject(selector);
            var array = clubsProperty.FindProperty("clubs");
            array.arraySize = clubs.Length;
            for (var i = 0; i < clubs.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = clubs[i];
            clubsProperty.ApplyModifiedProperties();

            var swing = hamster.GetComponent<GolfSwing>();
            SetReference(swing, "clubs", selector);
            SetReference(swing, "timer", game.GetComponent<ShotTimer>());

            var hud = canvasGo.AddComponent<GameUiHud>();
            SetReference(hud, "timer", game.GetComponent<ShotTimer>());
            SetReference(hud, "swing", swing);
            SetReference(hud, "timeBar", timeBar);
            SetReference(hud, "powerBar", powerBar);
            SetReference(hud, "infoText", info);

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            MagicMenuUtil.Log("Created Canvas (time bar, power meter, minimap, club cards, info text)");
        }

        static SelectionPanel CreateClubPanel(Transform root, int count, AudioClip selectSound)
        {
            const float cardHeight = 120f, gap = 20f;
            var height = count * cardHeight + (count - 1) * gap;
            var panelGo = new GameObject("ClubPanel", typeof(RectTransform));
            panelGo.transform.SetParent(root, false);
            Place(panelGo.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-30f, -380f), new Vector2(300f, height));
            var panel = panelGo.AddComponent<SelectionPanel>();
            var audio = panelGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            var cards = new SerializedObject(panel);
            var list = cards.FindProperty("cards");
            list.arraySize = count;
            for (var i = 0; i < count; i++)
            {
                var card = CreateCard(panelGo.transform, i, new Vector2(0f, -i * (cardHeight + gap)), new Vector2(300f, cardHeight));
                list.GetArrayElementAtIndex(i).objectReferenceValue = card;
            }
            cards.FindProperty("audioSource").objectReferenceValue = audio;
            cards.FindProperty("selectSound").objectReferenceValue = selectSound;
            cards.ApplyModifiedProperties();
            return panel;
        }

        // Card = frame (root Image, tinted when selected) + dark background + colour swatch + title + details.
        static SelectableCard CreateCard(Transform parent, int index, Vector2 position, Vector2 size)
        {
            var frame = Panel(parent, $"Card_{index + 1}", new Vector2(0.5f, 1f), position, size, new Color(1f, 1f, 1f, 0.25f));
            frame.GetComponent<Image>().raycastTarget = true;   // the frame receives the mouse clicks
            var background = Panel(frame.transform, "Background", Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.08f, 0.08f, 0.12f, 0.9f));
            Stretch(background.GetComponent<RectTransform>(), 4f);
            var swatch = Panel(frame.transform, "Icon", new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(56f, 56f), Color.white);
            swatch.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            var title = Label(frame.transform, "Title", "Club", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(86f, -12f), new Vector2(200f, 32f));
            title.rectTransform.pivot = new Vector2(0f, 1f);
            var details = Label(frame.transform, "Details", "", 18, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(86f, -46f), new Vector2(200f, 60f));
            details.rectTransform.pivot = new Vector2(0f, 1f);

            var button = frame.AddComponent<Button>();
            button.targetGraphic = background.GetComponent<Image>();
            button.transition = Selectable.Transition.None;              // the card draws its own highlight
            button.navigation = new Navigation { mode = Navigation.Mode.None };   // arrow keys walk, not navigate

            var card = frame.AddComponent<SelectableCard>();
            SetReference(card, "frame", frame.GetComponent<Image>());
            SetReference(card, "icon", swatch.GetComponent<Image>());
            SetReference(card, "title", title);
            SetReference(card, "details", details);
            return card;
        }

        // Hit burst: little opaque confetti squares in world space, moved to the ball by GolfSwing on each hit.
        static ParticleSystem CreateHitParticles()
        {
            var go = new GameObject("HitParticles");
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.2f), new Color(1f, 0.4f, 0.7f));
            main.gravityModifier = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 0f;   // only bursts from GolfSwing (Emit)

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GetMaterial("Confetti", Color.white, "Universal Render Pipeline/Particles/Unlit");
            Undo.RegisterCreatedObjectUndo(go, "Create HitParticles");
            MagicMenuUtil.Log("Created HitParticles (burst at the ball on each hit)");
            return particles;
        }

        static BarView Bar(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, bool vertical,
            Color fillColor, float smoothTime)
        {
            var background = Panel(parent, name, anchor, position, size, new Color(0f, 0f, 0f, 0.6f));
            background.GetComponent<RectTransform>().pivot = anchor;
            var fill = Panel(background.transform, "Fill", Vector2.zero, Vector2.zero, Vector2.zero, fillColor);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var label = Label(background.transform, "Label", "", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size);

            var bar = background.AddComponent<BarView>();
            SetReference(bar, "fill", fillRect);
            SetReference(bar, "label", label);
            var serialized = new SerializedObject(bar);
            serialized.FindProperty("vertical").boolValue = vertical;
            serialized.FindProperty("smoothTime").floatValue = smoothTime;
            serialized.ApplyModifiedProperties();
            return bar;
        }

        static GameObject Panel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), anchor, position, size);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        static Text Label(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), anchor, position, size);
            var label = go.GetComponent<Text>();
            label.font = _font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            go.AddComponent<Shadow>();
            return label;
        }

        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        // ---------- Small helpers ----------

        static GameObject MapIcon(Transform parent, string name, int layer, float size, Material material)
        {
            var icon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            icon.name = name;
            Object.DestroyImmediate(icon.GetComponent<Collider>());
            icon.layer = layer;
            icon.transform.SetParent(parent, false);
            icon.transform.localPosition = new Vector3(0f, 5f, 0f);          // above the object, under the map camera
            icon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);    // lying flat, facing up
            icon.transform.localScale = Vector3.one * size;
            icon.GetComponent<Renderer>().sharedMaterial = material;
            icon.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return icon;
        }

        static void Wall(GameObject arena, string name, Vector3 position, Vector3 scale, Material material)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(arena.transform, false);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        static void Part(GameObject parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        static void SetReference(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }

        static GameObject Find(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }
    }
}
