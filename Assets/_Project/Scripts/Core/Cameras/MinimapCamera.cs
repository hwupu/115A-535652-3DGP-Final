using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Core.Cameras
{
    /// <summary>
    /// Top-down minimap: an orthographic camera straight above the target, looking down (rotation X = 90).
    /// It renders into a RenderTexture, which a RawImage shows in the corner of the screen.
    /// Map icons live on their own layer: this camera sees that layer, the main camera doesn't.
    /// Minus / Equals keys zoom (orthographic size = half the visible height in metres).
    ///
    /// The pipeline: Minimap Camera -> Minimap.renderTexture (512 x 512) -> RawImage "Map" on the Canvas.
    /// Orthographic = no perspective: things far away are not smaller, which is what a map needs.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float height = 30f;
        [Tooltip("On = the map turns with the target (target always points up). Off = north is always up.")]
        [SerializeField] bool rotateWithTarget;
        [SerializeField] float minSize = 5f;
        [SerializeField] float maxSize = 30f;
        [SerializeField] float zoomStep = 2.5f;

        Camera _camera;

        void Awake() => _camera = GetComponent<Camera>();

        void LateUpdate()
        {
            // Zoom: bigger orthographic size = the camera sees more ground = zoomed out.
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.minusKey.wasPressedThisFrame)
                    _camera.orthographicSize = Mathf.Min(_camera.orthographicSize + zoomStep, maxSize);
                if (keyboard.equalsKey.wasPressedThisFrame)
                    _camera.orthographicSize = Mathf.Max(_camera.orthographicSize - zoomStep, minSize);
            }

            if (target == null)
                return;

            // Follow X/Z only; stay `height` metres above. Looking straight down = rotation X 90 degrees.
            var position = target.position;
            transform.position = new Vector3(position.x, position.y + height, position.z);
            var yaw = rotateWithTarget ? target.eulerAngles.y : 0f;
            transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        }
    }
}
