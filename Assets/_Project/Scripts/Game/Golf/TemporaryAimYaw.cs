using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// TEMP(#19): aim yaw from the main camera until CameraRig exists.
    /// Once CameraRig (#19, Bahadir) is merged, DELETE this file and use CameraRig.AimYaw in its callers
    /// (ShotController, AimCone); the compiler then points at both. See the comment on #15 / #19.
    /// </summary>
    public static class TemporaryAimYaw
    {
        /// <summary>Horizontal forward of the main camera as a yaw in degrees (0 = +Z, 90 = +X). 0 if no camera.</summary>
        public static float FromMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                return 0f;
            var forward = camera.transform.forward;
            // Atan2(x, z): the angle of the forward arrow seen from above, measured from +Z towards +X.
            return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }
    }
}
