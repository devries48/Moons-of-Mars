using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MoonsOfMars.Shared.EffectsManager;
using static MoonsOfMars.Shared.Utils;

namespace MoonsOfMars.Shared
{
    public class EffectsManager : MonoBehaviour
    {
        public enum Effect
        {
            smallExplosion,
            bigExplosion,
            dustExplosion,
            greenExplosion,
            redExplosion,
            spawn,
            portal,
            hit2,
            hit4,
            hitLaser
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

        GameObjectPool _smallExplosionPool;
        GameObjectPool _bigExplosionPool;
        GameObjectPool _dustExplosionPool;
        GameObjectPool _greenExplosionPool;
        GameObjectPool _redExplosionPool;
        GameObjectPool _spawnPool;
        GameObjectPool _portalPool;
        GameObjectPool _hit2Pool;
        GameObjectPool _hit4Pool;
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
                    Effect.smallExplosion => _smallExplosionPool,
                    Effect.bigExplosion => _bigExplosionPool,
                    Effect.dustExplosion => _dustExplosionPool,
                    Effect.greenExplosion => _greenExplosionPool,
                    Effect.redExplosion => _redExplosionPool,
                    Effect.spawn => _spawnPool,
                    Effect.portal => _portalPool,
                    Effect.hit2 => _hit2Pool,
                    Effect.hit4 => _hit4Pool,
                    Effect.hitLaser => _hitLaserPool,
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

            _smallExplosionPool = CreatePool(smallExplosionPrefab);
            _bigExplosionPool = CreatePool(bigExplosionPrefab);
            _dustExplosionPool = CreatePool(dustExplosionPrefab);
            _greenExplosionPool = CreatePool(greenExplosionPrefab);
            _redExplosionPool = CreatePool(redExplosionPrefab);
            _spawnPool = CreatePool(spawnPrefab);
            _portalPool = CreatePool(portalPrefab);
            _hit2Pool = CreatePool(hit2Prefab);
            _hit4Pool = CreatePool(hit4Prefab);
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
                Effect.smallExplosion => _smallExplosionPool.GetFromPool(),
                Effect.bigExplosion => _bigExplosionPool.GetFromPool(),
                Effect.dustExplosion => _dustExplosionPool.GetFromPool(),
                Effect.greenExplosion => _greenExplosionPool.GetFromPool(),
                Effect.redExplosion => _redExplosionPool.GetFromPool(),
                Effect.spawn => _spawnPool.GetFromPool(),
                Effect.portal => _portalPool.GetFromPool(),
                Effect.hit2 => _hit2Pool.GetFromPool(),
                Effect.hit4 => _hit4Pool.GetFromPool(),
                Effect.hitLaser => _hitLaserPool.GetFromPool(),
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

                if (effect != Effect.spawn)
                    trans.Rotate(eulerZ);
            }
            else
                trans.rotation = rotation;

            _effectsPlaying.Add(ctrl);

        }
    }
}