using System.IO;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.Movement;
using Project3DGP.Core.Portals;
using Project3DGP.Core.UI;
using Project3DGP.MiniEx.Henry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Mini-Exercises > Henry 4.A.3: Set Up Scene (#60). In the 4A3_GamePortals scene, creates what is
    /// missing (found by name): the Portal prefab and materials in the exercise folder, the Hub with 3 portals
    /// (one locked + its floor switch), 3 walled areas (Pond Island, High Platform, Hedge Maze) with a return
    /// portal each, the Hamster (walker + traveler), arrival particles, the screen fader, the HUD and the camera
    /// follow. One Undo step for scene objects.
    /// </summary>
    public static class HenryGamePortalsSetupMenu
    {
        const string ExerciseFolder = "Assets/_Project/MiniExercises/Henry/4A3_GamePortals";
        const string SceneName = "4A3_GamePortals";
        const string PortalPrefabPath = ExerciseFolder + "/Prefabs/Portal.prefab";
        const float WallHeight = 2f;   // higher than the jump (1.2 m), so areas connect only through portals

        static readonly Color Purple = new Color(0.6f, 0.3f, 0.95f);
        static readonly Color Blue = new Color(0.25f, 0.6f, 1f);
        static readonly Color Orange = new Color(1f, 0.55f, 0.15f);
        static readonly Color Green = new Color(0.3f, 0.85f, 0.35f);

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Henry 4.A.3: Set Up Scene", priority = MagicMenuUtil.MiniExercisesPriority + 22)]
        static void SetUp()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (Henry/4A3_GamePortals/Scenes).");
                return;
            }

            var portalPrefab = EnsurePortalPrefab();

            Undo.SetCurrentGroupName("Set Up 4.A.3 Scene");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            // Areas first (each one has an "Arrival" child that portals point to).
            if (Find(scene, "Hub") == null) { CreateHub(); created++; }
            if (Find(scene, "Pond Island") == null) { CreatePondIsland(); created++; }
            if (Find(scene, "High Platform") == null) { CreateHighPlatform(); created++; }
            if (Find(scene, "Hedge Maze") == null) { CreateHedgeMaze(); created++; }

            var hub = Find(scene, "Hub").transform;
            var pond = Find(scene, "Pond Island").transform;
            var platform = Find(scene, "High Platform").transform;
            var maze = Find(scene, "Hedge Maze").transform;

            // Portals: 3 in the hub (one locked), one return portal per area.
            created += PortalIfMissing(portalPrefab, hub, "Portal_To_Pond", new Vector3(-4f, 0f, 4f), pond.Find("Arrival"), Blue, false);
            var lockedPortal = PortalIfMissing(portalPrefab, hub, "Portal_To_Platform", new Vector3(0f, 0f, 5f), platform.Find("Arrival"), Orange, true);
            created += lockedPortal;
            created += PortalIfMissing(portalPrefab, hub, "Portal_To_Maze", new Vector3(4f, 0f, 4f), maze.Find("Arrival"), Green, false);
            created += PortalIfMissing(portalPrefab, pond, "Portal_To_Hub", new Vector3(0f, 0f, 4f), hub.Find("Arrival"), Purple, false);
            created += PortalIfMissing(portalPrefab, platform, "Portal_To_Hub", new Vector3(0f, 4f, 3.5f), hub.Find("Arrival"), Purple, false);
            created += PortalIfMissing(portalPrefab, maze, "Portal_To_Hub", new Vector3(0f, 0f, -4.5f), hub.Find("Arrival"), Purple, false);

            var platformPortal = hub.Find("Portal_To_Platform").GetComponent<Portal>();
            if (hub.Find("UnlockSwitch") == null)
            {
                CreateSwitch(hub, platformPortal);
                created++;
            }

            var fader = Find(scene, "Fader");
            if (fader == null) { fader = CreateFader(); created++; }
            var burst = Find(scene, "ArrivalParticles");
            if (burst == null) { burst = CreateArrivalParticles(); created++; }

            var hamster = Find(scene, "Hamster");
            if (hamster == null)
            {
                hamster = CreateHamster(fader.GetComponent<ScreenFader>(), burst.GetComponent<ParticleSystem>());
                created++;
            }

            if (Find(scene, "HUD") == null)
            {
                var hud = new GameObject("HUD").AddComponent<PortalDemoHud>();
                SetReference(hud, "traveler", hamster.GetComponent<PortalTraveler>());
                SetReference(hud, "lockedPortal", platformPortal);
                Undo.RegisterCreatedObjectUndo(hud.gameObject, "Create HUD");
                MagicMenuUtil.Log("Created HUD");
                created++;
            }

            var camera = Find(scene, "Main Camera");
            if (camera != null && !camera.TryGetComponent(out OrbitFollowCamera _))
            {
                var follow = Undo.AddComponent<OrbitFollowCamera>(camera);
                SetReference(follow, "target", hamster.transform);
                SetReference(hamster.GetComponent<ThirdPersonWalker>(), "cameraTransform", camera.transform);
                MagicMenuUtil.Log("Added OrbitFollowCamera to Main Camera (target: Hamster)");
                created++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (created > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"4.A.3 scene: {created} things created. Save the scene, then press Play.");
        }

        // ---------- Areas ----------

        static void CreateHub()
        {
            var hub = Area("Hub", Vector3.zero);
            Floor(hub, Vector3.zero, new Vector2(14f, 14f), GetMaterial("HubFloor", new Color(0.8f, 0.78f, 0.7f)));
            Pen(hub, Vector3.zero, 7f, GetMaterial("Wall", new Color(0.55f, 0.55f, 0.6f)));
            Arrival(hub, new Vector3(0f, 0f, -1f), 0f);
            Done(hub);
        }

        static void CreatePondIsland()
        {
            var pond = Area("Pond Island", new Vector3(-17f, 0f, 15f));
            Floor(pond, Vector3.zero, new Vector2(12f, 12f), GetMaterial("Sand", new Color(0.9f, 0.82f, 0.55f)));
            Part(pond, PrimitiveType.Cylinder, "Water", new Vector3(0f, 0.02f, 0.5f), new Vector3(5f, 0.01f, 5f),
                GetMaterial("Water", new Color(0.2f, 0.45f, 0.8f)));
            Pen(pond, Vector3.zero, 6f, GetMaterial("Wall", new Color(0.55f, 0.55f, 0.6f)));
            Arrival(pond, new Vector3(0f, 0f, -3.5f), 0f);
            Done(pond);
        }

        // A raised block: the hamster arrives 4 m above the ground, which it could never reach by walking.
        static void CreateHighPlatform()
        {
            var platform = Area("High Platform", new Vector3(17f, 0f, 15f));
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);   // keeps its BoxCollider: it is the floor
            block.name = "Platform";
            block.transform.SetParent(platform.transform, false);
            block.transform.localPosition = new Vector3(0f, 2f, 0f);
            block.transform.localScale = new Vector3(12f, 4f, 12f);
            block.GetComponent<Renderer>().sharedMaterial = GetMaterial("Stone", new Color(0.6f, 0.58f, 0.55f));
            Pen(platform, new Vector3(0f, 4f, 0f), 6f, GetMaterial("Wall", new Color(0.55f, 0.55f, 0.6f)));
            Arrival(platform, new Vector3(0f, 4f, -2.5f), 0f);
            Done(platform);
        }

        static void CreateHedgeMaze()
        {
            var maze = Area("Hedge Maze", new Vector3(0f, 0f, -17f));
            Floor(maze, Vector3.zero, new Vector2(12f, 12f), GetMaterial("Grass", new Color(0.36f, 0.62f, 0.24f)));
            var hedge = GetMaterial("Hedge", new Color(0.18f, 0.42f, 0.18f));
            Pen(maze, Vector3.zero, 6f, hedge);
            // Two baffles: walk around them to reach the return portal at the back.
            Solid(maze, "Hedge_1", new Vector3(-1.5f, WallHeight / 2f, 1.5f), new Vector3(9f, WallHeight, 0.6f), hedge);
            Solid(maze, "Hedge_2", new Vector3(1.5f, WallHeight / 2f, -1.5f), new Vector3(9f, WallHeight, 0.6f), hedge);
            Arrival(maze, new Vector3(0f, 0f, 4f), 180f);
            Done(maze);
        }

        // ---------- Portals, switch, character ----------

        /// <summary>Instantiates the Portal prefab under the area, pointing at the destination. Returns 1 if created.</summary>
        static int PortalIfMissing(GameObject prefab, Transform area, string name, Vector3 localPosition, Transform destination,
            Color color, bool locked)
        {
            if (area.Find(name) != null)
                return 0;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, area.gameObject.scene);
            go.name = name;
            go.transform.SetParent(area, false);
            go.transform.localPosition = localPosition;
            var serialized = new SerializedObject(go.GetComponent<Portal>());
            serialized.FindProperty("destination").objectReferenceValue = destination;
            serialized.FindProperty("locked").boolValue = locked;
            serialized.FindProperty("openColor").colorValue = color;
            serialized.ApplyModifiedProperties();
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            MagicMenuUtil.Log($"Created {area.name}/{name} -> {destination.parent.name}{(locked ? " (locked)" : "")}");
            return 1;
        }

        static void CreateSwitch(Transform hub, Portal portal)
        {
            var go = new GameObject("UnlockSwitch");
            go.transform.SetParent(hub, false);
            go.transform.localPosition = new Vector3(-4f, 0f, -4f);
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.25f, 0f);
            trigger.size = new Vector3(1.2f, 0.5f, 1.2f);
            go.AddComponent<Rigidbody>().isKinematic = true;   // makes trigger events reliable with a CharacterController
            var plate = Part(go, PrimitiveType.Cylinder, "Plate", new Vector3(0f, 0.03f, 0f), new Vector3(1.2f, 0.03f, 1.2f),
                GetMaterial("Switch", new Color(0.85f, 0.2f, 0.2f)));
            var unlock = go.AddComponent<PortalUnlockSwitch>();
            SetReference(unlock, "portal", portal);
            SetReference(unlock, "plateRenderer", plate.GetComponent<Renderer>());
            Undo.RegisterCreatedObjectUndo(go, "Create UnlockSwitch");
            MagicMenuUtil.Log("Created Hub/UnlockSwitch (unlocks Portal_To_Platform)");
        }

        static GameObject CreateHamster(ScreenFader fader, ParticleSystem burst)
        {
            var hamster = new GameObject("Hamster");
            hamster.transform.position = new Vector3(0f, 0f, -4f);
            var controller = hamster.AddComponent<CharacterController>();
            controller.height = 0.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.4f, 0f);
            controller.stepOffset = 0.2f;
            hamster.AddComponent<ThirdPersonWalker>();
            var traveler = hamster.AddComponent<PortalTraveler>();
            SetReference(traveler, "fader", fader);
            SetReference(traveler, "arrivalBurst", burst);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(hamster.transform, false);
            var fur = GetMaterial("Hamster", new Color(0.85f, 0.6f, 0.35f));
            var dark = GetMaterial("HamsterNose", new Color(0.2f, 0.12f, 0.1f));
            Part(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 0.6f), fur);
            Part(visual, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "Nose", new Vector3(0f, 0.5f, 0.3f), Vector3.one * 0.1f, dark);

            Undo.RegisterCreatedObjectUndo(hamster, "Create Hamster");
            MagicMenuUtil.Log("Created Hamster (ThirdPersonWalker + PortalTraveler)");
            return hamster;
        }

        // Full-screen black Image with a CanvasGroup (alpha 0 = invisible) on top of everything.
        static GameObject CreateFader()
        {
            var go = new GameObject("Fader", typeof(Canvas), typeof(CanvasGroup));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            go.AddComponent<ScreenFader>();

            var black = new GameObject("Black", typeof(RectTransform), typeof(Image));
            black.transform.SetParent(go.transform, false);
            var rect = black.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = black.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            Undo.RegisterCreatedObjectUndo(go, "Create Fader");
            MagicMenuUtil.Log("Created Fader (ScreenFader)");
            return go;
        }

        // Arrival burst: world-space sparks around the hamster, emitted by PortalTraveler after each teleport.
        static GameObject CreateArrivalParticles()
        {
            var go = new GameObject("ArrivalParticles");
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.5f, 0.9f, 1f));
            main.gravityModifier = 0.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 0f;   // only bursts (PortalTraveler calls Emit)
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
            Undo.RegisterCreatedObjectUndo(go, "Create ArrivalParticles");
            MagicMenuUtil.Log("Created ArrivalParticles");
            return go;
        }

        // ---------- Portal prefab ----------

        // Root: trigger box + kinematic Rigidbody + Portal. Visual: glowing pad and a frame. Swirl: looping particles.
        static GameObject EnsurePortalPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath);
            if (existing != null)
                return existing;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(PortalPrefabPath));
            var buildScene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject("Portal");
                SceneManager.MoveGameObjectToScene(root, buildScene);
                var trigger = root.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1f, 0f);
                trigger.size = new Vector3(1.6f, 2f, 1.6f);
                root.AddComponent<Rigidbody>().isKinematic = true;   // makes trigger events reliable with a CharacterController
                var portal = root.AddComponent<Portal>();

                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                var pad = Part(visual, PrimitiveType.Cylinder, "Pad", new Vector3(0f, 0.03f, 0f), new Vector3(1.6f, 0.03f, 1.6f),
                    GetMaterial("PortalPad", Color.white));
                var frame = GetMaterial("PortalFrame", new Color(0.25f, 0.22f, 0.3f));
                Part(visual, PrimitiveType.Cube, "PillarL", new Vector3(-0.85f, 1.1f, 0f), new Vector3(0.15f, 2.2f, 0.15f), frame);
                Part(visual, PrimitiveType.Cube, "PillarR", new Vector3(0.85f, 1.1f, 0f), new Vector3(0.15f, 2.2f, 0.15f), frame);
                Part(visual, PrimitiveType.Cube, "Beam", new Vector3(0f, 2.2f, 0f), new Vector3(1.85f, 0.15f, 0.15f), frame);

                var swirl = CreateSwirl(root.transform);

                var serialized = new SerializedObject(portal);
                serialized.FindProperty("loopParticles").objectReferenceValue = swirl;
                serialized.FindProperty("padRenderer").objectReferenceValue = pad.GetComponent<Renderer>();
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PortalPrefabPath);
                Object.DestroyImmediate(root);
                MagicMenuUtil.Log("Created " + PortalPrefabPath);
                return prefab;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(buildScene);
            }
        }

        // Swirl: particles born on a flat ring, rising (velocity Y) while orbiting around the portal's Y axis.
        static ParticleSystem CreateSwirl(Transform parent)
        {
            var go = new GameObject("Swirl");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = particles.emission;
            emission.rateOverTime = 40f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.7f;
            shape.rotation = new Vector3(90f, 0f, 0f);   // the circle lies in XY by default; turn it flat onto the floor
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = 0f;
            velocity.y = 1.2f;        // rise
            velocity.z = 0f;
            velocity.orbitalX = 0f;
            velocity.orbitalY = 2f;   // orbit around Y = swirl
            velocity.orbitalZ = 0f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
            return particles;
        }

        // ---------- Small helpers ----------

        static GameObject Area(string name, Vector3 position)
        {
            var area = new GameObject(name);
            area.transform.position = position;
            return area;
        }

        static void Done(GameObject area)
        {
            Undo.RegisterCreatedObjectUndo(area, "Create " + area.name);
            MagicMenuUtil.Log("Created area " + area.name);
        }

        static void Arrival(GameObject area, Vector3 localPosition, float yaw)
        {
            var arrival = new GameObject("Arrival");
            arrival.transform.SetParent(area.transform, false);
            arrival.transform.localPosition = localPosition;
            arrival.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        // Thin coloured tile on top of the ground plane (top surface at y = 0.01).
        static void Floor(GameObject area, Vector3 localCenter, Vector2 size, Material material) =>
            Solid(area, "Floor", localCenter + new Vector3(0f, -0.04f, 0f), new Vector3(size.x, 0.1f, size.y), material);

        // Four walls around a square of half size `half`, standing on `localBase`.
        static void Pen(GameObject area, Vector3 localBase, float half, Material material)
        {
            var length = half * 2f + 0.4f;
            var y = localBase.y + WallHeight / 2f;
            Solid(area, "Wall_North", new Vector3(localBase.x, y, localBase.z + half), new Vector3(length, WallHeight, 0.4f), material);
            Solid(area, "Wall_South", new Vector3(localBase.x, y, localBase.z - half), new Vector3(length, WallHeight, 0.4f), material);
            Solid(area, "Wall_East", new Vector3(localBase.x + half, y, localBase.z), new Vector3(0.4f, WallHeight, length), material);
            Solid(area, "Wall_West", new Vector3(localBase.x - half, y, localBase.z), new Vector3(0.4f, WallHeight, length), material);
        }

        // A cube that keeps its collider (walls, floors, hedges).
        static void Solid(GameObject parent, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent.transform, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }

        // A primitive without collider (visual only).
        static GameObject Part(GameObject parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        static Material ParticleMaterial() => GetMaterial("Particle", Color.white, "Universal Render Pipeline/Particles/Unlit");

        // Materials live in the exercise folder so the exercise exports on its own.
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
