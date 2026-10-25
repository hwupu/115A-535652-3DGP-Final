using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Mini-Exercises > New Exercise...: scaffolds one exercise picked in a small window.
    /// Use it for exercises outside the plan (e.g., after swapping one). See MiniExerciseScaffold.
    /// </summary>
    public class NewMiniExerciseWindow : EditorWindow
    {
        int memberIndex;
        int exerciseIndex;
        string folderName;

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/New Exercise...", priority = MagicMenuUtil.MiniExercisesPriority)]
        static void Open()
        {
            var window = GetWindow<NewMiniExerciseWindow>(true, "New Mini-Exercise");
            window.minSize = new Vector2(360, 140);
        }

        void OnGUI()
        {
            var exercises = MiniExerciseCatalog.Exercises;
            memberIndex = EditorGUILayout.Popup("Member", memberIndex, MiniExerciseCatalog.Members);

            EditorGUI.BeginChangeCheck();
            exerciseIndex = EditorGUILayout.Popup("Exercise", exerciseIndex,
                exercises.Select(e => $"{e.Code} {e.Title}").ToArray());
            if (EditorGUI.EndChangeCheck() || string.IsNullOrEmpty(folderName))
                folderName = exercises[exerciseIndex].FolderName;

            folderName = EditorGUILayout.TextField("Folder name", folderName);

            var member = MiniExerciseCatalog.Members[memberIndex];
            var exercise = exercises[exerciseIndex];
            EditorGUILayout.HelpBox(MiniExerciseScaffold.ExerciseFolder(member, exercise, folderName), MessageType.None);

            if (GUILayout.Button("Create"))
            {
                var assignment = MiniExerciseCatalog.Plan.FirstOrDefault(a => a.Member == member && a.Code == exercise.Code);
                var scene = MiniExerciseScaffold.CreateFiles(member, exercise, folderName, assignment);
                AssetDatabase.Refresh();
                MagicMenuUtil.CreateMissingScenes(new[] { scene }, withGround: true);
                Close();
                GUIUtility.ExitGUI();
            }
        }
    }
}
