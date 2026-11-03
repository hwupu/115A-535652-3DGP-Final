using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// The Unity Asset Store packs the game uses (#74). They are never committed (CLAUDE.md rule 6); each member imports
    /// them (docs/asset-store-packs.md). Assets are found by GUID, which is the same on every machine and does not
    /// change when a pack folder is moved (e.g. into Assets/AssetStore/), so the tools never depend on a path.
    /// </summary>
    internal static class AssetStorePacks
    {
        public const string Guide = "docs/asset-store-packs.md";

        /// <summary>"Farm Game UI - Starter 2D" by maanetorn (256 px sprites, already 9-sliced).</summary>
        public static class FarmUi
        {
            public const string Name = "Farm Game UI - Starter 2D";
            public const string ButtonGreen = "8b572ed8117047647b2d3d47940bb1b9", ButtonGreenPressed = "7f85979030a88974992ae57906f3b04a";
            public const string ButtonYellow = "202cca839338a4d49aa5db82c2459568", ButtonYellowPressed = "a8aa511ef75cdd84ca98b1e39ff9d760";
            public const string ButtonRed = "e7cc71f7591cdfc4584df69c9acce28a", ButtonRedPressed = "fef7475174d129d488ffdac3175fed69";
            public const string ButtonGray = "dc64f8ab275be8141a12dd26f6324a99", ButtonGrayPressed = "5d039fe522528a74d9f73e3ae2fe0bb4";
            public const string ButtonBlue = "c6f3ddfa43da4ad4cad2375c9d4df858", ButtonBluePressed = "93e49e8ed4ce36449bf40e67cb7d07e8";
            public const string Panel = "1d7501232a26f3d4ba68eb61c8ac5dcf";
            public const string SliderBackground = "da657823a27fce44aacc30bef734d07e";
            public const string SliderFill = "341823109243a314f89c4fc3e44f1080";
        }

        /// <summary>"Free Stylized Hand-Painted Skybox" by BD Studios (cubemap skybox materials).</summary>
        public static class Skybox
        {
            public const string Name = "Free Stylized Hand-Painted Skybox";
            public const string DayClear = "1e5e82b01cae80b44b78975b9c2d7741";
            public const string Sunset = "170f2a047c448444ca1b8e2d20f504f1";
        }

        /// <summary>"Handpainted Grass &amp; Ground Textures" by Chromisu (2048 px albedo textures).</summary>
        public static class Ground
        {
            public const string Name = "Handpainted Grass & Ground Textures";
            public const string GrassNormal = "deb64ef6450ec574db0af67225551acf";
            public const string GrassLighted = "9273b0666f47d5e4e86165bf96cb7404";
            public const string GrassDarked = "df73a5b6a8127ff40a8c6840516304de";
            public const string DirtNormal = "0dd443a2df90ef247be261e60d0ce83b";
        }

        /// <summary>"Pandazole - Nature Environment Low poly Pack" (one atlas texture for every model).</summary>
        public static class Pandazole
        {
            public const string Name = "Pandazole - Nature Environment Low poly Pack";
            public const string AtlasTexture = "78cac7c1dfe137c4ab5a709bfa74e0f0";

            /// <summary>A model (FBX) of the pack by file name, e.g. "Tree_10_Spring"; null if the pack is missing.</summary>
            public static GameObject Model(string fileName)
            {
                var path = AssetDatabase.FindAssets(fileName + " t:Model")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(p => p.EndsWith("/Models/" + fileName + ".fbx"));
                return path == null ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        /// <summary>The asset with this GUID, or null if its pack is not imported.</summary>
        public static T Load<T>(string guid) where T : Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>True if the asset is there; otherwise logs how to install the pack.</summary>
        public static bool Require(string packName, string guid)
        {
            if (Load<Object>(guid) != null)
                return true;
            MagicMenuUtil.Log($"The Asset Store pack \"{packName}\" is not imported. Import it first (see {Guide}), then run this again.");
            return false;
        }
    }
}
