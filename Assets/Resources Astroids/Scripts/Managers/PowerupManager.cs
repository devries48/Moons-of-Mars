using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Astroids
{
    [CreateAssetMenu(fileName = "PowerupManager", menuName = "PowerupManager")]
    public class PowerupManager : ScriptableObject
    {
        #region editor fields
        [SerializeField, Range(10, 60)] int minSpawnWait = 10;
        [SerializeField, Range(10, 60)] int maxSpawnWait = 30;

        [Header("Prefabs")]
        [SerializeField, Tooltip("Shuttle delivering the power-up")] GameObject shuttle;
        [SerializeField] GameObject powerup;

        [Header("Materials")]
        public Material m_fireRatePowerup;
        public Material m_shieldPowerup;
        public Material m_weaponPowerup;

        [Header("Duration")]
        [Range(5, 30)] public int m_showTime = 15;
        [Range(5, 30)] public int m_powerDuration = 10;

        [Header("Score")]
        [SerializeField, Range(0, 200)] int pickupScore = 25;
        [SerializeField, Range(-200, 0)] int destructionScore = -50;
        [SerializeField, Range(-200, 0)] int enemyPickupScore = -50;
        [SerializeField, Range(-200, 0)] int enemyDestructionScore = -25;

        public PowerupSounds m_sounds = new();
        #endregion

        #region properties
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

        List<PowerupController> _powerupList;
        #endregion

        #region fields
        GameObjectPool _shuttlePool;
        GameObjectPool _powerupPool;
        #endregion

        public enum Powerup
        {
            fireRate, // red
            shield,   // blue
            weapon    // green
        }

        void OnDisable() => _powerupList = null;

        public void BuildPools()
        {
            _shuttlePool = GameObjectPool.Build(shuttle, 1);
            _powerupPool = GameObjectPool.Build(powerup, 1);
        }

        public void HideAllPowerups()
        {
            foreach (var powerup in _powerupList)
                powerup.RemoveFromGame();
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

        public void SpawnPowerup(Vector3 pos) => _powerupPool.GetFromPool(pos);
        public int GetPickupScore(bool isEnemy) => isEnemy ? enemyPickupScore : pickupScore;
        public int GetDestructionScore(bool isEnemy) => isEnemy ? enemyDestructionScore : destructionScore;
        public void PlayAudio(PowerupSounds.Clip clip, AudioSource audioSource) => m_sounds.PlayClip(clip, audioSource);
        public IEnumerator PlayDelayedAudio(PowerupSounds.Clip clip, AudioSource audioSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip, audioSource);
        }
    }
}
