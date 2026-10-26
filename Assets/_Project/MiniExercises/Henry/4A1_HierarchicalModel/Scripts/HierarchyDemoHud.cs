using Project3DGP.Core.Motion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// On-screen text for the TA: the A -> B -> C chain with each joint's axis, live values and direction,
    /// the swing phase, the ball timer, the tool and the controls. P pauses everything (Time.timeScale = 0).
    /// </summary>
    public class HierarchyDemoHud : MonoBehaviour
    {
        [SerializeField] AxisJoint jointA;
        [SerializeField] AxisJoint jointB;
        [SerializeField] SineMotion partC;
        [SerializeField] SwingAnimator swing;
        [SerializeField] FlyingBall ball;
        [SerializeField] ToolSwitcher tools;

        GUIStyle _style;

        void Update()
        {
            // P: Time.timeScale = 0 makes Time.deltaTime 0, so every animation and the ball freeze.
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
                Time.timeScale = Time.timeScale > 0f ? 0f : 1f;   // everything uses deltaTime, so it all stops
        }

        // Narrow and tall, in the top-left corner, so it doesn't cover the hamster.
        void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.UpperLeft, wordWrap = true };
            var ballText = ball.InFlight ? $"flying, reset in {ball.TimeLeft:0.0} s" : "on the tee";
            GUI.Box(new Rect(10f, 10f, 300f, 400f),
                "Hamster\n" +
                " > A_Shoulder\n" +
                "    > B_Wrist\n" +
                "       > C_Club > tool\n\n" +
                // Angle x Direction = the angle actually applied (negative after reversing with 1 or 2).
                $"A: about local Z\n    {jointA.Angle * jointA.Direction:0}°  dir {Sign(jointA.Direction)}\n" +
                $"B: about local Y\n    {jointB.Angle * jointB.Direction:0}°  dir {Sign(jointB.Direction)}\n" +
                $"C: sin aiming wiggle\n    {partC.CurrentOffset:0.0}° (at address)\n\n" +
                $"Swing: {swing.Current}\n" +
                $"Ball: {ballText}\n" +
                $"Tool: {tools.CurrentName}{(Time.timeScale == 0f ? "  (PAUSED)" : "")}\n\n" +
                "F swing\n" +
                "1 reverse A (other way)\n" +
                "2 reverse B (curve)\n" +
                "T swap tool   P pause\n" +
                "Right-drag orbit, scroll zoom",
                _style);
        }

        static string Sign(int direction) => direction > 0 ? "+" : "-";
    }
}
