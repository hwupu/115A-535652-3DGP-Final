using System.Collections.Generic;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// The level's sound effects (#72) for the events every level shares, read from the level's SoundSet:
    ///   ShotController.ShotTaken      shot (by the club's swing style)
    ///   GolfBall.Collided             bounce on the ground or against something hard (louder when faster)
    ///   GolfBall.WentOutOfBounds      out of bounds
    ///   avatar walking                footsteps every StepDistance metres
    ///   LevelGoal.Reached             ball in the cup / target hit
    ///   LevelExit.Opened              gate opens
    ///   XpCollectible.Collected       XP seed
    ///   GameSession.LeveledUp         level up
    ///   ILevelFlow.LevelFailed        time up
    /// Sounds play in 3D on a few child AudioSources ("voices", reused in turn: nothing is spawned), scaled by the SFX
    /// volume setting. Level code plays its own events with Play(cue, position) or PlayExtra(name, position).
    /// Magic > Game > Add Sounds puts it in a level as Systems/Sounds. (The shared audio manager, #42, can take over
    /// the playback later: only PlayAt would change.)
    /// </summary>
    public class GameplaySounds : MonoBehaviour
    {
        [SerializeField] SoundSet sounds;
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] Transform avatar;
        [SerializeField] AudioSource[] voices;
        [Tooltip("Slower impacts make no sound (m/s).")]
        [SerializeField] float minBounceSpeed = 1.2f;
        [Tooltip("Impacts at this speed or faster play at full volume (m/s).")]
        [SerializeField] float fullBounceSpeed = 10f;
        [SerializeField] float minBounceInterval = 0.08f;

        static GameplaySounds _instance;
        readonly List<LevelExit> _exits = new();
        readonly List<XpCollectible> _seeds = new();
        readonly List<LevelGoal> _goals = new();
        ILevelFlow _flow;
        GameSession _session;
        int _nextVoice;
        float _lastBounceTime = float.NegativeInfinity;
        Vector3 _lastAvatarPosition;
        float _walked;

        public SoundSet Sounds => sounds;

        void OnEnable()
        {
            _instance = this;
            if (shot != null) shot.ShotTaken += OnShotTaken;
            if (ball != null)
            {
                ball.Collided += OnBallCollided;
                ball.WentOutOfBounds += OnOutOfBounds;
            }
            _exits.AddRange(FindObjectsByType<LevelExit>(FindObjectsSortMode.None));
            foreach (var exit in _exits) exit.Opened += OnGateOpened;
            _seeds.AddRange(FindObjectsByType<XpCollectible>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            foreach (var seed in _seeds) seed.Collected += OnSeedCollected;
            _goals.AddRange(FindObjectsByType<LevelGoal>(FindObjectsSortMode.None));
            foreach (var goal in _goals) goal.Reached += OnGoalReached;
            if (avatar != null) _lastAvatarPosition = avatar.position;
        }

        void Start()
        {
            _flow = LevelFlowFinder.Find(null);
            if (_flow != null) _flow.LevelFailed += OnLevelFailed;
            _session = GameSession.Instance;
            if (_session != null) _session.LeveledUp += OnLeveledUp;
        }

        void OnDisable()
        {
            if (_instance == this) _instance = null;
            if (shot != null) shot.ShotTaken -= OnShotTaken;
            if (ball != null)
            {
                ball.Collided -= OnBallCollided;
                ball.WentOutOfBounds -= OnOutOfBounds;
            }
            foreach (var exit in _exits) if (exit != null) exit.Opened -= OnGateOpened;
            foreach (var seed in _seeds) if (seed != null) seed.Collected -= OnSeedCollected;
            foreach (var goal in _goals) if (goal != null) goal.Reached -= OnGoalReached;
            _exits.Clear();
            _seeds.Clear();
            _goals.Clear();
            if (_flow != null) _flow.LevelFailed -= OnLevelFailed;
            if (_session != null) _session.LeveledUp -= OnLeveledUp;
        }

        void Update()
        {
            if (avatar == null || sounds == null || Time.deltaTime <= 0f)
                return;
            var step = avatar.position - _lastAvatarPosition;
            step.y = 0f;
            _lastAvatarPosition = avatar.position;
            var speed = step.magnitude / Time.deltaTime;
            if (speed < 0.3f || speed > 8f)   // standing, or a teleport
                return;
            _walked += step.magnitude;
            if (_walked >= sounds.StepDistance)
            {
                _walked = 0f;
                PlayAt(sounds.Footstep, avatar.position, 1f);
            }
        }

        /// <summary>Plays a cue at a position in the open level (no-op without a GameplaySounds).</summary>
        public static void Play(SoundCue cue, Vector3 position, float volumeScale = 1f)
        {
            if (_instance != null)
                _instance.PlayAt(cue, position, volumeScale);
        }

        /// <summary>Plays one of the level's named extras (SoundSet.Extras) at a position.</summary>
        public static void PlayExtra(string name, Vector3 position, float volumeScale = 1f)
        {
            if (_instance != null && _instance.sounds != null)
                _instance.PlayAt(_instance.sounds.Extra(name), position, volumeScale);
        }

        void PlayAt(SoundCue cue, Vector3 position, float volumeScale)
        {
            var clip = cue?.Pick();
            if (clip == null || voices == null || voices.Length == 0)
                return;
            var voice = voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % voices.Length;
            if (voice == null)
                return;
            voice.transform.position = position;
            voice.pitch = cue.PickPitch();
            var sfx = GameSession.Instance != null ? GameSession.Instance.Data.Settings.SfxVolume : 1f;
            voice.PlayOneShot(clip, cue.Volume * volumeScale * sfx);
        }

        void OnShotTaken(ShotInfo info)
        {
            if (sounds != null)
                PlayAt(sounds.Shot(info.Club), ball != null ? ball.Position : transform.position, 1f);
        }

        void OnBallCollided(Collision collision)
        {
            if (sounds == null || collision.contactCount == 0)
                return;
            // Only the speed into the surface counts: rolling onto the next ground block is silent.
            var contact = collision.GetContact(0);
            var speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
            if (speed < minBounceSpeed || Time.time - _lastBounceTime < minBounceInterval)
                return;
            _lastBounceTime = Time.time;
            var cue = contact.normal.y > 0.6f ? sounds.BounceGround : sounds.BounceHard;
            PlayAt(cue, contact.point, Mathf.Clamp01(speed / fullBounceSpeed));
        }

        void OnOutOfBounds()
        {
            if (sounds != null) PlayAt(sounds.OutOfBounds, ball.Position, 1f);
        }

        void OnGoalReached(LevelGoal goal)
        {
            if (sounds != null) PlayAt(sounds.GoalReached, goal.transform.position, 1f);
        }

        void OnGateOpened()
        {
            if (sounds == null) return;
            foreach (var exit in _exits)
                if (exit != null) PlayAt(sounds.GateOpen, exit.transform.position, 1f);
        }

        void OnSeedCollected(XpCollectible seed)
        {
            if (sounds != null) PlayAt(sounds.XpCollect, seed.transform.position, 1f);
        }

        void OnLeveledUp(int level)
        {
            if (sounds != null) PlayAt(sounds.LevelUp, avatar != null ? avatar.position : transform.position, 1f);
        }

        void OnLevelFailed()
        {
            if (sounds != null) PlayAt(sounds.LevelFailed, avatar != null ? avatar.position : transform.position, 1f);
        }
    }
}
