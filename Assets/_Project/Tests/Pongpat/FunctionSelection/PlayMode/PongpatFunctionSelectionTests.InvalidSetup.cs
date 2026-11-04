#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using Project3DGP.MiniEx.Pongpat.FunctionSelection;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Project3DGP.MiniEx.Pongpat.Tests
{
    public partial class PongpatFunctionSelectionTests
    {
        [UnityTest]
        public IEnumerator MissingSetup_DisablesDemo_AndPersistentButtonCallbacksDoNotThrow()
        {
            _root = new GameObject("Incomplete function selection setup");
            _root.SetActive(false);
            var demo = _root.AddComponent<FunctionSelectionDemo>();
            var buttonObject = new GameObject("Inspector-wired button", typeof(RectTransform), typeof(Button));
            buttonObject.transform.SetParent(_root.transform, false);
            var button = buttonObject.GetComponent<Button>();

            // Match the Magic tool's persistent callbacks. Disabled MonoBehaviours still receive them.
            UnityEventTools.AddPersistentListener(button.onClick, demo.OpenLevel);
            UnityEventTools.AddPersistentListener(button.onClick, demo.OpenSettings);
            UnityEventTools.AddPersistentListener(button.onClick, demo.OpenCharacters);
            UnityEventTools.AddPersistentListener(button.onClick, demo.ReturnToMenu);
            UnityEventTools.AddPersistentListener(button.onClick, demo.SelectAmber);
            UnityEventTools.AddPersistentListener(button.onClick, demo.SelectFrost);
            UnityEventTools.AddPersistentListener(button.onClick, demo.PlayPreviewTone);
            UnityEventTools.AddPersistentListener(button.onClick, demo.RestartLevel);
            UnityEventTools.AddFloatPersistentListener(button.onClick, demo.SetVolume, 0.25f);
            UnityEventTools.AddFloatPersistentListener(button.onClick, demo.SetBrightness, 0.75f);
            Assert.That(button.onClick.GetPersistentEventCount(), Is.EqualTo(10));

            LogAssert.Expect(LogType.Error, "Function selection demo needs its scene references. " +
                "Run Magic > Mini-Exercises > Pongpat 4.E.2: Set Up Scene.");
            _root.SetActive(true);
            Assert.That(demo.enabled, Is.False);
            Assert.DoesNotThrow(() => button.onClick.Invoke());
            yield return null;

            Assert.That(demo.enabled, Is.False);
            Assert.That(demo.CollectedCount, Is.Zero);
            Assert.That(demo.LevelFinished, Is.False);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
