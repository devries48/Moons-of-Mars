using System.Collections.Generic;
using UnityEngine;

namespace Game.Astroids
{
    public class EffectsManager : MonoBehaviour
    {
        public enum Effect
        {
            explosion,
            dustExplosion,
            greenExplosion
        }

        public GameObject ExplosionPrefab;
        public GameObject DustExplosionPrefab;
        public GameObject GreenExplosionPrefab;

        PrefabObjectPool _explosionPool;
        PrefabObjectPool _dustExplosionPool;
        PrefabObjectPool _greenExplosionPool;

        bool _poolsBuilt;
        readonly List<System.Tuple<GameObject, Effect>> _effectsPlaying = new();

        void Awake() => BuildPools();

        void LateUpdate()
        {
            var i = 0;

            while (i < _effectsPlaying.Count)
            {
                _effectsPlaying[i].Item1.TryGetComponent(out ParticleSystem ps);

                if (ps)
                {
                    if (ps.IsAlive())
                    {
                        i++;
                        continue;
                    }

                    var pool = _effectsPlaying[i].Item2 switch
                    {
                        Effect.explosion => _explosionPool,
                        Effect.dustExplosion => _dustExplosionPool,
                        Effect.greenExplosion => _greenExplosionPool,
                        _ => null
                    };

                    pool?.ReturnToPool(_effectsPlaying[i].Item1);
                }

                _effectsPlaying.RemoveAt(i);
            }
        }

        public void BuildPools()
        {
            if (!_poolsBuilt)
            {
                _explosionPool =  PrefabObjectPool.Build(ExplosionPrefab, 1);
                _dustExplosionPool = PrefabObjectPool.Build(DustExplosionPrefab, 5);
                _greenExplosionPool = PrefabObjectPool.Build(GreenExplosionPrefab, 1);

                _poolsBuilt = true;
            }
        }

        public void StartEffect(Effect effect, Vector3 position, float scale)
        {
            var effectObj = effect switch
            {
                Effect.explosion => _explosionPool.GetFromPool(),
                Effect.dustExplosion => _dustExplosionPool.GetFromPool(),
                Effect.greenExplosion => _greenExplosionPool.GetFromPool(),
                _ => null
            };

            if (effectObj == null)
                return;

            var trans = effectObj.transform;
            var eulerZ = new Vector3(0f, 0f, Random.Range(0, 360));

            trans.localScale = new Vector3(scale, scale, scale);
            trans.localPosition = position;
            trans.Rotate(eulerZ);

            _effectsPlaying.Add(System.Tuple.Create(effectObj, effect));
        }

    }
}