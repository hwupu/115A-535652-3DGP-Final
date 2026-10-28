using Project3DGP.Game.Golf;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>
    /// TEMP(#7): stands in for DangerManager until it exists, so the cat can be tried in L1_Garden.
    /// F1 = Calm, F2 = Warning, F3 = Critical, F4 = Time up. Time up also locks the shot input (game over;
    /// LevelManager #27 will show the fail screen and Retry). Shows the cat state at the top of the screen.
    /// Delete this object once DangerManager drives the cat (DangerManager.StateChanged -> GardenCat.SetAlert).
    /// </summary>
    public class GardenCatTestKeys : MonoBehaviour
    {
        [SerializeField] GardenCat cat;
        [SerializeField] ShotController shot;

        void OnEnable() => cat.WokeUp += OnWokeUp;
        void OnDisable() => cat.WokeUp -= OnWokeUp;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.f1Key.wasPressedThisFrame) Set(CatAlert.Calm);
            if (keyboard.f2Key.wasPressedThisFrame) Set(CatAlert.Warning);
            if (keyboard.f3Key.wasPressedThisFrame) Set(CatAlert.Critical);
            if (keyboard.f4Key.wasPressedThisFrame) Set(CatAlert.TimeUp);
        }

        void Set(CatAlert alert)
        {
            cat.SetAlert(alert);
            if (alert != CatAlert.TimeUp && shot != null)
                shot.InputEnabled = true;
        }

        void OnWokeUp()
        {
            if (shot != null)
                shot.InputEnabled = false;
        }

        void OnGUI()
        {
            var text = $"Cat: {cat.State}  (danger: {cat.Alert})   F1 calm  F2 warning  F3 critical  F4 time up";
            if (cat.State == CatState.Awake)
                text += "\nThe cat woke up! GAME OVER (F1 to reset)";
            GUI.Label(new Rect(10f, 10f, 900f, 50f), text);
        }
    }
}
