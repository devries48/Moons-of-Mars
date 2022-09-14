using Game.Astroids;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public enum Effect
    {
        smallExplosion,
        bigExplosion,
        dustExplosion,
        greenExplosion,
    }

    [SerializeField] GameObject smallExplosionPrefab;
    [SerializeField] GameObject bigExplosionPrefab;
    [SerializeField] GameObject dustExplosionPrefab;
    [SerializeField] GameObject greenExplosionPrefab;

    GameObjectPool _bigExplosionPool;
    GameObjectPool _smallExplosionPool;
    GameObjectPool _dustExplosionPool;
    GameObjectPool _greenExplosionPool;

    readonly List<System.Tuple<GameObject, Effect>> _effectsPlaying = new();

    void Awake() => BuildPools();

    void Update()
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
                    Effect.smallExplosion => _smallExplosionPool,
                    Effect.bigExplosion => _bigExplosionPool,
                    Effect.dustExplosion => _dustExplosionPool,
                    Effect.greenExplosion => _greenExplosionPool,
                    _ => null
                };

                pool?.ReturnToPool(_effectsPlaying[i].Item1);
            }

            _effectsPlaying.RemoveAt(i);
        }
    }

    void BuildPools()
    {
        _smallExplosionPool = GameObjectPool.Build(smallExplosionPrefab, 1);
        _bigExplosionPool = GameObjectPool.Build(bigExplosionPrefab, 1);
        _dustExplosionPool = GameObjectPool.Build(dustExplosionPrefab, 1);
        _greenExplosionPool = GameObjectPool.Build(greenExplosionPrefab, 1);
    }

    public void StartEffect(Effect effect, Vector3 position, float scale)
    {
        var effectObj = effect switch
        {
            Effect.smallExplosion => _smallExplosionPool.GetFromPool(),
            Effect.bigExplosion => _bigExplosionPool.GetFromPool(),
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
