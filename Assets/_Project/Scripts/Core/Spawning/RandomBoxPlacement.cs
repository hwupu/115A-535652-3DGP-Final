using UnityEngine;

namespace Project3DGP.Core.Spawning
{
    /// <summary>Bounded random search for an upright box, including space around its edges.</summary>
    public static class RandomBoxPlacement
    {
        public static bool TryFind(Vector3 center, Vector2 size, Vector3 halfExtents,
            float clearance, int maxAttempts, out Vector3 position)
        {
            position = center;
            var padding = Mathf.Max(0f, clearance);
            var queryExtents = halfExtents + Vector3.one * padding;
            var horizontalRoom = size * 0.5f - new Vector2(queryExtents.x, queryExtents.z);
            if (horizontalRoom.x < 0f || horizontalRoom.y < 0f ||
                halfExtents.x <= 0f || halfExtents.y <= 0f || halfExtents.z <= 0f)
                return false;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = center;
                candidate.x += Random.Range(-horizontalRoom.x, horizontalRoom.x);
                candidate.z += Random.Range(-horizontalRoom.y, horizontalRoom.y);
                if (Physics.CheckBox(candidate, queryExtents, Quaternion.identity,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    continue;

                position = candidate;
                return true;
            }

            // The caller keeps the object queued rather than accepting an overlapping fallback.
            return false;
        }
    }
}
