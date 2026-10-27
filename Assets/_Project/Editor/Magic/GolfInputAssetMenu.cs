using System.IO;
using Project3DGP.Game.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Create Golf Input Asset (#13): creates Settings/Input/GolfInput.asset (the shared GolfInput
    /// that ShotController and CameraRig reference) and links it to GolfControls.inputactions.
    /// If the asset exists, only an empty actions field is filled; nothing else is changed.
    /// </summary>
    public static class GolfInputAssetMenu
    {
        public const string Folder = "Assets/_Project/Settings/Input";
        public const string ActionsPath = Folder + "/GolfControls.inputactions";
        public const string InputPath = Folder + "/GolfInput.asset";

        [MenuItem(MagicMenuUtil.Root + "Game/Create Golf Input Asset", priority = MagicMenuUtil.GamePriority)]
        public static GolfInput Create()
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            if (actions == null)
            {
                Debug.LogError($"[Magic] Missing {ActionsPath}. Pull the latest main (it is committed with #13).");
                return null;
            }

            var input = AssetDatabase.LoadAssetAtPath<GolfInput>(InputPath);
            if (input == null)
            {
                MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(InputPath));
                input = ScriptableObject.CreateInstance<GolfInput>();
                AssetDatabase.CreateAsset(input, InputPath);
                MagicMenuUtil.Log("Created " + InputPath);
            }
            else
            {
                MagicMenuUtil.Log("Skipped (exists) " + InputPath);
            }

            var so = new SerializedObject(input);
            var actionsProperty = so.FindProperty("actions");
            if (actionsProperty.objectReferenceValue == null)
            {
                actionsProperty.objectReferenceValue = actions;
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.Log("Linked GolfInput to " + ActionsPath);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = input;
            return input;
        }
    }
}
