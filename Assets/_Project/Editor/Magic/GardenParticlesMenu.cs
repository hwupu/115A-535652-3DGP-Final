using Project3DGP.Game.Effects;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Levels.Garden;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Add Particle Effects (#39): the five Garden particle systems as prefab instances
    /// (Prefabs/FX, see ParticlePrefabs; replace a prefab and every instance follows):
    ///   Gameplay/Effects/FX_ImpactSparks    + ShotImpactEffect on Systems/ShotController (every shot, at the ball)
    ///   Gameplay/Effects/FX_WaterSplash     + BallTriggerEffect on each Pond (ball lands in the water)
    ///   Gameplay/Effects/FX_Confetti        + BallTriggerEffect on Gameplay/Goal/Cup (ball in the cup)
    ///   Cat/FX_CatZzz                       + CatSleepEffect on the cat (while it sleeps)
    ///   Tree_*/FX_FallingLeaves             ambient, under every tree in Environment/Props
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class GardenParticlesMenu
    {
        const string SceneName = "L1_Garden";

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Add Particle Effects", priority = MagicMenuUtil.LevelPriority + 23)]
        static void Add()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (these are the Garden's effects).");
                return;
            }
            var gameplay = LevelSkeleton.Find(scene, "Gameplay");
            if (gameplay == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            Undo.SetCurrentGroupName("Add Garden Particle Effects");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;

            // Impact sparks on every shot.
            var shot = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
            if (shot != null && ball != null && shot.GetComponent<ShotImpactEffect>() == null)
            {
                var sparks = Instance(ParticlePrefabs.ImpactSparks, Effects(scene, gameplay));
                var effect = Undo.AddComponent<ShotImpactEffect>(shot.gameObject);
                Wire(effect, ("shot", shot), ("ball", ball), ("effect", sparks));
                MagicMenuUtil.Log("Added impact sparks (ShotImpactEffect on Systems/ShotController)");
                added++;
            }
            else if (shot == null || ball == null)
                MagicMenuUtil.Log("No ShotController or ball: run Magic > Level > Add Ball, Shot and Avatar first for the impact sparks.");

            // Water splash on each pond (the ponds are InvalidZones).
            foreach (var zone in Object.FindObjectsByType<InvalidZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!zone.name.StartsWith("Pond") || zone.GetComponent<BallTriggerEffect>() != null)
                    continue;
                var splash = Instance(ParticlePrefabs.WaterSplash, Effects(scene, gameplay));
                Wire(Undo.AddComponent<BallTriggerEffect>(zone.gameObject), ("effect", splash));
                MagicMenuUtil.Log($"Added the water splash to {zone.name}");
                added++;
            }

            // Confetti in the cup.
            var cup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            if (cup != null && cup.GetComponent<BallTriggerEffect>() == null)
            {
                if (cup.TryGetComponent(out Collider collider) && collider.isTrigger)
                {
                    var confetti = Instance(ParticlePrefabs.Confetti, Effects(scene, gameplay));
                    confetti.transform.position = cup.transform.position;
                    Wire(Undo.AddComponent<BallTriggerEffect>(cup), ("effect", confetti));
                    MagicMenuUtil.Log("Added confetti to Gameplay/Goal/Cup");
                    added++;
                }
                else
                    MagicMenuUtil.Log("Gameplay/Goal/Cup has no trigger collider: confetti skipped.");
            }

            // Zzz above the sleeping cat.
            var cat = Object.FindAnyObjectByType<GardenCat>(FindObjectsInactive.Include);
            if (cat != null && cat.GetComponent<CatSleepEffect>() == null)
            {
                var zzz = Instance(ParticlePrefabs.CatZzz, cat.transform);
                zzz.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                Wire(Undo.AddComponent<CatSleepEffect>(cat.gameObject), ("cat", cat), ("zzz", zzz));
                MagicMenuUtil.Log($"Added the Zzz to {cat.name} (move FX_CatZzz to its head if needed)");
                added++;
            }
            else if (cat == null)
                MagicMenuUtil.Log("No GardenCat: run Magic > Level > Garden: Set Up Cat first for the Zzz.");

            // Falling leaves under every tree.
            var props = LevelSkeleton.Find(scene, "Environment/Props");
            if (props != null)
            {
                foreach (Transform tree in props.transform)
                {
                    if (!tree.name.StartsWith("Tree") || tree.Find(ParticlePrefabs.FallingLeaves.name) != null)
                        continue;
                    var leaves = Instance(ParticlePrefabs.FallingLeaves, tree);
                    leaves.transform.localPosition = new Vector3(0f, 3.5f, 0f);
                    added++;
                }
                MagicMenuUtil.Log("Falling leaves: one FX_FallingLeaves under each Tree_* (move it to the crown if needed)");
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Garden particle effects: {added} added. Replace the prefabs in Prefabs/FX any time. Save the scene.");
        }

        static Transform Effects(Scene scene, GameObject gameplay)
        {
            var existing = LevelSkeleton.Find(scene, "Gameplay/Effects");
            if (existing != null)
                return existing.transform;
            var go = new GameObject("Effects");
            go.transform.SetParent(gameplay.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Add Gameplay/Effects");
            return go.transform;
        }

        static ParticleSystem Instance(GameObject prefab, Transform parent)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene);
            go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, "Add " + prefab.name);
            return go.GetComponent<ParticleSystem>();
        }

        static void Wire(Object component, params (string Field, Object Value)[] fields)
        {
            var so = new SerializedObject(component);
            foreach (var (field, value) in fields)
                so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
