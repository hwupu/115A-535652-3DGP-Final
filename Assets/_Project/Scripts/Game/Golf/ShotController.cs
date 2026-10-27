using System;
using System.Collections.Generic;
using Project3DGP.Core.Gauges;
using Project3DGP.Game.Controls;
using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// The shot sequence (#15, architecture §3 and §4.2):
    ///   Aiming   : the power marker runs up and down (Core PingPongGauge); the aim follows the camera's yaw
    ///   click    : power = marker position, direction = random inside the club's cone, ball.Hit(velocity)
    ///   InFlight : clicks are ignored until the ball raises CameToRest, then back to Aiming
    /// Club switching (#16): keys 1-3 pick a slot, the wheel steps through the list (wrapping around).
    /// Only while Aiming: the club can't change in mid-flight.
    /// </summary>
    public class ShotController : MonoBehaviour
    {
        [SerializeField] GolfInput input;
        [SerializeField] GolfBall ball;
        [Tooltip("Clubs the player can use. Later from GameSession unlocks (#70). The first one is selected at start.")]
        [SerializeField] List<ClubProfile> clubs = new();

        readonly PingPongGauge _marker = new();

        public ShotPhase Phase { get; private set; } = ShotPhase.Aiming;
        public ClubProfile CurrentClub { get; private set; }
        public IReadOnlyList<ClubProfile> AvailableClubs => clubs;
        /// <summary>Power marker position 0..1 for the HUD power meter.</summary>
        public float PowerMarker01 => _marker.Value01;
        /// <summary>LevelManager turns it off on complete / fail / pause.</summary>
        public bool InputEnabled { get; set; } = true;
        // Where the player is aiming now (degrees). TEMP(#19): replace with CameraRig.AimYaw. Internal on purpose:
        // not part of the §4.2 contract, other code should read CameraRig.AimYaw.
        internal float AimYaw => TemporaryAimYaw.FromMainCamera();

        public event Action<ShotPhase> PhaseChanged;
        public event Action<ClubProfile> ClubChanged;
        /// <summary>Danger deducts time, Score counts a stroke, the cat hears the noise.</summary>
        public event Action<ShotInfo> ShotTaken;

        void Awake()
        {
            if (clubs.Count > 0)
                SelectClub(clubs[0]);
        }

        void OnEnable()
        {
            input.Acquire();
            input.Shoot += OnShoot;
            input.ClubSlotSelected += OnClubSlotSelected;
            input.ClubCycled += OnClubCycled;
            ball.CameToRest += OnBallCameToRest;
            if (Phase == ShotPhase.Aiming)
                _marker.Begin();
        }

        void OnDisable()
        {
            input.Shoot -= OnShoot;
            input.ClubSlotSelected -= OnClubSlotSelected;
            input.ClubCycled -= OnClubCycled;
            input.Release();
            ball.CameToRest -= OnBallCameToRest;
        }

        void Update() => _marker.Tick(Time.deltaTime);

        public void SelectClub(ClubProfile club)
        {
            if (club == null || club == CurrentClub)
                return;
            CurrentClub = club;
            // Control: the club decides how fast the marker runs (one up-and-down = MarkerCycleSeconds).
            _marker.CyclesPerSecond = 1f / Mathf.Max(0.1f, club.MarkerCycleSeconds);
            ClubChanged?.Invoke(club);
        }

        bool CanChangeClub => InputEnabled && Phase == ShotPhase.Aiming && clubs.Count > 0;

        // Keys 1-3 -> slot 0-2. A slot without a club (e.g. still locked, #70) is ignored.
        void OnClubSlotSelected(int slot)
        {
            if (CanChangeClub && slot >= 0 && slot < clubs.Count)
                SelectClub(clubs[slot]);
        }

        // Wheel: step -1 / +1 through the list. The modulo wraps around: next after the last is the first.
        void OnClubCycled(int step)
        {
            if (!CanChangeClub)
                return;
            var index = clubs.IndexOf(CurrentClub);
            var next = ((index + step) % clubs.Count + clubs.Count) % clubs.Count;   // "+ Count" keeps it positive
            SelectClub(clubs[next]);
        }

        void OnShoot()
        {
            if (!InputEnabled || Phase != ShotPhase.Aiming || !ball.IsAtRest || CurrentClub == null)
                return;

            var power = _marker.Stop();                       // the player's timing = the power
            var aimYaw = AimYaw;
            var spread = UnityEngine.Random.Range(-1f, 1f);   // accuracy: somewhere inside the cone
            var velocity = ShotMath.Velocity(CurrentClub, aimYaw, power, spread);

            SetPhase(ShotPhase.InFlight);
            ball.Hit(velocity);
            ShotTaken?.Invoke(new ShotInfo(CurrentClub, aimYaw, power, velocity));
        }

        void OnBallCameToRest()
        {
            if (Phase != ShotPhase.InFlight)
                return;
            SetPhase(ShotPhase.Aiming);
            _marker.Begin();
        }

        void SetPhase(ShotPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        /// <summary>For tests: wire the references before the object is activated.</summary>
        internal void Configure(GolfInput golfInput, GolfBall golfBall, IEnumerable<ClubProfile> clubList)
        {
            input = golfInput;
            ball = golfBall;
            clubs = new List<ClubProfile>(clubList);
        }
    }
}
