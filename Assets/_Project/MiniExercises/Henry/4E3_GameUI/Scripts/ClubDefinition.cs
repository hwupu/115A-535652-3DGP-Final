using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Data of one golf club (a ScriptableObject = a data file in the project, edited in the Inspector).
    /// The club cards show these numbers, and GolfSwing uses them for the shot.
    /// The 3 clubs are the files in Data/ (StandardClub, Sledgehammer, Putter). Changing a number there
    /// changes the card text and the shots, no code change needed. CreateAssetMenu adds
    /// "Create > Project3DGP > Henry 4.E.3 > Club Definition" to the Project window for a new club.
    /// </summary>
    [CreateAssetMenu(menuName = "Project3DGP/Henry 4.E.3/Club Definition", fileName = "Club")]
    public class ClubDefinition : ScriptableObject
    {
        public string displayName = "Club";
        public Color color = Color.white;
        [Tooltip("How far the ball travels at 100% power (m).")]
        public float maxDistance = 15f;
        [Tooltip("1 = always straight; lower = the ball can go off to the side by up to (1 - accuracy) x 20 degrees.")]
        [Range(0f, 1f)] public float accuracy = 0.8f;
        [Tooltip("Power meter speed (full up-and-down cycles per second). Faster = harder to time.")]
        public float meterSpeed = 0.8f;
    }
}
