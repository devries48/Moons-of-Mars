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
        [SerializeField] float _skyboxSpeed= -0.154f;
        
        [Header("Boss ship")]
        [SerializeField, Range(10, 120)] int _minSpawnWait = 10;
        [SerializeField, Range(10, 120)] int _maxSpawnWait = 30;
        [SerializeField, Tooltip("The boss-ship prefab")] GameObject _bossShipPrefab;
        #endregion

        GameObjectPool _bossShipPool;

        AsteroidsGameManager GameManager => AsteroidsGameManager.GmManager;

        void OnDisable() => StopAllCoroutines();

        void Start() => StartCoroutine(Initialize());

        void Update() => RenderSettings.skybox.SetFloat("_Rotation", _skyboxSpeed * Time.time);

        public IEnumerator Initialize()
        {
            yield return new WaitUntil(() => GameManager != null);

            GameManager.CreateObjectPool(BuildPoolsAction);
            RenderSettings.sun = _sun;

            var t = _stageBackgroundCamera.transform;
            GameManager.m_BackgroundCamera.transform.SetPositionAndRotation(t.position, t.rotation);

            StartCoroutine(BackgroundBossSpawnLoop());
        }

        public IEnumerator BackgroundBossSpawnLoop()
        {
            yield return new WaitUntil(() => GameManager != null);

            while (!GameManager.IsGameExit)
            {
                while (!GameManager.IsGamePlaying && !GameManager.IsGameInMenu
                    || _bossShipPool.CountActive > 0
                    || GameManager.m_LevelManager.HasActiveShuttle)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(_minSpawnWait, _maxSpawnWait));

                BossShipLaunch();
            }
        }

        public void BossShipLaunch() => _bossShipPool.GetFromPool();

        void BuildPoolsAction()
        {
            _bossShipPool = GameManager.CreateObjectPool(_bossShipPrefab, 1);
            Debug.Log("Boss-ship Pool Created");

        }

    }
}