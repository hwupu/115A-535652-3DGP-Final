using Project3DGP.Core.UI;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The vertical power meter on the left of the HUD (#15, architecture §4.8). Reads ShotController, never
    /// writes: the bar shows PowerMarker01, the label the club and the speed under the marker.
    /// After the click the bar stays frozen at the chosen power (dimmed) until the next shot.
    /// </summary>
    public class PowerMeterView : MonoBehaviour
    {
        [SerializeField] ShotController shot;
        [SerializeField] BarView bar;   // vertical, smoothTime 0 (a power meter must not lag)
        [SerializeField] Color aimingColor = new(1f, 0.75f, 0.1f);
        [SerializeField] Color frozenColor = new(0.55f, 0.55f, 0.55f);

        void Update()
        {
            bar.SetValue(shot.PowerMarker01, instant: true);
            var club = shot.CurrentClub;
            if (club != null)
                bar.SetLabel($"{club.DisplayName}\n{ShotMath.Speed(club, shot.PowerMarker01):0} m/s");
            var fill = bar.FillGraphic;
            if (fill != null)
                fill.color = shot.Phase == ShotPhase.Aiming ? aimingColor : frozenColor;
        }
    }
}
