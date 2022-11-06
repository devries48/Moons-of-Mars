using UnityEngine;
using System.Collections;

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
        [SerializeField] Material jumpPowerup;
        [SerializeField] Material shieldPowerup;
        [SerializeField] Material weaponPowerup;

        [Header("Duration")]
        [Range(5, 30)] public int m_ShowTime = 15;
        [Range(5, 30)] public int m_PowerDuration = 10;

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
        #endregion

        #region fields
        GameObjectPool _shuttlePool;
        GameObjectPool _powerupPool;
        #endregion

        public enum Powerup
        {
            jump,     // green
            shield,   // blue
            weapon    // red
        }

        public enum PowerupWeapon
        {
            firerate,
            shotSpread
        }

        void OnEnable() => BuildPools();

        public IEnumerator PowerupSpawnLoop()
        {
            while (GameManager.m_gamePlaying)
            {
                while (GameManager.m_gamePaused)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(minSpawnWait, maxSpawnWait));

                if (GameManager.m_level.AstroidsActive > 2 && !GameManager.m_gamePaused)
                    ShuttleLaunch();
            }
        }

        public void ShuttleLaunch() => _shuttlePool.GetFromPool();
        public void SpawnPowerup(Vector3 pos) => _powerupPool.GetFromPool(pos);
        public int GetPickupScore(bool isEnemy) => isEnemy ? enemyPickupScore : pickupScore;
        public int GetDestructionScore(bool isEnemy) => isEnemy ? enemyDestructionScore : destructionScore;
        public void PlayAudio(PowerupSounds.Clip clip, AudioSource audioSource) => m_sounds.PlayClip(clip, audioSource);

        public IEnumerator PlayDelayedAudio(PowerupSounds.Clip clip, AudioSource audioSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip, audioSource);
        }

        public void SetPowerupMaterial(PowerupController pwr)
        {
            var mat = pwr.m_powerup switch
            {
                Powerup.jump => jumpPowerup,
                Powerup.shield => shieldPowerup,
                Powerup.weapon => weaponPowerup,
                _ => null
            };
            if (mat != null)
                pwr.Renderer.material = mat;

        }

        void BuildPools()
        {
            _shuttlePool = GameObjectPool.Build(shuttle, 1);
            _powerupPool = GameObjectPool.Build(powerup, 1);
        }

    }
}
