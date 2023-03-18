using Game.Asteroids;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

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
        hit4
    }

    [SerializeField] GameObject smallExplosionPrefab;
    [SerializeField] GameObject bigExplosionPrefab;
    [SerializeField] GameObject dustExplosionPrefab;
    [SerializeField] GameObject greenExplosionPrefab;
    [SerializeField] GameObject redExplosionPrefab;
    [SerializeField] GameObject spawnPrefab;
    [SerializeField] GameObject portalPrefab;
    [SerializeField] GameObject hit2Prefab;
    [SerializeField] GameObject hit4Prefab;

    GameObjectPool _smallExplosionPool;
    GameObjectPool _bigExplosionPool;
    GameObjectPool _dustExplosionPool;
    GameObjectPool _greenExplosionPool;
    GameObjectPool _redExplosionPool;
    GameObjectPool _spawnPool;
    GameObjectPool _portalPool;
    GameObjectPool _hit2Pool;
    GameObjectPool _hit4Pool;

    readonly List<EffectController> _effectsPlaying = new();

    void Awake() => BuildPools();

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
                _ => null
            };

            pool?.ReturnToPool(ctrl.gameObject);
            _effectsPlaying.RemoveAt(i);
        }
    }

    void BuildPools()
    {
        _smallExplosionPool = GameObjectPool.Build(smallExplosionPrefab, 1);
        _bigExplosionPool = GameObjectPool.Build(bigExplosionPrefab, 1);
        _dustExplosionPool = GameObjectPool.Build(dustExplosionPrefab, 1);
        _greenExplosionPool = GameObjectPool.Build(greenExplosionPrefab, 1);
        _redExplosionPool = GameObjectPool.Build(redExplosionPrefab, 1);
        _spawnPool = GameObjectPool.Build(spawnPrefab, 1);
        _portalPool = GameObjectPool.Build(portalPrefab, 1);
        _hit2Pool = GameObjectPool.Build(hit2Prefab, 1);
        _hit4Pool = GameObjectPool.Build(hit4Prefab, 1);
    }

    public void StartEffect(Effect effect, Vector3 position, float scale, OjectLayer layer)
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
            _ => null
        };

        if (effectObj == null)
            return;

        SetGameObjectLayer(effectObj, layer);

        var ctrl = effectObj.GetComponent<EffectController>();
        var trans = effectObj.transform;
        var eulerZ = new Vector3(0f, 0f, Random.Range(0, 360));

        ctrl.m_effect = effect;
        trans.localScale = new Vector3(scale, scale, scale);
        trans.localPosition = position;

        if (effect != Effect.spawn)
            trans.Rotate(eulerZ);

        _effectsPlaying.Add(ctrl);
    }

}
