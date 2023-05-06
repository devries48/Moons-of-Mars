using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MoonsOfMars.Shared.Utils;

namespace MoonsOfMars.Shared
{
    public class EffectsManager : MonoBehaviour
    {
        public enum Effect
        {
            ExplosionSmall,
            ExplosionBig,
            ExplosionDust,
            ExplosionGreen,
            ExplosionRed,
            Spawn,
            JumpPortal,
            HyperJump,
            Teleport,
            HitLaser
        }

        [SerializeField] bool useObjectPoolScene;

        [Header("Effects")]
        [SerializeField] GameObject smallExplosionPrefab;
        [SerializeField] GameObject bigExplosionPrefab;
        [SerializeField] GameObject dustExplosionPrefab;
        [SerializeField] GameObject greenExplosionPrefab;
        [SerializeField] GameObject redExplosionPrefab;
        [SerializeField] GameObject spawnPrefab;
        [SerializeField] GameObject portalPrefab;
        [SerializeField] GameObject hit2Prefab;
        [SerializeField] GameObject hit4Prefab;
        [SerializeField] GameObject hitLaserPrefab;

        GameObjectPool _explosionSmallPool;
        GameObjectPool _explosionBigPool;
        GameObjectPool _explosionDustPool;
        GameObjectPool _explosionGreenPool;
        GameObjectPool _explosionRedPool;
        GameObjectPool _spawnPool;
        GameObjectPool _jumpPortalPool;
        GameObjectPool _hyperJumpPool;
        GameObjectPool _teleportPool;
        GameObjectPool _hitLaserPool;

        readonly List<EffectController> _effectsPlaying = new();
        Scene _objectPoolScene;

        public Scene ObjectPoolScene => _objectPoolScene;
        public bool UseObjectPoolScene => useObjectPoolScene;
        public bool ObjectPoolSceneLoaded => _objectPoolScene != null && _objectPoolScene.isLoaded;

        void Start() => StartCoroutine(BuildPools());

        void Update()
        {
            var i = 0;

            while (i < _effectsPlaying.Count)
            {
                var ctrl = _effectsPlaying[i];
                if (ctrl.IsAlive())
                {
                    i++;
                    continue;
                }

                var pool = ctrl.m_effect switch
                {
                    Effect.ExplosionSmall => _explosionSmallPool,
                    Effect.ExplosionBig => _explosionBigPool,
                    Effect.ExplosionDust => _explosionDustPool,
                    Effect.ExplosionGreen => _explosionGreenPool,
                    Effect.ExplosionRed => _explosionRedPool,
                    Effect.Spawn => _spawnPool,
                    Effect.JumpPortal => _jumpPortalPool,
                    Effect.HyperJump => _hyperJumpPool,
                    Effect.Teleport => _teleportPool,
                    Effect.HitLaser => _hitLaserPool,
                    _ => null
                };

                pool?.ReturnToPool(ctrl.gameObject);
                _effectsPlaying.RemoveAt(i);
            }
        }

        IEnumerator BuildPools()
        {
            if (useObjectPoolScene)
                CreateObjectPoolScene();

            while (UseObjectPoolScene && !ObjectPoolSceneLoaded)
                yield return null;

            _explosionSmallPool = CreatePool(smallExplosionPrefab);
            _explosionBigPool = CreatePool(bigExplosionPrefab);
            _explosionDustPool = CreatePool(dustExplosionPrefab);
            _explosionGreenPool = CreatePool(greenExplosionPrefab);
            _explosionRedPool = CreatePool(redExplosionPrefab);
            _spawnPool = CreatePool(spawnPrefab);
            _jumpPortalPool = CreatePool(portalPrefab);
            _hyperJumpPool = CreatePool(hit2Prefab);
            _teleportPool = CreatePool(hit4Prefab);
            _hitLaserPool = CreatePool(hitLaserPrefab);
        }

        void CreateObjectPoolScene()
        {
            _objectPoolScene = SceneManager.GetSceneByName(GameObjectPool.OBJECTPOOL_SCENE);
            if (_objectPoolScene.isLoaded)
                return;

            _objectPoolScene = SceneManager.CreateScene(GameObjectPool.OBJECTPOOL_SCENE);
        }

        GameObjectPool CreatePool(GameObject prefab)
        {
            if (prefab != null)
            {
                return GameObjectPool.Build(prefab, 1, 50, useObjectPoolScene ? _objectPoolScene : default);
            }

            return null;
        }

        public void StartEffect(Effect effect, Vector3 position, float scale, OjectLayer layer)
        {
            StartEffect(effect, position, Quaternion.identity, scale, layer);
        }

        public void StartEffect(Effect effect, Vector3 position, Quaternion rotation = default, float scale = 1f, OjectLayer layer = OjectLayer.Default)
        {
            var effectObj = effect switch
            {
                Effect.ExplosionSmall => _explosionSmallPool.GetFromPool(),
                Effect.ExplosionBig => _explosionBigPool.GetFromPool(),
                Effect.ExplosionDust => _explosionDustPool.GetFromPool(),
                Effect.ExplosionGreen => _explosionGreenPool.GetFromPool(),
                Effect.ExplosionRed => _explosionRedPool.GetFromPool(),
                Effect.Spawn => _spawnPool.GetFromPool(),
                Effect.JumpPortal => _jumpPortalPool.GetFromPool(),
                Effect.HyperJump => _hyperJumpPool.GetFromPool(),
                Effect.Teleport => _teleportPool.GetFromPool(),
                Effect.HitLaser => _hitLaserPool.GetFromPool(),
                _ => null
            };

            if (effectObj == null)
                return;

            SetGameObjectLayer(effectObj, layer);

            var ctrl = effectObj.GetComponent<EffectController>();
            var trans = effectObj.transform;

            ctrl.m_effect = effect;
            trans.localScale = new Vector3(scale, scale, scale);
            trans.localPosition = position;

            if (rotation == Quaternion.identity)
            {
                var eulerZ = new Vector3(0f, 0f, Random.Range(0, 360));

                if (effect != Effect.Spawn)
                    trans.Rotate(eulerZ);
            }
            else
                trans.rotation = rotation;

            _effectsPlaying.Add(ctrl);

        }
    }
}