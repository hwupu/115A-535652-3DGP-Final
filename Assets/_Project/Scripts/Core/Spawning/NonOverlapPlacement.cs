using UnityEngine;

namespace Project3DGP.Core.Spawning
{
    /// <summary>
    /// Finds a random free spot for a box-shaped object.
    /// Rejection sampling: pick a random point in the area, ask the physics engine whether a box of the
    /// object's size there would touch any collider, and accept the first point where it wouldn't.
    /// (Not used by the 4.C.4 demo right now: tiles respawn at their home spot instead.)
    /// </summary>
    public static class NonOverlapPlacement
    {
        /// <param name="areaCenter">Centre of the area; its Y is the height of the box centre.</param>
        /// <param name="areaSize">Width (X) and depth (Z) of the area.</param>
        /// <param name="halfExtents">Half the object's size. Keep the box bottom above the ground, or the ground counts as overlap.</param>
        /// <returns>False if no free spot was found within <paramref name="maxTries"/> tries.</returns>
        public static bool TryFindSpot(Vector3 areaCenter, Vector2 areaSize, Vector3 halfExtents, Quaternion rotation,
            int layerMask, int maxTries, out Vector3 position)
        {
            // maxTries stops the loop if the area is too full to ever find a spot.
            for (var i = 0; i < maxTries; i++)
            {
                // Random point in the rectangle: centre + (random -0.5..0.5) * size, on X and Z.
                var candidate = areaCenter + new Vector3(
                    Random.Range(-0.5f, 0.5f) * areaSize.x,
                    0f,
                    Random.Range(-0.5f, 0.5f) * areaSize.y);

                // CheckBox = "would a box here touch any collider?" No -> the spot is free, take it.
                if (!Physics.CheckBox(candidate, halfExtents, rotation, layerMask, QueryTriggerInteraction.Ignore))
                {
                    position = candidate;
                    return true;
                }
            }

            position = areaCenter;
            return false;
        }
    }
}
