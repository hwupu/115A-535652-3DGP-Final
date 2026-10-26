using Project3DGP.Core.Portals;
using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// On-screen text for the TA: where the hamster is, how many teleports so far, whether the locked
    /// portal is open yet, and the controls. Updated through the traveler's Teleported event.
    /// </summary>
    public class PortalDemoHud : MonoBehaviour
    {
        [SerializeField] PortalTraveler traveler;
        [SerializeField] Portal lockedPortal;
        [SerializeField] string startArea = "Hub";

        string _area;
        string _lastTrip = "-";
        GUIStyle _style;

        // Subscribe to the traveler's event while enabled; always unsubscribe what you subscribed.
        void OnEnable() => traveler.Teleported += OnTeleported;
        void OnDisable() => traveler.Teleported -= OnTeleported;

        void Awake() => _area = startArea;

        // The destination's parent is the area object (Hub, Pond Island, ...), so its name is the area name.
        void OnTeleported(PortalTraveler who, Portal portal)
        {
            var from = _area;
            _area = portal.Destination.parent != null ? portal.Destination.parent.name : portal.Destination.name;
            _lastTrip = $"{from} -> {_area}";
        }

        // OnGUI = Unity's immediate-mode GUI: quick debug text with no Canvas setup needed.
        void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.UpperLeft };
            var lockText = lockedPortal == null ? "" :
                lockedPortal.IsLocked ? "High Platform portal: LOCKED (step on the switch)\n" : "High Platform portal: open\n";
            GUI.Box(new Rect(10f, 10f, 460f, 170f),
                $"Area: {_area}\n" +
                $"Teleports: {traveler.TeleportCount}   Last: {_lastTrip}\n" +
                lockText + "\n" +
                "WASD move   Shift run   Space jump\n" +
                "Right-drag orbit   Scroll zoom\n" +
                "Walk into a portal to teleport",
                _style);
        }
    }
}
