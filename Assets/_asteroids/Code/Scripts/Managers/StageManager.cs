using MoonsOfMars.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoonsOfMars.Game.Asteroids.PowerupManagerData;

namespace MoonsOfMars.Game.Asteroids
{
    public class StageManager : MonoBehaviour
    {
        #region editor fields
        [SerializeField, Range(10, 120)] int _minSpawnWait = 10;
        [SerializeField, Range(10, 120)] int _maxSpawnWait = 30;

        [Header("Prefabs")]
        [SerializeField, Tooltip("The boss-ship prefab")] GameObject _bossShip;
        #endregion

        GameObjectPool _bossShipPool;

        AsteroidsGameManager GameManager => AsteroidsGameManager.GmManager;
        void OnEnable()
        {
            BuildPools();
            StartCoroutine(PowerupSpawnLoop());
        }

        public IEnumerator PowerupSpawnLoop()
        {
            while (GameManager.IsGameActive)
            {
                while (!GameManager.IsGamePlaying && !GameManager.IsGameInMenu 
                    || _bossShipPool.CountActive>0
                    || GameManager.m_LevelManager.HasActiveShuttle)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(_minSpawnWait, _maxSpawnWait));

                if (GameManager.IsGameActive)
                    BossShipLaunch();
            }
        }

        public void BossShipLaunch() => _bossShipPool.GetFromPool();

        void BuildPools()
        {
            _bossShipPool = GameObjectPool.Build(_bossShip, 1);
        }

    }
}