using Project3DGP.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Project3DGP.MiniEx.Pongpat.FunctionSelection
{
    /// <summary>
    /// Three functions for Pongpat's standalone menu demo. The Magic tool authors every scene
    /// object; this component switches pages, moves existing objects and applies demo settings.
    /// </summary>
    public class FunctionSelectionDemo : MonoBehaviour
    {
        [Header("Pages: menu, level, settings, characters")]
        [SerializeField] PageRouter router;
        [SerializeField] int levelPageIndex = 1;

        [Header("Editor-authored level")]
        [SerializeField] CharacterController player;
        [SerializeField] Transform spawnPoint;
        [SerializeField] GameObject[] avatarVisuals = new GameObject[0];
        [SerializeField] Transform[] monsters = new Transform[0];
        [SerializeField] GameObject[] collectibles = new GameObject[0];
        [SerializeField, Min(0f)] float moveSpeed = 5f;
        [SerializeField, Min(1f)] float arenaHalfSize = 10f;
        [SerializeField, Min(0f)] float patrolDistance = 3f;
        [SerializeField, Min(0f)] float patrolSpeed = 1.2f;
        [SerializeField] Text levelStatus;
        [SerializeField] Text characterLabel;

        [Header("Settings affect this demo only")]
        [SerializeField] Light demoLight;
        [SerializeField] AudioSource demoAudio;
        [SerializeField] Slider volumeSlider;
        [SerializeField] Slider brightnessSlider;
        [SerializeField] Text volumeLabel;
        [SerializeField] Text brightnessLabel;

        Vector3[] _monsterPositions;
        AudioClip _previewTone;
        float _patrolTime;
        float _verticalSpeed;
        int _selectedCharacter;
        int _collected;
        bool _finished;
        bool _ready;

        public int SelectedCharacter => _selectedCharacter;
        public int CollectedCount => _collected;
        public bool LevelFinished => _finished;

        void Awake()
        {
            if (router == null || player == null || spawnPoint == null || demoLight == null ||
                demoAudio == null || volumeSlider == null || brightnessSlider == null ||
                !AllAssigned(avatarVisuals, 2) || !AllAssigned(monsters, 2) || !AllAssigned(collectibles, 3))
            {
                Debug.LogError("Function selection demo needs its scene references. " +
                    "Run Magic > Mini-Exercises > Pongpat 4.E.2: Set Up Scene.", this);
                enabled = false;
                return;
            }

            _monsterPositions = new Vector3[monsters.Length];
            for (var i = 0; i < monsters.Length; i++)
                if (monsters[i] != null)
                    _monsterPositions[i] = monsters[i].position;

            // A short generated tone makes volume testable without importing an audio asset.
            const int sampleRate = 22050;
            var samples = new float[Mathf.RoundToInt(sampleRate * 0.15f)];
            for (var i = 0; i < samples.Length; i++)
            {
                var envelope = Mathf.Sin(Mathf.PI * i / (samples.Length - 1));
                samples[i] = 0.2f * envelope * Mathf.Sin(2f * Mathf.PI * 440f * i / sampleRate);
            }
            _previewTone = AudioClip.Create("Function Selection Preview Tone", samples.Length, 1, sampleRate, false);
            _previewTone.SetData(samples, 0);
            _ready = true;
        }

        static bool AllAssigned(Object[] references, int minimumCount)
        {
            if (references == null || references.Length < minimumCount)
                return false;
            for (var i = 0; i < references.Length; i++)
                if (references[i] == null)
                    return false;
            return true;
        }

        void OnEnable()
        {
            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(SetVolume);
            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        void Start()
        {
            SetVolume(volumeSlider.value);
            SetBrightness(brightnessSlider.value);
            SelectCharacter(_selectedCharacter);
            RestartLevel();
            router.Show(router.CurrentIndex);
        }

        void OnDisable()
        {
            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(SetVolume);
            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        }

        void OnDestroy()
        {
            if (_previewTone != null)
                Destroy(_previewTone);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                ReturnToMenu();
                return;
            }

            // Leaving the level pauses its local simulation without changing global time scale.
            if (router.CurrentIndex != levelPageIndex || _finished || !player.enabled || !player.gameObject.activeInHierarchy)
                return;

            MovePlayer(keyboard);
            MoveMonsters();
            CollectNearbyObjects();
            if (!_finished)
                CheckMonsterContact();
        }

        void MovePlayer(Keyboard keyboard)
        {
            var input = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            }

            // Clamp the input so diagonals move at the same speed as the four main directions.
            input = Vector2.ClampMagnitude(input, 1f);
            var direction = new Vector3(input.x, 0f, input.y);
            if (direction.sqrMagnitude > 0.001f)
                player.transform.rotation = Quaternion.LookRotation(direction);

            _verticalSpeed = player.isGrounded && _verticalSpeed < 0f ? -2f :
                _verticalSpeed + Physics.gravity.y * Time.deltaTime;
            var target = player.transform.position +
                (direction * moveSpeed + Vector3.up * _verticalSpeed) * Time.deltaTime;
            target.x = Mathf.Clamp(target.x, -arenaHalfSize, arenaHalfSize);
            target.z = Mathf.Clamp(target.z, -arenaHalfSize, arenaHalfSize);
            player.Move(target - player.transform.position);
        }

        void MoveMonsters()
        {
            _patrolTime += Time.deltaTime;
            for (var i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] == null)
                    continue;
                // One patrol goes east-west, the other north-south, around their authored centres.
                var axis = i % 2 == 0 ? Vector3.right : Vector3.forward;
                var offset = Mathf.Sin(_patrolTime * patrolSpeed) * patrolDistance;
                monsters[i].position = _monsterPositions[i] + axis * offset;
            }
        }

        void CollectNearbyObjects()
        {
            for (var i = 0; i < collectibles.Length; i++)
            {
                var item = collectibles[i];
                if (item == null || !item.activeSelf ||
                    PlanarDistanceSquared(player.transform.position, item.transform.position) > 1f)
                    continue;

                item.SetActive(false);
                _collected++;
                PlayPreviewTone();
                _finished = _collected == collectibles.Length;
                SetLevelStatus(_finished ? "All seeds collected! Use Restart to play again." : "Seed collected.");
            }
        }

        void CheckMonsterContact()
        {
            for (var i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] == null ||
                    PlanarDistanceSquared(player.transform.position, monsters[i].position) > 1.5f * 1.5f)
                    continue;

                MoveToSpawn();
                SetLevelStatus("A monster caught you. Your collected seeds are safe.");
                return;
            }
        }

        static float PlanarDistanceSquared(Vector3 a, Vector3 b)
        {
            var difference = a - b;
            difference.y = 0f;
            return difference.sqrMagnitude;
        }

        void MoveToSpawn()
        {
            // Temporarily disable the controller for an exact teleport to the authored spawn.
            var wasEnabled = player.enabled;
            player.enabled = false;
            player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            player.enabled = wasEnabled;
            _verticalSpeed = 0f;
        }

        void SetLevelStatus(string message)
        {
            if (levelStatus != null)
                levelStatus.text = $"Seeds: {_collected}/{collectibles.Length}  |  {message}";
        }

        public void OpenLevel()
        {
            if (_ready) router.Show(levelPageIndex);
        }

        public void OpenSettings()
        {
            if (_ready) router.Show(2);
        }

        public void OpenCharacters()
        {
            if (_ready) router.Show(3);
        }

        public void ReturnToMenu()
        {
            if (_ready) router.ReturnToMenu();
        }

        public void SelectAmber() => SelectCharacter(0);
        public void SelectFrost() => SelectCharacter(1);

        void SelectCharacter(int index)
        {
            if (!_ready || index < 0 || index >= avatarVisuals.Length || avatarVisuals[index] == null)
                return;
            _selectedCharacter = index;
            for (var i = 0; i < avatarVisuals.Length; i++)
                if (avatarVisuals[i] != null)
                    avatarVisuals[i].SetActive(i == index);
            if (characterLabel != null)
                characterLabel.text = "Selected: " + (index == 0 ? "Amber" : "Frost") +
                    ". This avatar will appear in the level.";
        }

        public void SetVolume(float value)
        {
            if (!_ready)
                return;
            var volume = Mathf.Clamp01(value);
            demoAudio.volume = volume;
            if (volumeLabel != null)
                volumeLabel.text = $"Demo volume: {Mathf.RoundToInt(volume * 100f)}%";
        }

        public void SetBrightness(float value)
        {
            if (!_ready)
                return;
            var brightness = Mathf.Clamp01(value);
            demoLight.intensity = Mathf.Lerp(0.4f, 1.6f, brightness);
            if (brightnessLabel != null)
                brightnessLabel.text = $"Brightness: {Mathf.RoundToInt(brightness * 100f)}%";
        }

        public void PlayPreviewTone()
        {
            if (_ready && _previewTone != null && demoAudio != null && demoAudio.isActiveAndEnabled)
                demoAudio.PlayOneShot(_previewTone);
        }

        public void RestartLevel()
        {
            // Inspector-wired buttons can invoke a disabled component after setup validation fails.
            if (!_ready)
                return;
            MoveToSpawn();
            _patrolTime = 0f;
            _collected = 0;
            _finished = false;
            for (var i = 0; i < monsters.Length; i++)
                if (monsters[i] != null)
                    monsters[i].position = _monsterPositions[i];
            for (var i = 0; i < collectibles.Length; i++)
                if (collectibles[i] != null)
                    collectibles[i].SetActive(true);
            SetLevelStatus("WASD/arrows to move. Collect all seeds and avoid the red monsters.");
        }
    }
}
