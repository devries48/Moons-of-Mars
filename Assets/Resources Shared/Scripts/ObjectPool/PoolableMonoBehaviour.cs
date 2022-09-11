using Game.Astroids;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolableMonoBehaviour : MonoBehaviour, IPoolable
{
    public bool IsPooled => _pool != null;

    GameObjectPool _pool;

    public void ReturnToPool() => _pool.ReturnToPool(gameObject);
    public void ReturnToPool(GameObject obj) => _pool.ReturnToPool(obj);
    public void SetPool(GameObjectPool pool) => _pool = pool;
}
