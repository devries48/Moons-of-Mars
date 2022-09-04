using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Astroids
{
    [CreateAssetMenu(fileName = "PowerupManager", menuName = "PowerupManager")]
    public class PowerupManager : ScriptableObject
    {
        [SerializeField, Range(10, 60)]
        int minSpawnWait = 10;

        [SerializeField, Range(10, 60)]
        int maxSpawnWait = 30;

        [SerializeField, Tooltip("Shuttle delivering the power-up")]
        GameObject ShuttlePrefab;

        [SerializeField]
        GameObject FirePowerupPrefab;

        List<Powerup> _powerupList;

        GameObjectPool _shuttlePool;
        GameObjectPool _firePool;

        void OnDisable() => _powerupList = null;

        public void BuildPools()
        {
            _shuttlePool = GameObjectPool.Build(ShuttlePrefab, 1);
            _firePool = GameObjectPool.Build(FirePowerupPrefab, 1);
        }

        public void HideAllPowerups()
        {
            foreach (var powerup in _powerupList)
                powerup.RemoveFromGame();
        }

        public void DenyAllPower()
        {
            foreach (var powerup in _powerupList)
                powerup.DenyPower();
        }

        public IEnumerator PowerupSpawnLoop()
        {
            if (_shuttlePool == null)
                BuildPools();

            while (true)
            {
                var wait = Random.Range(minSpawnWait, maxSpawnWait);
                yield return new WaitForSeconds(wait);

                Debug.Log("Powerup pool");
                var powerup = _firePool.GetFromPool();
                var shuttle = _shuttlePool.GetFromPool();

                Debug.Log("shuttle");
                var control = shuttle.GetComponent<AlliedShipController>();
                control.SetPowerup(powerup);
                control.enabled = true;
            }
        }
    }
}
