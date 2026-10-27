using UnityEngine;

namespace Project3DGP.Game.Golf
{
    public enum ShotPhase { Aiming, InFlight }

    /// <summary>Everything about one shot, sent with ShotController.ShotTaken (architecture §4.2).</summary>
    public readonly struct ShotInfo
    {
        public readonly ClubProfile Club;
        public readonly float AimYaw;        // camera yaw at the click, degrees
        public readonly float Power01;       // marker position at the click
        public readonly Vector3 Velocity;    // after cone randomization and LaunchPitch

        public ShotInfo(ClubProfile club, float aimYaw, float power01, Vector3 velocity)
        {
            Club = club;
            AimYaw = aimYaw;
            Power01 = power01;
            Velocity = velocity;
        }
    }
}
