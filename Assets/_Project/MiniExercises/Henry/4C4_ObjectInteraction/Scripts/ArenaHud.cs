using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// On-screen numbers for the TA: obstacle count, how many tiles collided with another tile,
    /// how many are colliding right now, and the controls. Text is rebuilt 10 times per second only.
    /// Point at these numbers during the demo to prove each requirement:
    ///   "Obstacles: 100 / 100"            -> >= 100 obstacles
    ///   "Collided with another obstacle"  -> >= 10 obstacles collide with each other (knock over a line)
    ///   "Respawned"                       -> extra: vanish and respawn
    /// </summary>
    public class ArenaHud : MonoBehaviour
    {
        [SerializeField] DominoArena arena;
        [Tooltip("A tile counts as 'colliding now' if it touched another tile within this many seconds.")]
        [SerializeField] float recentWindow = 1f;

        string _text = "";
        float _nextRefresh;
        GUIStyle _style;

        void Update()
        {
            // Only refresh every 0.1 s: building strings every frame would create garbage for nothing.
            if (arena == null || Time.time < _nextRefresh)
                return;
            _nextRefresh = Time.time + 0.1f;

            // Count over all tiles. Inactive (vanished) tiles are skipped.
            int active = 0, hit = 0, collidingNow = 0;
            foreach (var domino in arena.Dominoes)
            {
                if (!domino.gameObject.activeSelf)
                    continue;
                active++;
                if (domino.IsHit)
                    hit++;
                if (Time.time - domino.LastBodyContactTime <= recentWindow)
                    collidingNow++;
            }

            _text =
                $"Obstacles: {active} / {arena.Dominoes.Count}\n" +
                $"Hit (orange): {hit}\n" +
                $"Collided with another obstacle: {arena.CollidedCount}\n" +
                $"Colliding now (last {recentWindow:0.#} s): {collidingNow}\n" +
                $"Respawned: {arena.RespawnCount}\n" +
                $"Vanish after hit: {(arena.VanishEnabled ? "ON" : "OFF")} ({arena.VanishDelay:0.#} s)\n\n" +
                "WASD move   Shift run   Space jump\n" +
                "Right-drag orbit   Scroll zoom\n" +
                "R rebuild lines   V toggle vanish";
        }

        // OnGUI = Unity's immediate-mode GUI: quick debug text with no Canvas setup needed.
        void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.UpperLeft };
            GUI.Box(new Rect(10f, 10f, 420f, 260f), _text, _style);
        }
    }
}
