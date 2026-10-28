using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Eyes for the placeholder cat (#21): EyeLeft / EyeRight on the front of the head, so GardenCat can show
    /// them closed (asleep), half open (suspicious) and wide open (awake). New cats get them from
    /// PlaceholderPrefabsMenu; Ensure adds them to an existing Characters/Cat prefab once.
    /// </summary>
    internal static class CatPrefabEyes
    {
        public static string PrefabPath => PlaceholderPrefabsMenu.Folder + "/Characters/Cat.prefab";

        static Material EyeMaterial => PlaceholderMaterials.Get("CatEye", new Color(0.15f, 0.3f, 0.1f));

        public static void Add(Transform visual)
        {
            Eye(visual, "EyeLeft", -0.15f);
            Eye(visual, "EyeRight", 0.15f);
        }

        public static void Ensure()
        {
            if (!File.Exists(PrefabPath))
                return;
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var visual = root.transform.Find("Visual");
                if (visual == null || visual.Find("EyeLeft") != null)
                    return;
                Add(visual);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                MagicMenuUtil.Log("Added eyes to " + PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // A flat dark disc on the head sphere (centre (0, 0.8, 1.2), radius 0.4).
        static void Eye(Transform visual, string name, float x)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.name = name;
            eye.transform.SetParent(visual, false);
            eye.transform.localPosition = new Vector3(x, 0.9f, 1.55f);
            eye.transform.localScale = new Vector3(0.14f, 0.14f, 0.06f);
            eye.GetComponent<MeshRenderer>().sharedMaterial = EyeMaterial;
        }
    }
}
