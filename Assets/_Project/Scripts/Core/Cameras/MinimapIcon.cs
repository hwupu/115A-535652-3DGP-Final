using UnityEngine;

namespace Project3DGP.Core.Cameras
{
    /// <summary>
    /// A flat marker for the minimap, on a layer only the minimap camera sees. It is a child of the thing it marks
    /// (ball, player, goal, hazard) but ignores the parent's rotation and scale: every frame it lies flat, faces
    /// north, floats `lift` metres above the parent and is `size` metres wide, so a rolling ball's icon doesn't spin.
    /// </summary>
    public class MinimapIcon : MonoBehaviour
    {
        [Tooltip("Width of the icon on the map in metres.")]
        [SerializeField] float size = 1.5f;
        [Tooltip("Metres above the parent, so the icon is drawn on top of the things around it.")]
        [SerializeField] float lift = 5f;

        void LateUpdate() => Apply();

        /// <summary>Puts the icon in place now (the Magic tool calls it so it looks right in the Editor).</summary>
        public void Apply()
        {
            var parent = transform.parent;
            if (parent == null)
                return;
            transform.SetPositionAndRotation(parent.position + Vector3.up * lift, Quaternion.Euler(90f, 0f, 0f));
            // Undo the parent's scale: local scale = wanted world size / parent's world scale.
            var parentScale = parent.lossyScale;
            transform.localScale = new Vector3(
                size / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                size / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
                size / Mathf.Max(0.0001f, Mathf.Abs(parentScale.z)));
        }
    }
}
