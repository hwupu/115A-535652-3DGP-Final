using UnityEditor;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Setup > Create Core and Game Assemblies: creates the asmdef files from ADR-0010
    /// (Core = generic, reusable; Game = golf game, references Core). Existing asmdefs are left alone.
    /// </summary>
    public static class AssembliesMenu
    {
        public const string CoreName = "Project3DGP.Core";
        public const string GameName = "Project3DGP.Game";

        // Packages most of our code needs; Unity ignores a reference whose package is not installed.
        public const string CommonReferences =
            "\"Unity.InputSystem\", \"UnityEngine.UI\", \"Unity.TextMeshPro\", \"Unity.AI.Navigation\"";

        [MenuItem(MagicMenuUtil.Root + "Setup/Create Core and Game Assemblies", priority = MagicMenuUtil.SetupPriority + 1)]
        static void CreateAssemblies()
        {
            MagicMenuUtil.EnsureTextFile(
                $"Assets/_Project/Scripts/Core/{CoreName}.asmdef",
                AsmdefJson(CoreName, CommonReferences));
            MagicMenuUtil.EnsureTextFile(
                $"Assets/_Project/Scripts/Game/{GameName}.asmdef",
                AsmdefJson(GameName, $"\"{CoreName}\", {CommonReferences}"));
        }

        public static string AsmdefJson(string name, string references) =>
$@"{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{name}"",
    ""references"": [{references}],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
    }
}
