using Project3DGP.Core.Portals;
using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// A floor switch: when the character steps on it (enters its trigger), it unlocks a portal and
    /// turns green. One use only. In the game the same Portal.Unlock() call would come from
    /// "level completed" instead (#38).
    /// </summary>
    public class PortalUnlockSwitch : MonoBehaviour
    {
        [SerializeField] Portal portal;
        [SerializeField] Renderer plateRenderer;
        [SerializeField] Color usedColor = new Color(0.3f, 0.85f, 0.35f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public bool IsPressed { get; private set; }

        void OnTriggerEnter(Collider other)
        {
            // Only the character (it has a CharacterController) presses the switch, once.
            if (IsPressed || !other.TryGetComponent(out CharacterController _))
                return;

            IsPressed = true;
            portal.Unlock();   // the portal turns from grey to its colour and starts its swirl

            if (plateRenderer != null)
            {
                // Turn the plate green (MaterialPropertyBlock = per-renderer colour, the material stays shared).
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, usedColor);
                plateRenderer.SetPropertyBlock(block);
            }
        }
    }
}
