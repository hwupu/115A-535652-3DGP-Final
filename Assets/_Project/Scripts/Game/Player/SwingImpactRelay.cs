using UnityEngine;

namespace Project3DGP.Game.Player
{
    /// <summary>
    /// Sits on the hamster model (the object with the Animator, #36). Unity sends animation events to that object,
    /// so the swing clips' "OnSwingImpact" event lands here and is passed on to the GolferAvatar above.
    /// Added by Magic > Prefabs > Use Character Models.
    /// </summary>
    public class SwingImpactRelay : MonoBehaviour
    {
        GolferAvatar _avatar;

        void Awake() => _avatar = GetComponentInParent<GolferAvatar>();

        // Animation event (Swing_Club / Swing_Hammer / Swing_Mysterious).
        void OnSwingImpact()
        {
            if (_avatar != null)
                _avatar.RaiseSwingImpact();
        }
    }
}
