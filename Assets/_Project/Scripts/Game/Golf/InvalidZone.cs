using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// Marks a trigger volume the ball may not stay in: water, outside the course, ... (#14, architecture §4.1).
    /// When the GolfBall enters it, the ball goes back to its last valid position.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InvalidZone : MonoBehaviour
    {
        void Reset() => GetComponent<Collider>().isTrigger = true;
    }
}
