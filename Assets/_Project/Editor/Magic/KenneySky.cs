using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// The Kenney panoramic skies (CC0, Assets/ThirdParty/Kenney/Skyboxes): "day" for L1 and the main menu, "morning"
    /// for the Ending. Creates Materials/Kenney/KenneySky&lt;Name&gt;.mat (Skybox/Panoramic) once.
    /// </summary>
    internal static class KenneySky
    {
        const string TextureFolder = "Assets/ThirdParty/Kenney/Skyboxes";
        const string MaterialFolder = "Assets/_Project/Materials/Kenney";

        /// <summary>The sky material for "day" or "morning"; null if the texture is missing.</summary>
        public static Material Get(string name)
        {
            var title = char.ToUpper(name[0]) + name.Substring(1);
            var path = $"{MaterialFolder}/KenneySky{title}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            var texturePath = $"{TextureFolder}/skybox-{name}.png";
            if (AssetImporter.GetAtPath(texturePath) is not TextureImporter importer)
                return null;
            importer.mipmapEnabled = false;   // no seam where the panorama wraps
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();

            material = new Material(Shader.Find("Skybox/Panoramic"));
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_Mapping", 1f);     // latitude-longitude layout
            material.SetFloat("_ImageType", 0f);   // 360 degrees
            material.EnableKeyword("_MAPPING_LATITUDE_LONGITUDE_LAYOUT");
            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }
    }
}
