using System.Collections;
using System.IO;
using NUnit.Framework;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// ShotController (#15) end to end: a simulated left click shoots the real ball; clicks during the
    /// flight are ignored; when the ball stops, the controller is back to Aiming.
    /// </summary>
    public class ShotControllerTests : InputTestFixture
    {
        Mouse _mouse;
        Keyboard _keyboard;
        InputActionAsset _actions;
        GolfInput _input;
        ClubProfile _club, _club2, _club3;
        GameObject _ground, _ballObject, _shotObject;
        GolfBall _ball;
        ShotController _shot;
        int _shots, _clubChanges;
        ShotInfo _lastShot;

        public override void Setup()
        {
            base.Setup();
            Time.timeScale = 4f;
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();

            _actions = InputActionAsset.FromJson(File.ReadAllText("Assets/_Project/Settings/Input/GolfControls.inputactions"));
            _input = ScriptableObject.CreateInstance<GolfInput>();
            _input.SetActions(_actions);

            _club = ScriptableObject.CreateInstance<ClubProfile>();
            _club.MinSpeed = 2f;
            _club.MaxSpeed = 6f;
            _club2 = ScriptableObject.CreateInstance<ClubProfile>();
            _club3 = ScriptableObject.CreateInstance<ClubProfile>();

            _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _ground.transform.localScale = new Vector3(3f, 1f, 3f);
            _ballObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _ballObject.transform.localScale = Vector3.one * 0.2f;
            _ballObject.transform.position = new Vector3(0f, 0.1f, 0f);
            _ballObject.AddComponent<Rigidbody>().mass = 0.1f;
            _ball = _ballObject.AddComponent<GolfBall>();

            _shotObject = new GameObject("Shot");
            _shotObject.SetActive(false);   // wire before Awake / OnEnable
            _shot = _shotObject.AddComponent<ShotController>();
            _shot.Configure(_input, _ball, new[] { _club, _club2, _club3 });
            _shotObject.SetActive(true);

            _shots = _clubChanges = 0;
            _shot.ShotTaken += info => { _shots++; _lastShot = info; };
            _shot.ClubChanged += _ => _clubChanges++;
        }

        public override void TearDown()
        {
            Time.timeScale = 1f;
            Object.Destroy(_shotObject);
            Object.Destroy(_ballObject);
            Object.Destroy(_ground);
            Object.Destroy(_input);
            Object.Destroy(_club);
            Object.Destroy(_club2);
            Object.Destroy(_club3);
            Object.Destroy(_actions);
            base.TearDown();
        }

        // In a [UnityTest], Press/Release only QUEUE the input event; the Input System processes it in the next
        // frame's player loop. So: press, wait a frame (the action fires), release, wait a frame.
        IEnumerator Click()
        {
            Press(_mouse.leftButton);
            yield return null;
            Release(_mouse.leftButton);
            yield return null;
        }

        IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        IEnumerator Scroll(float y)
        {
            Set(_mouse.scroll.y, y);
            yield return null;
            Set(_mouse.scroll.y, 0f);
            yield return null;
            yield return new WaitForSecondsRealtime(0.2f);   // past GolfInput's wheel cooldown (unscaled 0.15 s)
        }

        [UnityTest]
        public IEnumerator NumberKeys_SelectClubSlots()
        {
            yield return Tap(_keyboard.digit3Key);
            Assert.That(_shot.CurrentClub, Is.SameAs(_club3));
            yield return Tap(_keyboard.digit2Key);
            Assert.That(_shot.CurrentClub, Is.SameAs(_club2));
            Assert.That(_clubChanges, Is.EqualTo(2), "ClubChanged raised for each change");

            yield return Tap(_keyboard.digit2Key);   // same club again: no event
            Assert.That(_clubChanges, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Wheel_StepsThroughClubs_AndWrapsAround()
        {
            yield return Scroll(-120f);   // wheel down = next
            Assert.That(_shot.CurrentClub, Is.SameAs(_club2));
            yield return Scroll(120f);    // wheel up = previous
            Assert.That(_shot.CurrentClub, Is.SameAs(_club));
            yield return Scroll(120f);    // previous from the first = the last
            Assert.That(_shot.CurrentClub, Is.SameAs(_club3));
        }

        [UnityTest]
        public IEnumerator ClubCannotChange_InFlight()
        {
            yield return Click();
            Assert.That(_shot.Phase, Is.EqualTo(ShotPhase.InFlight));
            yield return Tap(_keyboard.digit2Key);
            Assert.That(_shot.CurrentClub, Is.SameAs(_club), "still the club the shot was taken with");
        }

        [UnityTest]
        public IEnumerator Click_ShootsAtMarkerPower_ThenBackToAimingAtRest()
        {
            Assert.That(_shot.CurrentClub, Is.SameAs(_club), "first club selected at start");
            yield return new WaitForSeconds(0.3f);   // let the marker run a bit
            Assert.That(_shot.PowerMarker01, Is.GreaterThan(0f), "marker is running while aiming");

            yield return Click();

            Assert.That(_shots, Is.EqualTo(1));
            Assert.That(_shot.Phase, Is.EqualTo(ShotPhase.InFlight));
            Assert.That(_ball.IsAtRest, Is.False);
            Assert.That(_lastShot.Velocity.magnitude, Is.EqualTo(ShotMath.Speed(_club, _lastShot.Power01)).Within(1e-3f));
            Assert.That(_shot.PowerMarker01, Is.EqualTo(_lastShot.Power01), "marker frozen at the chosen power");

            yield return Click();   // during the flight: ignored
            Assert.That(_shots, Is.EqualTo(1));

            var start = Time.time;
            while (_shot.Phase == ShotPhase.InFlight && Time.time - start < 25f)
                yield return null;
            Assert.That(_shot.Phase, Is.EqualTo(ShotPhase.Aiming), "ball stopped -> aiming again");
        }

        [UnityTest]
        public IEnumerator InputDisabled_ClickIsIgnored()
        {
            yield return null;
            _shot.InputEnabled = false;
            yield return Click();
            Assert.That(_shots, Is.Zero);
            Assert.That(_shot.Phase, Is.EqualTo(ShotPhase.Aiming));
        }
    }
}
