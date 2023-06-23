using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    using static Utils;

    [CreateAssetMenu(fileName = "Effects Data", menuName = "Moons-of-Mars/Effects Data")]
    public class EffectsData : ScriptableObject
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

        [Header("Prefabs Effects")]
        [SerializeField] GameObject _smallExplosion;
        [SerializeField] GameObject _bigExplosion;
        [SerializeField] GameObject _dustExplosion;
        [SerializeField] GameObject _greenExplosion;
        [SerializeField] GameObject _redExplosion;
        [SerializeField] GameObject _spawn;
        [SerializeField] GameObject _jumpPortal;
        [SerializeField] GameObject _hyperJump;
        [SerializeField] GameObject _teleport;
        [SerializeField] GameObject _hitLaser;

        public bool ClearEffectsActive { get; private set; }

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

        bool _useObjectPoolScene;
        readonly List<EffectController> _effectsPlaying = new();

        public IEnumerator BuildPools(bool useObjectPoolScene)
        {
            _useObjectPoolScene = useObjectPoolScene;

            _explosionSmallPool = CreatePool(_smallExplosion);
            _explosionBigPool = CreatePool(_bigExplosion);
            _explosionDustPool = CreatePool(_dustExplosion);
            _explosionGreenPool = CreatePool(_greenExplosion);
            _explosionRedPool = CreatePool(_redExplosion);
            _spawnPool = CreatePool(_spawn);
            _jumpPortalPool = CreatePool(_jumpPortal);
            _hyperJumpPool = CreatePool(_hyperJump);
            _teleportPool = CreatePool(_teleport);
            _hitLaserPool = CreatePool(_hitLaser);

            yield return null;
        }

        public IEnumerator ClearEffects()
        {
            ClearEffectsActive = true;

            var i = 0;

            while (i < _effectsPlaying.Count)
            {
                yield return null;

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
            ClearEffectsActive = true;
        }

        public void StartEffect(Effect effect, Vector3 position, float scale, ObjectLayer layer) => StartEffect(effect, position, Quaternion.identity, scale, layer);

        public void StartEffect(Effect effect, Vector3 position, Quaternion rotation = default, float scale = 1f, ObjectLayer layer = ObjectLayer.Effects)
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

        GameObjectPool CreatePool(GameObject prefab)
        {
            if (prefab != null)
                return GameObjectPool.Build(prefab, 1, 50, _useObjectPoolScene);

            return null;
        }
    }
}