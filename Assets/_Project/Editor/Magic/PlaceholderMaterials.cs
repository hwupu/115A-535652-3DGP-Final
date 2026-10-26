using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Plain-colour URP materials for placeholder prefabs (#9). Created on first use in
    /// Materials/Placeholder; an existing material is returned as is, so colours tweaked by hand stay.
    /// </summary>
    internal static class PlaceholderMaterials
    {
        public const string Folder = "Assets/_Project/Materials/Placeholder";

        public static Material Grass => Get("Grass", new Color(0.36f, 0.62f, 0.24f));
        public static Material Leaves => Get("Leaves", new Color(0.22f, 0.5f, 0.2f));
        public static Material Wood => Get("Wood", new Color(0.45f, 0.3f, 0.18f));
        public static Material Stone => Get("Stone", new Color(0.55f, 0.55f, 0.52f));
        public static Material Water => Get("Water", new Color(0.2f, 0.45f, 0.8f));
        public static Material White => Get("White", new Color(0.95f, 0.95f, 0.95f));
        public static Material Flag => Get("Flag", new Color(0.85f, 0.15f, 0.15f));
        public static Material Hole => Get("Hole", new Color(0.08f, 0.08f, 0.08f));
        public static Material Cat => Get("Cat", new Color(0.85f, 0.55f, 0.25f));
        public static Material Portal => Get("Portal", new Color(0.6f, 0.3f, 0.9f));

        public static Material Get(string name, Color color, string shaderName = "Universal Render Pipeline/Lit")
        {
            var path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            MagicMenuUtil.EnsureFolder(Folder);
            material = new Material(Shader.Find(shaderName) ?? Shader.Find("Standard")) { color = color };
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }
    }
}
