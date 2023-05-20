using Cinemachine;
using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class StageManager : MonoBehaviour
    {
        #region editor fields
        [SerializeField] Light _sun;
        [SerializeField] CinemachineVirtualCamera _stageBackgroundCamera;
        [SerializeField] float _skyboxSpeed = -0.154f;

        [Header("Background Meteor")]
        [SerializeField, Range(10, 120)] int _meteorMinSpawnWait = 10;
        [SerializeField, Range(10, 120)] int _meteorMaxSpawnWait = 30;
        [SerializeField, Tooltip("The meteor prefab")] GameObject _meteorPrefab;

        [Header("Background Boss ship")]
        [SerializeField, Range(10, 120)] int _bossMinSpawnWait = 10;
        [SerializeField, Range(10, 120)] int _bossMaxSpawnWait = 30;
        [SerializeField, Tooltip("The boss-ship prefab")] GameObject _bossShipPrefab;
        #endregion

        GameObjectPool _bossShipPool;
        GameObjectPool _meteorPool;

        GameManager GameManager => GameManager.GmManager;

        void OnDisable() => StopAllCoroutines();

        void Start() => StartCoroutine(Initialize());

        void Update() => RenderSettings.skybox.SetFloat("_Rotation", _skyboxSpeed * Time.time);

        public IEnumerator Initialize()
        {
            yield return new WaitUntil(() => GameManager != null);

            //RenderSettings.sun = _sun;

            var t = _stageBackgroundCamera.transform;
            GameManager.m_BackgroundCamera.transform.SetPositionAndRotation(t.position, t.rotation);

            BuildPools();
            StartCoroutine(BackgroundBossSpawnLoop());
            StartCoroutine(BackgroundMeteorSpawnLoop());
        }

        public IEnumerator BackgroundBossSpawnLoop()
        {
            yield return new WaitUntil(() => GameManager != null);

            while (!GameManager.IsGameExit)
            {
                while (!GameManager.IsGamePlaying && !GameManager.IsGameInMenu
                    || GameManager.IsGamePlaying && GameManager.m_LevelManager.AsteroidsActive < 3
                    || GameManager.IsGameStageComplete
                    || GameManager.m_LevelManager.HasActiveShuttle
                    || _bossShipPool.CountActive > 0)

                    yield return null;

                yield return new WaitForSeconds(Random.Range(_bossMinSpawnWait, _bossMaxSpawnWait));

                BossShipLaunch();
            }
        }

        public void BossShipLaunch() => _bossShipPool.GetFromPool();

        public IEnumerator BackgroundMeteorSpawnLoop()
        {
            yield return new WaitUntil(() => GameManager != null);

            while (!GameManager.IsGameExit)
            {
                while (!GameManager.IsGamePlaying && !GameManager.IsGameInMenu
                    || GameManager.IsGameStageComplete)

                    yield return null;

                yield return new WaitForSeconds(Random.Range(_meteorMinSpawnWait, _meteorMaxSpawnWait));

                MeteorLaunch();
            }
        }

        public void MeteorLaunch() => _meteorPool.GetFromPool();

        /// <summary>
        /// Objectpool will be added to the level scene instead of the pool scene.
        /// The pool will now be deleted when the level scene unloads.
        /// </summary>
        void BuildPools()
        {
            _bossShipPool = GameManager.CreateObjectPool(_bossShipPrefab, 1);
            _meteorPool = GameManager.CreateObjectPool(_meteorPrefab, 1);
        }
    }
}