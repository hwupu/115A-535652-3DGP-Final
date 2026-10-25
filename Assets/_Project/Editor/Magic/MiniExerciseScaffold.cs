using System.Linq;
using System.Text;
using UnityEditor;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Creates the files of a mini-exercise (docs/mini-exercises.md, ADR-0010):
    /// the member's asmdef (once per member), the exercise folder, its README and its demo scene.
    /// Only missing files are created; existing ones are never touched.
    /// </summary>
    public static class MiniExerciseScaffold
    {
        public const string RootFolder = "Assets/_Project/MiniExercises";

        public static string ExerciseFolder(string member, MiniExerciseCatalog.Exercise exercise, string folderName) =>
            $"{RootFolder}/{member}/{exercise.ShortCode}_{folderName}";

        public static string ScenePath(string member, MiniExerciseCatalog.Exercise exercise, string folderName) =>
            $"{ExerciseFolder(member, exercise, folderName)}/Scenes/{exercise.ShortCode}_{folderName}.unity";

        /// <summary>Creates the asmdef, folder and README. Returns the scene path for the caller to create.</summary>
        public static string CreateFiles(string member, MiniExerciseCatalog.Exercise exercise, string folderName,
            MiniExerciseCatalog.Assignment assignment)
        {
            var asmdefName = $"Project3DGP.MiniEx.{member}";
            MagicMenuUtil.EnsureTextFile(
                $"{RootFolder}/{member}/{asmdefName}.asmdef",
                AssembliesMenu.AsmdefJson(asmdefName, $"\"{AssembliesMenu.CoreName}\", {AssembliesMenu.CommonReferences}"));

            var folder = ExerciseFolder(member, exercise, folderName);
            MagicMenuUtil.EnsureTextFile($"{folder}/README.md", Readme(member, exercise, folderName, assignment));
            return ScenePath(member, exercise, folderName);
        }

        static string Readme(string member, MiniExerciseCatalog.Exercise exercise, string folderName,
            MiniExerciseCatalog.Assignment assignment)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {exercise.Code} {exercise.Title} — {member}");
            sb.AppendLine();
            if (assignment != null)
                sb.AppendLine($"Issue: #{assignment.Issue} · Demo: {assignment.DemoDate}");
            sb.AppendLine($"Scene: `Scenes/{exercise.ShortCode}_{folderName}.unity` · Guide: `docs/mini-exercises.md`");
            sb.AppendLine();
            sb.AppendLine("## What's new (developed for this exercise)");
            sb.AppendLine("- TODO");
            sb.AppendLine();
            sb.AppendLine("## Controls");
            sb.AppendLine("- TODO");
            sb.AppendLine();
            sb.AppendLine("## Requirement checklist");
            foreach (var item in exercise.Checklist)
                sb.AppendLine($"- [ ] {item}");
            sb.AppendLine();
            sb.AppendLine("## Reusable code in Core");
            sb.AppendLine("- TODO (generic classes moved to `Assets/_Project/Scripts/Core/`)");
            sb.AppendLine();
            sb.AppendLine("## TA modification points");
            sb.AppendLine("- TODO (parameters or functions the TA may ask to change live)");
            sb.AppendLine();
            sb.AppendLine("## References");
            sb.AppendLine("- Course spec: `docs/references/course/mini-exercises.md`");
            sb.AppendLine("- Third-party code: none (if used: >= 3 new features, a loop creating >= 20 objects, links here)");
            return sb.ToString();
        }

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Create All Planned", priority = MagicMenuUtil.MiniExercisesPriority + 1)]
        static void CreateAllPlanned()
        {
            var scenes = MiniExerciseCatalog.Plan
                .Select(a =>
                {
                    var exercise = MiniExerciseCatalog.Find(a.Code);
                    return CreateFiles(a.Member, exercise, exercise.FolderName, a);
                })
                .ToList();

            AssetDatabase.Refresh();
            var created = MagicMenuUtil.CreateMissingScenes(scenes, withGround: true);
            MagicMenuUtil.Log($"Mini-exercises: {created.Count} scenes created, {scenes.Count - created.Count} already existed.");
        }
    }
}
