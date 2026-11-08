using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Keeps exactly one rolling bush alive. When the bush leaves the wind area (or lives too long)
    /// it is destroyed and, after a short delay, a new one is spawned at the upwind side.
    /// </summary>
    public class BushSpawner : MonoBehaviour
    {
        [SerializeField] GameObject bushPrefab;
        [Tooltip("The wind zone's trigger collider; leaving its bounds removes the bush.")]
        [SerializeField] Collider area;
        [SerializeField] Transform spawnPoint;
        [SerializeField] float respawnDelay = 0.5f;
        [Tooltip("Safety: respawn a bush that got stuck.")]
        [SerializeField] float maxLifetime = 12f;

        GameObject _bush;
        float _timer;
        float _age;

        void Update()
        {
            if (_bush != null)
            {
                _age += Time.deltaTime;
                if (!area.bounds.Contains(_bush.transform.position) || _age > maxLifetime)
                {
                    Destroy(_bush);
                    _timer = respawnDelay;
                }
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _bush = Instantiate(bushPrefab, spawnPoint.position, Quaternion.identity);
                _age = 0f;
            }
        }
    }
}