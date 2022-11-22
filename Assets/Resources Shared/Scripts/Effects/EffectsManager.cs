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
        redExplosion,
        spawn,
        portal
    }

    [SerializeField] GameObject smallExplosionPrefab;
    [SerializeField] GameObject bigExplosionPrefab;
    [SerializeField] GameObject dustExplosionPrefab;
    [SerializeField] GameObject greenExplosionPrefab;
    [SerializeField] GameObject redExplosionPrefab;
    [SerializeField] GameObject spawnPrefab;
    [SerializeField] GameObject portalPrefab;

    GameObjectPool _smallExplosionPool;
    GameObjectPool _bigExplosionPool;
    GameObjectPool _dustExplosionPool;
    GameObjectPool _greenExplosionPool;
    GameObjectPool _redExplosionPool;
    GameObjectPool _spawnPool;
    GameObjectPool _portalPool;

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
    }

    public void StartEffect(Effect effect, Vector3 position, float scale)
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
            _ => null
        };

        if (effectObj == null)
            return;

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
