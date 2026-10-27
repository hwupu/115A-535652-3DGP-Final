using UnityEngine;

namespace Project3DGP.Game.Controls
{
    /// <summary>
    /// Development helper (#13): shows what GolfInput reports, so the bindings can be checked in Play mode
    /// before ShotController and CameraRig exist. Not part of the game; don't leave it in a level scene.
    /// </summary>
    public class GolfInputDebugOverlay : MonoBehaviour
    {
        [SerializeField] GolfInput input;

        string _lastEvent = "(none)";
        int _eventCount;

        void OnEnable()
        {
            input.Acquire();
            input.Shoot += OnShoot;
            input.ClubSlotSelected += OnClubSlot;
            input.ClubCycled += OnClubCycled;
            input.ToggleView += OnToggleView;
            input.Pause += OnPause;
            input.Retry += OnRetry;
        }

        void OnDisable()
        {
            input.Shoot -= OnShoot;
            input.ClubSlotSelected -= OnClubSlot;
            input.ClubCycled -= OnClubCycled;
            input.ToggleView -= OnToggleView;
            input.Pause -= OnPause;
            input.Retry -= OnRetry;
            input.Release();
        }

        void OnShoot() => Report("Shoot");
        void OnClubSlot(int slot) => Report($"Club slot {slot + 1}");
        void OnClubCycled(int step) => Report(step < 0 ? "Club previous (wheel up)" : "Club next (wheel down)");
        void OnToggleView() => Report("Toggle view");
        void OnPause() => Report("Pause");
        void OnRetry() => Report("Retry");

        void Report(string message)
        {
            _eventCount++;
            _lastEvent = message;
            Debug.Log($"[GolfInput] {message}");
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 320, 220), GUI.skin.box);
            GUILayout.Label("GolfInput debug (#13)");
            GUILayout.Label($"Orbiting (hold right mouse): {input.IsOrbiting}");
            GUILayout.Label($"Orbit delta: {input.OrbitDelta}");
            GUILayout.Label($"Last event #{_eventCount}: {_lastEvent}");
            GUILayout.Label($"Gameplay enabled: {input.GameplayEnabled}");
            // Simulates the flight / pause lock: gameplay actions stop, Esc and R still work.
            if (GUILayout.Button(input.GameplayEnabled ? "Lock gameplay (like ball in flight)" : "Unlock gameplay"))
                input.GameplayEnabled = !input.GameplayEnabled;
            GUILayout.EndArea();
        }
    }
}
