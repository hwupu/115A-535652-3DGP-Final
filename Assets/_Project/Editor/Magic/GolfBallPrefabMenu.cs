using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Create Golf Ball Prefab (#14): Prefabs/Shared/GolfBall.prefab (Rigidbody + SphereCollider +
    /// GolfBall, white sphere) and its bouncy Ball.physicsMaterial. Only what is missing is created.
    /// </summary>
    public static class GolfBallPrefabMenu
    {
        public const string Folder = "Assets/_Project/Prefabs/Shared";
        public const string PrefabPath = Folder + "/GolfBall.prefab";
        public const string MaterialPath = Folder + "/Ball.physicsMaterial";
        public const float Diameter = 0.2f;   // bigger than a real golf ball (4.3 cm) so it reads next to the hamster

        [MenuItem(MagicMenuUtil.Root + "Game/Create Golf Ball Prefab", priority = MagicMenuUtil.GamePriority + 10)]
        public static GameObject Create()
        {
            MagicMenuUtil.EnsureFolder(Folder);
            var physicsMaterial = EnsurePhysicsMaterial();

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                MagicMenuUtil.Log("Skipped (exists) " + PrefabPath);
                return existing;
            }

            // Build in a preview scene so the open scene isn't touched.
            var buildScene = EditorSceneManager.NewPreviewScene();
            try
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);   // SphereCollider radius 0.5 (x scale)
                ball.name = "GolfBall";
                SceneManager.MoveGameObjectToScene(ball, buildScene);
                ball.transform.localScale = Vector3.one * Diameter;
                ball.GetComponent<MeshRenderer>().sharedMaterial = PlaceholderMaterials.White;
                ball.GetComponent<SphereCollider>().sharedMaterial = physicsMaterial;
                var body = ball.AddComponent<Rigidbody>();
                body.mass = 0.1f;   // light: wind/traps with ForceMode.Force push it noticeably
                ball.AddComponent<GolfBall>();

                var prefab = PrefabUtility.SaveAsPrefabAsset(ball, PrefabPath);
                MagicMenuUtil.Log("Created " + PrefabPath);
                return prefab;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(buildScene);
            }
        }

        static PhysicsMaterial EnsurePhysicsMaterial()
        {
            // Never overwrite an existing file, even if it doesn't load as the expected type.
            if (System.IO.File.Exists(MaterialPath))
            {
                MagicMenuUtil.Log("Skipped (exists) " + MaterialPath);
                return AssetDatabase.LoadMainAssetAtPath(MaterialPath) as PhysicsMaterial;
            }

            var material = new PhysicsMaterial("Ball")
            {
                bounciness = 0.45f,       // keeps 45% of the bounce speed
                dynamicFriction = 0.4f,   // friction is what makes the sphere spin = roll
                staticFriction = 0.5f,
                bounceCombine = PhysicsMaterialCombine.Average,
                frictionCombine = PhysicsMaterialCombine.Average,
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
            MagicMenuUtil.Log("Created " + MaterialPath);
            return material;
        }
    }
}
