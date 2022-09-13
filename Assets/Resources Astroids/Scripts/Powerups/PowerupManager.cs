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

        protected AstroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AstroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AstroidsGameManager __gameManager;

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

            while (GameManager.m_GamePlaying)
            {
                while (GameManager.m_GamePaused)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(minSpawnWait, maxSpawnWait));

                if (!GameManager.m_GamePaused)
                    _shuttlePool.GetFromPool();
            }
        }

        public void SpawnPowerup(Vector3 pos)
        {
            Debug.Log("Powerup pool");
            _firePool.GetFromPool(pos);
        }
    }
}