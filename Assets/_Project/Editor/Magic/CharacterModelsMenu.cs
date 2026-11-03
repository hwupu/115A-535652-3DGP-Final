using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Prefabs > Use Character Models (#36, #37): the rigged, animated Tripo3D models in Art/Characters
    /// (Blender scripts in Art/Characters/BlenderScripts~) replace the placeholder looks. For each of Hamster,
    /// Princess and Cat, only what is missing:
    ///   1. FBX import settings: Generic rig, axis conversion baked, clips named and looped, swing impact events
    ///      (only while the FBX has no clip list yet, so later Inspector tweaks stay).
    ///   2. Material  Art/Characters/X/X.mat (URP Lit, the base colour texture).
    ///   3. Animator controller  Art/Characters/X/X.controller (states and parameters below).
    ///   4. Prefabs/Characters/X.prefab: the placeholder "Visual" child is replaced by the model, named "Visual",
    ///      with the Animator using the controller. Hamster: a "ToolSocket" in the right hand and a SwingImpactRelay.
    ///      Cat: the box collider is fitted to the new body. Prefabs already using the model are skipped.
    ///   5. Club profiles: Sledgehammer swings like a hammer, Mysterious taps (only if still the default).
    /// GolferAvatar (a variant of Hamster) and every placed cat follow the prefabs. No scene is touched.
    ///   Hamster  Idle, Walk (Speed), AimIdle (Aiming), Swing_Club / Swing_Hammer / Swing_Mysterious (trigger Swing +
    ///            int Tool = ClubProfile.Swing), Equip, Celebrate, Fail, Flinch (triggers); bool Walking (WalkTo)
    ///   Princess Idle, Walk (Speed), Wave, Cheer (triggers), Scared (bool)
    ///   Cat      Asleep, Twitching, Suspicious, Searching, Awake (int State = CatState)
    /// </summary>
    public static class CharacterModelsMenu
    {
        const string ArtFolder = "Assets/_Project/Art/Characters";

        sealed class Clip
        {
            public string Name; public bool Loop; public string Event; public int EventFrame;
            public Clip(string name, bool loop, string evt = null, int frame = 0) { Name = name; Loop = loop; Event = evt; EventFrame = frame; }
        }

        // Clip table from the Blender export (BlenderScripts~/anim.py): looping, impact event frame (24 fps, Blender
        // frame 1 = start; Unity's takes start at 0). The "Rest" take (T-pose, keeps the FBX's default pose) is unused.
        static readonly Dictionary<string, Clip[]> Clips = new()
        {
            ["Hamster"] = new[]
            {
                new Clip("Idle", true), new Clip("Walk", true), new Clip("AimIdle", true), new Clip("Celebrate", true),
                new Clip("Swing_Club", false, "OnSwingImpact", 10), new Clip("Swing_Hammer", false, "OnSwingImpact", 16),
                new Clip("Swing_Mysterious", false, "OnSwingImpact", 9), new Clip("Equip", false), new Clip("Fail", false),
                new Clip("Flinch", false),
            },
            ["Princess"] = new[] { new Clip("Idle", true), new Clip("Walk", true), new Clip("Wave", false), new Clip("Cheer", true), new Clip("Scared", true) },
            ["Cat"] = new[] { new Clip("Asleep", true), new Clip("Twitching", true), new Clip("Suspicious", true), new Clip("Searching", true), new Clip("Awake", true) },
        };

        [MenuItem(MagicMenuUtil.Root + "Prefabs/Use Character Models", priority = MagicMenuUtil.PrefabsPriority + 5)]
        static void UseModels()
        {
            foreach (var name in Clips.Keys)
                Setup(name);
            SetSwingStyle(ClubProfilesMenu.SledgehammerPath, SwingStyle.Hammer);
            SetSwingStyle(ClubProfilesMenu.MysteriousPath, SwingStyle.Tap);
            AssetDatabase.SaveAssets();
            MagicMenuUtil.Log("Character models done. Open a level and press Play: the hamster, the cat (L1) and the princess use the new models.");
        }

        static void Setup(string name)
        {
            var fbxPath = $"{ArtFolder}/{name}/{name}.fbx";
            if (!File.Exists(fbxPath))
            {
                MagicMenuUtil.Log($"Skipped {name}: no {fbxPath}");
                return;
            }
            var material = EnsureMaterial(name);
            ConfigureImport(name, fbxPath, material);
            var controller = EnsureController(name);
            SwapPrefab(name, fbxPath, controller);
        }

        // ---------- 1. import settings ----------

        static void ConfigureImport(string name, string fbxPath, Material material)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
            if (importer.clipAnimations.Length > 0)
            {
                MagicMenuUtil.Log($"Skipped (configured) import settings of {fbxPath}");
                return;
            }
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name + "_Mat"), material);
            importer.SaveAndReimport();   // the takes are known only after an import with animation on

            var table = Clips[name].ToDictionary(c => c.Name);
            var clips = new List<ModelImporterClipAnimation>();
            foreach (var take in importer.defaultClipAnimations)
            {
                // Blender names the takes "<Rig>|<Action>".
                var clipName = take.takeName.Contains('|') ? take.takeName.Substring(take.takeName.LastIndexOf('|') + 1) : take.takeName;
                take.name = clipName;
                if (table.TryGetValue(clipName, out var spec))
                {
                    take.loopTime = spec.Loop;
                    if (spec.Event != null)
                    {
                        var length = Mathf.Max(1f, take.lastFrame - take.firstFrame);
                        take.events = new[] { new AnimationEvent { functionName = spec.Event, time = Mathf.Clamp01((spec.EventFrame - 1 - take.firstFrame) / length) } };
                    }
                }
                clips.Add(take);
            }
            importer.clipAnimations = clips.ToArray();
            importer.SaveAndReimport();
            MagicMenuUtil.Log($"Import settings of {fbxPath}: Generic rig, {clips.Count} clips ({string.Join(", ", clips.Select(c => c.name))})");
        }

        // ---------- 2. material ----------

        static Material EnsureMaterial(string name)
        {
            var path = $"{ArtFolder}/{name}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = name };
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtFolder}/{name}/{name}_BaseColor.jpg");
            if (texture != null)
            {
                material.SetTexture("_BaseMap", texture);
                material.mainTexture = texture;
            }
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.15f);   // matte, hand-painted look
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }

        // ---------- 3. animator controller ----------

        static AnimatorController EnsureController(string name)
        {
            var path = $"{ArtFolder}/{name}/{name}.controller";
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null)
                return existing;
            var clips = AssetDatabase.LoadAllAssetsAtPath($"{ArtFolder}/{name}/{name}.fbx").OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var sm = controller.layers[0].stateMachine;
            AnimatorState State(string clip, Vector2 at)
            {
                var state = sm.AddState(clip, new Vector3(at.x, at.y, 0f));
                if (clips.TryGetValue(clip, out var c))
                    state.motion = c;
                else
                    MagicMenuUtil.Log($"{name}: no clip {clip} in the FBX");
                return state;
            }

            switch (name)
            {
                case "Hamster": BuildHamster(controller, sm, State); break;
                case "Princess": BuildPrincess(controller, sm, State); break;
                case "Cat": BuildCat(controller, sm, State); break;
            }
            EditorUtility.SetDirty(controller);
            MagicMenuUtil.Log("Created " + path);
            return controller;
        }

        delegate AnimatorState StateMaker(string clip, Vector2 at);

        static void BuildHamster(AnimatorController c, AnimatorStateMachine sm, StateMaker make)
        {
            c.AddParameter("Speed", AnimatorControllerParameterType.Float);
            c.AddParameter("Aiming", AnimatorControllerParameterType.Bool);
            c.AddParameter("Walking", AnimatorControllerParameterType.Bool);
            c.AddParameter("Tool", AnimatorControllerParameterType.Int);
            foreach (var t in new[] { "Swing", "Equip", "Celebrate", "Fail", "Flinch" })
                c.AddParameter(t, AnimatorControllerParameterType.Trigger);

            var idle = make("Idle", new Vector2(300, 0));
            var walk = make("Walk", new Vector2(300, 120));
            var aim = make("AimIdle", new Vector2(600, 0));
            sm.defaultState = idle;
            Link(idle, walk, 0.15f, (AnimatorConditionMode.Greater, "Speed", 0.15f));
            Link(idle, walk, 0.15f, (AnimatorConditionMode.If, "Walking", 0f));
            Link(walk, idle, 0.15f, (AnimatorConditionMode.Less, "Speed", 0.1f), (AnimatorConditionMode.IfNot, "Walking", 0f));
            Link(idle, aim, 0.2f, (AnimatorConditionMode.If, "Aiming", 0f));
            Link(aim, idle, 0.2f, (AnimatorConditionMode.IfNot, "Aiming", 0f));
            Link(aim, walk, 0.15f, (AnimatorConditionMode.Greater, "Speed", 0.15f), (AnimatorConditionMode.IfNot, "Aiming", 0f));

            string[] swings = { "Swing_Club", "Swing_Hammer", "Swing_Mysterious" };
            for (var i = 0; i < swings.Length; i++)
            {
                var swing = make(swings[i], new Vector2(900, i * 80));
                var t = sm.AddAnyStateTransition(swing);
                Setup(t, 0.05f, (AnimatorConditionMode.If, "Swing", 0f), (AnimatorConditionMode.Equals, "Tool", i));
                t.canTransitionToSelf = false;
                ExitTo(swing, idle);
            }
            var equip = make("Equip", new Vector2(600, 160));
            Link(aim, equip, 0.05f, (AnimatorConditionMode.If, "Equip", 0f));
            Link(idle, equip, 0.05f, (AnimatorConditionMode.If, "Equip", 0f));
            ExitTo(equip, aim);
            var flinch = make("Flinch", new Vector2(600, 240));
            Link(idle, flinch, 0.05f, (AnimatorConditionMode.If, "Flinch", 0f));
            Link(aim, flinch, 0.05f, (AnimatorConditionMode.If, "Flinch", 0f));
            ExitTo(flinch, idle);
            foreach (var (trigger, y) in new[] { ("Celebrate", 320), ("Fail", 400) })
            {
                var end = make(trigger, new Vector2(900, y));   // Celebrate loops, Fail holds its last pose
                var t = sm.AddAnyStateTransition(end);
                Setup(t, 0.1f, (AnimatorConditionMode.If, trigger, 0f));
                t.canTransitionToSelf = false;
            }
        }

        static void BuildPrincess(AnimatorController c, AnimatorStateMachine sm, StateMaker make)
        {
            c.AddParameter("Speed", AnimatorControllerParameterType.Float);
            c.AddParameter("Scared", AnimatorControllerParameterType.Bool);
            c.AddParameter("Wave", AnimatorControllerParameterType.Trigger);
            c.AddParameter("Cheer", AnimatorControllerParameterType.Trigger);
            var idle = make("Idle", new Vector2(300, 0));
            var walk = make("Walk", new Vector2(300, 120));
            sm.defaultState = idle;
            Link(idle, walk, 0.15f, (AnimatorConditionMode.Greater, "Speed", 0.15f));
            Link(walk, idle, 0.15f, (AnimatorConditionMode.Less, "Speed", 0.1f));
            var wave = make("Wave", new Vector2(600, 0));
            Link(idle, wave, 0.1f, (AnimatorConditionMode.If, "Wave", 0f));
            ExitTo(wave, idle);
            var cheer = make("Cheer", new Vector2(600, 100));
            var toCheer = sm.AddAnyStateTransition(cheer);
            Setup(toCheer, 0.1f, (AnimatorConditionMode.If, "Cheer", 0f));
            toCheer.canTransitionToSelf = false;
            var scared = make("Scared", new Vector2(600, 200));
            Link(idle, scared, 0.15f, (AnimatorConditionMode.If, "Scared", 0f));
            Link(scared, idle, 0.25f, (AnimatorConditionMode.IfNot, "Scared", 0f));
        }

        static void BuildCat(AnimatorController c, AnimatorStateMachine sm, StateMaker make)
        {
            c.AddParameter("State", AnimatorControllerParameterType.Int);
            string[] states = { "Asleep", "Twitching", "Suspicious", "Searching", "Awake" };   // = CatState order
            for (var i = 0; i < states.Length; i++)
            {
                var state = make(states[i], new Vector2(400, i * 80));
                if (i == 0)
                    sm.defaultState = state;
                var t = sm.AddAnyStateTransition(state);
                Setup(t, 0.4f, (AnimatorConditionMode.Equals, "State", i));
                t.canTransitionToSelf = false;
            }
        }

        static void Link(AnimatorState from, AnimatorState to, float blend, params (AnimatorConditionMode Mode, string Param, float Value)[] conditions) =>
            Setup(from.AddTransition(to), blend, conditions);

        static void Setup(AnimatorStateTransition t, float blend, params (AnimatorConditionMode Mode, string Param, float Value)[] conditions)
        {
            t.hasExitTime = false;
            t.duration = blend;
            foreach (var (mode, param, value) in conditions)
                t.AddCondition(mode, value, param);
        }

        // A one-shot clip goes back when it ends.
        static void ExitTo(AnimatorState from, AnimatorState to)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime = 0.95f;
            t.duration = 0.1f;
        }

        // ---------- 4. prefab ----------

        static void SwapPrefab(string name, string fbxPath, AnimatorController controller)
        {
            var prefabPath = $"{PlaceholderPrefabsMenu.Folder}/Characters/{name}.prefab";
            if (!File.Exists(prefabPath))
            {
                MagicMenuUtil.Log($"Skipped: no {prefabPath} (run Magic > Prefabs > Create Placeholder Prefabs first)");
                return;
            }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var old = root.transform.Find("Visual");
                if (old != null && old.GetComponentInChildren<Animator>() != null)
                {
                    MagicMenuUtil.Log($"Skipped (uses the model) {prefabPath}");
                    return;
                }
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);

                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (!visual.TryGetComponent(out Animator animator))   // not ??: Unity's fake null
                    animator = visual.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                FixFacing(root.transform, visual.transform, name);

                if (name == "Hamster")
                {
                    AddToolSocket(root.transform, visual.transform);
                    visual.AddComponent<SwingImpactRelay>();
                }
                if (name == "Cat" && root.TryGetComponent(out BoxCollider box))
                    FitBox(root.transform, box, visual);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                MagicMenuUtil.Log($"{prefabPath}: placeholder Visual replaced by the {name} model");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // The character must face +Z with its left hand on -X; turn the Visual if the axis conversion did otherwise.
        static void FixFacing(Transform root, Transform visual, string name)
        {
            var hand = Deep(visual, "Hand_L");
            var head = Deep(visual, "Head");
            var hips = Deep(visual, "Hips");
            if (hand == null || head == null || hips == null)
            {
                MagicMenuUtil.Log($"{name}: bones not found, facing not checked");
                return;
            }
            if (root.InverseTransformPoint(head.position).y < root.InverseTransformPoint(hips.position).y)
                MagicMenuUtil.Log($"{name}: the head is below the hips: check the FBX axis settings");
            if (root.InverseTransformPoint(hand.position).x > 0f)
            {
                visual.localRotation = Quaternion.Euler(0f, 180f, 0f);
                MagicMenuUtil.Log($"{name}: turned the model 180° so it faces +Z");
            }
        }

        // Right hand: the tool's grip goes here; the tool (hanging down along its -Y in its prefab) points along the
        // arm, so with the arm lowered in the aim pose the club reaches the ground in front.
        static void AddToolSocket(Transform root, Transform visual)
        {
            var hand = Deep(visual, "Hand_R");
            var lower = Deep(visual, "LowerArm_R");
            if (hand == null || lower == null)
            {
                MagicMenuUtil.Log("Hamster: no Hand_R bone, ToolSocket not added");
                return;
            }
            var socket = new GameObject("ToolSocket").transform;
            socket.SetParent(hand, false);
            var outward = (hand.position - lower.position).normalized;
            var forward = Vector3.ProjectOnPlane(root.forward, outward).normalized;
            socket.rotation = Quaternion.LookRotation(forward, -outward);
            MagicMenuUtil.Log("Hamster: added ToolSocket under Hand_R (move / turn it if the tool sits oddly)");
        }

        static void FitBox(Transform root, BoxCollider box, GameObject visual)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            box.center = root.InverseTransformPoint(bounds.center);
            box.size = Vector3.Scale(bounds.size, new Vector3(0.8f, 1f, 0.8f));   // a bit inside the fur and tail
            MagicMenuUtil.Log($"Cat: box collider fitted to the model ({box.size.x:0.0} x {box.size.y:0.0} x {box.size.z:0.0} m)");
        }

        // ---------- 5. clubs ----------

        static void SetSwingStyle(string path, SwingStyle style)
        {
            var club = AssetDatabase.LoadAssetAtPath<ClubProfile>(path);
            if (club == null || club.Swing != SwingStyle.Club)
                return;
            club.Swing = style;
            EditorUtility.SetDirty(club);
            MagicMenuUtil.Log($"{path}: swing = {style}");
        }

        static Transform Deep(Transform parent, string name) =>
            parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    }
}
