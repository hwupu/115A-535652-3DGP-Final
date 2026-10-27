using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// The shot formula, kept pure (no scene, no randomness inside) so it can be unit-tested.
    ///   speed     = Lerp(MinSpeed, MaxSpeed, power)       the power meter picks a point in the club's range
    ///   yaw       = aim yaw + spread x cone / 2            spread in -1..1: random inside the cone
    ///   direction = turn "forward" up by LaunchPitch, then around Y by yaw
    /// </summary>
    public static class ShotMath
    {
        public static float Speed(ClubProfile club, float power01) =>
            Mathf.Lerp(club.MinSpeed, club.MaxSpeed, Mathf.Clamp01(power01));

        /// <param name="spread">-1 = left edge of the cone, 0 = centre, 1 = right edge.</param>
        public static float ShotYaw(ClubProfile club, float aimYaw, float spread) =>
            aimYaw + Mathf.Clamp(spread, -1f, 1f) * club.AimConeDegrees * 0.5f;

        public static Vector3 Velocity(ClubProfile club, float aimYaw, float power01, float spread)
        {
            // Euler(-pitch, yaw, 0): a negative X rotation tilts forward UP; the Y rotation turns it to the yaw.
            var direction = Quaternion.Euler(-club.LaunchPitch, ShotYaw(club, aimYaw, spread), 0f) * Vector3.forward;
            return direction * Speed(club, power01);
        }
    }
}
