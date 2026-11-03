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
