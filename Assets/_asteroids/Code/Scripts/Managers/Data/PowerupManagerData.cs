using UnityEngine;
using System.Collections;
using MoonsOfMars.Shared;

namespace MoonsOfMars.Game.Asteroids
{
    [CreateAssetMenu(fileName = "Powerup Manager data", menuName = "Asteroids/Powerup Manager")]
    public class PowerupManagerData : ScriptableObject
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
        protected AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.GmManager;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;
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
            normal,
            firerate,
            shotSpread
        }

        public void Initialize()
        {
            GameManager.CreateObjectPool(BuildPoolsAction);
        }

        public IEnumerator PowerupSpawnLoop()
        {
            while (!GameManager.IsGameExit)
            {
                while (!GameManager.IsGamePlaying || !GameManager.m_LevelManager.CanActivate(Level.LevelAction.powerUp) || GameManager.m_debug.NoPowerups)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(minSpawnWait, maxSpawnWait));

                if ( GameManager.IsGamePlaying && GameManager.m_LevelManager.AsteroidsActive > 2 )
                    ShuttleLaunch();
            }
        }

        public int ActiveShuttleCount => _shuttlePool != null ? _shuttlePool.CountActive:0;
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

        void BuildPoolsAction()
        {
            _shuttlePool = GameManager.CreateObjectPool(shuttle, 1);
            _powerupPool = GameManager.CreateObjectPool(powerup, 1);

            Debug.Log("Shuttle Pool Created");
        }

    }
}
